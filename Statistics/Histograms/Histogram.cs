using System;
using System.Linq;
using Utilities;
using System.Xml.Linq;

namespace Statistics.Histograms
{
    public class Histogram
    {
        #region Fields
        private Int32[] _BinCounts = new Int32[] { };
        private double _SampleMean;
        private double _SampleVariance;
        private double _Min;
        private double _Max;
        private double _SampleMin;
        private double _SampleMax;
        private Int64 _N;
        private double _BinWidth;
        private bool _Converged = false;
        private long _ConvergedIterations = Int64.MinValue;
        private bool _ConvergedOnMax = false;
        private ConvergenceCriteria _ConvergenceCriteria;      
        #endregion
        #region Properties

        public bool IsConverged
        {
            get
            {
                return _Converged;
            }
        }
        public Int64 ConvergedIteration
        {
            get
            {
                return _ConvergedIterations;
            }
        }
        public double BinWidth{
            get{
                return _BinWidth;
            }
        }
        public Int32[] BinCounts{
            get{
                return _BinCounts;
            }
        }
        public double Min {
            get{
                return _Min;
            }
            private set{
                _Min = value;
            }
        }
        public double Max {
            get{
                return _Max;
            }
            set{
                _Max = value;
            }
        }
        public double Mean {
            get{
                return _SampleMean;
            }
            private set{
                _SampleMean = value;
            }
        }
        public double Variance {
            get{
                return _SampleVariance*(double)((double)(_N-1)/(double)_N);
            }
        }
        public double StandardDeviation { 
            get {
                return Math.Pow(Variance, 0.5);
            } 
        }
        public Int64 SampleSize {
            get{
                return _N;
            }
            private set{
                _N = value;
            }
        }
        #endregion      
        #region Constructor
        public Histogram(double min, double binWidth)
        {
            _BinWidth = binWidth;
            Min = min;
            Max = Min + _BinWidth;
            Int64 numberOfBins = 1;
            _BinCounts = new Int32[numberOfBins];
            _ConvergenceCriteria = new ConvergenceCriteria();
        }
        public Histogram(double min, double binWidth, ConvergenceCriteria _c)
        {
            _BinWidth = binWidth;
            Min = min;
            Max = Min + _BinWidth;
            Int64 numberOfBins = 1;
            _BinCounts = new Int32[numberOfBins];
            _ConvergenceCriteria = _c;
        }
        public Histogram(double[] data, double binWidth)
        {
            _BinWidth = binWidth;
            if (data == null)
            {
                throw new ArgumentNullException("The histogram cannot be constructed without data. Use a different constructor.");
            }
            else if(data.Length==1){
                Min = data.Min();
                Int64 numberOfBins = 1;
                Max = Min + binWidth;
                _BinCounts = new Int32[numberOfBins];
                AddObservationsToHistogram(data);
            }else
            {
                Min = data.Min();
                Int64 numberOfBins = Convert.ToInt64(Math.Ceiling((data.Max() - Min) / binWidth));
                Max = Min + (numberOfBins * binWidth);
                _BinCounts = new Int32[numberOfBins];
                AddObservationsToHistogram(data);
            }
            _ConvergenceCriteria = new ConvergenceCriteria();
        }
        private Histogram(double min, double max, double binWidth, Int32[] binCounts)
        {
            Min = min;
            Max = max;
            _BinWidth = binWidth;
            _BinCounts = binCounts;
            _ConvergenceCriteria = new ConvergenceCriteria();
        }
        #endregion
        public double Skewness()
        {
            double deviation = 0, deviation2 = 0, deviation3 = 0;

            for (int i = 0; i < _BinCounts.Length; i++)
            {
                double midpoint = Min + (i * _BinWidth) + (0.5 * _BinWidth);

                deviation += midpoint - Mean;
                deviation2 += deviation * deviation;
                deviation3 += deviation2 * deviation;

            }
            double skewness = SampleSize > 2 ? deviation3 / SampleSize / Math.Pow(Variance, 3 / 2) : 0;
            return skewness;
        }
        #region Functions
        public double HistogramMean()
        {           
            double sum = 0;
            double min = Min;
                for (int i = 0; i < BinCounts.Length; i++)
                {
                    sum += (min + (i * BinWidth) + (0.5 * BinWidth)) * BinCounts[i];
                }
            double mean = SampleSize > 0 ? sum / SampleSize : double.NaN;
            return mean;
        }
        public double HistogramVariance()
        {
            double deviation = 0, deviation2 = 0;

            for (int i = 0; i < BinCounts.Length; i++)
            {
                double midpoint = Min + (i * BinWidth) + (0.5 * BinWidth);

                deviation = midpoint - Mean;
                deviation2 += deviation * deviation;

            }
            double variance = SampleSize > 1 ? deviation2 / (SampleSize - 1) : 0;
            return variance;
        }
        public double HistogramStandardDeviation(){
            return Math.Sqrt(HistogramVariance());
        }
        public void AddObservationToHistogram(double observation)
        {   
            if (_N == 0){
                _SampleMax = observation;
                _SampleMin = observation;
                _SampleMean = observation;
                _SampleVariance = 0;
                _N = 1;
            }else{
                if (observation>_SampleMax) _SampleMax = observation;
                if (observation<_SampleMin) _SampleMin = observation;
                _N +=1;
                double tmpMean = _SampleMean +((observation -_SampleMean)/(double)_N);
                _SampleVariance = ((((double)(_N-2)/(double)(_N-1))*_SampleVariance)+(Math.Pow(observation-_SampleMean,2))/(double)_N);
                _SampleMean = tmpMean;
            }
            Int64 quantityAdditionalBins = 0;
            if (observation < Min)
            {   
                quantityAdditionalBins = Convert.ToInt64(Math.Ceiling((Min - observation)/_BinWidth));
                Int32[] newBinCounts = new Int32[quantityAdditionalBins + _BinCounts.Length];

                for (Int64 i = _BinCounts.Length + quantityAdditionalBins -1; i > (quantityAdditionalBins-1); i--)
                {
                    newBinCounts[i] = _BinCounts[i - quantityAdditionalBins];
                }
                _BinCounts = newBinCounts;
                _BinCounts[0] += 1;
                double newMin = Min - (quantityAdditionalBins * _BinWidth);
                double max = Max;
                Min = newMin;
            } else if (observation > Max)
            {
                quantityAdditionalBins = Convert.ToInt64(Math.Ceiling((observation - Max+_BinWidth) / _BinWidth));
                Int32[] newBinCounts = new Int32[quantityAdditionalBins + _BinCounts.Length];
                for (Int64 i = 0; i < _BinCounts.Length; i++)
                {
                    newBinCounts[i] = _BinCounts[i];
                }
                newBinCounts[_BinCounts.Length + quantityAdditionalBins-1] += 1;
                _BinCounts = newBinCounts;
                double newMax = Min + (_BinCounts.Length * _BinWidth); //is this right?
                Max = newMax;
            } else
            {
                Int64 newObsIndex = Convert.ToInt64(Math.Floor((observation - Min) / _BinWidth));
                if (observation == Max)
                {
                    quantityAdditionalBins = 1;
                    Int32[] newBinCounts = new Int32[quantityAdditionalBins + _BinCounts.Length];
                    for (Int64 i = 0; i < _BinCounts.Length; i++)
                    {
                        newBinCounts[i] = _BinCounts[i];
                    }
                    _BinCounts = newBinCounts;
                    double newMax = Min + (_BinCounts.Length * _BinWidth);//double check
                    Max = newMax;
                }
                _BinCounts[newObsIndex] += 1;
            }
        }
        public void AddObservationsToHistogram(double[] data)
        {
            foreach (double x in data)
            {
                AddObservationToHistogram(x);
            }
        }
        private double FindBinCount(double x, bool cumulative = true)
        {
            Int64 obsIndex = Convert.ToInt64(Math.Floor((x - Min) / _BinWidth));
            if (cumulative)
            {
                double sum = 0;
                for (int i = 0; i<obsIndex+1; i++)
                {
                    sum += _BinCounts[i];
                }
                return sum;
            }
            else
            {
                return _BinCounts[obsIndex];
            }

        }
        public double PDF(double x)
        {
            double nAtX = Convert.ToDouble(FindBinCount(x, false));
            double n = Convert.ToDouble(SampleSize);
            return nAtX/n;
        }
        public double CDF(double x)
        {
            double nAtX = Convert.ToDouble(FindBinCount(x));
            double n = Convert.ToDouble(SampleSize);
            return nAtX / n;
        }
        public double InverseCDF(double p)
        {
            if (!p.IsOnRange(0, 1)) throw new ArgumentOutOfRangeException($"The provided probability value: {p} is not on the a valid range: [0, 1]");
            else
            {
                if (p==0)
                {
                    return Min;
                }
                if (p==1)
                {
                    return Max;
                }
                Int64 numobs = Convert.ToInt64(SampleSize * p);
                if (p <= 0.5)
                {
                    Int64 index = 0;
                    double obs = _BinCounts[index];
                    double cobs = obs;
                    while (cobs < numobs)
                    {
                        index++;
                        obs = _BinCounts[index];
                        cobs += obs;

                    }
                    double fraction = (cobs - numobs) / obs;
                    double binOffSet = Convert.ToDouble(index + 1);
                    return Min + _BinWidth * binOffSet - _BinWidth * fraction;
                } else
                {
                    Int64 index = _BinCounts.Length - 1;
                    double obs = _BinCounts[index];
                    double cobs = SampleSize - obs;
                    while (cobs > numobs)
                    {
                        index--;
                        obs = _BinCounts[index];
                        cobs -= obs;
                    }
                    double fraction = (numobs - cobs) / obs;
                    double binOffSet = Convert.ToDouble(_BinCounts.Length - index);
                    return Max - _BinWidth * binOffSet + _BinWidth * fraction;
                }
                
            }
        }
        /*
        public double Sample(Random r = null) => InverseCDF(r == null ? new Random().NextDouble() : r.NextDouble());
        public double[] Sample(int sampleSize, Random r = null)
        {
            double[] sample = new double[sampleSize];
            for (int i = 0; i < sampleSize; i++) sample[i] = Sample(r);
            return sample;
        }
        */

