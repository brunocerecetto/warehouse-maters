// Source: VALIDATION.md row 01-02-* + RESEARCH §PlayMode smoke test code example.
// Covers BOOT-02 runtime gate — scene loads at runtime AND GameManager logs init line.
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using WM.Core;

namespace WM.Tests.PlayMode
{
    public class PlayModeSmokeTests
    {
        [UnityTest]
        public IEnumerator Scene_Loads_GameManagerInitializes()
        {
            // Scene must be in Build Settings for SceneManager.LoadSceneAsync to find it.
            // Plan 01-02 Task 3 Step F adds Warehouse_MVP.unity to EditorBuildSettings.
            LogAssert.Expect(LogType.Log, "GameManager initialized");

            yield return SceneManager.LoadSceneAsync("Warehouse_MVP", LoadSceneMode.Single);
            // Wait one frame for Awake/Start chain to finish.
            yield return null;

            GameObject gm = GameObject.Find("GameManager");
            Assert.That(gm, Is.Not.Null, "GameManager GameObject not found in loaded scene");
            Assert.That(gm.GetComponent<GameManager>(), Is.Not.Null,
                "GameObject 'GameManager' is missing the WM.Core.GameManager component");

            GameObject bootstrap = GameObject.Find("Bootstrap");
            Assert.That(bootstrap, Is.Not.Null, "Bootstrap GameObject not found");
            Assert.That(bootstrap.GetComponent<Bootstrap>(), Is.Not.Null,
                "GameObject 'Bootstrap' is missing the WM.Core.Bootstrap component");
        }
    }
}
