using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using MathNet.Numerics.Statistics;
using Statistics.Histograms;
using Utilities;

namespace Statistics
{
    /// <summary>
    /// Provide factory methods for the construction of objects implementing the <see cref="IDistribution"/> interface.
    /// </summary>
    public static class IDistributionFactory
    {
        //TODO: Validate
        //TODO: Call other constructors with inputs and IDistributions Enum (may require switch case on enum values) 
        
        /// <summary>
        /// Prints a <see cref="string"/> describing the parameterization requirement for the specified <see cref="IDistributionEnum"/>.
        /// </summary>
        /// <param name="type"> The type of distribution to analyze. </param>
        /// <returns> A <see cref="string"/> description. </returns>
        public static string PrintParamaterizationRequirements(IDistributionEnum type)
        {
            switch (type) 
            {
                case IDistributionEnum.Histogram:
                    throw new NotImplementedException("Parametrization is not implemented for histograms");
                case IDistributionEnum.Beta4Parameters:
                    return Distributions.Beta4Parameters.RequiredParameterization(true);
                case IDistributionEnum.LogPearsonIII:
                    return Distributions.LogPearson3.RequiredParameterization(true);
                case IDistributionEnum.Normal:
                    return Distributions.Normal.RequiredParameterization(true);
                case IDistributionEnum.Triangular:
                    return Distributions.Triangular.RequiredParameterization(true);
                case IDistributionEnum.Uniform:
                    return Distributions.Uniform.RequiredParameterization(true);
                case IDistributionEnum.TruncatedBeta4Parameter:
                case IDistributionEnum.TruncatedHistogram:
                case IDistributionEnum.TruncatedNormal:
                case IDistributionEnum.TruncatedTriangular:
                case IDistributionEnum.TruncatedUniform:
                    //return Distributions.TruncatedDistribution.RequiredParameterization(true);
                case IDistributionEnum.NotSupported:
                default:
                    throw new NotImplementedException();
            }
        }


        internal static IDistribution Fit(IEnumerable<double> sample, IDistributionEnum returnType)
        {
            if ((int)returnType >= 10)
            {
                ISampleStatistics stats = ISampleStatisticsFactory.Factory(sample);
                return Fit(sample, stats.Range.Min, stats.Range.Max, returnType);
            }
            else
            {
                switch (returnType)
                {
                    case IDistributionEnum.Normal:
                        return Distributions.Normal.Fit(sample);
                    case IDistributionEnum.Uniform:
                        return Distributions.Uniform.Fit(sample);
                    case IDistributionEnum.Beta4Parameters:
                        return Distributions.Beta4Parameters.Fit(sample);
                    case IDistributionEnum.Triangular:
                        return Distributions.Triangular.Fit(sample);
                    case IDistributionEnum.Histogram:
                        return (IDistribution)Fit(sample, nBins: 100);
                    case IDistributionEnum.LogPearsonIII:
                        return Distributions.LogPearson3.Fit(sample,sample.Count());
                    default:
                        throw new NotImplementedException($"An unexpected error occurred. The requested return type: {returnType} is unsupported");
                }
            }          
        }
        internal static Statistics.Histograms.Histogram Fit(IEnumerable<double> sample, int nBins)
        {
            double min = sample.Min();
            double max = sample.Max();
            double binWidth = (min - max) / nBins;
            IData data = new Data(sample);
            Statistics.Histograms.Histogram histogram = new Statistics.Histograms.Histogram(data, binWidth);
            return histogram;
        }
        internal static IDistribution Fit(IEnumerable<double> sample, double minimum, double maximum, IDistributionEnum returnType)
        {
            if ((int)returnType < 10)
            {
                return Fit(sample, returnType);
            }
            else
            {
                throw new NotImplementedException();
                //IDistribution distribution = (IDistribution)Fit(sample, (int)returnType / 10);//this is all sorts of broken, return type is a distribution enum, being forced into number of bins.
                //return new Distributions.TruncatedDistribution(distribution, minimum, maximum);
            }
        }
        
