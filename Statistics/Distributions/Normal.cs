using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Utilities.Serialization;
using Utilities;

namespace Statistics.Distributions
{
    public class Normal : IDistribution, Utilities.IValidate<Normal> //IOrdinate<IDistribution>
    {
        //TODO: Sample
        #region Fields and Propertiesj
        private double _mean;
        private double _standardDeviation;
        private double _min;
        private double _max;
        internal IRange<double> _ProbabilityRange;

        #region IDistribution Properties
        public IDistributionEnum Type => IDistributionEnum.Normal;
        [Stored(Name = "Mean", type = typeof(double))]
         public double Mean { get{return _mean;} set{_mean = value;} }
        [Stored(Name = "Standard_Deviation", type = typeof(double))]
        public double StandardDeviation { get{return _standardDeviation;} set{_standardDeviation = value;} }
        [Stored(Name = "Min", type =typeof(double))]
        public double Min { get{return _min;} set{_min = value;} }
        [Stored(Name = "Max", type = typeof(double))]
        public double Max { get{return _max;} set{_max = value;} }
        [Stored(Name = "SampleSize", type = typeof(Int32))]
        public int SampleSize { get; set; }
        [Stored(Name = "Truncated", type = typeof(bool))]
        public bool Truncated { get; set; }
        #endregion
        #region IMessagePublisher Properties
        public IMessageLevels State { get; private set; }
        public IEnumerable<Utilities.IMessage> Messages { get; private set; }
        #endregion

        #endregion

        #region Constructor
        public Normal()
        {
            //for reflection;
            Mean = 0;
            StandardDeviation = 1.0;
            Min = double.NegativeInfinity;
            Max = double.PositiveInfinity;
            if (!Validation.NormalValidator.IsConstructable(Mean, StandardDeviation, SampleSize, out string msg)) throw new Utilities.InvalidConstructorArgumentsException(msg);
            _ProbabilityRange = IRangeFactory.Factory(0.0, 1.0);
            State = Validate(new Validation.NormalValidator(), out IEnumerable<Utilities.IMessage> msgs);
            Messages = msgs;
        }
        public Normal(double mean, double sd, int sampleSize = int.MaxValue)
        {
            Mean = mean;
            StandardDeviation = sd;
            SampleSize = sampleSize;
            Min = double.NegativeInfinity;
            Max = double.PositiveInfinity;
            if (!Validation.NormalValidator.IsConstructable(Mean, StandardDeviation, SampleSize, out string msg)) throw new Utilities.InvalidConstructorArgumentsException(msg);
            _ProbabilityRange = IRangeFactory.Factory(0.0, 1.0);
            State = Validate(new Validation.NormalValidator(), out IEnumerable<Utilities.IMessage> msgs);
            Messages = msgs;

        }
        public Normal(double mean, double sd, double minValue, double maxValue, int sampleSize = int.MaxValue)
        {
            Mean = mean;
            StandardDeviation = sd;
            SampleSize = sampleSize;
            Min = minValue;
            Max = maxValue;
            Truncated = true;
            if (!Validation.NormalValidator.IsConstructable(Mean, StandardDeviation, SampleSize, out string msg)) throw new Utilities.InvalidConstructorArgumentsException(msg);
            _ProbabilityRange = FiniteRange(Min, Max);
            State = Validate(new Validation.NormalValidator(), out IEnumerable<Utilities.IMessage> msgs);
            Messages = msgs;
            
        }

        #endregion

        #region Functions
        public IMessageLevels Validate(Utilities.IValidator<Normal> validator, out IEnumerable<Utilities.IMessage> msgs)
        {
            return validator.IsValid(this, out msgs);
        }
        private IRange<double> FiniteRange(double min, double max)
        {
            double pmin = 0;
            double pmax = 1 - pmin;
            if (min.IsFinite() || max.IsFinite())
            {
                pmin = CDF(min);
                pmax = CDF(max);
            }
            return IRangeFactory.Factory(pmin, pmax);
        }
        
        #region IDistribution Functions
        public double PDF(double x){
            return Math.Exp(-(x-Mean)*(x-Mean)/(2.0*StandardDeviation*StandardDeviation))/(Math.Sqrt(2.0*Math.PI)*StandardDeviation);
        }
        public double CDF(double x){
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
        public double InverseCDF(double p)
        {
            if (Truncated)
            {
                //https://en.wikipedia.org/wiki/Truncated_normal_distribution
                p = _ProbabilityRange.Min + (p) * (_ProbabilityRange.Max - _ProbabilityRange.Min);
            }
            if (p <= _ProbabilityRange.Min) return Min;
            if (p >= _ProbabilityRange.Max) return Max;
            return invCDFNewton(p, Mean, 1e-10,100);
        }


        public string Print(bool round = false) => round ? Print(Mean, StandardDeviation, SampleSize): $"Normal(mean: {Mean}, sd: {StandardDeviation}, sample size: {SampleSize})";
        public string Requirements(bool printNotes) => RequiredParameterization(printNotes);
        public bool Equals(IDistribution distribution){
            if (Type==distribution.Type){
                Normal dist = (Normal)distribution;
                if (Min == dist.Min){
                    if(Max == dist.Max){
                        if(SampleSize == dist.SampleSize){
                            if(Mean==dist.Mean){
                                if(StandardDeviation==dist.StandardDeviation){
                                    return true;
                                }
                            } 
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


        public static Normal Fit(IEnumerable<double> sample)
        {
            IData data = sample.IsNullOrEmpty() ? throw new ArgumentNullException(nameof(sample)) : IDataFactory.Factory(sample);
            if (!(data.State < IMessageLevels.Error) || data.Elements.Count() < 3) throw new ArgumentException($"The {nameof(sample)} is invalid because it contains an insufficient number of finite, numeric values (3 are required but only {data.Elements.Count()} were provided).");
            ISampleStatistics stats = ISampleStatisticsFactory.Factory(data);
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
