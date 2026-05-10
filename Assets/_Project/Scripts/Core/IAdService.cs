using System;

namespace WM.Core
{
    /// <summary>Rewarded-ad boundary. Real impl arrives in Phase 10 (ADS-01).</summary>
    public interface IAdService
    {
        void ShowRewarded(Action<bool> onComplete);
    }
}
