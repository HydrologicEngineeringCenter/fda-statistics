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
        private double _Mean;
        private double _StandardDeviation;
        private bool _Truncated;
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
        [Stored(Name = "Mean", type = typeof(double))]
        public double Mean
        {
            get
            {
                return _Mean;
            }
            set
            {
                _Mean = value;
            }
        }
        [Stored(Name = "Standard Deviation", type = typeof(double))]
        public double StandardDeviation
        {
            get
            {
                return _StandardDeviation;
            }
            set
            {
                _StandardDeviation = value;
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
        public IMessageLevels State { get; private set; }

        public IEnumerable<IMessage> Messages { get; private set; }
        #endregion

        #region Constructor 

        #endregion



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
