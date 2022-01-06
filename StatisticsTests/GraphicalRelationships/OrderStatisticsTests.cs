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
        /// <summary>
        /// Example data obtained from Table 2.5, "Uncertainty Estimates for Graphical (Non-Analytic) Frequency Curves - 
        /// HEC-FDA Technical Reference" CPD-72a.
        /// </summary>
        private static double[] exceedanceProbabilities = new double[] { .99, .95, .9, .85, .80, .75, .7, .65, .6, .55, .5, .45, .4, .35, .3, .25, .2, .15, .1, .05, .02, .01, .005, .0025 };
        private static double[] stageQuantiles = new double[] { 6.6, 7.4, 8.55, 9.95, 11.5, 12.7, 13.85, 14.7, 15.8, 16.7, 17.5, 18.25, 19, 19.7, 20.3, 21.1, 21.95, 23, 24.2, 25.7, 27.4, 28.4, 29.1, 29.4 };

        /// <summary>
        /// Example data obtained from Table 2.1, "Uncertainty Estimates for Graphical (Non-Analytic) Frequency Curves - 
        /// HEC-FDA Technical Reference" CPD-72a.
        /// </summary>
        private static double[] observations = new double[] { 1, 2, 3, 4, 5 };
        private static double[] weibullPlottingPositionNonExceedanceProbabilities = new double[] { .166667, .33333, .5, .666667, .833333 };

        [Theory]
        [InlineData(.965)] // This test is based on Table 2.1 
        public void ComputeCDFOfQuantile_Test(double expected)
        {
            double[] exceedanceProbabilitiesExample21 = new double[weibullPlottingPositionNonExceedanceProbabilities.Length];
            for (int i = 0; i < weibullPlottingPositionNonExceedanceProbabilities.Length; i++)
            {
                exceedanceProbabilitiesExample21[i] = 1 - weibullPlottingPositionNonExceedanceProbabilities[i];
            }
            OrderStatistics orderStatistics = new OrderStatistics(exceedanceProbabilitiesExample21, observations);
            int indexOfFifthObservation = 4;
            double[] cdf = orderStatistics.ComputeCDFOfQuantile(indexOfFifthObservation);
            int indexOfThirdProbability = 2;
            double actual = cdf[indexOfThirdProbability];
            double error = (actual - expected) / expected;
            double tolerance = 0.01;
            Assert.True(error < tolerance);
        }

        [Theory]
        [InlineData(4, 24)]
        public void ComputeFactorial_Test(int value, int expected)
        {
            double actual = OrderStatistics.ComputeFactorial(value);
            Assert.Equal(expected, actual);

        }

        [Theory]
        [InlineData(6, 10, 210)]
        public void ComputeCombination_Test(int successes, int trials, int expected)
        {
            double actual = OrderStatistics.ComputeCombination(trials, successes);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(6, 10, .6, 0.250822656)]
        public void ComputeBinomial_Test(int successes, int trials, double probabilityOfSuccess, double expected)
        {
            double actual = OrderStatistics.ComputeBinomialProbability(trials, successes, probabilityOfSuccess);
            double error = (actual - expected) / expected;
            double tolerance = 0.01;
            Assert.True(error < tolerance);
        }

        [Theory]
        [InlineData( new double[] {8.04, 8.61, 9.53, 10.59, 11.7, 12.79, 13.82, 14.8, 15.71, 16.56, 17.37, 18.13, 18.86, 19.58, 20.30, 21.06, 21.86, 22.7, 23.55, 24.36, 24.79, 24.93, 24.99, 25.02})]
        public void ComputeMean(double[] expected)
        {
            OrderStatistics orderStatistics = new OrderStatistics(exceedanceProbabilities, stageQuantiles);
            orderStatistics.ComputeOrderStatisticsCDFs();
            for (int i = 0; i<expected.Length; i++)
            {
                double actual = orderStatistics.Means[i];
                double error = (actual - expected[i]) / expected[i];
                double tolerance = 0.01;
                Assert.True(error < tolerance);
            }

        }

        [Theory]
        [InlineData(new double[] {0.678,1.16,1.646,1.967,2.126,2.167,2.139,2.076,1.994,1.9,1.8,1.706,1.628,1.580,1.567,1.58,1.591,1.552,1.415,1.15,.924,.836,.79,.766 })]
        public void ComputeStandardDeviation(double[] expected) //Table 2.5
        {//this test worked with one sd but not many? 
            OrderStatistics orderStatistics = new OrderStatistics(exceedanceProbabilities, stageQuantiles);
            orderStatistics.ComputeOrderStatisticsCDFs();
            for (int i = 0; i < expected.Length; i++)
            {
                double actual = orderStatistics.StandardDeviations[i];
                double error = (actual - expected[i]) / expected[i];
                double tolerance = 0.01;
                Assert.True(error < tolerance);
            }
 
        }
        
    }
}
