// Source: VALIDATION.md rows 01-02-* (RequiredGameObject_IsPresent) and 01-03-* (Cinemachine, UICanvas, EventSystem checks).
// Covers BOOT-02 — required GameObjects + Cinemachine + UICanvas + EventSystem configuration.
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using WM.UI;

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

        // ---- BOOT-02: 12 required GameObjects (from plan 01-02) ----
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

        // ---- BOOT-02 (plan 01-03): Cinemachine 3.x rig ----
        [Test]
        public void CinemachineCamera_IsConfigured()
        {
            var brains = Object.FindObjectsByType<CinemachineBrain>(FindObjectsSortMode.None);
            Assert.That(brains.Length, Is.GreaterThanOrEqualTo(1),
                "Scene must contain at least one CinemachineBrain (expected on Main Camera under CameraRig).");

            var vcams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
            Assert.That(vcams.Length, Is.GreaterThanOrEqualTo(1),
                "Scene must contain at least one CinemachineCamera (expected named 'CM vcam1' under CameraRig).");

            // Phase 1: Tracking Target must be null (D-12). Phase 2 sets it to Player.
            Assert.That(vcams[0].Follow, Is.Null,
                "Phase 1 CinemachineCamera must have no Follow target (D-12). Phase 2 will set it to Player.");

            // Phase 1 (this plan): Lens Mode = Perspective, FOV = 40 (locked).
            // Cinemachine 3.x exposes Lens.FieldOfView (perspective) on the LensSettings struct.
            Assert.That(vcams[0].Lens.FieldOfView, Is.EqualTo(40f).Within(0.001f),
                "CinemachineCamera Lens FieldOfView must be 40 (locked here for Phase 2 follow damping determinism).");
        }

        // ---- BOOT-02 (plan 01-03): UICanvas Screen-Space Camera + SafeAreaPanel ----
        [Test]
        public void UICanvas_IsSafeAreaConfigured()
        {
            GameObject ui = GameObject.Find("UICanvas");
            Assert.That(ui, Is.Not.Null, "UICanvas GameObject missing.");

            var canvas = ui.GetComponent<Canvas>();
            Assert.That(canvas, Is.Not.Null, "UICanvas missing Canvas component.");
            Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceCamera),
                "UICanvas must be Screen Space - Camera (D-15).");

            var scaler = ui.GetComponent<CanvasScaler>();
            Assert.That(scaler, Is.Not.Null, "UICanvas missing CanvasScaler.");
            Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1080, 1920)));
            Assert.That(scaler.matchWidthOrHeight, Is.EqualTo(0.5f).Within(0.001f));

            var safeArea = Object.FindObjectsByType<SafeAreaPanel>(FindObjectsSortMode.None);
            Assert.That(safeArea.Length, Is.GreaterThanOrEqualTo(1),
                "Scene must contain a SafeAreaPanel (expected as child of UICanvas).");
            Assert.That(safeArea[0].transform.parent, Is.EqualTo(ui.transform),
                "SafeAreaPanel must be parented under UICanvas.");
        }

        // ---- BOOT-02 (plan 01-03): EventSystem uses InputSystemUIInputModule ----
        [Test]
        public void EventSystem_UsesInputSystemUIInputModule()
        {
            GameObject es = GameObject.Find("EventSystem");
            Assert.That(es, Is.Not.Null, "EventSystem GameObject missing.");
            Assert.That(es.GetComponent<EventSystem>(), Is.Not.Null,
                "EventSystem GameObject missing EventSystem component.");
            Assert.That(es.GetComponent<InputSystemUIInputModule>(), Is.Not.Null,
                "EventSystem must use InputSystemUIInputModule (D-16, RESEARCH Pitfall 6).");

            // StandaloneInputModule must NOT coexist (RESEARCH Pitfall 6).
            var legacy = es.GetComponent<StandaloneInputModule>();
            Assert.That(legacy, Is.Null,
                "EventSystem must not have a legacy StandaloneInputModule alongside InputSystemUIInputModule.");
        }

        [TearDown]
        public void Cleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }
    }
}
