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
        #region Fields and Properties
        internal IRange<double> _ProbabilityRange;
        private MathNet.Numerics.Distributions.Normal _Distribution;
        #region IDistribution Properties
        public IDistributionEnum Type => IDistributionEnum.Normal;
        [Stored(Name = "Mean", type = typeof(double))]
        public double Mean
        {
            get { return _Distribution.Mean; }
            set {
                _Distribution = new MathNet.Numerics.Distributions.Normal(value,_Distribution.StdDev);
            }
        }
        public double Median => _Distribution.Median;
        public double Mode => _Distribution.Mode;
        public double Variance => _Distribution.Variance;
        [Stored(Name = "St_Dev", type = typeof(double))]
        public double StandardDeviation
        {
            get { return _Distribution.StdDev; }
            set
            {
                _Distribution = new MathNet.Numerics.Distributions.Normal(_Distribution.Mean, value);
            }
        }
        public double Skewness => _Distribution.Skewness;
        public Utilities.IRange<double> Range { get; }
        public double Min
        {
            get { return Range.Min(); }
        }
        public double Max
        {
            get { return Range.Max(); }
        }
        [Stored(Name = "SampleSize", type = typeof(Int32))]
        public int SampleSize { get; set; }
        #endregion
        #region IMessagePublisher Properties
        public IMessageLevels State { get; }
        public IEnumerable<Utilities.IMessage> Messages { get; }
        #endregion

        #endregion

        #region Constructor
        public Normal()
        {
            //for reflection;
        }
        public Normal(double mean, double sd, int sampleSize = int.MaxValue)
        {
            if (!Validation.NormalValidator.IsConstructable(mean, sd, sampleSize, out string msg)) throw new Utilities.InvalidConstructorArgumentsException(msg);
            _Distribution = new MathNet.Numerics.Distributions.Normal(mean, stddev: sd);
            _ProbabilityRange = FiniteRange();
            Range = IRangeFactory.Factory(_Distribution.InverseCumulativeDistribution(_ProbabilityRange.Min), _Distribution.InverseCumulativeDistribution(_ProbabilityRange.Max));           
            SampleSize = sampleSize;
            State = Validate(new Validation.NormalValidator(), out IEnumerable<Utilities.IMessage> msgs);
            Messages = msgs;
        }
        #endregion

        #region Functions
        public IMessageLevels Validate(Utilities.IValidator<Normal> validator, out IEnumerable<Utilities.IMessage> msgs)
        {
            return validator.IsValid(this, out msgs);
        }
        private IRange<double> FiniteRange()
        {
            double min = double.NegativeInfinity, max = double.PositiveInfinity, p = 0, epsilon = 1 / 1000000000d;
            while (!(min.IsFinite() && max.IsFinite()))
            {
                p += epsilon;
                if (!min.IsFinite()) min = _Distribution.InverseCumulativeDistribution(p);
                if (!max.IsFinite()) max = _Distribution.InverseCumulativeDistribution(1 - p);
            }
            return IRangeFactory.Factory(p, 1 - p);
        }
        
        #region IDistribution Functions
        public double PDF(double x) => _Distribution.Density(x);
        public double CDF(double x) => _Distribution.CumulativeDistribution(x);
        public double InverseCDF(double p)
        {
            if (p <= _ProbabilityRange.Min) return Range.Min;
            if (p >= _ProbabilityRange.Max) return Range.Max;
            return _Distribution.InverseCumulativeDistribution(p);
        }


        public string Print(bool round = false) => round ? Print(Mean, StandardDeviation, SampleSize): $"Normal(mean: {Mean}, sd: {StandardDeviation}, sample size: {SampleSize})";
        public string Requirements(bool printNotes) => RequiredParameterization(printNotes);
        public bool Equals(IDistribution distribution) => string.Compare(Print(), distribution.Print()) == 0 ? true : false;
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

        public XElement WriteToXML()
        {
            XElement ordinateElem = new XElement(SerializationConstants.NORMAL);
            //mean
            ordinateElem.SetAttributeValue(SerializationConstants.MEAN, Mean);
            //st dev
            ordinateElem.SetAttributeValue(SerializationConstants.ST_DEV, StandardDeviation);
            //sample size
            ordinateElem.SetAttributeValue(SerializationConstants.SAMPLE_SIZE, SampleSize);

            return ordinateElem;
        }

        #endregion
    }
}
