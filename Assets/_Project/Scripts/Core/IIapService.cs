using System;

namespace WM.Core
{
    /// <summary>IAP boundary. Out of MVP scope but reserved here for parity with spec section 4.</summary>
    public interface IIapService
    {
        void Purchase(string productId, Action<bool> onComplete);
    }
}
