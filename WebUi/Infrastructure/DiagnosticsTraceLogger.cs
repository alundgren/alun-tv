using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Text;

namespace WebUi.Infrastructure
{
    public class DiagnosticsTraceLogger : ILogger
    {
        private string FormatException(Exception ex)
        {
            var depth = 0;
            var sb = new StringBuilder();
            while (ex != null && depth < 5)
            {
                sb.AppendLine(ex.Message + ":" + ex.StackTrace);
                ex = ex.InnerException;
            }
            return sb.ToString();
        }

        public void LogException(Exception ex)
        {
           Trace.TraceError(FormatException(ex));
           Trace.Flush();
        }

        public void LogException(string msg, Exception ex)
        {
            Trace.TraceError("{0}: {1}", msg, FormatException(ex));
            Trace.Flush();
        }
    }
}