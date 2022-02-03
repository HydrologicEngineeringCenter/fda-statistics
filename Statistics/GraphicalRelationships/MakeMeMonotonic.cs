using Statistics.Distributions;
using System;

namespace Statistics.GraphicalRelationships
{
    static class MakeMeMonotonic
    {
        static double[] probsForChecking = new double[] { .45, .2, .1, .05, .02, .01, .005, .002 };
        static Normal standardNormal = new Normal(0,1);
        public static Normal[] IAmNormalMakeMeMonotonic(Normal[] distributionArray)
        {

            Normal[] monotonicDistributionArray = new Normal[distributionArray.Length];
            monotonicDistributionArray[0] = distributionArray[0];

            for (int j = 0; j < probsForChecking.Length; j++)
            {
                double q = probsForChecking[j];
                double qComplement = 1 - q; 
                for (int i = 1; i < distributionArray.Length; i++)
                {
                    double lowerBoundPreviousDistribution = monotonicDistributionArray[i - 1].InverseCDF(q);
                    double lowerBoundCurrentDistribution = distributionArray[i].InverseCDF(q);
                    double lowerBoundDifference = lowerBoundCurrentDistribution - lowerBoundPreviousDistribution;

                    double upperBoundPreviousDistribution = monotonicDistributionArray[i - 1].InverseCDF(qComplement);
                    double upperBoundCurrentDistribution = distributionArray[i].InverseCDF(qComplement);
                    double upperBoundDifference = upperBoundCurrentDistribution - upperBoundPreviousDistribution;

                    if ((upperBoundDifference < 0) || (lowerBoundDifference < 0))
                    {
                        monotonicDistributionArray[i] = new Normal(distributionArray[i].Mean, monotonicDistributionArray[i - 1].StandardDeviation, distributionArray[i].SampleSize);
                    }
                    else
                    {
                        monotonicDistributionArray[i] = distributionArray[i];
                    }
                }
            }
            return monotonicDistributionArray;
        }

    }
}
