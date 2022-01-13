using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statistics.Distributions;

namespace Statistics.GraphicalRelationships
{
    class MakeMeMonotonic
    {
        /// <summary>
        /// This function should generally given all recurrence intervals are represented as non-exceedance probabilities 
        /// as is required by Simulation. This method assumes that the same distribution type is used for all y. 
        /// This is not the complete solution. 
        /// 
        /// 
        /// 
        /// First we want to validate uncertain paired data 
        /// remove factory 
        /// 
        /// /// </summary>
        /// <param name="ys"></param> The array of IDistributions 
        public IDistribution[] MakeTheseDistributionsMonotonic(IDistribution[] yDistributions)
        {
            IDistribution[] monotonicYDistributions = new IDistribution[yDistributions.Length];
            switch (yDistributions[0].Type)
            {
                case IDistributionEnum.Normal:
                    {
                        monotonicYDistributions = IAmNormalMakeMeMonotonic(yDistributions);
                        break;
                    }
                case IDistributionEnum.LogNormal:
                    {
                        monotonicYDistributions = IAmNormalMakeMeMonotonic(yDistributions);
                        break;
                    }
                case IDistributionEnum.NotSupported:
                    {
                        throw new Exception("This distribution type is not supported");
                    }
                case IDistributionEnum.Triangular:
                    {
                        monotonicYDistributions = IAmTriangularMakeMeMonotonic(yDistributions);
                        break;
                    }
                case IDistributionEnum.TruncatedNormal:
                    {
                        monotonicYDistributions = IAmNormalMakeMeMonotonic(yDistributions);
                        break;
                    }
                case IDistributionEnum.TruncatedTriangular:
                    {
                        monotonicYDistributions = IAmTriangularMakeMeMonotonic(yDistributions);
                        break;
                    }
                case IDistributionEnum.TruncatedUniform:
                    {
                        monotonicYDistributions = IAmUniformMakeMeMonotonic(yDistributions);
                        break;
                    }
                case IDistributionEnum.Uniform:
                    {
                        monotonicYDistributions = IAmUniformMakeMeMonotonic(yDistributions);
                        break;
                    }
            }
            return monotonicYDistributions;
        }

