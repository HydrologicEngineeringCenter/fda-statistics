using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Utilities;

using Statistics.Distributions;

namespace Statistics.Validation
{
    public class EmpiricalValidator: IValidator<Distributions.Empirical>
    {
        public EmpiricalValidator()
        {
        }
        //TODO: Fix these functions. Not fixing now due to higher priorities. -RN
        public IMessageLevels IsValid(Distributions.Empirical entity, out IEnumerable<IMessage> msgs)
        {
            msgs = ReportErrors(entity);
            return msgs.Max();
        }

        public IEnumerable<IMessage> ReportErrors(Distributions.Empirical obj)
        {
            List<IMessage> msgs = new List<IMessage>();
            if (obj.IsNull()) throw new ArgumentNullException(nameof(obj), "The empirical distribution could not be validated because it is null.");
            if (!(obj.SampleSize > 0)) msgs.Add(IMessageFactory.Factory(IMessageLevels.Error, $"{Resources.InvalidParameterizationNotice(obj.Print(true))} {obj.Requirements(false)} {Resources.SampleSizeSuggestion()}."));
            //I don't think we need to check for finite range - an empirical distribution is finite by definition
            return msgs;
        }
        internal static bool IsConstructable(double[] cumulativeProbabilities, double[] observationValues, out string msg)
        {
            msg = ReportFatalErrors(cumulativeProbabilities, observationValues);
            return !msg.Any();
        }
        private static string ReportFatalErrors(double[] cumulativeProbabilities, double[] observationValues)
        {
            string msg = "";
            if ((cumulativeProbabilities.Min() < 0) || (cumulativeProbabilities.Max() > 1)) msg += $"{Resources.FatalParameterizationNotice(Empirical.Print(observationValues, cumulativeProbabilities))} {Empirical.RequiredParameterization(true)} {Resources.SampleSizeSuggestion()}.";
            return msg;
        }
    }
}
