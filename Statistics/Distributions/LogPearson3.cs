using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Utilities;
using Utilities.Serialization;

namespace Statistics.Distributions
{
    public class LogPearson3: IDistribution, IValidate<LogPearson3> 
    {
        internal PearsonIII _Distribution;
        internal IRange<double> _ProbabilityRange;
        private bool _Constructed;

        #region Properties
        public IDistributionEnum Type => IDistributionEnum.LogPearsonIII;
        [Stored(Name = "Mean", type = typeof(double))]
        public double Mean { get; set; }
        public double Median { get; private set; }
        public double Variance { get; set; }
        [Stored(Name = "St_Dev", type = typeof(double))]
        public double StandardDeviation { get; set; }
        [Stored(Name = "Skew", type = typeof(double))]
        public double Skewness { get; set; }
        public Utilities.IRange<double> Range { get; set; }
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
        [Stored(Name = "Truncated", type = typeof(bool))]
        public bool Truncated
        {
            get; set;
        }
        [Stored(Name = "SampleSize", type = typeof(Int32))]
        public int SampleSize { get; set; }
        public IMessageLevels State { get; private set; }
        public IEnumerable<Utilities.IMessage> Messages { get; private set; }

        public double Mode => throw new NotImplementedException();
        #endregion

        #region Constructor
        public LogPearson3()
        {
            //for reflection;
            Mean = 0.1;
            StandardDeviation = .01;
            Skewness = .01;
            SampleSize = 1;
            Min = double.NegativeInfinity;
            Max = double.PositiveInfinity;
            BuildFromProperties();
        }
        public LogPearson3(double mean, double standardDeviation, double skew, int sampleSize = int.MaxValue)
        {
            Mean = mean;
            StandardDeviation = standardDeviation;
            Skewness = skew;
            SampleSize = sampleSize;
            Min = double.NegativeInfinity;
            Max = double.PositiveInfinity;
            BuildFromProperties();
        }
        public LogPearson3(double mean, double standardDeviation, double skew, double min, double max, int sampleSize = int.MaxValue)
        {
            Mean = mean;
            StandardDeviation = standardDeviation;
            Skewness = skew;
            SampleSize = sampleSize;
            Min = min;
            Max = max;
            Truncated = true;
            BuildFromProperties();
            
        }
        public void BuildFromProperties()
        {
            if (!Validation.LogPearson3Validator.IsConstructable(Mean, StandardDeviation, Skewness, SampleSize, out string error)) throw new Utilities.InvalidConstructorArgumentsException(error);
            else
            {
                _Distribution = new PearsonIII(Mean, StandardDeviation, Skewness, SampleSize);
                Variance = Math.Pow(StandardDeviation, 2);
                Median = InverseCDF(0.50);
                _ProbabilityRange = FiniteRange(Min, Max);
                Range = IRangeFactory.Factory(InverseCDF(_ProbabilityRange.Min), InverseCDF(_ProbabilityRange.Max));
                Min = Range.Min;
                Max = Range.Max;
                State = Validate(new Validation.LogPearson3Validator(), out IEnumerable<Utilities.IMessage> msgs);
                Messages = msgs;
            }
            _Constructed = true;
        }
        #endregion

        #region Functions
        public IMessageLevels Validate(IValidator<LogPearson3> validator, out IEnumerable<Utilities.IMessage> msgs)
        {
            return validator.IsValid(this, out msgs);
        }

