// Source: D-09 (service interface stubs in WM.Core)
using System.Collections.Generic;

namespace WM.Core
{
    /// <summary>Analytics emission boundary. Real impl arrives in Phase 9 (TEL-01).</summary>
    public interface IAnalyticsService
    {
        void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters = null);
    }
}
