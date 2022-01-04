using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Statistics.Distributions
{
    class Graphical
    {
        #region Fields
        private int _SampleSize;
        private bool _Truncated;
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
        private double[] _ExceedanceProbabilities;
        /// <summary>
        /// _FlowOrStageDistributions represet the set of normal distributions with mean and standard deviation computed using the less simple method
        /// </summary>
        private IDistribution[] _FlowOrStageDistributions;
        private bool _UsingFlows;
        #endregion

        #region Properties
        public IDistributionEnum Type => IDistributionEnum.Graphical;
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
        [Stored(Name = "Truncated", type = typeof(bool))]
        public bool Truncated
        {
            get
            {
                return _Truncated;
            }
            set
            {
                _Truncated = value;
            }
        }
        [Stored(Name = "ExceedanceProbabilities", type = typeof(double[]))]
        public double[] ExceedanceProbabilities
        {
            get
            {
                return _ExceedanceProbabilities;
            }
            set
            {
                _ExceedanceProbabilities = value;
            }
        }
        [Stored(Name = "FlowOrStageDistributions", type = typeof(IDistribution[]))]
        public IDistribution[] FlowOrStageDistributions
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
        public IMessageLevels State { get; private set; }

        public IEnumerable<IMessage> Messages { get; private set; }
        #endregion

        #region Constructor 

        #endregion
        public Graphical(double[] exceedanceProbabilities, double[] flowOrStageValues, int equivalentRecordLength, bool usingFlows = false, bool flowsAreLogged = false)
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

            //2. Log flows if using flows 
            if (usingFlows)
            {
                if (flowsAreLogged == true)
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

        #region Private Methods
        public bool IsMonotonicallyIncreasing(double[] arrayOfData)
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
        public bool IsMonotonicallyDecreasing(double[] arrayOfData)
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


        #endregion

        #region Public Methods
        public bool Equals(IDistribution distribution)
        {
            throw new NotImplementedException();
        }

        public string Print(bool round = false)
        {
            throw new NotImplementedException();
        }

        public string Requirements(bool printNotes)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
