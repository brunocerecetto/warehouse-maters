using System.Collections.Generic;
using UnityEngine;

namespace WM.Core
{
    public sealed class NullAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters = null)
        {
            Debug.Log($"[NullAnalyticsService] {eventName}");
        }
    }
}
