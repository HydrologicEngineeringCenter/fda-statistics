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
    class SerializationTests
    {
        [Theory]
        [InlineData(2.33, 1d, 1)]
        public void SerializationRoundTrip_Normal(double mean, double sd, int n)
        {
            IDistribution d = new Normal(mean, sd, n);
            XElement ele = d.ToXML();
            IDistribution d2 = IDistributionExtensions.FromXML(ele);
            Assert.Equal(d, d2);
        }
    }
}
