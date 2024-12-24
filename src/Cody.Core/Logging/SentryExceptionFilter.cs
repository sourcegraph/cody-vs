using Sentry.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Logging
{
    public class SentryExceptionFilter : IExceptionFilter
    {
        public bool Filter(Exception ex)
        {
            //https://sourcegraph.sentry.io/issues/6123815161
            if (ex is AggregateException &&
                ex.InnerException != null &&
                ex.InnerException is TimeoutException &&
                ex.InnerException.TargetSite != null &&
                ex.InnerException.TargetSite.DeclaringType.Name == "TplExtensions" &&
                ex.InnerException.TargetSite?.Name == "WithTimeout") return false;

            //https://sourcegraph.sentry.io/issues/6152558932
            if(ex.TargetSite.DeclaringType.Assembly.FullName.StartsWith("JetBrains") ||
               ex.InnerException != null && ex.InnerException.TargetSite.DeclaringType.Assembly.FullName.StartsWith("JetBrains")) return false;

            //https://sourcegraph.sentry.io/issues/6115819359
            if (ex.GetType().Name.StartsWith("JetBrains")) return false;

            return true;
        }
    }
}
