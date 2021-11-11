using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Utilities;
using Xunit;
using Statistics.Distributions;



namespace StatisticsTests.Distributions
{
    public class EmpiricalTests
    {   
        [Theory]
        [InlineData(1234,1000000,0,1)]
        [InlineData(4321,1000000,0,1)]
        [InlineData(2345,1000000,0,1)]
        [InlineData(5432,1000000,0,1)]
        public void SummaryStatisticsMonotonicTests(int seed, int sampleSize, double expectedMean, double expectedStandardDeviation)
        {
            Random random = new Random(seed);
            Normal normal = new Normal(0, 1);
            double[] cumulativeProbabilities = new double[sampleSize];
            double[] observationValues = new double[sampleSize];
            for (int i = 0; i<sampleSize; i++)
            {
                double randomProbability = random.NextDouble();
                cumulativeProbabilities[i] = randomProbability;
                observationValues[i] = normal.InverseCDF(randomProbability);
            }
            //sorted so must be monotonic
            Array.Sort(cumulativeProbabilities);
            Array.Sort(observationValues);
            Empirical empirical = new Empirical(cumulativeProbabilities, observationValues);
            double actualMean = empirical.Mean;
            double actualStandardDeviation = empirical.StandardDeviation;
            Assert.True(Math.Abs(expectedMean - actualMean) < 0.001);
            Assert.True(Math.Abs(expectedStandardDeviation - actualStandardDeviation) < 0.001);
        }

        [Theory]
        [InlineData(1234, 1000000, 0,1)]
        [InlineData(4321, 1000000, 0,1)]
        [InlineData(2345, 1000000, 0,1)]
        [InlineData(5432, 1000000, 0,1)]
        public void SummaryStatisticsNonMonotonicTests(int seed, int sampleSize, double expectedMean, double expectedStandardDeviation)
        {
            Random random = new Random(seed);
            Normal normal = new Normal(0, 1);
            double[] cumulativeProbabilities = new double[sampleSize];
            double[] observationValues = new double[sampleSize];
            for (int i = 0; i < sampleSize; i++)
            {
                double randomProbability = random.NextDouble();
                cumulativeProbabilities[i] = randomProbability;
                observationValues[i] = normal.InverseCDF(randomProbability);
            }
            //NOT SORTED SO NON-MONOTONIC
            Empirical empirical = new Empirical(cumulativeProbabilities, observationValues);
            double actualMean = empirical.Mean;
            double actualStandardDeviation = empirical.StandardDeviation;
            Assert.True(Math.Abs(expectedMean - actualMean) < 0.001);
            Assert.True(Math.Abs(expectedStandardDeviation - actualStandardDeviation) < 0.001);
        }
    }
}
