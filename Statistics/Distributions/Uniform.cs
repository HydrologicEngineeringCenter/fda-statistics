﻿using System;
using System.Collections.Generic;
using Utilities;
using System.Linq;
using Base.Implementations;
using Base.Enumerations;

namespace Statistics.Distributions
{
    public class Uniform: ContinuousDistribution, IValidate<Uniform>
    {
        //TODO: Validation
        #region Fields and Properties
        private double _min;
        private double _max;

        #region IDistribution Properties
        public override IDistributionEnum Type => IDistributionEnum.Uniform;
        [Stored(Name = "Min", type =typeof(double))]
        public double Min { get{return _min;} set{_min = value;} }
        [Stored(Name = "Max", type = typeof(double))]
        public double Max { get{return _max;} set{_max = value;} }
        [Stored(Name = "SampleSize", type = typeof(Int32))]
        public override int SampleSize { get; protected set; }
        public override bool Truncated { get; protected set; }
        #endregion
        public override IMessageLevels State { get; protected set; }
        public override IEnumerable<IMessage> Messages { get; protected set; }
        #endregion

        #region Constructor
        public Uniform()
        {
            //for reflection
            Min = 0;
            Max = 1;
            SampleSize = 0;
            addRules();
        }
        public Uniform(double min, double max, int sampleSize = int.MaxValue)
        {
            Min = min;
            Max = max;
            SampleSize = sampleSize;
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
            AddSinglePropertyRule(nameof(Max),
                new Rule(() => {
                    return Min != Max;
                },
                "Max cannot equal Min.",
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
        public IMessageLevels Validate(IValidator<Uniform> validator, out IEnumerable<IMessage> msgs)
        {
            return validator.IsValid(this, out msgs);
        }
        
        #region IDistribution Functions
        public override double PDF(double x){
            if(x<Min){
                return 0;
            }else if(x<= Max){
                return 1/(Max-Min);
            }else{
                return 0;
            }
        }
        public override double CDF(double x){
            if(x<Min){
                return 0;
            }else if(x<= Max){
                return (x-Min)/(Max-Min);
            }else{
                return 0;
            }
        }
        public override double InverseCDF(double p){
            return Min +((Max-Min)*p);
        }
        public override string Print(bool round = false) {
           return "Uniform(range: {Min:"+Min+", Max:"+Max+"})";
        }
        public override string Requirements(bool printNotes) => RequiredParameterization(printNotes);
        public override bool Equals(IDistribution distribution){
            if (Type==distribution.Type){
                Uniform dist = (Uniform)distribution;
                if (Min == dist.Min)
                {
                    if(Max == dist.Max){
                        if(SampleSize == dist.SampleSize){
                            return true;
                        } 
                    }
                }                
            }
            return false;
        }
        #endregion

        internal static string Print(IRange<double> range) => $"Uniform(range: {range.Print(true)})";
        internal static string RequiredParameterization(bool printNotes = false) => $"The Uniform distribution requires the following parameterization: {Parameterization()}.";
        internal static string Parameterization() => $"Uniform({Validation.Resources.DoubleRangeRequirements()})";

        public static Uniform Fit(IEnumerable<double> sample)
        {
            IData data = sample.IsNullOrEmpty() ? throw new ArgumentNullException(nameof(sample)) : IDataFactory.Factory(sample);
            if (!(data.State < IMessageLevels.Error) || data.Elements.Count() < 3) throw new ArgumentException($"The {nameof(sample)} is invalid because it contains an insufficient number of finite, numeric values (3 are required but only {data.Elements.Count()} were provided).");
            ISampleStatistics stats = ISampleStatisticsFactory.Factory(data);
            return new Uniform(stats.Range.Min, stats.Range.Max, stats.SampleSize);
        }
        #endregion
    }
}
