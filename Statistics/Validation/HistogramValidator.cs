using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Utilities;

using Statistics.Distributions;

namespace Statistics.Validation
{
    public class HistogramValidator : IValidator<Histograms.Histogram>
    {
        internal HistogramValidator()
        {
        }
        public IMessageLevels IsValid(Histograms.Histogram entity, out IEnumerable<IMessage> msgs)
        {
            msgs = ReportErrors(entity);
            return msgs.Max();
        }

        public IEnumerable<IMessage> ReportErrors(Histograms.Histogram obj)
        {
            List<IMessage> msgs = new List<IMessage>();
            if (obj.IsNull()) throw new ArgumentNullException(nameof(obj), "The histogram could not be validated because it is null.");
            //if (!(obj.SampleSize > 0)) msgs.Add(IMessageFactory.Factory(IMessageLevels.Error, $"{Resources.InvalidParameterizationNotice(obj.Print(true))} {obj.Requirements(false)} {Resources.SampleSizeSuggestion()}."));
            //I don't think we need to check for finite range - a histogram is finite by definition
            return msgs;
        }
        internal static bool IsConstructable(IData data, double binWidth, out string msg)
        {
            msg = ReportFatalErrors(data, binWidth);
            return !msg.Any();
        }
        internal static bool IsConstructable(double min, double max, double binWidth, double[] binCounts, out string msg)
        {
            msg = ReportFatalErrors(min,max,binWidth,binCounts);
            return !msg.Any();
        }
        private static string ReportFatalErrors(IData data, double binWidth)
        {
            string msg = "";
            //if (binWidth < 0) msg += $"{Resources.FatalParameterizationNotice(Histograms.Histogram.Print(data.Elements.Count(), Convert.ToInt32((data.Elements.Max() - data.Elements.Min()) / binWidth), IRangeFactory.Factory(data.Elements.Min(), data.Elements.Max())))} {Histograms.Histogram.RequiredParameterization(true)} {Resources.SampleSizeSuggestion()}.";
            return msg;
        }
        private static string ReportFatalErrors(double min, double max, double binWidth, double[] binCounts)
        {
            string msg = "";
            //if (binWidth < 0 || binCounts.IsNull() || min > max) msg += $"{Resources.FatalParameterizationNotice(Histograms.Histogram.Print(Convert.ToInt32(binCounts.Sum()),binCounts.Length,IRangeFactory.Factory(min,max)))} {Histograms.Histogram.RequiredParameterization(true)} {Resources.SampleSizeSuggestion()}.";
            return msg;
        }


    }
}
