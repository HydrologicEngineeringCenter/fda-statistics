using System;
using System.Collections.Generic;
using System.Text;

namespace Statistics.Distributions
{
    internal class ShiftedGamma
    {
        private readonly MathNet.Numerics.Distributions.Gamma _Gamma;
        
        internal double Shift { get; }
        
        internal ShiftedGamma(double alpha, double beta, double shift)
        {
            // The Gamma distribution is defined by 2 parameters: 
            //      (1) alpha is the shape parameter 
            //      (2) beta is the rate or inverse scale parameter

            //attempt to get around a gamma bug
            double alphaRounded = Math.Round(alpha, 4);
            double betaRounded = Math.Round(beta, 4);
            _Gamma = new MathNet.Numerics.Distributions.Gamma(alphaRounded, 1/betaRounded);
            Shift = shift;
        }

        internal double CDF(double x)
        {
            double val = x - Shift;
            if(Math.Abs(val) < .001)
            {
                val = 0;
            }
            return _Gamma.CumulativeDistribution(val);
        }
        internal double PDF(double x)
        {
            return _Gamma.Density(x - Shift);
        }

        internal double InverseCDF(double p)
        {
            return _Gamma.InverseCumulativeDistribution(p) + Shift;
        }
    }
}
