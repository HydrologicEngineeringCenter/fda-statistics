using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xunit;

namespace StatisticsTests.Distributions
{
    [ExcludeFromCodeCoverage]
    public class NormalTests
    {
        [Theory]
        [InlineData(0d, -1d, 1)]
        [InlineData(-1d, -2d, 1)]
        [InlineData(-1d, 1d, -1)]
        [InlineData(1d, 1d, 0)]
        public void BadValidation(double mean, double sd, int n)
        {
            Statistics.Distributions.Normal dist = new Statistics.Distributions.Normal(mean, sd, n);
            dist.Validate();
            Assert.True(dist.HasErrors);
        }
        [Theory]
        [InlineData(0d, 1d, 1)]
        [InlineData(-1d, 2d, 1)]
        public void GoodValidation(double mean, double sd, int n)
        {
            Statistics.Distributions.Normal dist = new Statistics.Distributions.Normal(mean, sd, n);
            dist.Validate();
            Assert.False(dist.HasErrors);
        }
        //https://en.wikipedia.org/wiki/Standard_normal_table
        [Theory]
        [InlineData(0d, 1,0, .99865, 3d)]
        [InlineData(0d, 1,0, .97725, 2d)]
        [InlineData(0d, 1,0, .84134, 1d)]
        [InlineData(0d, 1, 0, .5, 0d)]
        [InlineData(0d, 1,0, .15866, -1d)]
        [InlineData(0d, 1,0, .02275, -2d)]
        [InlineData(0d, 1,0, .00135, -3d)]
        public void StandardNormal_InverseCDF(double mean, double sd, int n, double p, double z)
        {
            var testObj = new Statistics.Distributions.Normal(mean, sd, n);
            Assert.Equal(z,testObj.InverseCDF(p),4);
        }

        [Theory]
        [InlineData(0d, 1, -2d,2d, 0, .99865, 1.97668)]
        [InlineData(0d, 1, -2d, 2d, 0, .97725, 1.701069)]
        [InlineData(0d, 1, -2d, 2d, 0, .84134, .93773)]
        [InlineData(0d, 1, -2d, 2d, 0, .5, 0d)]
        [InlineData(0d, 1, -2d, 2d, 0, .15866, -.93773)]
        [InlineData(0d, 1, -2d, 2d, 0, .02275, -1.701069)]
        [InlineData(0d, 1, -2d, 2d, 0, .00135, -1.97668)]
        public void Truncated_StandardNormal_InverseCDF(double mean, double sd, double min, double max, int n, double p, double z)
        {
            var testObj = new Statistics.Distributions.TruncatedNormal(mean, sd, min, max, n);
            Assert.Equal(z, testObj.InverseCDF(p), 4);
        }
    }
}