        /// <summary>
        /// Constructs a <see cref="IDistributionEnum.Normal"/> distribution.
        /// </summary>
        /// <param name="mean"> The mean of the distribution. </param>
        /// <param name="stDev"> The standard deviation of the distribution. </param>
        /// <param name="sampleSize"> An optional sample size parameter. If a population rather than sample distribution is intended leave this parameter blank. Set to <see cref="int.MaxValue"/> by default. </param>
        /// <returns> A new <see cref="IDistribution"/>. </returns>
        public static IDistribution FactoryNormal(double mean, double stDev, int sampleSize = int.MaxValue)
        {
            return new Distributions.Normal(mean, stDev, sampleSize);
        }
        /// <summary>
        /// Constructs a <see cref="IDistributionEnum.Triangular"/> distribution.
        /// </summary>
        /// <param name="min"> The minimum value of the distribution. </param>
        /// <param name="mostLikely"> The most likely value of the distribution. </param>
        /// <param name="max"> The maximum value of the distribution. </param>
        /// <param name="sampleSize"> An optional sample size parameter. If a population rather than sample distribution is intended leave this parameter blank. Set to <see cref="int.MaxValue"/> by default. </param>
        /// <returns> A new <see cref="IDistribution"/>. </returns>
        public static IDistribution FactoryTriangular(double min, double mostLikely, double max, int sampleSize = int.MaxValue)
        {
            return new Distributions.Triangular(min, mostLikely, max, sampleSize);
        }
        /// <summary>
        /// Constructs a <see cref="IDistributionEnum.Uniform"/> distribution.
        /// </summary>
        /// <param name="min"> The minimum value of the distribution. </param>
        /// <param name="max"> The maximum value of the distribution. </param>
        /// <param name="sampleSize"> An optional sample size parameter. If a population rather than sample distribution is intended leave this parameter blank. Set to <see cref="int.MaxValue"/> by default. </param>
        /// <returns> A new <see cref="IDistribution"/>. </returns>
        public static IDistribution FactoryUniform(double min, double max, int sampleSize = int.MaxValue)
        {
            return new Distributions.Uniform(min, max, sampleSize);
        }
        /// <summary>
        /// Constructs a Truncated <see cref="IDistributionEnum.Normal"/> distribution.
        /// </summary>
        /// <param name="mean"> The mean of the distribution. </param>
        /// <param name="stDev"> The standard deviation of the distribution. </param>
        /// <param name="min"> The lower (minimum) truncation value. </param>
        /// <param name="max"> The upper (maximum) truncation value. </param>
        /// <param name="sampleSize"> An optional sample size parameter. If a population rather than sample distribution is intended leave this parameter blank. Set to <see cref="int.MaxValue"/> by default. </param>
        /// <returns> A new <see cref="IDistribution"/>. </returns>
        public static IDistribution FactoryTruncatedNormal(double mean, double stDev, double min, double max, int sampleSize = int.MaxValue)
        {
            IDistribution normal = new Distributions.Normal(mean, stDev, min, max, sampleSize);
            return normal;
        }
        /// <summary>
        /// Constructs a scaled beta distribution.
        /// </summary>
        /// <param name="alpha"> Exponential shape parameter, must be positive, alpha > 0. </param>
        /// <param name="beta"> Exponential shape parameter, must be positive, beta > 0. </param>
        /// <param name="location"> The lower bound or minimum of the scaled distribution (e.g. shift from an unscaled distribution). </param>
        /// <param name="scale"> The range of the distribution (e.g. upper bound or maximum minus the lower bound or minimum), must be positive scale > 0. </param>
        /// <param name="sampleSize"> An optional sample size parameter. If a population rather than sample distribution is intended leave this parameter blank. Set to <see cref="int.MaxValue"/> by default. </param>
        /// <returns> A scaled beta distribution. </returns>
        public static IDistribution FactoryBeta(double alpha, double beta, double location, double scale, int sampleSize = int.MaxValue)
        {
            return new Distributions.Beta4Parameters(alpha, beta, location, scale, sampleSize); 
        }
        /// <summary>
        /// Constructs a <see cref="IDistributionEnum.LogPearsonIII"/> <see cref="IDistribution"/>
        /// </summary>
        /// <param name="mean"> The mean of the logged distribution (or sample data). NOTE: this is the mean of the logged data, NOT the log of the mean. </param>
        /// <param name="stDev"> The standard deviation of the logged distribution (or sample data). NOTE: this is the standard deviation of the logged data, NOT the log of the standard deviation. </param>
        /// <param name="skew"> The skew of the logged distribution (or sample data). NOTE: this is the skew of the logged data, NOT the log of the skew.  </param>
        /// <param name="sampleSize"> An optional parameter describing the sample size used to calculated the sample <paramref name="mean"/>, <paramref name="stDev"/> and <paramref name="skew"/>. Leave blank if a population distribution is intended. </param>
        /// <returns> A <see cref="Statistics.Distributions.LogPearson3"/> object returned as an implementation of the  <see cref="IDistribution"/> interface. </returns>
        public static IDistribution FactoryLogPearsonIII(double mean, double stDev, double skew, int sampleSize = int.MaxValue)
        {
            return new Distributions.LogPearson3(mean, stDev, skew, sampleSize);
        }
        /// <summary>
        /// Constructs a <see cref="IDistributionEnum.LogPearsonIII"/> <see cref="IDistribution"/>, by fitting as set of sample data to the distribution.
        /// </summary>
        /// <param name="sample"> The data to be fit to the Log Pearson Type III distribution. </param>
        /// <param name="isLogSample"> An optional parameter. <see langword="true"/> if the <paramref name="sample"/> data values are logged, set to <see langword="false"/> by default. </param>
        /// <param name="sampleSize"> An optional parameter describing the effective sample size, this value is inferred from the size of the <paramref name="sample"/> data if it is not provided. </param>
        /// <returns> A <see cref="Statistics.Distributions.LogPearson3"/> object returned as an implementation of the  <see cref="IDistribution"/> interface. </returns>
        public static IDistribution FactoryFitLogPearsonIII(IEnumerable<double> sample, bool isLogSample = false, int sampleSize = -404)
        {
            return Distributions.LogPearson3.Fit(sample, sampleSize, isLogSample);
        }
        /// <summary>
        /// Constructs a <see cref="IDistributionEnum.LogPearsonIII"/> <see cref="IDistribution"/> bound on the range specified by the <paramref name="min"/> and <paramref name="max"/> values."/>
        /// </summary>
        /// <param name="mean"> The mean of the logged distribution (or sample data). NOTE: this is the mean of the logged data, NOT the log of the mean. </param>
        /// <param name="stDev"> The standard deviation of the logged distribution (or sample data). NOTE: this is the standard deviation of the logged data, NOT the log of the standard deviation. </param>
        /// <param name="skew"> The skew of the logged distribution (or sample data). NOTE: this is the skew of the logged data, NOT the log of the skew.  </param>
        /// <param name="sampleSize"> An optional parameter describing the sample size used to calculated the sample <paramref name="mean"/>, <paramref name="stDev"/> and <paramref name="skew"/>. Leave blank if a population distribution is intended. </param>
        /// <param name="min"> The lower (minimum) truncation value. </param>
        /// <param name="max"> The upper (maximum) truncation value. </param>
        /// <returns> A <see cref="Statistics.Distributions.LogPearson3"/> object bound on the range: [<paramref name="min"/>, <paramref name="max"/>] returned as an implementation of the  <see cref="IDistribution"/> interface. </returns>
        /// <returns></returns>
        public static IDistribution FactoryTruncatedLogPearsonIII(double mean, double stDev, double skew, double min = 0, double max = double.PositiveInfinity, int sampleSize = int.MaxValue)
        {
            IDistribution lpIII = new Distributions.LogPearson3(mean, stDev, skew, sampleSize);
            return FactoryTruncatedLogPearsonIII(lpIII, min, max);
        }
        /// <summary>
        /// Constructs a <see cref="IDistributionEnum.LogPearsonIII"/> <see cref="IDistribution"/> bound on the range specified by the <paramref name="min"/> and <paramref name="max"/> values."/>
        /// </summary>
        /// <param name="lpIII"> The <see cref="Statistics.Distributions.LogPearson3"/> distribution to be bound. </param>
        /// <param name="min"> The lower (minimum) truncation value. </param>
        /// <param name="max"> The upper (maximum) truncation value. </param>
        /// <returns> A <see cref="Statistics.Distributions.LogPearson3"/> object bound on the range: [<paramref name="min"/>, <paramref name="max"/>] returned as an implementation of the  <see cref="IDistribution"/> interface. </returns>
        public static IDistribution FactoryTruncatedLogPearsonIII(IDistribution lpIII, double min = 0, double max = double.PositiveInfinity)
        {
            if (lpIII.IsNull()) throw new ArgumentNullException(nameof(lpIII));
            if (lpIII.Type != IDistributionEnum.LogPearsonIII) throw new ArgumentException($"The {nameof(FactoryTruncatedLogPearsonIII)} factory requires a {nameof(IDistributionEnum.LogPearsonIII)} {nameof(lpIII)} parameter, instead a {nameof(lpIII.Type)} was provided.");
            Statistics.Distributions.LogPearson3 ldist = (Statistics.Distributions.LogPearson3)lpIII;
            return new Statistics.Distributions.LogPearson3(ldist.Mean,ldist.StandardDeviation,ldist.Skewness, min, max, lpIII.SampleSize);
        }
        /// <summary>
        /// Constructs a <see cref="IDistribution"/> bound on the range specified by the <paramref name="min"/> and <paramref name="max"/> values."/>
        /// </summary>
        /// <param name="distribution"> The <see cref="IDistribution"/> to be bound. </param>
        /// <param name="min"> The lower (minimum) truncation value. </param>
        /// <param name="max"> The upper (maximum) truncation value. </param>
        /// <returns> A <see cref="IDistribution"/> bound on the range: [<paramref name="min"/>, <paramref name="max"/>] returned as an implementation of the  <see cref="IDistribution"/> interface. </returns>
        public static IDistribution FactoryTruncatedDistribution(IDistribution distribution, double min = double.NegativeInfinity, double max = double.PositiveInfinity)
        {
            if (distribution.IsNull()) throw new ArgumentNullException(nameof(distribution));
            switch (distribution.Type) 
            {
                case IDistributionEnum.Normal:
                    Statistics.Distributions.Normal ndist = (Statistics.Distributions.Normal)distribution;
                    return FactoryTruncatedNormal(ndist.Mean, ndist.StandardDeviation, min, max, distribution.SampleSize);
                case IDistributionEnum.LogPearsonIII:
                    return FactoryTruncatedLogPearsonIII(distribution, min, max);
                default:
                    throw new NotSupportedException($"The specified {distribution.Type} distribution is not a supported truncated distribution type.");
            }

        }
    }
}
