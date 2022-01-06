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
        public Empirical[] OrderStatisticsCDFs
        {
            get
            {
                return _OrderStatisticsCDFs;
            }
            set
            {
                _OrderStatisticsCDFs = value;
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
    }
}
