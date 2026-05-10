// Source: VALIDATION.md row 01-02-* + RESEARCH §EditMode smoke test code example.
// Covers BOOT-02 — required GameObjects in Warehouse_MVP scene.
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WM.Tests.EditMode
{
    public class BootstrapSmokeTests
    {
        private const string ScenePath = "Assets/_Project/Scenes/Warehouse_MVP.unity";

        [SetUp]
        public void OpenScene()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        [TestCase("Bootstrap")]
        [TestCase("GameManager")]
        [TestCase("CameraRig")]
        [TestCase("UICanvas")]
        [TestCase("EventSystem")]
        [TestCase("Player")]
        [TestCase("LoadingDock")]
        [TestCase("PackingStation")]
        [TestCase("DeliveryZone")]
        [TestCase("UpgradeStation")]
        [TestCase("ShelfArea")]
        [TestCase("WorkerSpawn")]
        public void RequiredGameObject_IsPresent(string name)
        {
            GameObject go = GameObject.Find(name);
            Assert.That(go, Is.Not.Null,
                $"Required GameObject '{name}' missing from {ScenePath}");
        }

        [TearDown]
        public void Cleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }
    }
}
