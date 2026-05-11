using System;
using UnityEngine;

namespace WM.Core
{
    public sealed class NullIapService : IIapService
    {
        public void Purchase(string productId, Action<bool> onComplete)
        {
            Debug.Log($"[NullIapService] Purchase {productId} (no-op)");
            onComplete?.Invoke(false);
        }
    }
}
