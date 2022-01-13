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
        static double[] standardDeviations = new double[] { 0.482052902, 1.055902721, 1.710592003, 2.355386115, 2.459674775, 2.275377716, 2.049390153, 2.079746078, 2.19089023, 1.891130614, 1.732952683, 1.66864466, 1.588395417, 1.386497386, 1.434573107, 1.59760563, 1.699411663, 1.796480935, 1.811215062, 1.949358869, 2.113084239, 2.521507486, 2.521507486, 2.521507486 };
        static int equivalentRecordLength = 20;

        //THIS TEST SHOULD YIELD A NON-MONOTONIC FUNCTION
        [Fact]
        public void GraphicalFunction_Test()
        {
            
            Graphical graphical = new Graphical(exceedanceProbabilities, quantileValues, equivalentRecordLength,.99,.01);
            graphical.ComputeGraphicalConfidenceLimits();
            double[] computedStandardDeviations = new double[standardDeviations.Length];
            double[] confirmExceedanceProbabilities = new double[standardDeviations.Length];
            for (int i = 0; i < exceedanceProbabilities.Length; i ++)
            {
                int idx = Array.BinarySearch(graphical.ExceedanceProbabilities, exceedanceProbabilities[i]);
                if (idx > 0)
                {
                   computedStandardDeviations[i] = graphical.FlowOrStageDistributions[idx].StandardDeviation;
                   confirmExceedanceProbabilities[i] = graphical.ExceedanceProbabilities[idx];
                }
            }
            for (int i = 0; i<standardDeviations.Length; i++)
            {
                double expected = standardDeviations[i];
                double actual = computedStandardDeviations[i];
                Assert.Equal(expected, actual, 2);
            }

        }
    }
}
