using Statistics.Graphical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statistics.Distributions;
using Utilities;

namespace Statistics.GraphicalRelationships
{
    public class Graphical
    {
        #region Fields
        private double _pMax; //the maximum exceedance probability possible in the frequency curve
        private double _pMin; //the minimum exceedanace probability possible in the frequency curve
        private double _Tolerance = 0.0001;
        private int _SampleSize;
        private double[] _ExpandedFlowOrStageValues;
        private double[] _FlowOrStageStandardErrorsComputed;
        private double[] _FinalProbabilities;
        /// <summary>
        /// _InputExceedanceProbabilities represents the 8 or so exceedance probabilities passed into the constructor 
        /// </summary>
        private double[] _InputExceedanceProbabilities; 
        /// <summary>
        /// _InputFlowOrStageValues represent the 8 or so flow or stage values passed into the constructor 
        /// </summary>
        private double[] _InputFlowOrStageValues;
        /// <summary>
        /// _ExceedanceProbabilities represent the interpolated and extrapolated set of exceedance probabilities. this should probably always be the same.  
        /// </summary>
        private double[] _RequiredExceedanceProbabilities = { .999, .99, .95, .90, .80, .70, .60, .50, .475, .45, .425, .40, .375, .35, .325, .30, .275, .25, .225, .20, .175, .15, .125, .10, .075, .05, .04, .025, .02, .015, .01, 0.009, 0.008, 0.007, 0.006, 0.005, 0.004, .002, .001 };
        /// <summary>
        /// _FlowOrStageDistributions represet the set of normal distributions with mean and standard deviation computed using the less simple method
        /// </summary>
        private Normal[] _FlowOrStageDistributions;
        private bool _UsingFlows;
        #endregion

        #region Properties
        [Stored(Name = "Sample Size", type = typeof(int))]
        public int SampleSize
        {
            get
            {
                return _SampleSize;
            }
            set
            {
                _SampleSize = value;
            }
        }
        [Stored(Name = "ExceedanceProbabilities", type = typeof(double[]))]
        public double[] ExceedanceProbabilities
        {
            get
            {
                return _RequiredExceedanceProbabilities;
            }
            set
            {
                _RequiredExceedanceProbabilities = value;
            }
        }
        [Stored(Name = "FlowOrStageDistributions", type = typeof(IDistribution[]))]
        public Normal[] FlowOrStageDistributions
        {
            get
            {
                return _FlowOrStageDistributions;
            }
            set
            {
                _FlowOrStageDistributions = value;
            }
        }
        [Stored(Name = "UsingFlows", type = typeof(bool))]
        public bool UsingFlows
        {
            get
            {
                return _UsingFlows;
            }
            set
            {
                _UsingFlows = value;
            }
        }
        //TODO: Add validation and set these properties 
        public IMessageLevels State { get; private set; }

        public IEnumerable<IMessage> Messages { get; private set; }
        #endregion

        #region Constructor 
        /// <summary>
        /// Steps to get a complete graphical relationship: 1. Construct Graphical, 2. Compute Confidence Limits, 3. Access Exceedance Probability and FlowOrStageDistribution Public Properties.
        /// ExceedanceProbabilities array and FlowOrStageDistribution IDistribution array can then be used to construct an uncertain paired data object. 
        /// </summary>
        /// <param name="exceedanceProbabilities"></param> User-provided exceedance probabilities. There should be at least 8.
        /// <param name="flowOrStageValues"></param> User-provided flow or stage values. A value should correspond to a probability. 
        /// <param name="equivalentRecordLength"></param> The equivalent record length in years.
        /// <param name="maximumProbability"></param> The maximum exceedance probability used in the frequency relationship.
        /// <param name="minimumProbability"></param> The minimum exceedance probability used in the frequency relationship. 
        /// <param name="usingFlows"></param> True if the frequency relationship is a flow-frequency relationship.
        /// <param name="flowsAreNotLogged"></param> True if the flows provided by the user have not been logged. 
      
        public Graphical(double[] exceedanceProbabilities, double[] flowOrStageValues, int equivalentRecordLength, double maximumProbability = 0.9999, double minimumProbability = 0.0001, bool usingFlows = false, bool flowsAreNotLogged = false)
        {
            //1. Check for null data and check for monotonicity 
            if(exceedanceProbabilities != null && flowOrStageValues != null)
            {
                if (!IsMonotonicallyDecreasing(exceedanceProbabilities))
                {
                    Array.Sort(exceedanceProbabilities);
                }
                if (!IsMonotonicallyIncreasing(flowOrStageValues))
                {
                    Array.Sort(flowOrStageValues);
                }
            }

            _SampleSize = equivalentRecordLength;
            _UsingFlows = usingFlows;
            _InputExceedanceProbabilities = exceedanceProbabilities;
            _pMax = maximumProbability;
            _pMin = minimumProbability;

            //2. Log flows if using flows 
            if (usingFlows)
            {
                if (flowsAreNotLogged)
                {
                    _InputFlowOrStageValues = LogFlows(flowOrStageValues);

                } else
                {
                    _InputFlowOrStageValues = flowOrStageValues;
                }
            } else
            {
                _InputFlowOrStageValues = flowOrStageValues;
            }
        }
        #endregion