        private IRange<double> FiniteRange(double min = double.NegativeInfinity, double max = double.PositiveInfinity)
        {
            double pmin = 0, epsilon = 1 / 1000000000d;
            double pmax = 1 - pmin;
            if (min.IsFinite() || max.IsFinite())//not entirely sure how inclusive or works with one sided truncation and the while loop below.
            {
                pmin = CDF(min);
                pmax = CDF(max);
            }
            else
            {
                pmin = .0000001;
                pmax = 1 - pmin;
                min = InverseCDF(pmin);
                max = InverseCDF(pmax);
            }
            while (!(min.IsFinite() && max.IsFinite()))
            {
                pmin += epsilon;
                pmax -= epsilon;
                if (!min.IsFinite()) min = InverseCDF(pmin);
                if (!max.IsFinite()) max = InverseCDF(pmax);
                if (pmin > 0.25)
                    throw new InvalidConstructorArgumentsException($"The log Pearson III object is not constructable because 50% or more of its distribution returns {double.NegativeInfinity} and {double.PositiveInfinity}.");
            }
            return IRangeFactory.Factory(pmin, pmax);

        }
        #region IDistribution Functions
        public double PDF(double x)
        {
            if (x < Range.Min || x > Range.Max) return double.Epsilon;
            else
            {
                return _Distribution.PDF(Math.Log10(x))/x/Math.Log(10);

            }          
        }
        public double CDF(double x)
        {
            if (x < Range.Min) return 0;
            if (x == Range.Min) return _ProbabilityRange.Min;
            if (x == Range.Max) return _ProbabilityRange.Max;
            if (x > Range.Max) return 1;
            if (x > 0)
            {
                return _Distribution.CDF(Math.Log10(x));
            }
            else return 0;
        }
        public double InverseCDF(double p)
        {
            if (Truncated && _Constructed)
            {
                p = _ProbabilityRange.Min + (p) * (_ProbabilityRange.Max - _ProbabilityRange.Min);
            }
            if (!p.IsFinite()) 
            {
                throw new ArgumentException($"The value of specified probability parameter: {p} is invalid because it is not on the valid probability range: [0, 1].");
            }
            else // Range has been set check p against [_ProbabilityRange.Min, _ProbabilityRange.Max]
            {
                if (Range.IsNull()) // object is being constructued
                { 
                    if (p < 0 || p > 1) throw new ArgumentException($"The value of specified probability parameter: {p} is invalid because it is not on the valid probability range: [0, 1].");
                }
                else
                {
                    if (p <= _ProbabilityRange.Min) return Range.Min;
                    if (p >= _ProbabilityRange.Max) return Range.Max;
                }
            }
            return Math.Pow(10, _Distribution.InverseCDF(p));
        }
        public string Print(bool round = false) => round ? Print(Mean, StandardDeviation, Skewness, SampleSize) : $"log PearsonIII(mean: {Mean}, sd: {StandardDeviation}, skew: {Skewness}, sample size: {SampleSize})";
        public string Requirements(bool printNotes) => RequiredParameterization(printNotes);
        public bool Equals(IDistribution distribution) => string.Compare(Print(), distribution.Print(), StringComparison.InvariantCultureIgnoreCase) == 0 ? true : false;
        #endregion

        internal static string Print(double mean, double sd, double skew, int n) => $"log PearsonIII(mean: {mean.Print()}, sd: {sd.Print()}, skew: {skew.Print()}, sample size: {n.Print()})";
        internal static string RequiredParameterization(bool printNotes = true)
        {
            string s = $"The log PearsonIII distribution requires the following parameterization: {Parameterization()}.";
            if (printNotes) s += RequirementNotes();
            return s;
        }
        internal static string Parameterization() => $"log PearsonIII(mean: (0, {Math.Log10(double.MaxValue).Print()}], sd: (0, {Math.Log10(double.MaxValue).Print()}], skew: [{(Math.Log10(double.MaxValue) * -1).Print()}, {Math.Log10(double.MaxValue).Print()}], sample size: > 0)";
        internal static string RequirementNotes() => $"The distribution parameters are computed from log base 10 random numbers (e.g. the log Pearson III distribution is a distribution of log base 10 Pearson III distributed random values). Therefore the mean and standard deviation parameters must be positive finite numbers and while a large range of numbers are acceptable a relative small rate will produce meaningful results.";

        public static LogPearson3 Fit(IEnumerable<double> sample, bool isLogSample = false)
        {
            return Fit(sample, isLogSample);
        }
        public static LogPearson3 Fit(IEnumerable<double> sample, int sampleSize, bool isLogSample = false)
        {
            return Fit(sample, isLogSample, sampleSize);
        }
        private static LogPearson3 Fit(IEnumerable<double> sample, bool isLogSample = false, int sampleSize = -404)
        {
            List<double> logSample = new List<double>();
            if (!isLogSample) foreach (double x in sample) logSample.Add(Math.Log10(x));
            IData data = sample.IsNullOrEmpty() ? throw new ArgumentNullException(nameof(sample)) : isLogSample ? IDataFactory.Factory(sample) : IDataFactory.Factory(logSample);
            if (!(data.State < IMessageLevels.Error) || data.Elements.Count() < 3) throw new ArgumentException($"The {nameof(sample)} is invalid because it contains an insufficient number of finite, numeric values (3 are required but only {data.Elements.Count()} were provided).");
            ISampleStatistics stats = ISampleStatisticsFactory.Factory(data);
            return new LogPearson3(stats.Mean, stats.StandardDeviation, stats.Skewness, sampleSize == -404 ? stats.SampleSize : sampleSize);
        }

        public XElement WriteToXML()
        {
            XElement ordinateElem = new XElement(SerializationConstants.LOG_PEARSON3);
            //mean
            ordinateElem.SetAttributeValue(SerializationConstants.MEAN, Mean);
            //st dev
            ordinateElem.SetAttributeValue(SerializationConstants.ST_DEV, StandardDeviation);
            //skew
            ordinateElem.SetAttributeValue("Skew", Skewness);
            //sample size
            ordinateElem.SetAttributeValue(SerializationConstants.SAMPLE_SIZE, SampleSize);
            return ordinateElem;
        }
        #endregion
    }
}
