using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Statistics.Distributions;

namespace Statistics.Validation
{
    public class LogPearson3Validator: IValidator<LogPearson3>
    {
        public LogPearson3Validator()
        {
        }

        public IMessageLevels IsValid(LogPearson3 obj, out IEnumerable<Utilities.IMessage> msgs)
        {
            msgs = ReportErrors(obj);
            return msgs.Max();
        }
        public IEnumerable<IMessage> ReportErrors(LogPearson3 obj)
        {
            List<IMessage> msgs = new List<IMessage>();
            if (obj.IsNull()) throw new ArgumentNullException(nameof(obj), "The log Pearson III distribution could not be validated because it is null.");
            if (obj.StandardDeviation < 0) throw new ArgumentException(nameof(obj), "The log pearson III distribution could not be constructed because standard deviation must be positive");
            if (!(obj.SampleSize > 0)) msgs.Add(IMessageFactory.Factory(IMessageLevels.Error, $"{Resources.InvalidParameterizationNotice(obj.Print(true))} {obj.Requirements(false)} {Resources.SampleSizeSuggestion()}."));
            msgs.Add(IMessageFactory.Factory(IMessageLevels.Message, $"The log Pearson III distribution has been restricted to a finite range which spans the probability range: {obj._ProbabilityRange.Print(false)}."));
            return msgs;
        }
        internal static bool IsConstructable(double mean, double sd, double skew, int n, out string error)
        {
            error = ReportFatalError(mean, sd, skew, n);
            return error.Length == 0;
        }
        internal static string ReportFatalError(double mean, double sd, double skew, int n)
        {
            string msg = "";
            if (!mean.IsOnRange(0, 10) || !sd.IsOnRange(0, 10) || !skew.IsOnRange(-10, 10) || !n.IsOnRange(0, int.MaxValue, false, true)) msg += $"{Resources.FatalParameterizationNotice(LogPearson3.Print(mean, sd, skew, n))} {LogPearson3.RequiredParameterization(true)} {Resources.SampleSizeSuggestion()}";
            return msg;
        }
    }
}
