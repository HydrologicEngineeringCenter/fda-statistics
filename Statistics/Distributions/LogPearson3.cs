﻿using Base.Implementations;
using Base.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Statistics.Distributions
{
    public class LogPearson3: ContinuousDistribution
    {

        #region Properties
        public override IDistributionEnum Type => IDistributionEnum.LogPearsonIII;
        [Stored(Name = "Mean", type = typeof(double))]
        public double Mean { get; set; }
        [Stored(Name = "St_Dev", type = typeof(double))]
        public double StandardDeviation { get; set; }
        [Stored(Name = "Skew", type = typeof(double))]
        public double Skewness { get; set; }
        #endregion

        #region Constructor
        public LogPearson3()
        {
            //for reflection;
            Mean = 0.1;
            StandardDeviation = .01;
            Skewness = .01;
            SampleSize = 1;
            addRules();
            
        }
        public LogPearson3(double mean, double standardDeviation, double skew, int sampleSize = int.MaxValue)
        {
            Mean = mean;
            StandardDeviation = standardDeviation;
            Skewness = skew;
            SampleSize = sampleSize;
            addRules();
            
        }
        private void addRules()
        {
            AddSinglePropertyRule(nameof(StandardDeviation),
                new Rule(() => {
                    return StandardDeviation > 0;
                },
                "Standard Deviation must be greater than 0.",
                ErrorLevel.Fatal));
            AddSinglePropertyRule(nameof(StandardDeviation),
                new Rule(() => {
                    return StandardDeviation < 3;
                },
                "Standard Deviation must be less than 3.",
                ErrorLevel.Fatal));
            AddSinglePropertyRule(nameof(Mean),
                new Rule(() => {
                    return Mean > 0;
                },
                "Mean must be greater than 0.",
                ErrorLevel.Fatal));
            AddSinglePropertyRule(nameof(Mean),
                new Rule(() => {
                    return Mean < 7; //log base 10 mean annual max flow in cfs of amazon river at mouth is 6.7
                },
                "Mean must be less than 7.",
                ErrorLevel.Fatal));
            AddSinglePropertyRule(nameof(Skewness),
                new Rule(() => {
                    return Skewness > -3.0;
                },
                "Skewness must be greater than -3.0.",
                ErrorLevel.Fatal));
            AddSinglePropertyRule(nameof(Skewness),
                new Rule(() => {
                    return Skewness < 3.0;
                },
                "Skewness must be less than 3.0.",
                ErrorLevel.Fatal));
            AddSinglePropertyRule(nameof(SampleSize),
                new Rule(() => {
                    return SampleSize > 0;
                },
                "SampleSize must be greater than 0.",
                ErrorLevel.Fatal));
        }
        #endregion

        #region Functions

        #region IDistribution Functions
        public override double PDF(double x)
        {
            PearsonIII d = new PearsonIII(Mean, StandardDeviation, Skewness, SampleSize);
            return d.PDF(Math.Log10(x))/x/Math.Log(10);        
        }
        public override double CDF(double x)
        {
            if (x > 0)
            {
                PearsonIII d = new PearsonIII(Mean, StandardDeviation, Skewness, SampleSize);
                return d.CDF(Math.Log10(x));
            }
            else return 0;
        }
        public override double InverseCDF(double p)
        {

            if (p <= 0) return 0;
            if (p >= 1) return Double.PositiveInfinity;

            PearsonIII d = new PearsonIII(Mean, StandardDeviation, Skewness, SampleSize);
            return Math.Pow(10, d.InverseCDF(p));
        }
        public override string Print(bool round = false) => round ? Print(Mean, StandardDeviation, Skewness, SampleSize) : $"log PearsonIII(mean: {Mean}, sd: {StandardDeviation}, skew: {Skewness}, sample size: {SampleSize})";
        public override string Requirements(bool printNotes) => RequiredParameterization(printNotes);
        public override bool Equals(IDistribution distribution) => string.Compare(Print(), distribution.Print(), StringComparison.InvariantCultureIgnoreCase) == 0 ? true : false;
        #endregion

        internal static string Print(double mean, double sd, double skew, int n) => $"log PearsonIII(mean: {mean.Print()}, sd: {sd.Print()}, skew: {skew.Print()}, sample size: {n.Print()})";
        internal static string RequiredParameterization(bool printNotes = true)
        {
            string s = $"The log PearsonIII distribution requires the following parameterization: {Parameterization()}.";
            if (printNotes) s += RequirementNotes();
            return s;
        }
        internal static string Parameterization() => $"log PearsonIII(mean: (0, {Math.Log10(double.MaxValue).Print()}], sd: (0, {Math.Log10(double.MaxValue).Print()}], skew: [{(Math.Log10(double.MaxValue) * -1).Print()}, {Math.Log10(double.MaxValue).Print()}], sample size: > 0)";
        internal static string RequirementNotes() => $"The distribution parameters are computed from log base 10 random numbers (e.g. the log Pearson III distribution is a distribution of log base 10 Pearson III distributed random values). Therefore the mean and standard deviation parameters must be positive finite numbers and while a large range of numbers are acceptable a relative small rate will produce meaningful results.";

        public override IDistribution Fit(double[] sample)
        {
            for(int i = 0; i<sample.Count(); i++) {
                sample[i] = Math.Log10(sample[i]);
            }
            ISampleStatistics stats = new SampleStatistics(sample);
            return new LogPearson3(stats.Mean, stats.StandardDeviation, stats.Skewness, stats.SampleSize);
        }
        #endregion
    }
}
