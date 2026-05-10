// Source: Plan 01-02 (Warehouse_MVP scene composition).
// Phase 1 / Plan 02 Editor-only builder: programmatically creates the 7 placeholder
// URP/Lit materials, composes Warehouse_MVP.unity with all 12 required GameObjects
// + Floor, wires Bootstrap.gameManager → GameManager, and registers the scene at
// EditorBuildSettings index 0.
//
// Invoked headlessly:
//   <UNITY> -batchmode -quit -nographics -projectPath . \
//           -executeMethod WM.Editor.Phase01SceneBuilder.Build -logFile -
//
// Also available as Tools/Phase01/Build Warehouse_MVP Scene menu item for manual rebuilds.
// Idempotent: replaces existing materials/scene at known paths.

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WM.Core;

namespace WM.Editor
{
    public static class Phase01SceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Warehouse_MVP.unity";
        private const string MaterialsDir = "Assets/_Project/Materials/Placeholder";
        private const string UrpLitShaderName = "Universal Render Pipeline/Lit";

        // Plan 01-02 Task 2 color palette (D-13).
        private struct MatSpec
        {
            public string FileName;
            public Color Color;
            public MatSpec(string fileName, Color color) { FileName = fileName; Color = color; }
        }

        private static readonly MatSpec[] Materials =
        {
            new MatSpec("Mat_Floor",          Hex("#9A9A9A")),
            new MatSpec("Mat_LoadingDock",    Hex("#6E6E6E")),
            new MatSpec("Mat_PackingStation", Hex("#2D7FFF")),
            new MatSpec("Mat_DeliveryZone",   Hex("#3CB371")),
            new MatSpec("Mat_UpgradeStation", Hex("#FFD43B")),
            new MatSpec("Mat_Shelf",          Hex("#8B5A2B")),
            new MatSpec("Mat_WorkerSpawn",    Hex("#FF00FF"))
        };

