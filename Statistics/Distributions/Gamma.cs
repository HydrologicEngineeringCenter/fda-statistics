using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;

namespace Statistics.Distributions
{
    class Gamma : IDistribution
    {
        #region fields
        private double _Shape;
        private double _Scale;
        #endregion
        #region IDistribution Properties

        public IDistributionEnum Type => throw new NotImplementedException();

        public double Mean => throw new NotImplementedException();

        public double Median => throw new NotImplementedException();

        public double Variance => throw new NotImplementedException();

        public double StandardDeviation => throw new NotImplementedException();

        public double Min => throw new NotImplementedException();

        public double Max => throw new NotImplementedException();

        public double Skewness => throw new NotImplementedException();

        public IRange<double> Range => throw new NotImplementedException();

        public int SampleSize => throw new NotImplementedException();

        public bool Truncated => throw new NotImplementedException();

        public IMessageLevels State => throw new NotImplementedException();

        public IEnumerable<IMessage> Messages => throw new NotImplementedException();
        #endregion

        #region Constructors
        public Gamma(double shape, double scale)
        {
            _Shape = shape;
            _Scale = scale;
        }

        public void BuildFromProperties()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IDistribution Functions
        public double CDF(double x)
        {
            if (x <= 0)
            {
                return 0;
            } else if(x >= double.PositiveInfinity)
            {
                return 1;
            }
            else
            {
                return SpecialFunctions.regIncompleteGamma(_Shape, x / _Scale);
            }
        }

        public bool Equals(IDistribution distribution)
        {
            throw new NotImplementedException();
        }

        public double InverseCDF(double p)
        {
            if (p <= 0.0)
            {
                return 0.0;
            }
            else if (p >= 1.0)
            {
                return double.PositiveInfinity;
            }
            else
            {
                double xMin = 0.0;
                double xMax = 1.0;
                for (int j = 0; j < 100; j++)
                {
                    double pMax = CDF(xMax);
                    if (pMax > p)
                    {
                        return invCDFNewtonBiSearch(p, xMin, xMax, 1E-12, 100);
                    }
                    xMax *= 2.0;
                }
            }
            
        }

        private double invCDFNewtonBiSearch(double p, double xMin, double xMax, double v1, int v2)
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
        #endregion

    }
}