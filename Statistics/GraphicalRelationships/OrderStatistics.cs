using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statistics.Distributions;

namespace Statistics.GraphicalRelationships
{
    class OrderStatistics
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
        public OrderStatistics(double[] exceedanceProbabilities, double[] flowOrStageValues)
        {
            _ExceedanceProbabilities = exceedanceProbabilities;
            _FlowOrStageValues = flowOrStageValues;
            _NonExceedanceProbabilities = ExceedanceToNonExceedance(exceedanceProbabilities);

        }
        #endregion

        #region Functions
        private double[] ExceedanceToNonExceedance(double[] exceedanceProbabilities)
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
            Empirical[] cdfs = ComputeCDFs();
            _Means = ComputeMeans(cdfs);
            _StandardDeviations = ComputeStandardDeviations(cdfs);

        }

        private Empirical[] ComputeCDFs()
        {
            //compute CDFs using equation 1 here 
        }

        private double ComputeBinomialPDF(int quantityTrials, int quantitySuccesses, double probabilityOfSuccess)
        {
            double probabilityOfFailure = 1 - probabilityOfSuccess;
            int quantityFailure = quantityTrials - quantitySuccesses;
            double combination = ComputeCombination(quantityTrials, quantitySuccesses);
            double probabilityOfSuccesses = Math.Pow(probabilityOfSuccess, quantitySuccesses);
            double probabilityOfFailures = Math.Pow(probabilityOfFailure, quantityFailure);
        }

        private double ComputeCombination(int quantityTrials, int quantitySuccesses)
        {
            int trialsFactorial = ComputeFactorial(quantityTrials);
            int successesFactorial = ComputeFactorial(quantitySuccesses);
            int trialsLessSuccessesFactorial = ComputeFactorial(quantityTrials - quantitySuccesses);
            double combination = trialsFactorial / (successesFactorial * trialsLessSuccessesFactorial);
            return combination;
        }

        private int ComputeFactorial(int value)
        {
            int product = 1;
            for (int i = 1; i < value; i++)
            {
                product *= i;
            }
            return product;
        }

        private double[] ComputeMeans(Empirical[] cdfs)
        {
            //compute means here
        }

        private double[] ComputeStandardDeviations(Empirical[] cdfs)
        {
            //compute standard deviations here 
        }
    }
}
