using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;
using Utilities.Serialization;

namespace Statistics.Distributions
{
    public class Deterministic : IDistribution
    {
        #region IDistribution Properties
        public IDistributionEnum Type => IDistributionEnum.Deterministic;

        public double Mean { get; set;  }

        public double Median { get; set; }

        public double Mode { get; set; }

        public double Min { get; set; }
        public double Max { get; set; }
        public double Variance { get; set; }

        public double StandardDeviation { get; set; }

        public double Skewness { get; set; }

        public IRange<double> Range { get; set; }

        public int SampleSize { get; set; }
        public bool Truncated { get; set; }
        public IMessageLevels State => throw new NotImplementedException();

        public IEnumerable<IMessage> Messages => throw new NotImplementedException();
        #endregion
        [Stored(Name = "Value", type = typeof(double))]
        public double Value { get; set; }
        #region constructor
        public Deterministic(double x)
        {
            Value = x;
        }
        public Deterministic()
        {

        }
        #endregion

        #region functions
        public double CDF(double x)
        {
            if (x >= Value)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public bool Equals(IDistribution distribution)
        {
            if (Type==distribution.Type){
                Deterministic dist = (Deterministic)distribution;
                if (Value == dist.Value)
                {
                    return true;
                }                
            }
            return false;
        }

        public double InverseCDF(double p)
        {
            return Value;
        }

        public double PDF(double x)
        {
            if (x == Value)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public string Print(bool round = false)
        {
            return $"The Value is {Value}";
        }

        public string Requirements(bool printNotes)
        {
            return "A value is required";
        }

        public String WriteToXML()
        {
            return $"{Value}";

        }
        #endregion
    }
}