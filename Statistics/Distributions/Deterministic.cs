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
    public class Deterministic : ContinuousDistribution
    {
        #region IDistribution Properties
        public override IDistributionEnum Type => IDistributionEnum.Deterministic;
        public IRange<double> Range { get; set; }

        public override int SampleSize { get; }
        public override bool Truncated { get; }
        public override IMessageLevels State => throw new NotImplementedException();

        public override IEnumerable<IMessage> Messages => throw new NotImplementedException();
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
        public override double CDF(double x)
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

        public override bool Equals(IDistribution distribution)
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

        public override double InverseCDF(double p)
        {
            return Value;
        }

        public override double PDF(double x)
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

        public override string Print(bool round = false)
        {
            return $"The Value is {Value}";
        }

        public override string Requirements(bool printNotes)
        {
            return "A value is required";
        }
        #endregion
    }
}