        [MenuItem("Tools/Phase01/Build Warehouse_MVP Scene")]
        public static void Build()
        {
            EnsureMaterialsDir();
            CreatePlaceholderMaterials();
            BuildScene();
            RegisterSceneInBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase01SceneBuilder] Build complete.");
        }

        private static void EnsureMaterialsDir()
        {
            if (!AssetDatabase.IsValidFolder(MaterialsDir))
            {
                Directory.CreateDirectory(MaterialsDir);
                AssetDatabase.Refresh();
            }
        }

        private static void CreatePlaceholderMaterials()
        {
            Shader urpLit = Shader.Find(UrpLitShaderName);
            if (urpLit == null)
            {
                throw new System.InvalidOperationException(
                    $"[Phase01SceneBuilder] Shader '{UrpLitShaderName}' not found. URP is missing or not yet imported.");
            }

            foreach (MatSpec spec in Materials)
            {
                string path = $"{MaterialsDir}/{spec.FileName}.mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                {
                    mat = new Material(urpLit);
                    AssetDatabase.CreateAsset(mat, path);
                }
                else
                {
                    mat.shader = urpLit;
                }
                // URP/Lit accepts both legacy _Color and modern _BaseColor; set both for safety.
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", spec.Color);
                if (mat.HasProperty("_Color"))     mat.SetColor("_Color", spec.Color);
                EditorUtility.SetDirty(mat);
            }
        }

        private static void BuildScene()
        {
            // Start from an empty scene.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera + light so the scene is visually viable in the Editor.
            GameObject cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            Camera cameraComp = cam.AddComponent<Camera>();
            cameraComp.clearFlags = CameraClearFlags.Skybox;
            cam.transform.position = new Vector3(0, 15, -15);
            cam.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            cam.AddComponent<AudioListener>();

            GameObject light = new GameObject("Directional Light");
            Light lt = light.AddComponent<Light>();
            lt.type = LightType.Directional;
            lt.intensity = 1f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Floor
            CreateStation(
                name: "Floor",
                prim: PrimitiveType.Plane,
                position: new Vector3(0, 0, 0),
                scale: new Vector3(3, 1, 3),
                materialName: "Mat_Floor");

            // 6 stations
            CreateStation("LoadingDock",    PrimitiveType.Cube,     new Vector3(-8, 0.75f,  6), new Vector3(4f,   1.5f, 3f),  "Mat_LoadingDock");
            CreateStation("PackingStation", PrimitiveType.Cube,     new Vector3( 0, 0.5f,   0), new Vector3(2f,   1f,   2f),  "Mat_PackingStation");
            CreateStation("DeliveryZone",   PrimitiveType.Cube,     new Vector3( 8, 0.25f,  6), new Vector3(4f,   0.5f, 3f),  "Mat_DeliveryZone");
            CreateStation("UpgradeStation", PrimitiveType.Cube,     new Vector3( 8, 0.5f,  -4), new Vector3(1.5f, 1f,   1.5f),"Mat_UpgradeStation");
            CreateStation("ShelfArea",      PrimitiveType.Cube,     new Vector3(-8, 1f,    -4), new Vector3(5f,   2f,   1.5f),"Mat_Shelf");
            CreateStation("WorkerSpawn",    PrimitiveType.Cylinder, new Vector3(-8, 0.05f,  4), new Vector3(0.5f, 0.05f,0.5f),"Mat_WorkerSpawn");

            // Player placeholder
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0, 1, -3);
            player.transform.localScale = new Vector3(0.6f, 1f, 0.6f);
            // Ensure a CapsuleCollider exists (CreatePrimitive adds one by default) and add Rigidbody.
            if (player.GetComponent<CapsuleCollider>() == null) player.AddComponent<CapsuleCollider>();
            Rigidbody rb = player.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;

            // Empty placeholders for plan 01-03
            CreateEmpty("CameraRig",  Vector3.zero);
            CreateEmpty("UICanvas",   Vector3.zero);
            CreateEmpty("EventSystem",Vector3.zero);

            // GameManager + Bootstrap (and Inspector wiring)
            GameObject gmGo = CreateEmpty("GameManager", Vector3.zero);
            GameManager gm = gmGo.AddComponent<GameManager>();

            GameObject bootGo = CreateEmpty("Bootstrap", Vector3.zero);
            Bootstrap boot = bootGo.AddComponent<Bootstrap>();

            // Wire Bootstrap.gameManager via SerializedObject (private [SerializeField] field).
            SerializedObject so = new SerializedObject(boot);
            SerializedProperty prop = so.FindProperty("gameManager");
            if (prop == null)
            {
                throw new System.InvalidOperationException(
                    "[Phase01SceneBuilder] SerializedProperty 'gameManager' not found on Bootstrap component.");
            }
            prop.objectReferenceValue = gm;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Save scene under known path.
            string sceneDir = Path.GetDirectoryName(ScenePath);
            if (!string.IsNullOrEmpty(sceneDir) && !Directory.Exists(sceneDir))
            {
                Directory.CreateDirectory(sceneDir);
                AssetDatabase.Refresh();
            }
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            if (!saved)
            {
                throw new System.InvalidOperationException(
                    $"[Phase01SceneBuilder] Failed to save scene at {ScenePath}.");
            }
        }

        private static GameObject CreateStation(
            string name, PrimitiveType prim, Vector3 position, Vector3 scale, string materialName)
        {
            GameObject go = GameObject.CreatePrimitive(prim);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;

            string matPath = $"{MaterialsDir}/{materialName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Debug.LogWarning($"[Phase01SceneBuilder] Material not found at {matPath} when assigning to {name}.");
            }
            else
            {
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = mat;
            }
            return go;
        }

        private static GameObject CreateEmpty(string name, Vector3 position)
        {
            GameObject go = new GameObject(name);
            go.transform.position = position;
            return go;
        }

        private static void RegisterSceneInBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }

        private static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c)) return c;
            return Color.magenta;
        }
    }
}
