
using System;

namespace Bookkeeping.Common.Interfaces
{
    public interface ILogger
    {
        void Error(string message, Exception exception = null, string prefix = null);
        void Info(string message, string prefix = null);
    }
}
