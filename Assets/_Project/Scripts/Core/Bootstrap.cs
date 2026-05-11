// Source: D-07 + RESEARCH §Pattern 2 (Bootstrap Composition Root) + spec 06 §4
// Plain-C# composition root. No singletons. No DI container. No SO service locator.
using UnityEngine;

namespace WM.Core
{
    /// <summary>
    /// Scene-level composition root. Runs before all other MonoBehaviours
    /// (DefaultExecutionOrder = -100), constructs plain-C# service implementations,
    /// and injects them into GameManager via an explicit Init(...) call.
    /// In Phase 1 all services are Null*Service stubs; real implementations land
    /// in their owning phases (Save = Phase 6, Analytics = Phase 9, Ads = Phase 10).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class Bootstrap : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private void Awake()
        {
            if (gameManager == null)
            {
                Debug.LogError("[Bootstrap] GameManager reference is missing. Wire it in the Inspector.");
                return;
            }

            IAnalyticsService analytics = new NullAnalyticsService();
            IAdService        ads       = new NullAdService();
            IIapService       iap       = new NullIapService();
            ISaveService      save      = new NullSaveService();

            gameManager.Init(analytics, ads, iap, save);
        }
    }
}
