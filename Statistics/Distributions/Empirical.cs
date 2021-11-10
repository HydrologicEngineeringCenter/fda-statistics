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

        public double Mean { get 
            {
                return ComputeMean();
            } 
        }
        public double Median { get 
            {
                return ComputeMedian();
            } 
        }

        public double Mode { get 
            {
                return ComputeMode(); 
            } 
        }

        public double StandardDeviation { get
            {
                return ComputeStandardDeviation();
            }
        }

        public double Variance { get 
            {
                return Math.Pow(StandardDeviation, 2);

            } 
        }

        public double Min { get; set; }

        public double Max { get; set; }

        public double Skewness { get 
            {
                return ComputeSkewness();
            } 
        }

        public IRange<double> Range { get; set; }

        public int SampleSize { get {
                return ObservationValues.Length;
            } 
        }

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
            double differenceFromMeanCubed = 0;
            for (int i = 0; i < SampleSize; ++i)
            {
                differenceFromMeanCubed += Math.Pow((Mean - ObservationValues[i]), 3);
            }
            return (differenceFromMeanCubed / SampleSize) / Math.Pow(StandardDeviation, 3);
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
            int index = ObservationValues.ToList().IndexOf(x);
            if (index >= 0)
            {
                return CumulativeProbabilities[index];
            }
            else
            {
                int size = SampleSize;
                index = -(index + 1);
                if (index == 0)
                {   // first value
                    return 0.0;
                }
                // in between index-1 and index: interpolate
                else if (index < size)
                {
                    double weight = (x - ObservationValues[index - 1]) / (ObservationValues[index] - ObservationValues[index - 1]);
                    return (1 - weight) * CumulativeProbabilities[index - 1] + weight * CumulativeProbabilities[index];
                }
                else
                {   // last value
                    return 1.0;
                }
            }
        }

        public bool Equals(IDistribution distribution)
        {

            if (distribution.Type == IDistributionEnum.Empirical)
            {
                Empirical distCompared = distribution as Empirical;
                if(ObservationValues == distCompared.ObservationValues && CumulativeProbabilities == distCompared.ObservationValues )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public double InverseCDF(double p)
        {
            int index = CumulativeProbabilities.ToList().IndexOf(p);
            if (index >= 0)
            {
                return ObservationValues[index];
            }
            else
            {
                index = -(index + 1); // may need to take the bitwise complement using the ~ operator in C#, e.g. index = ~index-1? not sure if the same
                // in between index-1 and index: interpolate
                if (index == 0)
                {   // first value
                    return ObservationValues[0];
                }
                else if (index < SampleSize)
                {
                    double weight = (p - CumulativeProbabilities[index - 1]) / (CumulativeProbabilities[index] - CumulativeProbabilities[index - 1]);
                    return (1.0 - weight) * ObservationValues[index - 1] + weight * ObservationValues[index];
                }
                else
                {   // last value
                    return ObservationValues[SampleSize - 1];
                }
            }
        }

        public double PDF(double x)
        {
            int index = ObservationValues.ToList().IndexOf(x);
            if (index >= 0)
            {
                double pdfLeft;
                if (index == 0)
                {   // first value
                    pdfLeft = 0;
                }
                else
                {
                    pdfLeft = (CumulativeProbabilities[index] - CumulativeProbabilities[index - 1]) / (ObservationValues[index] - ObservationValues[index - 1]);
                }
                double pdfRight;
                if (index < SampleSize - 1)
                {
                    pdfRight = (CumulativeProbabilities[index + 1] - CumulativeProbabilities[index]) / (ObservationValues[index + 1] - ObservationValues[index]);
                }
                else
                {   //last value
                    pdfRight = 0;
                }
                double pdfValue = 0.5 * (pdfLeft + pdfRight);
                return pdfValue;

            }
            else
            {
                index = -(index + 1);
                // in between index-1 and index: interpolate
                if (index == 0)
                {   // first value
                    return 0.0;
                }
                else if (index < SampleSize)
                {
                    double pdfValue = (CumulativeProbabilities[index] - CumulativeProbabilities[index - 1]) / (ObservationValues[index] - ObservationValues[index - 1]);
                    return pdfValue;
                }
                else
                {   // last value
                    return 0.0;
                }

            }
        }

        public static string Print(double[] observationValues, double[] cumulativeProbabilities)
        {
            string returnString = "Empirical Distribution \n Observation Values | Cumulative Probabilities \n";
            for (int i=0; i<observationValues.Length; i++)
            {
                returnString += $"{observationValues[i]} | {cumulativeProbabilities[i]}";
            }
            return returnString;
        }

        public string Requirements(bool printNotes)
        {
            return RequiredParameterization(printNotes);
        }
        public static string RequiredParameterization(bool printNotes = false)
        {
            return $"The empirical distribution requires the following parameterization: {Parameterization()}.";
        }
        internal static string Parameterization()
        {
            return $"Empirical(Observation Values: [{double.MinValue.Print()}, {double.MaxValue.Print()}], Cumulative Probabilities [0,1])";
        }
        public XElement WriteToXML()
        {
            XElement masterElem = new XElement("Empirical Distribution");
            masterElem.SetAttributeValue("Ordinate_Count", SampleSize);
            for (int i = 0; i<SampleSize; i++)
            {
                XElement rowElement = new XElement("Coordinate");
                XElement xRowElement = new XElement("X");
                xRowElement.SetAttributeValue("Value", ObservationValues[i]);
                XElement yRowElement = new XElement("Y");
                yRowElement.SetAttributeValue("Cumulative Probability", CumulativeProbabilities[i]);
                rowElement.Add(xRowElement);
                rowElement.Add(yRowElement);
                masterElem.Add(rowElement);
            }
            return masterElem;

        }
        #endregion
    }

}
