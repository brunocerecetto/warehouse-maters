using System;
using UnityEngine;

namespace WM.Core
{
    public sealed class NullAdService : IAdService
    {
        public void ShowRewarded(Action<bool> onComplete)
        {
            Debug.Log("[NullAdService] ShowRewarded (no-op)");
            onComplete?.Invoke(false);
        }
    }
}
