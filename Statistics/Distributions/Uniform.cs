using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Utilities.Serialization;
using Utilities;
using System.Linq;

namespace Statistics.Distributions
{
    public class Uniform: IDistribution, IValidate<Uniform>
    {
        //TODO: Validation
        #region Fields and Properties
        private double _min;
        private double _max;

        #region IDistribution Properties
        public IDistributionEnum Type => IDistributionEnum.Uniform;
        [Stored(Name = "Min", type =typeof(double))]
        public double Min { get{return _min;} set{_min = value;} }
        [Stored(Name = "Max", type = typeof(double))]
        public double Max { get{return _max;} set{_max = value;} }
        [Stored(Name = "SampleSize", type = typeof(Int32))]
        public int SampleSize { get; set; }
        public bool Truncated { get; set; }
        #endregion
        public IMessageLevels State { get; private set; }
        public IEnumerable<IMessage> Messages { get; private set; }
        #endregion

        #region Constructor
        public Uniform()
        {
            //for reflection
            Min = 0;
            Max = 1;
            SampleSize = 0;
        }
        public Uniform(double min, double max, int sampleSize = int.MaxValue)
        {
            Min = min;
            Max = max;
            SampleSize = sampleSize;
        }

        #endregion

        #region Functions
        public IMessageLevels Validate(IValidator<Uniform> validator, out IEnumerable<IMessage> msgs)
        {
            return validator.IsValid(this, out msgs);
        }
        
        #region IDistribution Functions
        public double PDF(double x){
            if(x<Min){
                return 0;
            }else if(x<= Max){
                return 1/(Max-Min);
            }else{
                return 0;
            }
        }
        public double CDF(double x){
            if(x<Min){
                return 0;
            }else if(x<= Max){
                return (x-Min)/(Max-Min);
            }else{
                return 0;
            }
        }
        public double InverseCDF(double p){
            return Min +((Max-Min)*p);
        }
        public string Print(bool round = false) {
           return "Uniform(range: {Min:"+Min+", Max:"+Max+"})";
        }
        public string Requirements(bool printNotes) => RequiredParameterization(printNotes);
        public bool Equals(IDistribution distribution) => string.Compare(Print(), distribution.Print()) == 0 ? true : false;
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
