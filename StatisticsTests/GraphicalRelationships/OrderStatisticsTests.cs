using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;
using Utilities;
using Xunit;
using Statistics;
using Statistics.GraphicalRelationships;

namespace StatisticsTests.Graphical
{
    public class OrderStatisticsTests
    {
        [Theory]
        [InlineData(4,24)]
        public void ComputeFactorial_Test(int value, int expected)
        {
            double actual = OrderStatistics.ComputeFactorial(value);
            Assert.Equal(expected, actual);

        }
    }
}
