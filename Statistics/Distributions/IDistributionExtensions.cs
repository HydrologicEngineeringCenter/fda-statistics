using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Xml.Linq;
using Utilities;

namespace Statistics
{
    /// <summary>
    /// Extension methods for objects implementing the <see cref="IDistribution"/> interface.
    /// </summary>
    public static class IDistributionExtensions
    {
        /// <summary>
        /// Calculates the number of random values required to sample the distribution.
        /// </summary>
        /// <param name="distribution"></param>
        /// <returns> An integer equal to the <see cref="IDistribution.SampleSize"/>. </returns>
        public static int SampleParameters(this IDistribution distribution) => !distribution.IsNull() ? distribution.SampleSize : throw new ArgumentNullException();
        /// <summary>
        /// Generates a parametric bootstrap sample of the distribution.
        /// </summary>
        /// <param name="distribution"></param>
        /// <param name="packetOfRandomNumbers"> Random numbers used to generate the bootstrap sample values, the array length must be equal to or longer than <see cref="IDistribution.SampleSize"/>. </param>
        /// <returns> A new <see cref="IDistribution"/> constructed from a bootstrap sample from the underlying distribution. </returns>
        public static IDistribution Sample(this IDistribution distribution, double[] packetOfRandomNumbers)
        {
            if (distribution.IsNull()) throw new ArgumentNullException($"The sample distribution cannot be constructed because the input distribution is null.");
            if (!(distribution.State < Utilities.IMessageLevels.Error)) throw new ArgumentException($"The specified distribution cannot be sampled because it is being held in an invalid state with the following messages: {distribution.Messages.PrintTabbedListOfMessages()}");           
            if (!packetOfRandomNumbers.IsItemsOnRange(IRangeFactory.Factory(0d, 1d))) throw new ArgumentException($"The sample distribution cannot be sampled because the provided packet or random numbers is null, empty or contains members outside the valid range of: [0, 1].");
            if (packetOfRandomNumbers.Length < distribution.SampleSize) throw new ArgumentException($"The parametric bootstrap sample cannot be constructed using the {distribution.Print(true)} distribution. It requires at least {distribution.SampleSize} random value but only {packetOfRandomNumbers.Length} were provided.");
            double[] X = new double[distribution.SampleSize];
            for (int i = 0; i < distribution.SampleSize; i++) X[i] = distribution.InverseCDF(packetOfRandomNumbers[i]);
            return IDistributionFactory.Fit(X, distribution.Type);
        }
        private static bool IsItemsOnRange<T>(this IEnumerable<T> input, IRange<T> range)
        {
            if (range.IsNull()) throw new ArgumentNullException(nameof(range), "The input IEnumerable items cannot not be checked for membership on the specified range because the range is null.");
            if (input.IsNullOrEmpty()) return false;
            else
            {
                foreach (var p in input) if (!range.IsOnRange(p)) return false;
                return true;
            }
        }
        public static XElement ToXML(this IDistribution dist)
        {
            XElement ele = new XElement(dist.GetType().Name);
            PropertyInfo[] pilist = dist.GetType().GetProperties();
            foreach(PropertyInfo pi in pilist)
            {
                Distributions.StoredAttribute sa = (Distributions.StoredAttribute)pi.GetCustomAttribute(typeof(Distributions.StoredAttribute));
                if (sa != null)
                {
                    ele.SetAttributeValue(sa.Name, pi.GetValue(dist));
                }
            }
            return ele;
        }
        public static IDistribution FromXML(XElement ele)
        {
            string n = ele.Name.ToString();
            string ns = "Statistics";//this libraries name and the appropriate namespace.
            ObjectHandle oh = System.Activator.CreateInstance(ns, ns + ".Distributions." + n);//requires empty constructor
            IDistribution dist = oh.Unwrap() as IDistribution;
            PropertyInfo[] pilist = dist.GetType().GetProperties();
            foreach (PropertyInfo pi in pilist)
            {
                Distributions.StoredAttribute sa = (Distributions.StoredAttribute)pi.GetCustomAttribute(typeof(Distributions.StoredAttribute));
                if (sa != null)
                {
                    switch (sa.type.Name){
                        case "double":
                            double vald = 0.0;
                            if(ele.Attribute(sa.Name).Value=="Infinity"){
                                vald = double.PositiveInfinity;
                            }else if(ele.Attribute(sa.Name).Value=="-Infinity"){
                                vald = double.NegativeInfinity;
                            }else{
                                vald = Convert.ToDouble(ele.Attribute(sa.Name).Value);
                            }
                            pi.SetValue(dist, vald);
                            break;
                        case "Double":
                        double valD = 0.0;
                            if(ele.Attribute(sa.Name).Value=="Infinity"){
                                valD = double.PositiveInfinity;
                            }else if(ele.Attribute(sa.Name).Value=="-Infinity"){
                                valD = double.NegativeInfinity;
                            }else{
                                valD = Convert.ToDouble(ele.Attribute(sa.Name).Value);
                            }
                            pi.SetValue(dist, valD);
                            break;
                        case "Boolean":
                            bool valB = Convert.ToBoolean(ele.Attribute(sa.Name).Value);
                            pi.SetValue(dist, valB);
                            break;
                        case "Int32":
                            int vali = Convert.ToInt32(ele.Attribute(sa.Name).Value);
                            pi.SetValue(dist, vali);
                            break;
                        default:
                            throw new ArgumentException("unrecognized type in serialization " +  sa.type.Name);
                    }
                    
                }
            }
            if(dist.Type==Statistics.IDistributionEnum.LogPearsonIII){
                Statistics.Distributions.LogPearson3 lp3 = (Statistics.Distributions.LogPearson3)dist;
                lp3.BuildFromProperties();
                return lp3 as Statistics.IDistribution;
            }
            return dist;
        }
    }
}
