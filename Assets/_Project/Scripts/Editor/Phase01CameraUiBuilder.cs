// Source: Plan 01-03 (CameraRig Cinemachine 3.x + UICanvas + EventSystem).
// Phase 1 / Plan 03 Editor-only builder: idempotently mutates Warehouse_MVP.unity
// to (a) re-parent Main Camera under CameraRig and attach a CinemachineBrain;
// (b) create CM vcam1 under CameraRig with a passive CinemachineCamera (no
// tracking target, Perspective FOV 40); (c) delete the empty UICanvas + EventSystem
// placeholders from plan 01-02 and recreate them with real Canvas (Screen-Space
// Camera, 1080x1920, Match=0.5) + SafeAreaPanel child + EventSystem with
// InputSystemUIInputModule.
//
// Invoked headlessly:
//   <UNITY> -batchmode -quit -nographics -projectPath . \
//           -executeMethod WM.Editor.Phase01CameraUiBuilder.Build -logFile -
//
// Also available as Tools/Phase01/Build Camera+UI (Plan 01-03) menu item.
// Idempotent: deletes prior CameraRig children + UICanvas + EventSystem before re-creating.

using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WM.UI;

namespace WM.Editor
{
    public static class Phase01CameraUiBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Warehouse_MVP.unity";

        // Plan 01-03 Task 2 Step A: isometric vcam pose (RESEARCH Pattern 3 defaults).
        private static readonly Vector3 VcamPosition = new Vector3(10f, 12f, -10f);
        private static readonly Vector3 VcamEuler = new Vector3(45f, -45f, 0f);
        private const float VcamFieldOfView = 40f; // Locked by D-12 + plan 01-03.

        // UICanvas reference resolution + match (D-15).
        private static readonly Vector2 CanvasReferenceResolution = new Vector2(1080f, 1920f);
        private const float CanvasMatchWidthOrHeight = 0.5f;

        [MenuItem("Tools/Phase01/Build Camera+UI (Plan 01-03)")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject cameraRig = FindRootGameObject(scene, "CameraRig");
            if (cameraRig == null)
            {
                throw new System.InvalidOperationException(
                    $"[Phase01CameraUiBuilder] CameraRig root GameObject missing from {ScenePath} — run Phase01SceneBuilder.Build first.");
            }

            BuildCameraRig(cameraRig);
            BuildUICanvas(scene);
            BuildEventSystem(scene);

            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            if (!saved)
            {
                throw new System.InvalidOperationException(
                    $"[Phase01CameraUiBuilder] Failed to save scene at {ScenePath}.");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase01CameraUiBuilder] Build complete.");
        }

        private static void BuildCameraRig(GameObject cameraRig)
        {
            // Idempotency: remove any prior children authored by previous runs.
            DestroyAllChildren(cameraRig);

            // Locate or create the Main Camera. The scene from plan 01-02 has it at root.
            GameObject mainCam = GameObject.Find("Main Camera");
            if (mainCam == null)
            {
                mainCam = new GameObject("Main Camera");
                mainCam.tag = "MainCamera";
                Camera cam = mainCam.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.Skybox;
            }
            mainCam.name = "Main Camera";
            mainCam.tag = "MainCamera";

            // Re-parent under CameraRig (preserves world transform so Brain can drive it).
            mainCam.transform.SetParent(cameraRig.transform, worldPositionStays: true);

            // Ensure Camera + AudioListener.
            Camera cameraComp = mainCam.GetComponent<Camera>();
            if (cameraComp == null) cameraComp = mainCam.AddComponent<Camera>();
            cameraComp.clearFlags = CameraClearFlags.Skybox;
            if (mainCam.GetComponent<AudioListener>() == null)
                mainCam.AddComponent<AudioListener>();

            // Ensure CinemachineBrain (Pitfall 2: namespace Unity.Cinemachine).
            if (mainCam.GetComponent<CinemachineBrain>() == null)
                mainCam.AddComponent<CinemachineBrain>();

            // Create CM vcam1 as a child of CameraRig (sibling of Main Camera).
            GameObject vcamGo = new GameObject("CM vcam1");
            vcamGo.transform.SetParent(cameraRig.transform, worldPositionStays: false);
            vcamGo.transform.localPosition = VcamPosition;
            vcamGo.transform.localRotation = Quaternion.Euler(VcamEuler);

            CinemachineCamera vcam = vcamGo.AddComponent<CinemachineCamera>();
            // Passive vcam: Tracking Target left null (D-12). Phase 2 will set it.
            vcam.Target = default;

            // Lens: Perspective, FOV 40 (locked here for cross-phase determinism).
            LensSettings lens = LensSettings.Default;
            lens.ModeOverride = LensSettings.OverrideModes.Perspective;
            lens.FieldOfView = VcamFieldOfView;
            vcam.Lens = lens;
        }

        private static void BuildUICanvas(Scene scene)
        {
            // Step B-1: delete the empty UICanvas placeholder from plan 01-02 (must-have truth #5).
            // Plan 01-02 left UICanvas as a bare GameObject with only a Transform. Remove every
            // root GameObject named "UICanvas" to guarantee one-of-a-kind name semantics.
            DeleteAllRootGameObjectsNamed(scene, "UICanvas");

            // Step B-2: recreate UICanvas as a real Canvas + CanvasScaler + GraphicRaycaster.
            GameObject uiGo = new GameObject(
                "UICanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            Canvas canvas = uiGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 1f;
            canvas.sortingLayerID = 0; // Default
            canvas.sortingOrder = 0;

            CanvasScaler scaler = uiGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = CanvasReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = CanvasMatchWidthOrHeight;
            scaler.referencePixelsPerUnit = 100f;

            // Step B-3: SafeAreaPanel as a child of UICanvas, stretched 0..1.
            GameObject safeGo = new GameObject("SafeAreaPanel", typeof(RectTransform));
            safeGo.transform.SetParent(uiGo.transform, worldPositionStays: false);

            RectTransform srt = safeGo.GetComponent<RectTransform>();
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = Vector2.one;
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            srt.localScale = Vector3.one;
            srt.localPosition = Vector3.zero;

            safeGo.AddComponent<SafeAreaPanel>();
        }

        private static void BuildEventSystem(Scene scene)
        {
            // Step C: remove every EventSystem root GameObject (placeholder + any auto-added duplicates).
            DeleteAllRootGameObjectsNamed(scene, "EventSystem");

            GameObject esGo = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));

            // Sanity: explicitly forbid a coexisting legacy StandaloneInputModule (RESEARCH Pitfall 6).
            StandaloneInputModule legacy = esGo.GetComponent<StandaloneInputModule>();
            if (legacy != null)
            {
                Object.DestroyImmediate(legacy);
            }
        }

        // ---- Helpers ----

        private static GameObject FindRootGameObject(Scene scene, string name)
        {
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                if (go.name == name) return go;
            }
            return null;
        }

        private static void DeleteAllRootGameObjectsNamed(Scene scene, string name)
        {
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                if (go.name == name)
                {
                    Object.DestroyImmediate(go);
                }
            }
        }

        private static void DestroyAllChildren(GameObject parent)
        {
            // Iterate over a snapshot — DestroyImmediate mutates the child list.
            var children = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                children.Add(parent.transform.GetChild(i).gameObject);
            }
            foreach (GameObject child in children)
            {
                Object.DestroyImmediate(child);
            }
        }
    }
}
