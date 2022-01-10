using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Utilities;
using Xunit;
using Statistics.GraphicalRelationships;

namespace StatisticsTests.GraphicalRelationships
{
    public class GraphicalTests
    {
        /// <summary>
        /// Test data based on Table 2.13 from "Uncertainty Estimates for Grahpical (Non-Analytic) Frequency Curves - HEC-FDA Technical Reference" CPD-72a
        /// </summary>
        static double[] exceedanceProbabilities = new double[] { .99, .95, .90, .85, .8, .75, .7, .65, .6, .55, .5, .45, .4, .35, .3, .25, .2, .15, .1, .05, .02, .01, .005, .0025 };
        static double[] quantileValues = new double[] { 6.6, 7.4, 8.55, 9.95, 11.5, 12.7, 13.85, 14.7, 15.8, 16.7, 17.5, 18.25, 19, 19.7, 20.3, 21.1, 21.95, 23, 24.2, 25.7, 27.4, 28.4, 29.1, 29.4 };
        static double[] standardDeviations = new double[] { .32, 1.046, 1.705, 2.353, 2.446, 2.275, 2.034, 2.068, 2.184, 1.889, 1.732, 1.669, 1.588, 1.384, 1.425, 1.597, 1.693, 1.794, 1.804, 2.045, 2.480, 1.658, 1.081, .732 };
        static int equivalentRecordLength = 20;

        [Fact]
        public void GraphicalFunction_Test()
        {
            Graphical graphical = new Graphical(exceedanceProbabilities, quantileValues, equivalentRecordLength);
            graphical.ComputeGraphicalConfidenceLimits();
            for (int i = 0; i < exceedanceProbabilities.Length; i ++)
            {
                int idx = Array.BinarySearch(graphical.ExceedanceProbabilities, exceedanceProbabilities[i]);
                if (idx > 0)
                {
                    double expected = standardDeviations[i];
                    double actual = graphical.FlowOrStageDistributions[idx].StandardDeviation;
                    double error = Math.Abs((actual - expected) / expected);
                    double tolerance = 0.01;
                    Assert.True(error < tolerance);
                }
            }
        }
    }
}