        public XElement WriteToXML()
        {
            XElement masterElem = new XElement("Histogram");
            masterElem.SetAttributeValue("Min", Min);
            masterElem.SetAttributeValue("Max", Max);
            masterElem.SetAttributeValue("Bin Width", _BinWidth);
            masterElem.SetAttributeValue("Ordinate_Count", SampleSize);
            for (int i = 0; i < SampleSize; i++)
            {
                XElement rowElement = new XElement("Coordinate");
                rowElement.SetAttributeValue("Bin Counts", _BinCounts[i]);
                masterElem.Add(rowElement);
            }
            return masterElem;
        }
        public static Histogram ReadFromXML(XElement element)
        {
            string minString = element.Attribute("Min").Value;
            double min = Convert.ToDouble(minString);
            string maxString = element.Attribute("Max").Value;
            double max = Convert.ToDouble(maxString);
            string binWidthString = element.Attribute("Bin Width").Value;
            double binWidth = Convert.ToDouble(binWidthString);
            string sampleSizeString = element.Attribute("Ordinate_Count").Value;
            int sampleSize = Convert.ToInt32(sampleSizeString);
            Int32[] binCounts = new Int32[sampleSize];
            int i = 0;
            foreach (XElement binCountElement in element.Elements())
            {
                binCounts[i] = Convert.ToInt32(binCountElement.Value);
                i++;
            }
            return new Histogram(min, max, binWidth, binCounts);
        }
        public bool TestForConvergence(double upperq, double lowerq)
        {
            if (_Converged) { return true; }
            if (_N< _ConvergenceCriteria.MinIterations) { return false; }
            if (_N >= _ConvergenceCriteria.MaxIterations) {
                _Converged = true;
                _ConvergedIterations = _N;
                _ConvergedOnMax = true;
                return true;
            }
            double qval = InverseCDF(lowerq);
            double qslope = PDF(lowerq);
            double variance = (lowerq * (1 - lowerq)) / (((double)_N) * qslope * qslope);
            bool lower = false;
            double lower_comparison = Math.Abs(_ConvergenceCriteria.ZAlpha * Math.Sqrt(variance) / qval);
            if (lower_comparison <= (_ConvergenceCriteria.Tolerance *.5)){ lower = true; }
            qval = InverseCDF(upperq);
            qslope = PDF(upperq);
            variance = (upperq * (1 - upperq)) / (((double)_N) * qslope * qslope);
            bool upper = false;
            double upper_comparison = Math.Abs(_ConvergenceCriteria.ZAlpha * Math.Sqrt(variance) / qval);
            if ( upper_comparison <= (_ConvergenceCriteria.Tolerance *.5)) { upper = true; }
            if (lower)
            {
                _Converged = true;
                _ConvergedIterations = _N;
            }
            if (upper)
            {
                _Converged = true;
                _ConvergedIterations = _N;
            }
            return _Converged;
        }
        #endregion
    }
}
