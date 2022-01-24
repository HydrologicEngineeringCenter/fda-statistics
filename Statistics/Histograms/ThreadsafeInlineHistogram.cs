using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Statistics.Histograms
{
    public class ThreadsafeInlineHistogram
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
        private bool _minHasNotBeenSet = false;
        private int _maxQueueCount = 10000;
        private object _lock = new object();
        private object _bwListLock = new object();
        private static int _enqueue;
        private static int _dequeue;
        private System.ComponentModel.BackgroundWorker _bw;
        private System.Collections.Concurrent.ConcurrentQueue<double> _observations;
        #endregion
        #region Properties

        public bool IsConverged
        {
            get
            {
                ForceDeQueue();//would need to test for convergence if anything is dequeued...
                return _Converged;
            }
        }
        public Int64 ConvergedIteration
        {
            get
            {
                ForceDeQueue();//would need to test for convergence if anything is dequeued...
                return _ConvergedIterations;
            }
        }
        public bool ConvergedOnMax
        {
            get
            {
                ForceDeQueue();//would need to test for convergence if anything is dequeued...
                return _ConvergedOnMax;
            }
        }
        public double BinWidth
        {
            get
            {
                return _BinWidth;
            }
        }
        public Int32[] BinCounts
        {
            get
            {
                ForceDeQueue();
                return _BinCounts;
            }
        }
        public double Min
        {
            get
            {
                ForceDeQueue();
                return _Min;
            }
            private set
            {
                _Min = value;
            }
        }
        public double Max
        {
            get
            {
                ForceDeQueue();
                return _Max;
            }
            set
            {
                _Max = value;
            }
        }
        public double Mean
        {
            get
            {
                ForceDeQueue();
                return _SampleMean;
            }
            private set
            {
                _SampleMean = value;
            }
        }
        public double Variance
        {
            get
            {
                ForceDeQueue();
                return _SampleVariance * (double)((double)(_N - 1) / (double)_N);
            }
        }
        public double StandardDeviation
        {
            get
            {
                //force is triggered on variance.
                return Math.Pow(Variance, 0.5);
            }
        }
        public Int64 SampleSize
        {
            get
            {
                ForceDeQueue();
                return _N;
            }
            private set
            {
                _N = value;
            }
        }
        #endregion
        #region Constructor
        public ThreadsafeInlineHistogram(double min, double binWidth)
        {
            _observations = new System.Collections.Concurrent.ConcurrentQueue<double>();
            _BinWidth = binWidth;
            Min = min;
            Max = Min + _BinWidth;
            Int64 numberOfBins = 1;
            _BinCounts = new Int32[numberOfBins];
            _ConvergenceCriteria = new ConvergenceCriteria();
            _bw = new System.ComponentModel.BackgroundWorker();
            _bw.DoWork += _bw_DoWork;
        }
        public ThreadsafeInlineHistogram(double min, double binWidth, ConvergenceCriteria _c)
        {
            _observations = new System.Collections.Concurrent.ConcurrentQueue<double>();
            _BinWidth = binWidth;
            Min = min;
            Max = Min + _BinWidth;
            Int64 numberOfBins = 1;
            _BinCounts = new Int32[numberOfBins];
            _ConvergenceCriteria = _c;
            _bw = new System.ComponentModel.BackgroundWorker();
            _bw.DoWork += _bw_DoWork;
        }
        public ThreadsafeInlineHistogram(double[] data, double binWidth)
        {
            _observations = new System.Collections.Concurrent.ConcurrentQueue<double>();
            _BinWidth = binWidth;
            if (data != null)
            {
                if (data.Length == 1)
                {
                    Min = data.Min();
                    Int64 numberOfBins = 1;
                    Max = _Min + binWidth;
                    _BinCounts = new Int32[numberOfBins];
                    AddObservationsToHistogram(data);
                }
                else
                {
                    Min = data.Min();
                    Int64 numberOfBins = Convert.ToInt64(Math.Ceiling((data.Max() - _Min) / binWidth));
                    Max = _Min + (numberOfBins * binWidth);
                    _BinCounts = new Int32[numberOfBins];
                    AddObservationsToHistogram(data);
                }
            }
            else
            {
                _minHasNotBeenSet = true;
            }
            _ConvergenceCriteria = new ConvergenceCriteria();
            _bw = new System.ComponentModel.BackgroundWorker();
            _bw.DoWork += _bw_DoWork;
        }
        private ThreadsafeInlineHistogram(double min, double max, double binWidth, Int32[] binCounts)
        {
            _observations = new System.Collections.Concurrent.ConcurrentQueue<double>();
            Min = min;
            Max = max;
            _BinWidth = binWidth;
            _BinCounts = binCounts;
            _ConvergenceCriteria = new ConvergenceCriteria();
            _bw = new System.ComponentModel.BackgroundWorker();
            _bw.DoWork += _bw_DoWork;
        }
        #endregion
        private void _bw_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            DeQueue();
        }
        public double Skewness()
        {
            ForceDeQueue();
            if (_N == 0)
            {
                return double.NaN;
            }
            if (_N <=2)
            {
                return 0;
            }
            if (_Min == (_Max - _BinWidth))
            {
                return 0.0;
            }
            double deviation = 0, deviation2 = 0, deviation3 = 0;

            for (int i = 0; i < _BinCounts.Length; i++)
            {
                double midpoint = Min + (i * _BinWidth) + (0.5 * _BinWidth);

                deviation += midpoint - _SampleMean;
                deviation2 += deviation * deviation;
                deviation3 += deviation2 * deviation;

            }
            double variance = _SampleVariance * (double)((double)(_N - 1) / (double)_N);
            return deviation3 / _N / Math.Pow(variance, 3 / 2);
        }
        #region Functions
        public double HistogramMean()
        {
            ForceDeQueue();
            if (_N == 0)
            {
                return double.NaN;
            }
            if (_Min == (_Max - _BinWidth))
            {
                return _Min + (.5 * _BinWidth);
            }
            double sum = 0;
            for (int i = 0; i < _BinCounts.Length; i++)
            {
                sum += (_Min + (i * _BinWidth) + (0.5 * _BinWidth)) * _BinCounts[i];
            }
            return sum / _N;
        }
        public double HistogramVariance()
        {
            ForceDeQueue();
            if (_N == 0)
            {
                return double.NaN;
            }
            if (_N <=1)
            {
                return 0;
            }
            if (_Min == (_Max - _BinWidth))
            {
                return 0.0;
            }
            double deviation = 0, deviation2 = 0;

            for (int i = 0; i < _BinCounts.Length; i++)
            {
                double midpoint = _Min + (i * _BinWidth) + (0.5 * _BinWidth);

                deviation = midpoint - _SampleMean;
                deviation2 += deviation * deviation;

            }
            return deviation2 / (_N - 1);
        }
        public double HistogramStandardDeviation()
        {
            //force dequeue is triggered in histogram variance.
            return Math.Sqrt(HistogramVariance());
        }
        public void AddObservationToHistogram(double observation)
        {
            _observations.Enqueue(observation);
            Interlocked.Increment(ref _enqueue);
            SafelyDeQueue();
        }
        private void SafelyDeQueue()
        {
            if (_observations.Count > _maxQueueCount && !_bw.IsBusy)
            {
                lock (_bwListLock)
                {
                    if (!_bw.IsBusy) _bw.RunWorkerAsync();
                }
            }
        }
        public void ForceDeQueue()
        {
            if (_observations.Count > 0)
            {
                lock (_bwListLock)
                {
                    if (!_bw.IsBusy)
                    {
                        DeQueue();
                    }
                    else
                    {
                        while (_bw.IsBusy)
                        {
                            Thread.Sleep(1);
                        }
                    }
                }
            }
        }
        private void DeQueue()
        {
            //do NOT reference any properties of this class in this method!
            //it will trigger unsafe operations across threads.
            double observation;
            while (_observations.TryDequeue(out observation))
            {
                if (_N == 0)
                {
                    _SampleMax = observation;
                    _SampleMin = observation;
                    _SampleMean = observation;
                    _SampleVariance = 0;
                    _N = 1;
                    if (_minHasNotBeenSet)
                    {
                        Min = observation;
                    }
                }
                else
                {
                    if (observation > _SampleMax) _SampleMax = observation;
                    if (observation < _SampleMin) _SampleMin = observation;
                    _N += 1;
                    double tmpMean = _SampleMean + ((observation - _SampleMean) / (double)_N);
                    _SampleVariance = ((((double)(_N - 2) / (double)(_N - 1)) * _SampleVariance) + (Math.Pow(observation - _SampleMean, 2)) / (double)_N);
                    _SampleMean = tmpMean;
                }
                Int64 quantityAdditionalBins = 0;
                if (observation < _Min)
                {
                    quantityAdditionalBins = Convert.ToInt64(Math.Ceiling((_Min - observation) / _BinWidth));
                    Int32[] newBinCounts = new Int32[quantityAdditionalBins + _BinCounts.Length];

                    for (Int64 i = _BinCounts.Length + quantityAdditionalBins - 1; i > (quantityAdditionalBins - 1); i--)
                    {
                        newBinCounts[i] = _BinCounts[i - quantityAdditionalBins];
                    }
                    _BinCounts = newBinCounts;
                    _BinCounts[0] += 1;
                    double newMin = _Min - (quantityAdditionalBins * _BinWidth);
                    double max = _Max;
                    Min = newMin;
                }
                else if (observation > _Max)
                {
                    quantityAdditionalBins = Convert.ToInt64(Math.Ceiling((observation - _Max + _BinWidth) / _BinWidth));
                    Int32[] newBinCounts = new Int32[quantityAdditionalBins + _BinCounts.Length];
                    for (Int64 i = 0; i < _BinCounts.Length; i++)
                    {
                        newBinCounts[i] = _BinCounts[i];
                    }
                    newBinCounts[_BinCounts.Length + quantityAdditionalBins - 1] += 1;
                    _BinCounts = newBinCounts;
                    double newMax = _Min + (_BinCounts.Length * _BinWidth); //is this right?
                    Max = newMax;
                }
                else
                {
                    Int64 newObsIndex = 0;
                    if (observation != _Min)
                    {
                        newObsIndex = Convert.ToInt64(Math.Floor((observation - _Min) / _BinWidth));
                    }
                    if (observation == _Max)
                    {
                        quantityAdditionalBins = 1;
                        Int32[] newBinCounts = new Int32[quantityAdditionalBins + _BinCounts.Length];
                        for (Int64 i = 0; i < _BinCounts.Length; i++)
                        {
                            newBinCounts[i] = _BinCounts[i];
                        }
                        _BinCounts = newBinCounts;
                        double newMax = _Min + (_BinCounts.Length * _BinWidth);//double check
                        Max = newMax;
                    }
                    _BinCounts[newObsIndex] += 1;
                }
                Interlocked.Increment(ref _dequeue);
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
            Int64 obsIndex = Convert.ToInt64(Math.Floor((x - _Min) / _BinWidth));
            if (cumulative)
            {
                double sum = 0;
                for (int i = 0; i < obsIndex + 1; i++)
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
            //ForceDeQueue();
            if (_N == 0)
            {
                return double.NaN;
            }
            if (_Min == (_Max - _BinWidth))
            {
                if (x > _Min)
                {
                    if (x <= _Max)
                    {
                        return 1.0;
                    }
                }
                return 0.0;
            }
            double nAtX = Convert.ToDouble(FindBinCount(x, false));
            double n = Convert.ToDouble(_N);
            return nAtX / n;
        }
        public double CDF(double x)
        {
            //ForceDeQueue();
            if (_N == 0)
            {
                return double.NaN;
            }
            if (_Min == (_Max - _BinWidth))
            {
                if (x > _Min)
                {
                    if (x <= _Max)
                    {
                        return (_Max - x) / (_Max - _Min);
                    }
                    else
                    {
                        return 1.0;
                    }
                }
                return 0.0;
            }
            double nAtX = Convert.ToDouble(FindBinCount(x));
            double n = Convert.ToDouble(_N);
            return nAtX / n;
        }
        public double InverseCDF(double p)
        {
            //ForceDeQueue();
            if (p <= 0) return _Min;
            if (p >= 1) return _Max;
            else
            {
                if (_N == 0)
                {
                    return double.NaN;
                }
                if (_Min == (_Max - _BinWidth))
                {
                    return _Min + (_BinWidth * p);
                }
                Int64 numobs = Convert.ToInt64(_N * p);
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
                    double fraction = 0.0;
                    if (obs == 0){
                        fraction = .5;
                    }
                    else
                    {
                        fraction = (cobs - numobs) / obs;
                    }
                    
                    double binOffSet = Convert.ToDouble(index + 1);
                    return _Min + _BinWidth * binOffSet - _BinWidth * fraction;
                }
                else
                {
                    Int64 index = _BinCounts.Length - 1;
                    double obs = _BinCounts[index];
                    double cobs = _N - obs;
                    while (cobs > numobs)
                    {
                        index--;
                        obs = _BinCounts[index];
                        cobs -= obs;
                    }
                    double fraction = 0.0;
                    if (obs == 0)
                    {
                        fraction = .5;
                    }
                    else
                    {
                        fraction = (numobs - cobs) / obs;
                    }
                    double binOffSet = Convert.ToDouble(_BinCounts.Length - index);
                    return _Max - _BinWidth * binOffSet + _BinWidth * fraction;
                }

            }
        }

        public XElement WriteToXML()
        {
            ForceDeQueue();
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
        public static ThreadsafeInlineHistogram ReadFromXML(XElement element)
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
            return new ThreadsafeInlineHistogram(min, max, binWidth, binCounts);
        }
        public bool TestForConvergence(double upperq, double lowerq)
        {
            ForceDeQueue();
            if (_Converged) { return true; }
            if (_N < _ConvergenceCriteria.MinIterations) { return false; }
            if (_N >= _ConvergenceCriteria.MaxIterations)
            {
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
            if (lower_comparison <= (_ConvergenceCriteria.Tolerance * .5)) { lower = true; }
            qval = InverseCDF(upperq);
            qslope = PDF(upperq);
            variance = (upperq * (1 - upperq)) / (((double)_N) * qslope * qslope);
            bool upper = false;
            double upper_comparison = Math.Abs(_ConvergenceCriteria.ZAlpha * Math.Sqrt(variance) / qval);
            if (upper_comparison <= (_ConvergenceCriteria.Tolerance * .5)) { upper = true; }
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
        public Int64 EstimateIterationsRemaining(double upperq, double lowerq)
        {
            if (_Converged) return 0;
            double up = upperq;
            double val = up * (1 - up);
            double uz2 = 2 * _ConvergenceCriteria.ZAlpha;
            double uxp = InverseCDF(up);
            double ufxp = PDF(uxp);
            Int64 upperestimate = Math.Abs((Int64)Math.Ceiling(val * (Math.Pow((uz2 / (uxp * _ConvergenceCriteria.Tolerance * ufxp)), 2.0))));
            double lp = lowerq;
            double lval = lp * (1 - lp);
            double lz2 = 2 * _ConvergenceCriteria.ZAlpha;
            double lxp = InverseCDF(lp);
            double lfxp = PDF(lxp);
            Int64 lowerestimate = Math.Abs((Int64)Math.Ceiling(val * (Math.Pow((lz2 / (lxp * _ConvergenceCriteria.Tolerance * lfxp)), 2.0))));
            Int64 biggestGuess = Math.Max(upperestimate, lowerestimate);
            Int64 remainingIters = _ConvergenceCriteria.MaxIterations - _N;
            return Math.Min(remainingIters,biggestGuess);
        }
        #endregion
    }
}