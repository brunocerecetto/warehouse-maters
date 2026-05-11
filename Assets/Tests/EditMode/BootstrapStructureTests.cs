// Source: VALIDATION.md Wave 0 row 01-01-* + RESEARCH Code Examples.
// Covers BOOT-01 — folder structure, asmdef presence, URP asset.
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace WM.Tests.EditMode
{
    public class BootstrapStructureTests
    {
        // ---- BOOT-01: Assets/_Project/ folder skeleton ----
        private static readonly string[] RequiredFolders =
        {
            "Assets/_Project",
            "Assets/_Project/Art",
            "Assets/_Project/Audio",
            "Assets/_Project/Materials",
            "Assets/_Project/Materials/Placeholder",
            "Assets/_Project/Prefabs",
            "Assets/_Project/Scenes",
            "Assets/_Project/ScriptableObjects",
            "Assets/_Project/ScriptableObjects/Boxes",
            "Assets/_Project/ScriptableObjects/Orders",
            "Assets/_Project/ScriptableObjects/Upgrades",
            "Assets/_Project/ScriptableObjects/Workers",
            "Assets/_Project/ScriptableObjects/Economy",
            "Assets/_Project/Scripts",
            "Assets/_Project/Scripts/Core",
            "Assets/_Project/Scripts/Player",
            "Assets/_Project/Scripts/Boxes",
            "Assets/_Project/Scripts/Orders",
            "Assets/_Project/Scripts/Stations",
            "Assets/_Project/Scripts/Upgrades",
            "Assets/_Project/Scripts/Workers",
            "Assets/_Project/Scripts/Economy",
            "Assets/_Project/Scripts/Save",
            "Assets/_Project/Scripts/Analytics",
            "Assets/_Project/Scripts/Monetization",
            "Assets/_Project/Scripts/UI",
            "Assets/_Project/Scripts/Editor",
            "Assets/_Project/Settings"
        };

        [TestCaseSource(nameof(RequiredFolders))]
        public void AssetsProject_FoldersExist(string path)
        {
            Assert.That(AssetDatabase.IsValidFolder(path), Is.True,
                $"Required _Project folder missing: {path}");
        }

        // ---- BOOT-01: 13 production asmdefs present ----
        private static readonly string[] ProductionAsmdefs =
        {
            "Assets/_Project/Scripts/Core/WM.Core.asmdef",
            "Assets/_Project/Scripts/Player/WM.Player.asmdef",
            "Assets/_Project/Scripts/Boxes/WM.Boxes.asmdef",
            "Assets/_Project/Scripts/Orders/WM.Orders.asmdef",
            "Assets/_Project/Scripts/Stations/WM.Stations.asmdef",
            "Assets/_Project/Scripts/Upgrades/WM.Upgrades.asmdef",
            "Assets/_Project/Scripts/Workers/WM.Workers.asmdef",
            "Assets/_Project/Scripts/Economy/WM.Economy.asmdef",
            "Assets/_Project/Scripts/Save/WM.Save.asmdef",
            "Assets/_Project/Scripts/Analytics/WM.Analytics.asmdef",
            "Assets/_Project/Scripts/Monetization/WM.Monetization.asmdef",
            "Assets/_Project/Scripts/UI/WM.UI.asmdef",
            "Assets/_Project/Scripts/Editor/WM.Editor.asmdef"
        };

        [TestCaseSource(nameof(ProductionAsmdefs))]
        public void AsmdefsPresent(string path)
        {
            Assert.That(File.Exists(path), Is.True,
                $"Required asmdef missing: {path}");
        }

        // ---- BOOT-01: URP_Mobile is the active render pipeline ----
        [Test]
        public void UrpAssetAssigned()
        {
            RenderPipelineAsset rp = GraphicsSettings.defaultRenderPipeline;
            Assert.That(rp, Is.Not.Null, "GraphicsSettings.defaultRenderPipeline is null — URP not assigned");
            Assert.That(rp.name, Does.Contain("URP_Mobile"),
                $"Active render pipeline asset name was '{rp.name}', expected to contain 'URP_Mobile'");
        }
    }
}
