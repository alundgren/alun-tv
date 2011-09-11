using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebUi.Infrastructure
{
    public interface ILogger
    {
        void LogException(Exception ex);
        void LogException(string msg, Exception ex);
    }
}
