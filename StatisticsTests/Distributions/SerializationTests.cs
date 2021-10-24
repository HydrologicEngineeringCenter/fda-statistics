using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xunit;
using Statistics.Distributions;
using Statistics;
using System.Xml.Linq;

namespace StatisticsTests.Distributions
{
    [ExcludeFromCodeCoverage]
    public class SerializationTests
    {
        [Theory]
        [InlineData(2.33, 1d, 1)]
        public void SerializationRoundTrip_Normal(double mean, double sd, int n)
        {
            IDistribution d = new Normal(mean, sd, n);
            XElement ele = d.ToXML();
            IDistribution d2 = IDistributionExtensions.FromXML(ele);
            Assert.Equal(d.Mean, d2.Mean);
            Assert.Equal(d.StandardDeviation, d2.StandardDeviation);
            Assert.Equal(d.SampleSize, d2.SampleSize);
        }
        [Theory]
        [InlineData(2.33, 1d, 1)]
        public void SerializationRoundTrip_LogNormal(double mean, double sd, int n)
        {
            IDistribution d = new LogNormal(mean, sd, n);
            XElement ele = d.ToXML();
            IDistribution d2 = IDistributionExtensions.FromXML(ele);
            Assert.Equal(d.Mean, d2.Mean);
            Assert.Equal(d.StandardDeviation, d2.StandardDeviation);
            Assert.Equal(d.SampleSize, d2.SampleSize);
        }
        [Theory]
        [InlineData(2.33, .033d, .022, 1)]
        public void SerializationRoundTrip_LogPearsonIII(double mean, double sd, double skew, int n)
        {
            IDistribution d = new LogPearson3(mean, sd, skew, n);
            XElement ele = d.ToXML();
            IDistribution d2 = IDistributionExtensions.FromXML(ele);
            Assert.Equal(d.Mean, d2.Mean);
            Assert.Equal(d.StandardDeviation, d2.StandardDeviation);
            Assert.Equal(d.Skewness, d2.Skewness);
            Assert.Equal(d.SampleSize, d2.SampleSize);
        }
        [Theory]
        [InlineData(3.4, 4.5, 1)]
        public void SerializationRoundTrip_Uniform(double min, double max, int n)
        {
            IDistribution d = new Uniform(min, max, n);
            XElement ele = d.ToXML();
            IDistribution d2 = IDistributionExtensions.FromXML(ele);
            Assert.Equal(d.Min, d2.Min);
            Assert.Equal(d.Max, d2.Max);
            Assert.Equal(d.SampleSize, d2.SampleSize);
        }
        [Theory]
        [InlineData(3.4, 4.5, 5.6, 1)]
        public void SerializationRoundTrip_Triangular(double min, double mostlikely, double max, int n)
        {
            IDistribution d = new Triangular(min, mostlikely,max ,n);
            XElement ele = d.ToXML();
            IDistribution d2 = IDistributionExtensions.FromXML(ele);
            Assert.Equal(d.Min, d2.Min);
            Assert.Equal(d.Max, d2.Max);
            Assert.Equal(d.Mode, d2.Mode);
            Assert.Equal(d.SampleSize, d2.SampleSize);
        }
    }
}
