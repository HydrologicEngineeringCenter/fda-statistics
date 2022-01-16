using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;

namespace Statistics
{
    public abstract class ContinuousDistribution : Base.Implementations.Validation, IDistribution
    {   
        public abstract IDistributionEnum Type { get; }
        public abstract int SampleSize { get; protected set; }
        public abstract bool Truncated { get; protected set; }
        public abstract IMessageLevels State { get; protected set; }
        public abstract IEnumerable<IMessage> Messages { get; protected set; }
        public abstract double CDF(double x);
        public abstract bool Equals(IDistribution distribution);
        public abstract double InverseCDF(double p);
        public abstract double PDF(double x);
        public abstract string Print(bool round = false);
        public abstract string Requirements(bool printNotes);
        public XElement ToXML()
        {
            XElement ele = new XElement(this.GetType().Name);
            PropertyInfo[] pilist = this.GetType().GetProperties();
            foreach (PropertyInfo pi in pilist)
            {
                Distributions.StoredAttribute sa = (Distributions.StoredAttribute)pi.GetCustomAttribute(typeof(Distributions.StoredAttribute));
                if (sa != null)
                {
                    ele.SetAttributeValue(sa.Name, pi.GetValue(this));
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
                    switch (sa.type.Name)
                    {
                        case "double":
                            double vald = 0.0;
                            if (ele.Attribute(sa.Name).Value == "Infinity")
                            {
                                vald = double.PositiveInfinity;
                            }
                            else if (ele.Attribute(sa.Name).Value == "-Infinity")
                            {
                                vald = double.NegativeInfinity;
                            }
                            else
                            {
                                vald = Convert.ToDouble(ele.Attribute(sa.Name).Value);
                            }
                            pi.SetValue(dist, vald);
                            break;
                        case "Double":
                            double valD = 0.0;
                            if (ele.Attribute(sa.Name).Value == "Infinity")
                            {
                                valD = double.PositiveInfinity;
                            }
                            else if (ele.Attribute(sa.Name).Value == "-Infinity")
                            {
                                valD = double.NegativeInfinity;
                            }
                            else
                            {
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
                            throw new ArgumentException("unrecognized type in serialization " + sa.type.Name);
                    }

                }
            }
            if (dist.Type == IDistributionEnum.LogPearsonIII)
            {
                Distributions.LogPearson3 lp3 = (Distributions.LogPearson3)dist;
                lp3.BuildFromProperties();
                return lp3;
            }
            return dist;
        }
    }
}
