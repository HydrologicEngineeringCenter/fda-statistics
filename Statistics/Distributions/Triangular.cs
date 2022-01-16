using Base.Implementations;
using Base.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Statistics.Distributions
{
    public class Triangular: ContinuousDistribution
    {
        //TODO: Sample
        #region Fields and Properties
        private double _min;
        private double _max;
        private double _mostlikely;
        #region IDistribution Properties
        public override IDistributionEnum Type => IDistributionEnum.Triangular;
        [Stored(Name = "Min", type =typeof(double))]
        public double Min { get{return _min;} set{_min = value;} }
        [Stored(Name = "Max", type = typeof(double))]
        public double Max { get{return _max;} set{_max = value;} }
        [Stored(Name = "MostLikely", type = typeof(double))]
        public double MostLikely{ get{return _mostlikely;} set{_mostlikely = value;} }
        #endregion
        #endregion

        #region Constructor
        public Triangular()
        {
            //for reflection
            Min = 0;
            Max = 1;
            MostLikely = .5;
            SampleSize = 0;
            addRules();
        }
        public Triangular(double min, double mode, double max, int sampleSize = int.MaxValue)
        {
            Min = min;
            Max = max;
            SampleSize = sampleSize;
            MostLikely = mode;
            //if (!Validation.TriangularValidator.IsConstructable(MostLikely, min, max, out string error)) throw new InvalidConstructorArgumentsException(error);
            addRules();
        }
        private void addRules()
        {
            AddSinglePropertyRule(nameof(Min),
                new Rule(() => {
                    return Min < Max;
                },
                "Min must be smaller than Max.",
                ErrorLevel.Fatal));
            AddSinglePropertyRule(nameof(Min),
                new Rule(() => {
                    return Min < MostLikely;
                },
                "Min must be smaller than MostLikely.",
                ErrorLevel.Fatal));
            AddSinglePropertyRule(nameof(Max),
                new Rule(() => {
                    return Min != Max;
                },
                "Max cannot equal Min.",
                ErrorLevel.Fatal));
            AddSinglePropertyRule(nameof(Max),
                new Rule(() => {
                    return MostLikely < Max;
                },
                "MostLikely must be smaller than Max.",
                ErrorLevel.Fatal));
            AddSinglePropertyRule(nameof(SampleSize),
                new Rule(() => {
                    return SampleSize > 0;
                },
                "SampleSize must be greater than 0.",
                ErrorLevel.Fatal));
        }
        #endregion

        #region Functions
        public IMessageLevels Validate(IValidator<Triangular> validator, out IEnumerable<IMessage> msgs)
        {
            return validator.IsValid(this, out msgs);
        }
        internal static string Print(double mode, IRange<double> range) => $"Triangular(mode: {mode.Print()}, range: [{range.Min.Print()}, {range.Max.Print()}])";
        internal static string RequiredParameterization(bool printNotes)
        {
            string s = $"The Triangular distribution requires the following parameterization: {Parameterization()}.";
            if (printNotes) s += RequirementNotes();
            return s;
        }
        internal static string Parameterization() => $"Triangular(mode: range minimum \u2264 mode \u2264 range maximum, sample size: > 0)";
        internal static string RequirementNotes() => "The mode parameter is also sometimes referred to as the most likely value.";
        
        #region IDistribution Functions
        public override double PDF(double x){
            if(x<Min){
                return 0;
            }else if(x<=MostLikely){
                return 2*(x-Min)/((Max-Min)*(MostLikely-Min));
            }else if(x<=Max){
                return 2*(Max-x)/((Max-Min)*(Max-MostLikely));
            }else{
                return 0;
            }
        }
        public override double CDF(double x){
            if(x<Min){
                return 0;
            }else if (x<= MostLikely){
                return Math.Pow(x-Min,2)/((Max-Min)*(MostLikely-Min));
            }else if(x <= Max){
                return 1- Math.Pow(Max-x,2)/((Max-Min)*(Max-MostLikely));
            }else{
                return 1;
            }
        }
        public override double InverseCDF(double p){
            double a = MostLikely - Min;
            double b = Max - MostLikely;
            if (p <= 0){
                return Min;
            }else if(p<a/(Max-Min)){
                return Min + Math.Sqrt(p*(Max-Min)*a);
            }else if( p<1){
                return Max - Math.Sqrt((1-p)*(Max-Min)*b);
            }else{
                return Max;
            }
        }
        public override string Print(bool round = false){
           return "Tringular(parameters: {Min:"+Min+", Max:"+Max+", Mostlikely:"+MostLikely+"})";
        }
        public override string Requirements(bool printNotes) => RequiredParameterization(printNotes);
        public override bool Equals(IDistribution distribution){
            if (Type==distribution.Type){
                Triangular dist = (Triangular)distribution;
                if (Min == dist.Min){
                    if(Max == dist.Max){
                        if(SampleSize == dist.SampleSize){
                            if(MostLikely== dist.MostLikely){
                                return true;
                            }
                        } 
                    }
                }                
            }
            return false;
        }
        #endregion
        public static Triangular Fit(IEnumerable<double> sample)
        {
            IData data = sample.IsNullOrEmpty() ? throw new ArgumentNullException(nameof(sample)) : IDataFactory.Factory(sample);
            if (!(data.State < IMessageLevels.Error) || data.Elements.Count() < 3) throw new ArgumentException($"The {nameof(sample)} is invalid because it contains an insufficient number of finite, numeric values (3 are required but only {data.Elements.Count()} were provided).");
            ISampleStatistics stats = ISampleStatisticsFactory.Factory(data);
            return new Triangular(stats.Range.Min, stats.Mean, stats.Range.Max, stats.SampleSize);
        }

        #endregion
    }
}
