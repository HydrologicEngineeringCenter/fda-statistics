using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;

namespace Statistics.Distributions
{
    class Empirical : IDistribution
    {
        public IDistributionEnum Type => IDistributionEnum.Empirical;

        public double Mean => throw new NotImplementedException();

        public double Median => throw new NotImplementedException();

        public double Mode => throw new NotImplementedException();

        public double Variance => throw new NotImplementedException();

        public double StandardDeviation => throw new NotImplementedException();

        public double Min => throw new NotImplementedException();

        public double Max => throw new NotImplementedException();

        public double Skewness => throw new NotImplementedException();

        public IRange<double> Range => throw new NotImplementedException();

        public int SampleSize => throw new NotImplementedException();

        public IMessageLevels State => throw new NotImplementedException();

        public IEnumerable<IMessage> Messages => throw new NotImplementedException();

        public void BuildFromProperties()
        {
            throw new NotImplementedException();
        }

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

        public XElement WriteToXML()
        {
            throw new NotImplementedException();
        }
    }
}
