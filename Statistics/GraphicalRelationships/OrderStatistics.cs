using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statistics.Distributions;

namespace Statistics.GraphicalRelationships
{
    public class OrderStatistics
    {
        #region Fields
        private double[] _ExceedanceProbabilities;
        private double[] _NonExceedanceProbabilities;
        private double[] _FlowOrStageValues;
        private Empirical[] _OrderStatisticsCDFs;
        private double[] _Means;
        private double[] _StandardDeviations;
        #endregion

        #region Properties 
        public double[] ExceedanceProbabilities
        {
            get
            {
                return _ExceedanceProbabilities;
            } 
            set
            {
                _ExceedanceProbabilities = value;
            }
        }
        public double[] Means
        {
            get
            {
                return _Means;
            }
            set
            {
                _Means = value;
            }
        }
        public double[] StandardDeviations
        {
            get
            {
                return _StandardDeviations;
            }
            set
            {
                _StandardDeviations = value;
            }
        }
        #endregion

        #region Constructor
        public OrderStatistics()
        {
        }
        public OrderStatistics(double[] exceedanceProbabilities, double[] flowOrStageValues)
        {
            _ExceedanceProbabilities = exceedanceProbabilities;
            _FlowOrStageValues = flowOrStageValues;
            _NonExceedanceProbabilities = ExceedanceToNonExceedance(exceedanceProbabilities);

        }
        #endregion

        #region Functions
        public static double[] ExceedanceToNonExceedance(double[] exceedanceProbabilities)
        {
            double[] nonExceedanceProbabilities = new double[exceedanceProbabilities.Length];
            for (int i = 0; i < exceedanceProbabilities.Length; i++)
            {
                nonExceedanceProbabilities[i] = 1 - exceedanceProbabilities[i];
            }
            return nonExceedanceProbabilities;
        }

        public void ComputeOrderStatisticsCDFs()
        {
            for (int i = 0; i < _NonExceedanceProbabilities.Length; i ++)
            {
                double[] cdf = ComputeCDF(i);
                _Means[i] = ComputeMean(cdf);
                _StandardDeviations[i] = ComputeStandardDeviation(cdf, _Means[i]);
            }
        }

        public double[] ComputeCDF(int index)
        {
            int quantityTrials = _NonExceedanceProbabilities.Length;
            double probabilityOfSuccess = _NonExceedanceProbabilities[index];
            double probabilityOfFailure = _ExceedanceProbabilities[index];

            double[] pdf = new double[quantityTrials];
            double[] cdf = new double[quantityTrials];
            // at least i observations are less than the quantile 
            for (int i = 0; i < quantityTrials; i++)
            {   
                pdf[i] = ComputeBinomialProbability(quantityTrials, i+1, probabilityOfSuccess);
            }
            for (int j = 0; j < quantityTrials; j++)
            {
                for (int k = quantityTrials-1; k >= j; k--)
                {
                    cdf[j] += pdf[k];
                }
            }
            return cdf;
        }

        public static double ComputeBinomialProbability(int quantityTrials, int quantitySuccesses, double probabilityOfSuccess)
        {
            double probabilityOfFailure = 1 - probabilityOfSuccess;
            int quantityFailure = quantityTrials - quantitySuccesses;
            double combination = ComputeCombination(quantityTrials, quantitySuccesses);
            double probabilityOfSuccesses = Math.Pow(probabilityOfSuccess, quantitySuccesses);
            double probabilityOfFailures = Math.Pow(probabilityOfFailure, quantityFailure);
            double pdf = combination * probabilityOfSuccesses * probabilityOfFailures;
            return pdf;
        }

        public static double ComputeCombination(int quantityTrials, int quantitySuccesses)
        {
            int trialsFactorial = ComputeFactorial(quantityTrials);
            int successesFactorial = ComputeFactorial(quantitySuccesses);
            int trialsLessSuccessesFactorial = ComputeFactorial(quantityTrials - quantitySuccesses);
            double combination = trialsFactorial / (successesFactorial * trialsLessSuccessesFactorial);
            return combination;
        }

        public static int ComputeFactorial(int value)
        {
            int product = 1;
            for (int i = 1; i < value+1; i++)
            {
                product = product * i;
            }
            return product;
        }

        public double ComputeMean(double[] cdf)
        {
            double sum = 0;
            for (int i = 1; i < cdf.Length; i++)
            {
                double valueAverage = 0.5 * (_FlowOrStageValues[i - 1] + _FlowOrStageValues[i]);
                double probabilityBetweenValues = (cdf[i - 1] - cdf[i]) / (cdf[0] - cdf[cdf.Length - 1]);
                sum += valueAverage * probabilityBetweenValues;
            }
            return sum;
        }

        public double ComputeStandardDeviation(double[] cdf, double mean)
        {
            double sum = 0;
            for (int i = 1; i < cdf.Length; i++)
            {
                double averageOfDeviationsSquared = 0.5 * (Math.Pow(_FlowOrStageValues[i - 1] - mean, 2) + Math.Pow(_FlowOrStageValues[i] - mean, 2));
                double probabilityBetweenValues = (cdf[i - 1] - cdf[i]) / (cdf[0] - cdf[cdf.Length - 1]);
                sum += averageOfDeviationsSquared * probabilityBetweenValues;
            }
            double standardDeviation = Math.Pow(sum, 0.5);
            return standardDeviation;
        }
        #endregion
    }
}
