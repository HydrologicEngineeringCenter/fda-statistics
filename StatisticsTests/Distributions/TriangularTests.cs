using Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Utilities;
using Xunit;

namespace StatisticsTests.Distributions
{
    [ExcludeFromCodeCoverage]
    public class TriangularTests
    {
        [Fact]
        public void Triangular_EqualMinMax_ReturnsObjWithMessage()
        {
            Assert.Throws<InvalidConstructorArgumentsException>(() => new Statistics.Distributions.Triangular(0, 0, 0));        
        }
        [Fact]
        public void Triangular_MinGreaterThanMax_Throws_InvalidConstructorArgumentsException()
        {
            Assert.Throws<InvalidConstructorArgumentsException>(() => new Statistics.Distributions.Triangular(1, 0, 0));
        }
                [Theory]
        [InlineData(0d,5d, 10d, 0.0, 0.0)]
[InlineData(0d,5d, 10d, 0.75, 6.464466)]
[InlineData(0d,5d, 10d, 0.25, 3.535534)]
[InlineData(0d,5d, 10d, 0.5, 5.0)]
[InlineData(0d,5d, 10d, 1.0, 10.0)]
        public void Triangular_INVCDF(double min, double mostlikely, double max, double prob, double expected)
        {
            var testObj = new Statistics.Distributions.Triangular(min, mostlikely, max);
            double result = testObj.InverseCDF(prob);
            Assert.Equal(result, expected, 5);
        }
    }
}
