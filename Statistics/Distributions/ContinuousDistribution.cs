using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Statistics
{
    public abstract class ContinuousDistribution : Base.Implementations.Validation, IDistribution
    {   
        public abstract IDistributionEnum Type { get; }
        public abstract int SampleSize { get; }
        public abstract bool Truncated { get; }
        public abstract IMessageLevels State { get; }
        public abstract IEnumerable<IMessage> Messages { get; }
        public abstract double CDF(double x);
        public abstract bool Equals(IDistribution distribution);
        public abstract double InverseCDF(double p);
        public abstract double PDF(double x);
        public abstract string Print(bool round = false);
        public abstract string Requirements(bool printNotes);
    }
}
