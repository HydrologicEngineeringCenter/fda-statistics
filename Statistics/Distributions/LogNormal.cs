using Base.Implementations;
using Base.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Statistics.Distributions
{
    public class LogNormal : ContinuousDistribution, Utilities.IValidate<LogNormal>
    {      
        #region Fields and Properties
        private double _mean;
        private double _standardDeviation;
        private double _min;
        private double _max;
        internal IRange<double> _ProbabilityRange;

        #region IDistribution Properties
        public override IDistributionEnum Type => IDistributionEnum.Normal;
        [Stored(Name = "Mean", type = typeof(double))]
         public double Mean { get{return _mean;} set{_mean = value;} }
        [Stored(Name = "Standard_Deviation", type = typeof(double))]
        public double StandardDeviation { get{return _standardDeviation;} set{_standardDeviation = value;} }
        [Stored(Name = "Min", type =typeof(double))]
        public double Min { get{return _min;} set{_min = value;} }
        [Stored(Name = "Max", type = typeof(double))]
        public double Max { get{return _max;} set{_max = value;} }
        [Stored(Name = "SampleSize", type = typeof(Int32))]
        public override int SampleSize { get; protected set; }
        [Stored(Name = "Truncated", type = typeof(bool))]
        public override bool Truncated { get; protected set; }
        #endregion
        #region IMessagePublisher Properties
        public override IMessageLevels State { get; protected set; }
        public override IEnumerable<Utilities.IMessage> Messages { get; protected set; }
        #endregion

        #endregion

        #region Constructor
        public LogNormal()
        {
            //for reflection;
            Mean = 0;
            StandardDeviation = 1.0;
            _ProbabilityRange = IRangeFactory.Factory(0.0, 1.0);
            Min = InverseCDF(0.0000000000001);
            Max = InverseCDF(1-0.0000000000001);
            State = Validate(new Validation.LogNormalValidator(), out IEnumerable<Utilities.IMessage> msgs);
            Messages = msgs;
            addRules();
        }
        public LogNormal(double mean, double sd, int sampleSize = int.MaxValue)
        {
            Mean = mean;
            StandardDeviation = sd;
            SampleSize = sampleSize;
            _ProbabilityRange = IRangeFactory.Factory(0.0, 1.0);
            Min = InverseCDF(0.0000000000001);
            Max = InverseCDF(1-0.0000000000001);
            State = Validate(new Validation.LogNormalValidator(), out IEnumerable<Utilities.IMessage> msgs);
            Messages = msgs;
            addRules();
        }
        public LogNormal(double mean, double sd, double minValue, double maxValue, int sampleSize = int.MaxValue)
        {
            Mean = mean;
            StandardDeviation = sd;
            SampleSize = sampleSize;
            Min = minValue;
            Max = maxValue;
            Truncated = true;
            _ProbabilityRange = FiniteRange(Min, Max);
            State = Validate(new Validation.LogNormalValidator(), out IEnumerable<Utilities.IMessage> msgs);
            Messages = msgs;
            addRules();
            
        }
        private void addRules()
        {
            AddSinglePropertyRule(nameof(StandardDeviation),
                new Rule(() => {
                    return StandardDeviation > 0;
                },
                "Standard Deviation must be greater than 0.",
                ErrorLevel.Fatal));
            AddSinglePropertyRule(nameof(Mean),
                new Rule(() => {
                    return Mean > 0;
                },
                "Mean must be greater than 0.",
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
        public IMessageLevels Validate(Utilities.IValidator<LogNormal> validator, out IEnumerable<Utilities.IMessage> msgs)
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

        #region IDistribution
        public override double PDF(double x){
            Normal sn = new Normal();
            return sn.PDF(Math.Log(x));
        }
        public override double CDF(double x){
            Normal sn = new Normal();
            return sn.CDF(Math.Log(x));
        }
        public override double InverseCDF(double p)
        {
            if (Truncated)
            {
                p = _ProbabilityRange.Min + (p) * (_ProbabilityRange.Max - _ProbabilityRange.Min);
            }
            if (p <= _ProbabilityRange.Min) return Min;
            if (p >= _ProbabilityRange.Max) return Max;
            Normal sn = new Normal();
            return Math.Exp(Mean+sn.InverseCDF(p)*StandardDeviation);
        }
        public override string Print(bool round = false) => round ? Print(Mean, StandardDeviation, SampleSize) : $"LogNormal(mean: {Mean}, sd: {StandardDeviation}, sample size: {SampleSize})";
        public override string Requirements(bool printNotes) => RequiredParameterization(printNotes);
        public override bool Equals(IDistribution distribution) => string.Compare(Print(), distribution.Print()) == 0;
        #endregion

        internal static string Print(double mean, double sd, int n) => $"LogNormal(mean: {mean.Print()}, sd: {sd.Print()}, sample size: {n.Print()})";
        public static string RequiredParameterization(bool printNotes)
        {
            string msg = $"The Log Normal distribution requires the following parameterization: {Parameterization()}.";
            if (printNotes) msg += $" {RequirementNotes()}";
            return msg;
        }
        private static string Parameterization() => $"LogNormal(mean: [{double.MinValue.Print()}, {double.MaxValue.Print()}], sd: [0, {double.MaxValue.Print()}], sample size: > 0)";
        private static string RequirementNotes() => $"The parameters should reflect the log-scale random number values.";
        
        public static IDistribution Fit(IEnumerable<double> sample, bool islogSample = false)
        {
            List<double> logSample = new List<double>();
            if (!islogSample) foreach (double x in sample) logSample.Add(Math.Log10(x));  
            IData data = sample.IsNullOrEmpty() ? throw new ArgumentNullException(nameof(sample)) : islogSample ? IDataFactory.Factory(sample): IDataFactory.Factory(logSample);
            if (!(data.State < IMessageLevels.Error) || data.Elements.Count() < 3) throw new ArgumentException($"The {nameof(sample)} is invalid because it contains an insufficient number of finite, numeric values (3 are required but only {data.Elements.Count()} were provided).");
            ISampleStatistics stats = ISampleStatisticsFactory.Factory(data);
            return new LogNormal(stats.Mean, stats.StandardDeviation, stats.SampleSize);
        }   
        #endregion
    }
}
