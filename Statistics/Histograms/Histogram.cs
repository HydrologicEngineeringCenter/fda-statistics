using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using Utilities.Ranges;
using Statistics.Validation;
using System.Xml.Linq;

namespace Statistics.Histograms
{
    public class Histogram
    {
        #region Properties
        private Int32[] _binCounts = new Int32[] { };
        private double _mean;
        private double _sampleVariance;
        private double _min;
        private double _max;
        private double _actualMin;
        private double _actualMax;
        private Int64 _n;
        private double _binWidth;
        public double BinWidth{
            get{
                return _binWidth;
            }
        }
        public Int32[] BinCounts{
            get{
                return _binCounts;
            }
        }
        public double Min {
            get{
                return _min;
            }
            private set{
                _min = value;
            }
        }
        public double Max {
            get{
                return _max;
            }
            set{
                _max = value;
            }
        }
        public double Mean {
            get{
                return _mean;
            }
            private set{
                _mean = value;
            }
        }
        public double Variance {
            get{
                return _sampleVariance*((_n-1)/_n);
            }
        }
        public double StandardDeviation { 
            get {
                return Math.Pow(Variance, 0.5);
            } 
        }
        public Int64 SampleSize {
            get{
                return _n;
            }
            private set{
                _n = value;
            }
        }
        #endregion      
        #region Constructor
        public Histogram(double[] data, double binWidth)
        {
            _binWidth = binWidth;
            if (data == null)
            {
                Min = 0;
                Max = Min + _binWidth;
                Int64 numberOfBins = 1;
                _binCounts = new Int32[numberOfBins];
            }
            else if(data.Length==1){
                Min = Math.Floor(data.Min()); //this need not be a integer - it just needs to be the nearest bin start - a function of bin width.
                Int64 numberOfBins = 1;
                Max = Min + binWidth;
                _binCounts = new Int32[numberOfBins];
                AddObservationsToHistogram(data);
            }else
            {
                Min = Math.Floor(data.Min()); //this need not be a integer - it just needs to be the nearest bin start - a function of bin width.
                Int64 numberOfBins = Convert.ToInt64(Math.Ceiling((data.Max() - Min) / binWidth));
                Max = Min + (numberOfBins * binWidth);
                _binCounts = new Int32[numberOfBins];
                AddObservationsToHistogram(data);
            }
        }
        private Histogram(double min, double max, double binWidth, Int32[] binCounts)
        {
            Min = min;
            Max = max;
            _binWidth = binWidth;
            _binCounts = binCounts;
        }
        #endregion
        public double Skewness()
        {
            double deviation = 0, deviation2 = 0, deviation3 = 0;

            for (int i = 0; i < _binCounts.Length; i++)
            {
                double midpoint = Min + (i * _binWidth) + (0.5 * _binWidth);

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
            if (_n == 0){
                _actualMax = observation;
                _actualMin = observation;
                _mean = observation;
                _sampleVariance = 0;
                _n = 1;
            }else{
                if (observation>_actualMax) _actualMax = observation;
                if (observation<_actualMin) _actualMin = observation;
                _n +=1;
                double tmpMean = _mean +((observation -_mean)/_n);
                _sampleVariance = ((((_n-2)/(_n-1))*_sampleVariance)+(Math.Pow(observation-_mean,2))/_n);
                _mean = tmpMean;
            }
            Int64 quantityAdditionalBins = 0;
            if (observation < Min)
            {   
                quantityAdditionalBins = Convert.ToInt64(Math.Ceiling((Min - observation)/_binWidth));
                Int32[] newBinCounts = new Int32[quantityAdditionalBins + _binCounts.Length];

                for (Int64 i = _binCounts.Length + quantityAdditionalBins -1; i > (quantityAdditionalBins-1); i--)
                {
                    newBinCounts[i] = _binCounts[i - quantityAdditionalBins];
                }
                _binCounts = newBinCounts;
                _binCounts[0] += 1;
                double newMin = Min - (quantityAdditionalBins * _binWidth);
                double max = Max;
                Min = newMin;
            } else if (observation > Max)
            {
                quantityAdditionalBins = Convert.ToInt64(Math.Ceiling((observation - Max+_binWidth) / _binWidth));
                Int32[] newBinCounts = new Int32[quantityAdditionalBins + _binCounts.Length];
                for (Int64 i = 0; i < _binCounts.Length; i++)
                {
                    newBinCounts[i] = _binCounts[i];
                }
                newBinCounts[_binCounts.Length + quantityAdditionalBins-1] += 1;
                _binCounts = newBinCounts;
                double newMax = Min + (_binCounts.Length * _binWidth); //is this right?
                Max = newMax;
            } else
            {
                Int64 newObsIndex = Convert.ToInt64(Math.Floor((observation - Min) / _binWidth));
                if (observation == Max)
                {
                    quantityAdditionalBins = 1;
                    Int32[] newBinCounts = new Int32[quantityAdditionalBins + _binCounts.Length];
                    for (Int64 i = 0; i < _binCounts.Length; i++)
                    {
                        newBinCounts[i] = _binCounts[i];
                    }
                    _binCounts = newBinCounts;
                    double newMax = Min + (_binCounts.Length * _binWidth);//double check
                    Max = newMax;
                }
                _binCounts[newObsIndex] += 1;
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
            Int64 obsIndex = Convert.ToInt64(Math.Floor((x - Min) / _binWidth));
            if (cumulative)
            {
                double sum = 0;
                for (int i = 0; i<obsIndex+1; i++)
                {
                    sum += _binCounts[i];
                }
                return sum;
            }
            else
            {
                return _binCounts[obsIndex];
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
                    double obs = _binCounts[index];
                    double cobs = obs;
                    while (cobs < numobs)
                    {
                        index++;
                        obs = _binCounts[index];
                        cobs += obs;

                    }
                    double fraction = (cobs - numobs) / obs;
                    double binOffSet = Convert.ToDouble(index + 1);
                    return Min + _binWidth * binOffSet - _binWidth * fraction;
                } else
                {
                    Int64 index = _binCounts.Length - 1;
                    double obs = _binCounts[index];
                    double cobs = SampleSize - obs;
                    while (cobs > numobs)
                    {
                        index--;
                        obs = _binCounts[index];
                        cobs -= obs;
                    }
                    double fraction = (numobs - cobs) / obs;
                    double binOffSet = Convert.ToDouble(_binCounts.Length - index);
                    return Max - _binWidth * binOffSet + _binWidth * fraction;
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
            masterElem.SetAttributeValue("Bin Width", _binWidth);
            masterElem.SetAttributeValue("Ordinate_Count", SampleSize);
            for (int i = 0; i < SampleSize; i++)
            {
                XElement rowElement = new XElement("Coordinate");
                rowElement.SetAttributeValue("Bin Counts", _binCounts[i]);
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
        public Histogram Fit(IEnumerable<double> sample, int nBins)
        {
            double min = sample.Min();
            double max = sample.Max();
            double binWidth = (min - max) / nBins;
            Histogram histogram = new Histogram(sample.ToArray(), binWidth);
  
            return histogram;

        }
        #endregion
    }
}
