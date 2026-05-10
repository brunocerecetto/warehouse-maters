// Source: VALIDATION.md Wave 0 row 01-02-* + RESEARCH Code Examples (PlayMode smoke).
// Plan 01-02 will append a Scene_Loads_GameManagerInitializes test that opens Warehouse_MVP.unity.
// In Phase 1 wave 1 we ship a trivial passing test so the PlayMode CLI returns exit 0.
using NUnit.Framework;
using UnityEngine;

namespace WM.Tests.PlayMode
{
    public class PlayModeSmokeTests
    {
        [Test]
        public void PlayModeAssembly_Compiles()
        {
            // Trivial assertion proves the test assembly was built and is runnable.
            // Real scene-load smoke test is added in plan 01-02 once Warehouse_MVP.unity exists.
            Assert.That(Application.isPlaying || !Application.isPlaying, Is.True);
        }
    }
}
