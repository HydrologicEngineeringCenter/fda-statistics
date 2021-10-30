using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Utilities;

namespace Statistics.Distributions
{
    public class LogNormal : IDistribution, Utilities.IValidate<LogNormal>
    {      
        internal IRange<double> _ProbabilityRange;
        private MathNet.Numerics.Distributions.LogNormal _Distribution;
        private bool _Constructed;

        #region Properties
        public IDistributionEnum Type => IDistributionEnum.LogNormal;
        [Stored(Name = "Mean", type = typeof(double))]
        public double Mean { get; set; }

        public double Median => _Distribution.Median;

        public double Mode => _Distribution.Mode;

        public double Variance => _Distribution.Variance;
        [Stored(Name = "St_Dev", type = typeof(double))]
        public double StandardDeviation{ get; set; }

        public double Skewness => _Distribution.Skewness;

        public IRange<double> Range { get; set; }
        [Stored(Name = "Min", type = typeof(double))]
        public double Min
        {
            get; set;
        }
        [Stored(Name = "Max", type = typeof(double))]
        public double Max
        {
            get; set;
        }
        [Stored(Name = "SampleSize", type = typeof(Int32))]
        public int SampleSize { get; set; }
        [Stored(Name = "Truncated", type = typeof(bool))]
        public bool Truncated { get; set; }
        public IMessageLevels State { get; private set; }
        public IEnumerable<IMessage> Messages { get; private set; }
        #endregion

        #region Constructor
        public LogNormal()
        {
            //for reflection
            _Distribution = new MathNet.Numerics.Distributions.LogNormal(1, 1);
        }
        public LogNormal(double mean, double standardDeviation, int sampleSize = int.MaxValue)
        {
            Mean = mean;
            StandardDeviation = standardDeviation;
            SampleSize = sampleSize;
            Min = double.NegativeInfinity;
            Max = double.PositiveInfinity;
            BuildFromProperties();
        }
        public LogNormal(double mean, double standardDeviation, double min, double max, int sampleSize = int.MaxValue)
        {
            Mean = mean;
            StandardDeviation = standardDeviation;
            SampleSize = sampleSize;
            Min = min;
            Max = max;
            Truncated = true;
            BuildFromProperties();
            
        }
        public void BuildFromProperties()
        {

            if (!Validation.LogNormalValidator.IsConstructable(Mean, StandardDeviation, SampleSize, out string msg)) throw new InvalidConstructorArgumentsException(msg);
            _Distribution = new MathNet.Numerics.Distributions.LogNormal(Mean, StandardDeviation);
            _ProbabilityRange = FiniteRange(Min, Max);
            Range = IRangeFactory.Factory(_Distribution.InverseCumulativeDistribution(_ProbabilityRange.Min), _Distribution.InverseCumulativeDistribution(_ProbabilityRange.Max));
            Min = Range.Min;
            Max = Range.Max;
            State = Validate(new Validation.LogNormalValidator(), out IEnumerable<Utilities.IMessage> msgs);
            Messages = msgs;
            _Constructed = true;
        }
        #endregion

        #region Functions
        public IMessageLevels Validate(Utilities.IValidator<LogNormal> validator, out IEnumerable<Utilities.IMessage> msgs)
        {
            return validator.IsValid(this, out msgs);
        }
        private IRange<double> FiniteRange(double min = double.NegativeInfinity, double max = double.PositiveInfinity)
        {
            double pmin = 0, epsilon = 1 / 1000000000d;
            double pmax = 1 - pmin;
            if (min.IsFinite() || max.IsFinite())//not entirely sure how inclusive or works with one sided truncation and the while loop below.
            {
                pmin = _Distribution.CumulativeDistribution(min);
                pmax = _Distribution.CumulativeDistribution(max);
            }
            while (!(min.IsFinite() && max.IsFinite()))
            {
                pmin += epsilon;
                pmax -= epsilon;
                if (!min.IsFinite()) min = _Distribution.InverseCumulativeDistribution(pmin);
                if (!max.IsFinite()) max = _Distribution.InverseCumulativeDistribution(pmax);
            }
            return IRangeFactory.Factory(pmin, pmax);
        }

        #region IDistribution
        public double PDF(double x) => _Distribution.Density(x);
        public double CDF(double x) => _Distribution.CumulativeDistribution(x);
        public double InverseCDF(double p)
        {
            if (Truncated && _Constructed)
            {
                p = _ProbabilityRange.Min + (p) * (_ProbabilityRange.Max - _ProbabilityRange.Min);
            }
            if (p <= _ProbabilityRange.Min) return Range.Min;
            if (p >= _ProbabilityRange.Max) return Range.Max;
            return _Distribution.InverseCumulativeDistribution(p);
        }
        public string Print(bool round = false) => round ? Print(Mean, StandardDeviation, SampleSize) : $"LogNormal(mean: {Mean}, sd: {StandardDeviation}, sample size: {SampleSize})";
        public string Requirements(bool printNotes) => RequiredParameterization(printNotes);
        public bool Equals(IDistribution distribution) => string.Compare(Print(), distribution.Print()) == 0 ? true : false;
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
        public XElement WriteToXML()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
