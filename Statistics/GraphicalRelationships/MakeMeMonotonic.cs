using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statistics.Distributions;

namespace Statistics.GraphicalRelationships
{
    static class MakeMeMonotonic
    {

        public static Normal[] IAmNormalMakeMeMonotonic(Normal[] distributionArray)
        {
            Normal[] monotonicDistributionArray = new Normal[distributionArray.Length];
            double lowerBoundNonExceedanceProbability = 0.0001;
            double upperBoundNonExceedanceProbability = 1 - lowerBoundNonExceedanceProbability;
            double lowerBoundFirstDistribution;
            double lowerBoundSecondDistribution;
            double upperBoundFirstDistribution;
            double upperBoundSecondDistribution;
            bool lowerBoundIsDecreasing;
            bool upperBoundIsDecreasing;

            monotonicDistributionArray[0] = distributionArray[0];
            for (int i = 1; i < distributionArray.Length; i++)
            {
                lowerBoundFirstDistribution = distributionArray[i - 1].InverseCDF(lowerBoundNonExceedanceProbability);
                lowerBoundSecondDistribution = distributionArray[i].InverseCDF(lowerBoundNonExceedanceProbability);
                upperBoundFirstDistribution = distributionArray[i - 1].InverseCDF(upperBoundNonExceedanceProbability);
                upperBoundSecondDistribution = distributionArray[i].InverseCDF(upperBoundNonExceedanceProbability);

                lowerBoundIsDecreasing = lowerBoundSecondDistribution < lowerBoundFirstDistribution;
                upperBoundIsDecreasing = upperBoundSecondDistribution < upperBoundFirstDistribution;

                if (lowerBoundIsDecreasing || upperBoundIsDecreasing)
                {
                    double meanSecondDistribution = (distributionArray[i]).Mean;
                    int sampleSizeSecondDistribution = (distributionArray[i]).SampleSize;
                    double standardDeviationFirstDistribution = (distributionArray[i - 1]).StandardDeviation;

                    if (distributionArray[i].Truncated)
                    {
                        double firstMin = ((TruncatedNormal)(distributionArray[i - 1])).Min;
                        double secondMin = ((TruncatedNormal)(distributionArray[i])).Min;
                        double firstMax = ((TruncatedNormal)(distributionArray[i - 1])).Max;
                        double secondMax = ((TruncatedNormal)(distributionArray[i])).Max;
                        bool minIsDecreasing = secondMin < firstMin;
                        bool maxIsDecreasing = secondMax < firstMax;
                        double epsilon = .0001;

                        if (minIsDecreasing && !maxIsDecreasing)
                        {
                            monotonicDistributionArray[i] = new TruncatedNormal(meanSecondDistribution, standardDeviationFirstDistribution, firstMin + epsilon, secondMax, sampleSizeSecondDistribution);
                        }
                        else if (!minIsDecreasing && maxIsDecreasing)
                        {
                            monotonicDistributionArray[i] = new TruncatedNormal(meanSecondDistribution, standardDeviationFirstDistribution, secondMin, firstMax + epsilon, sampleSizeSecondDistribution);
                        }
                        else if (minIsDecreasing && maxIsDecreasing)
                        {
                            monotonicDistributionArray[i] = new TruncatedNormal(meanSecondDistribution, standardDeviationFirstDistribution, firstMin + epsilon, firstMax + epsilon, sampleSizeSecondDistribution);
                        }
                    }
                    else
                    {

                        monotonicDistributionArray[i] = new Normal(meanSecondDistribution, standardDeviationFirstDistribution, sampleSizeSecondDistribution);
                    }
                }
                else
                {
                    monotonicDistributionArray[i] = distributionArray[i];
                }
            }
            return monotonicDistributionArray;
        }

      

 

    }
}
