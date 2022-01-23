using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using Base.Implementations;
using Base.Enumerations;

namespace Statistics.Distributions
{
    public class Normal : ContinuousDistribution
    {
        //TODO: Sample
        #region Fields and Propertiesj
        private double _mean;
        private double _standardDeviation;

        #region IDistribution Properties
        public override IDistributionEnum Type => IDistributionEnum.Normal;
        [Stored(Name = "Mean", type = typeof(double))]
         public double Mean { get{return _mean;} set{_mean = value;} }
        [Stored(Name = "Standard_Deviation", type = typeof(double))]
        public double StandardDeviation { get{return _standardDeviation;} set{_standardDeviation = value;} }
        #endregion

        #endregion

        #region Constructor
        public Normal()
        {
            //for reflection;
            Mean = 0;
            StandardDeviation = 1.0;
            addRules();
        }
        public Normal(double mean, double sd, int sampleSize = int.MaxValue)
        {
            Mean = mean;
            StandardDeviation = sd;
            SampleSize = sampleSize;
            addRules();

        }
        private void addRules()
        {
            AddSinglePropertyRule(nameof(StandardDeviation),
                new Rule(() => {
                    return StandardDeviation >= 0;
                },
                "Standard Deviation must be greater than or equal to 0.",
                ErrorLevel.Fatal));
            AddSinglePropertyRule(nameof(StandardDeviation),
                new Rule(() => {
                    return StandardDeviation > 0;
                },
                "Standard Deviation shouldnt be 0.",
                ErrorLevel.Minor));
            AddSinglePropertyRule(nameof(SampleSize),
                new Rule(() => {
                    return SampleSize > 0;
                },
                "SampleSize must be greater than 0.",
                ErrorLevel.Fatal));
        }
        #endregion

        #region Functions
        #region IDistribution Functions
        public override double PDF(double x){
            if (StandardDeviation == 0)
            {
                if (x == Mean)
                {
                    return 1.0;
                }
                else
                {
                    return 0.0;
                }
            }
            return Math.Exp(-(x-Mean)*(x-Mean)/(2.0*StandardDeviation*StandardDeviation))/(Math.Sqrt(2.0*Math.PI)*StandardDeviation);
        }
        public override double CDF(double x){
            if(StandardDeviation == 0)
            {
                if( x >= Mean) {
                    return 1.0;
                }
                else
                {
                    return 0.0;
                }
            }
            if (x == Double.PositiveInfinity){
			    return 1.0;
            }
            else if (x == Double.NegativeInfinity){
                return 0.0;
            }
            else if(x>=Mean){
                return 0.5*(1.0+SpecialFunctions.regIncompleteGamma(0.5, (x-Mean)*(x-Mean)/(2.0*StandardDeviation*StandardDeviation)));
            }
            else{
                return 0.5*(1.0-SpecialFunctions.regIncompleteGamma(0.5, (x-Mean)*(x-Mean)/(2.0*StandardDeviation*StandardDeviation)));
            }
        }
        public override double InverseCDF(double p)
        {
            if (p <= 0) return Double.NegativeInfinity;
            if (p >= 1) return Double.PositiveInfinity;
            if (StandardDeviation == 0) return Mean;
            return invCDFNewton(p, Mean, 1e-10,100);
        }

        public override string Print(bool round = false) => round ? Print(Mean, StandardDeviation, SampleSize): $"Normal(mean: {Mean}, sd: {StandardDeviation}, sample size: {SampleSize})";
        public override string Requirements(bool printNotes) => RequiredParameterization(printNotes);
        public override bool Equals(IDistribution distribution){
            if (Type==distribution.Type){
                Normal dist = (Normal)distribution;
                if(SampleSize == dist.SampleSize){
                    if(Mean==dist.Mean){
                        if(StandardDeviation==dist.StandardDeviation){
                            return true;
                        }
                    } 
                }              
            }
            return false;
        }
        #endregion

        internal static string Print(double mean, double sd, int n) => $"Normal(mean: {mean.Print()}, sd: {sd.Print()}, sample size: {n.Print()})";
        public static string RequiredParameterization(bool printNotes = false) => $"The Normal distribution requires the following parameterization: {Parameterization()}.";
        internal static string Parameterization() => $"Normal(mean: [{double.MinValue.Print()}, {double.MaxValue.Print()}], sd: [{double.MinValue.Print()}, {double.MaxValue.Print()}], sample size: > 0)";
        public override IDistribution Fit(double[] sample)
        {
            ISampleStatistics stats = new SampleStatistics(sample);
            return new Normal(stats.Mean, stats.StandardDeviation, stats.SampleSize);
        }

        	/**
	 * @param p = probability between 0 and 1
	 * @return a value corresponding to the inverse of 
	 *         cumulative probability distribution
	 *         found using the Newton method
	 *         This method is not guarantee to converge
	 */
	private double invCDFNewton(double p, double valGuess, double tolP, int maxIter){
        double x=valGuess;
        double testY = CDF(x)-p;
        for(int i=0;i<maxIter;i++){
        	
        	double dfdx = PDF(x);
        	if (Double.MinValue > Math.Abs(dfdx))
        	{
        		//this is a minimum or maximum. Can't get any closer
        		return x;
        	}
        	
            x = x - testY / dfdx;
			testY = CDF(x)-p;
            if(Math.Abs(testY) <= tolP){
                return x;
            } 
        }
        return Double.NaN;
	}
        #endregion
}
}