        public IDistribution[] IAmNormalMakeMeMonotonic(IDistribution[] distributionArray)
        {
            IDistribution[] monotonicDistributionArray = new IDistribution[distributionArray.Length];
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
                    double meanSecondDistribution = ((Normal)distributionArray[i]).Mean;
                    int sampleSizeSecondDistribution = ((Normal)distributionArray[i]).SampleSize;
                    double standardDeviationFirstDistribution = ((Normal)distributionArray[i - 1]).StandardDeviation;

                    if (distributionArray[i].Truncated)
                    {
                        double firstMin = ((Normal)distributionArray[i - 1]).Min;
                        double secondMin = ((Normal)distributionArray[i]).Min;
                        double firstMax = ((Normal)distributionArray[i - 1]).Max;
                        double secondMax = ((Normal)distributionArray[i]).Max;
                        bool minIsDecreasing = secondMin < firstMin;
                        bool maxIsDecreasing = secondMax < firstMax;
                        double epsilon = .0001;

                        if (minIsDecreasing && !maxIsDecreasing)
                        {
                            monotonicDistributionArray[i] = IDistributionFactory.FactoryTruncatedNormal(meanSecondDistribution, standardDeviationFirstDistribution, firstMin + epsilon, secondMax, sampleSizeSecondDistribution);
                        }
                        else if (!minIsDecreasing && maxIsDecreasing)
                        {
                            monotonicDistributionArray[i] = IDistributionFactory.FactoryTruncatedNormal(meanSecondDistribution, standardDeviationFirstDistribution, secondMin, firstMax + epsilon, sampleSizeSecondDistribution);
                        }
                        else if (minIsDecreasing && maxIsDecreasing)
                        {
                            monotonicDistributionArray[i] = IDistributionFactory.FactoryTruncatedNormal(meanSecondDistribution, standardDeviationFirstDistribution, firstMin + epsilon, firstMax + epsilon, sampleSizeSecondDistribution);
                        }
                    }
                    else
                    {

                        monotonicDistributionArray[i] = IDistributionFactory.FactoryNormal(meanSecondDistribution, standardDeviationFirstDistribution, sampleSizeSecondDistribution);
                    }
                }
                else
                {
                    monotonicDistributionArray[i] = distributionArray[i];
                }
            }
            return monotonicDistributionArray;
        }
        /// <summary>
        /// We should not ever need to handle monotonicity for triangular or uniform so this code might add time without benefit 
        /// However, if we want to let  users decide their distribution type for the 
        /// aggregated stage-damage compute then we take the responsibility of coming up with the 
        /// uncertain paired data specification and there is a chance our code produces something 
        /// non monotonic
        /// </summary>
        /// <param name="distributionArray"></param>
        /// <returns></returns>
        public IDistribution[] IAmTriangularMakeMeMonotonic(IDistribution[] distributionArray)
        {
            IDistribution[] monotonicDistributionArray = new IDistribution[distributionArray.Length];
            double minFirstDistribution;
            double minSecondDistribution;
            double maxFirstDistribution;
            double maxSecondDistribution;
            int sampleSizeSecondDistribution;
            double mostLikelySecondDistribution;
            double epsilon = 0.0001;
            monotonicDistributionArray[0] = distributionArray[0];
            for (int i = 0; i < distributionArray.Length; i++)
            {
                minFirstDistribution = ((Triangular)distributionArray[i - 1]).Min;
                minSecondDistribution = ((Triangular)distributionArray[i]).Min;
                maxFirstDistribution = ((Triangular)distributionArray[i - 1]).Min;
                maxSecondDistribution = ((Triangular)distributionArray[i]).Min;
                mostLikelySecondDistribution = ((Triangular)distributionArray[i]).MostLikely;
                sampleSizeSecondDistribution = ((Triangular)distributionArray[i]).SampleSize;
                bool minIsDecreasing = minSecondDistribution < minFirstDistribution;
                bool maxIsDecreasing = maxSecondDistribution < maxFirstDistribution;

                if (minIsDecreasing && !maxIsDecreasing)
                {
                    monotonicDistributionArray[i] = IDistributionFactory.FactoryTriangular(minFirstDistribution + epsilon, mostLikelySecondDistribution, maxSecondDistribution, sampleSizeSecondDistribution);
                }
                else if (!minIsDecreasing && maxIsDecreasing)
                {
                    monotonicDistributionArray[i] = IDistributionFactory.FactoryTriangular(minSecondDistribution, mostLikelySecondDistribution, maxFirstDistribution + epsilon, sampleSizeSecondDistribution);

                }
                else if (minIsDecreasing && maxIsDecreasing)
                {
                    monotonicDistributionArray[i] = IDistributionFactory.FactoryTriangular(minFirstDistribution + epsilon, mostLikelySecondDistribution, maxFirstDistribution + epsilon, sampleSizeSecondDistribution);

                }
                else
                {
                    monotonicDistributionArray[i] = distributionArray[i];
                }

            }
            return monotonicDistributionArray;

        }

        public IDistribution[] IAmUniformMakeMeMonotonic(IDistribution[] distributionArray)
        {
            IDistribution[] monotonicDistributionArray = new IDistribution[distributionArray.Length];
            double minFirstDistribution;
            double minSecondDistribution;
            double maxFirstDistribution;
            double maxSecondDistribution;
            int sampleSizeSecondDistribution;
            double epsilon = 0.0001;
            monotonicDistributionArray[0] = distributionArray[0];
            for (int i = 0; i < distributionArray.Length; i++)
            {
                minFirstDistribution = ((Uniform)distributionArray[i - 1]).Min;
                minSecondDistribution = ((Uniform)distributionArray[i]).Min;
                maxFirstDistribution = ((Uniform)distributionArray[i - 1]).Min;
                maxSecondDistribution = ((Uniform)distributionArray[i]).Min;
                sampleSizeSecondDistribution = ((Uniform)distributionArray[i]).SampleSize;
                bool minIsDecreasing = minSecondDistribution < minFirstDistribution;
                bool maxIsDecreasing = maxSecondDistribution < maxFirstDistribution;

                if (minIsDecreasing && !maxIsDecreasing)
                {
                    monotonicDistributionArray[i] = IDistributionFactory.FactoryUniform(minFirstDistribution + epsilon, maxSecondDistribution, sampleSizeSecondDistribution);
                }
                else if (!minIsDecreasing && maxIsDecreasing)
                {
                    monotonicDistributionArray[i] = IDistributionFactory.FactoryUniform(minSecondDistribution, maxFirstDistribution + epsilon, sampleSizeSecondDistribution);

                }
                else if (minIsDecreasing && maxIsDecreasing)
                {
                    monotonicDistributionArray[i] = IDistributionFactory.FactoryUniform(minFirstDistribution + epsilon, maxFirstDistribution + epsilon, sampleSizeSecondDistribution);

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
