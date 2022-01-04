using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Statistics.Distributions
{
    class Graphical : IDistribution
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
        /// _ExceedanceProbabilities represent the interpolated and extrapolated set of exceedance probabilities 
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
        public Graphical(double[] exceedanceProbabilities, double[] flowOrStageValues, int equivalentRecordLength, bool usingFlows = false)
        {
            _SampleSize = equivalentRecordLength;
            _UsingFlows = usingFlows;
            _InputExceedanceProbabilities = exceedanceProbabilities;
            _InputFlowOrStageValues = flowOrStageValues;
        }


        #region Methods
        public double CDF(double x)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IDistribution distribution)
        {
            throw new NotImplementedException();
        }

        public double InverseCDF(double p)
        {
            throw new NotImplementedException();
        }

        public double PDF(double x)
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