        #region Methods
        /// <summary>
        /// This method implements the less simple method to compute confidence limits about a graphical frequency relationship. 
        /// </summary>
        /// <param name="useConstantStandardError"></param> True if user wishes to use constant standard error. 
        /// <param name="probStdErrHighConst"></param> Frequent end of frequency curve after which to hold standard error constant. Default is 0.99.
        /// <param name="probStdErrLowConst"></param> Infrequent end of frequency curve after which to hold standard error constant. Default is 0.01.
        public void ComputeGraphicalConfidenceLimits(bool useConstantStandardError = true, double probStdErrHighConst = 0.99, double probStdErrLowConst = 0.01)
        {   
            ExtendFrequencyCurveBasedOnNormalProbabilityPaper();
            List<double> finalProbabilities = GetFinalProbabilities();
            InterpolateQuantiles interpolatedQuantiles = new InterpolateQuantiles(_InputFlowOrStageValues, _InputExceedanceProbabilities);
            _ExpandedFlowOrStageValues = interpolatedQuantiles.ComputeQuantiles(finalProbabilities.ToArray());
            _FinalProbabilities = finalProbabilities.ToArray();
            _FlowOrStageStandardErrorsComputed = ComputeStandardDeviations(useConstantStandardError, probStdErrHighConst, probStdErrLowConst);
            _FlowOrStageDistributions = ConstructNormalDistributions();
        }
      
        private bool IsMonotonicallyIncreasing(double[] arrayOfData)
        {

            for (int i = 0; i < arrayOfData.Length - 1; i++)
            {
                if (arrayOfData[i] >= arrayOfData[i + 1])
                {
                    return false;
                }
            }
            return true;
        }
        private bool IsMonotonicallyDecreasing(double[] arrayOfData)
        {

            for (int i = 0; i < arrayOfData.Length - 1; i++)
            {
                if (arrayOfData[i] <= arrayOfData[i + 1])
                {
                    return false;
                }
            }
            return true;
        }
        private double[] LogFlows(double[] unloggedFlows)
        {
            double[] loggedFlows = new double[unloggedFlows.Length];
            double minFlow = 0.01; //for log conversion not to fail 
            for (int i = 0; i<unloggedFlows.Length; i++)
            {
                if (unloggedFlows[i] < minFlow)
                {
                    loggedFlows[i] = Math.Log10(minFlow);
                } else
                {
                    loggedFlows[i] = Math.Log10(unloggedFlows[i]);
                }
            }
            return loggedFlows;
        }

        private void ExtendFrequencyCurveBasedOnNormalProbabilityPaper() //I think we need a better name. 
        {
            List<double> xvals = new List<double>();
            List<double> yvals = new List<double>();
            for (int i = 0; i < _InputExceedanceProbabilities.Count(); i++)
            {
                xvals.Add(_InputFlowOrStageValues[i]);
                yvals.Add(_InputExceedanceProbabilities[i]);
            }

            //more frequent of the frequency curve
            if (_pMax - yvals.First() > _Tolerance)
            { //if the maximum exceedance probability is sufficiently larger than the largest exceedance probabiltiy 


                // let x1 be the lowest value in xvals 
                double xl = xvals[0];

                //insert the maximum probability into the first location 
                yvals.Insert(0, _pMax);
               
                if (xl < 0) { xvals.Insert(0, 1.001 * xl); } //if the first value is negative then make it slightly more negative

                if (xl > 0)
                {
                    xvals.Insert(0, .999 * xl);
                } //insert a slightly smaller value 

                else if (xl < -1.0e-4)
                {
                    xvals[0] = 1.001 * xl;//why are we doing it a second time?
                }                   
                else
                {
                    xvals.Insert(0, -1.0e-4);//so if xl is really close to zero, set the value equal to -1e-4?
                } 
            }
            //less frequent end of the frequency curve
            if (yvals.Last() - _pMin > _Tolerance)
            {
                Distributions.Normal standardNormalDistribution = new Distributions.Normal();
                double probabilityLower = _pMin;
                double p1 = yvals[yvals.Count - 2];
                double p2 = yvals.Last();
                double xk = standardNormalDistribution.InverseCDF(probabilityLower);
                double xk1 = standardNormalDistribution.InverseCDF(p1);
                double xk2 = standardNormalDistribution.InverseCDF(p2);
                double x1 = xvals[xvals.Count - 2];
                double x2 = xvals.Last();
                double c = (xk2 - xk1) / (xk - xk1);
                double upperFlowOrStage = ((x2 - x1) + c * x1) / c;
                xvals.Add(upperFlowOrStage);
                yvals.Add(probabilityLower);
            }
            _InputFlowOrStageValues = xvals.ToArray();
            _InputExceedanceProbabilities = yvals.ToArray();
        }

