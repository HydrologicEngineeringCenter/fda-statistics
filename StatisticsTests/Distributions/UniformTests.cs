using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xunit;

namespace StatisticsTests.Distributions
{
    [ExcludeFromCodeCoverage]
    public class UniformTests
    {
        [Theory]
        [InlineData(0d, 1d)]
        public void GoodDataParameters_Set_Properly(double min, double max)
        {
            var testObj = new Statistics.Distributions.Uniform(min, max);
            Assert.Equal(min, testObj.Min, 2);
            Assert.Equal(max, testObj.Max, 2);
        }

    }
}
