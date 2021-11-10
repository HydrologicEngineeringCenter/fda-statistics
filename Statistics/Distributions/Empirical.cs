using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;

namespace Statistics.Distributions
{
    class Empirical : IDistribution
    {
        #region EmpiricalProperties
        /// <summary>
        /// Cumulative probabilities are non-exceedance probabilities 
        /// </summary>
        public double[] CumulativeProbabilities;
        public double[] ObservationValues;
        #endregion

        #region IDistributionProperties
        public IDistributionEnum Type => IDistributionEnum.Empirical;

        public double Mean { get; set; }
        public double Median { get; set; }

        public double Mode { get; set; }

        public double StandardDeviation { get; set; }

        public double Variance { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public double Skewness { get; set; }

        public IRange<double> Range { get; set; }

        public int SampleSize { get; set; }

        //are we still using IMessageLevels?
        public IMessageLevels State => throw new NotImplementedException();

        public IEnumerable<IMessage> Messages => throw new NotImplementedException();
        #endregion

        #region Constructor

        public Empirical(double[] probabilities, double[] observationValues, bool probsAreExceedance = false)
        {
            double[] probabilityArray = new double[probabilities.Length];
            if (probsAreExceedance == true)
            {
                probabilityArray = ConvertExceedanceToNonExceedance(probabilities);
                
            } else
            {
                probabilityArray = probabilities;
            }  
            if (!IsMonotonicallyIncreasing(probabilityArray))
            {   //sorting the arrays separately feels a little precarious 
                //what if the user provides a non-monotonically increasing relationship?
                //e.g. probs all increasing but values not or vice versa 
                Array.Sort(probabilityArray);
            }
            CumulativeProbabilities = probabilityArray;
            if (!IsMonotonicallyIncreasing(observationValues))
            {
                Array.Sort(observationValues);
            }
            ObservationValues = observationValues;
            SampleSize = ObservationValues.Length;
            Mean = ComputeMean();
            Median = ComputeMedian();
            Mode = ComputeMode();
            StandardDeviation = ComputeStandardDeviation();
            Variance = Math.Pow(StandardDeviation, 2);
            

        }

        public void BuildFromProperties()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region EmpiricalFunctions
        private double[] ConvertExceedanceToNonExceedance(double[] ExceedanceProbabilities)
        {
            double[] nonExceedanceProbabilities = new double[ExceedanceProbabilities.Length];
            for (int i = 0; i<ExceedanceProbabilities.Length; i++ )
            {
                nonExceedanceProbabilities[i] = 1 - ExceedanceProbabilities[i];
            }
            return nonExceedanceProbabilities;
        }

        private double ComputeMean()
        {
            if (SampleSize == 0)
            {
                return 0.0;
            }
            else if (SampleSize == 1)
            {
                return ObservationValues[0];
            }
            else
            {
                double mean = 0;
                int i;
                double stepPDF, stepVal;
                double valL, valR, cdfL, cdfR;
                // left singleton
                i = 0;
                valR = ObservationValues[i];
                cdfR = CumulativeProbabilities[i];
                stepPDF = cdfR - 0.0;
                mean += valR * stepPDF;
                valL = valR;
                cdfL = cdfR;
                // add interval values
                for (i = 1; i < SampleSize; ++i)
                {
                    valR = ObservationValues[i];
                    cdfR = CumulativeProbabilities[i];
                    stepPDF = cdfR - cdfL;
                    stepVal = (valL + valR) / 2.0;
                    mean += stepPDF * stepVal;
                    valL = valR;
                    cdfL = cdfR;
                }
                // add right singleton 
                i = SampleSize - 1;
                valR = ObservationValues[i];
                cdfR = 1.0;
                stepPDF = cdfR - cdfL;
                mean += valR * stepPDF;
                // 99% sure we dont need the following two lines. confirm in testing. 
                //valL = valR; 
                //cdfL = cdfR; 
                return mean;
            }
        }
        /// <summary>
        /// Method for computing the median of a data series
        /// <summary>
        public double ComputeMedian()
        {
            if (SampleSize == 0)
            {
                throw new ArgumentException("Sample cannot be null");
            }
            else if (SampleSize == 1)
            {
                return ObservationValues[SampleSize-1];
            }
            else
            {
                if ((SampleSize % 2) == 0)
                {
                    return (ObservationValues[SampleSize / 2] + ObservationValues[SampleSize / 2 - 1]) / 2;
                }
                else
                {
                    return ObservationValues[(SampleSize - 1) / 2];
                }

            }
        }
       private double ComputeMode()
        {
            if (SampleSize == 0)
            {
                throw new ArgumentException("Sample cannot be null");
            }
            else if (SampleSize == 1)
            {
                return ObservationValues[SampleSize - 1];
            }
            else
            {
                int i = 0;
                double[] pdf = new double[CumulativeProbabilities.Length];
                pdf[i] = CumulativeProbabilities[i];
                for (i = 1; i< CumulativeProbabilities.Length; i++)
                {
                    pdf[i] = CumulativeProbabilities[i] - CumulativeProbabilities[i - 1];
                }
                double maxPDF = pdf.Max();
                int indexMaxPDF = pdf.ToList().IndexOf(maxPDF);
                return ObservationValues[indexMaxPDF];
            }
        }
        private double ComputeStandardDeviation()
        {

            if (SampleSize == 0)
            {
                return 0.0;
            }
            else if (SampleSize == 1)
            {
                return 0.0;
            }
            else
            {
                double mean = Mean;
                double expect2 = 0.0;
                int i;
                double stepPDF, stepVal;
                double valL, valR, cdfL, cdfR;
                // add left singleton 
                i = 0;
                valR = ObservationValues[i];
                cdfR = CumulativeProbabilities[i];
                stepPDF = cdfR - 0.0;
                expect2 += valR * valR * stepPDF;
                valL = valR;
                cdfL = cdfR;
                // add interval values
                for (i = 1; i < SampleSize; i++)
                {
                    valR = ObservationValues[i];
                    cdfR = CumulativeProbabilities[i];
                    stepPDF = cdfR - cdfL;
                    stepVal = (valL * valL + valL * valR + valR * valR) / 3.0;
                    expect2 += stepVal * stepPDF;
                    valL = valR;
                    cdfL = cdfR;
                }
                // add last singleton 
                i = SampleSize - 1;
                valR = ObservationValues[i];
                cdfR = 1.0;
                stepPDF = cdfR - cdfL;
                expect2 += valR * stepPDF; //should this be valR*valR*stepPDF? Why did RN write this?
                //valL = valR; same note as mean
                //cdfL = cdfR;
                return expect2 - mean * mean;
            }
        }
        /// <summary>
        /// Method for computing skewness of a data series 
        /// <summary>
        public double ComputeSkewness()
        {
            var dist = new List<Tuple<double, double>>();
            dist = Distribution;
            double mean = Mean; // do I need to call the methods here?
            double standardDeviation = StandardDeviation;
            var size = dist.Count;
            double differenceFromMeanCubed = 0;
            for (int i = 0; i < size; ++i)
            {
                differenceFromMeanCubed += Math.Pow((mean - dist[i].Item1), 3);
            }
            return (differenceFromMeanCubed / size) / Math.Pow(standardDeviation, 3);
        }
        public bool IsMonotonicallyIncreasing(double[] arrayOfData)
        {

            for (int i = 0; i < arrayOfData.Length - 1; i++)
            {
                if (arrayOfData[i] >= arrayOfData[i + 1])
                {
                    return false;
                }
            }
            return true;
        }



        #endregion
        #region IDistributionFunctions


        public double CDF(double x)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IDistribution distribution)
        {
            throw new NotImplementedException();
        }

        public double InverseCDF(double p)
        {
            throw new NotImplementedException();
        }

        public double PDF(double x)
        {
            throw new NotImplementedException();
        }

        public string Print(bool round = false)
        {
            throw new NotImplementedException();
        }

        public string Requirements(bool printNotes)
        {
            throw new NotImplementedException();
        }

        public XElement WriteToXML()
        {
            throw new NotImplementedException();
        }
        #endregion
    }

}