        private List<double> GetFinalProbabilities()
        {
            //TODO: I think this code gets the final probabilities used in the graphical frequency relationship? 
            //_take pfreq and standard probablities and iclude them. EVSET

            List<double> finalProbabilities = new List<double>();
            int totalCount = _InputExceedanceProbabilities.Length + _RequiredExceedanceProbabilities.Length;
            int required = 0;
            int provided = 0;
            for (int i = 0; i < totalCount; i++)
            {
                if (required >= _RequiredExceedanceProbabilities.Length)
                {
                    if (_RequiredExceedanceProbabilities[required - 1] < _InputExceedanceProbabilities[provided])
                    {
                        provided++;
                    }
                    else
                    {
                        finalProbabilities.Add(_InputExceedanceProbabilities[provided]);
                        provided++;
                    }
                    continue;
                }
                if (provided >= _InputExceedanceProbabilities.Count())
                {
                    if (_RequiredExceedanceProbabilities[required] > _InputExceedanceProbabilities[provided - 1])
                    {
                        finalProbabilities.Add(_RequiredExceedanceProbabilities[required]);
                        required++;
                    }
                    else
                    {
                        required++;
                    }
                    continue;
                }
                if (Math.Abs(_RequiredExceedanceProbabilities[required] - _InputExceedanceProbabilities[provided]) < .000001)
                {
                    finalProbabilities.Add(_InputExceedanceProbabilities[provided]);
                    provided++;
                    required++;
                    i++;//skip one
                }
                else if (_RequiredExceedanceProbabilities[required] > _InputExceedanceProbabilities[provided])
                {
                    finalProbabilities.Add(_RequiredExceedanceProbabilities[required]);
                    required++;
                }
                else
                {
                    finalProbabilities.Add(_InputExceedanceProbabilities[provided]);
                    provided++;
                }
            }
            return finalProbabilities;
        }

        private double[] ComputeStandardDeviations(bool useConstantStandardError, double probStdErrHighConst, double probStdErrLowConst)
        {
            int ixSlopeHiConst = -1;
            int ixSlopeLoConst = -1;


            //  the index at which begin to hold standard error constant 
            double maxDiffHi = 1.0e30;
            double maxDiffLo = 1.0e30;
            double diffHi = 0;
            double diffLo = 0;
            double p;
            for (int i = 0; i < _FinalProbabilities.Count(); i++)
            {
                p = _FinalProbabilities[i];
                diffHi = Math.Abs(p - probStdErrHighConst);
                diffLo = Math.Abs(p - probStdErrLowConst);

                if (diffHi < maxDiffHi)
                {
                    ixSlopeHiConst = i;
                    maxDiffHi = diffHi;
                }
                if (diffLo < maxDiffLo)
                {
                    ixSlopeLoConst = i;
                    maxDiffLo = diffLo;
                }
            }
            double p1;
            double p2;
            double slope;
            double stdErrSq;
            int j = 0;
            double px;
            double[] scurveUnAdj = new double[_FinalProbabilities.Count()];
            double[] _scurve = new double[_FinalProbabilities.Count()];
            for (int i = 1; i < _FinalProbabilities.Count() - 1; i++)
            {
                p = 1 - _FinalProbabilities[i];
                p2 = 1 - _FinalProbabilities[i + 1];
                p1 = 1 - _FinalProbabilities[i - 1];
                slope = (_ExpandedFlowOrStageValues[i + 1] - _ExpandedFlowOrStageValues[i - 1]) / (p2 - p1);
                stdErrSq = (p * (1 - p) * Math.Pow(slope, 2.0D)) / _SampleSize;
                _scurve[i] = Math.Sqrt(stdErrSq);
                scurveUnAdj[i] = _scurve[i];

                //    !first and last points
                if (i == 2 | i == _FinalProbabilities.Count())
                {
                    if (i == 2) j = 1;
                    if (i == _FinalProbabilities.Count() - 1) j = _FinalProbabilities.Count();
                    px = 1 - _FinalProbabilities[j];
                    //Equation 6 in the technical reference 
                    _scurve[j] = Math.Sqrt((px * (1 - px) * Math.Pow(slope, 2.0D)) / _SampleSize);
                    scurveUnAdj[j] = _scurve[j];
                }
            }

            //            !Hold standard Error Constant
            if (useConstantStandardError)
            {
                for (int i = ixSlopeHiConst; i < _FinalProbabilities.Count(); i++)
                {
                    _scurve[i] = _scurve[ixSlopeHiConst];
                }
                for (int i = 0; i < ixSlopeLoConst; i++)
                {
                    _scurve[i] = _scurve[ixSlopeLoConst];
                }
            }

            return _scurve;
        }
        private Normal[] ConstructNormalDistributions()
        {
            Normal[] distributionArray = new Normal[_FlowOrStageStandardErrorsComputed.Length];
            for (int i = 0; i < _FlowOrStageStandardErrorsComputed.Length; i++)
            {
                distributionArray[i] = new Distributions.Normal(_ExpandedFlowOrStageValues[i], _FlowOrStageStandardErrorsComputed[i]);
            }
            return distributionArray;
        }

        public bool Equals(double[] probabilities, IDistribution[] distributionOfFlowsOrStages)
        {
            if (_FinalProbabilities == probabilities)
            {
                if (_FlowOrStageDistributions == distributionOfFlowsOrStages)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
