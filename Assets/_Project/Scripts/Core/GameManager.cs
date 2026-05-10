// Source: D-08 + spec 06 §4 (GameManager: "Initialize services")
// Phase 1 = thin orchestrator: receives services, logs init.
// Later phases will add: load save data, start tutorial or normal gameplay,
// coordinate game state transitions.
using UnityEngine;

namespace WM.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        private IAnalyticsService _analytics;
        private IAdService _ads;
        private IIapService _iap;
        private ISaveService _save;

        public void Init(
            IAnalyticsService analytics,
            IAdService ads,
            IIapService iap,
            ISaveService save)
        {
            _analytics = analytics;
            _ads = ads;
            _iap = iap;
            _save = save;
        }

        private void Start()
        {
            Debug.Log("GameManager initialized");
        }
    }
}
