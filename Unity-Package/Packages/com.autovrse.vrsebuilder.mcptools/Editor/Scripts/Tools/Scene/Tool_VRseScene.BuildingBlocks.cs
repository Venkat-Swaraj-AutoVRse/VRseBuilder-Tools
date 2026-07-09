/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Scene
{
    public partial class Tool_VRseScene
    {
        [AiTool("vrse-scene-building-blocks-list", Title = "VRseBuilder / Scene / Building Blocks List")]
        [Description("List VRseBuilder building blocks from the BuildingBlocks/VRseBlocks collection asset.")]
        public VrseBuildingBlockListResult BuildingBlocksList()
        {
            return MainThread.Instance.Run(() =>
            {
                object collection = LoadBuildingBlocksCollection();
                var blocks = VrseReflection.AsObjectList(VrseReflection.GetMemberValue(collection, "Blocks"))
                    .Select(block => new VrseBuildingBlockInfo
                    {
                        BlockName = VrseReflection.GetString(block, "BlockName"),
                        Description = VrseReflection.GetString(block, "Description"),
                        Category = VrseReflection.GetString(block, "Category"),
                        Tags = VrseReflection.AsObjectList(VrseReflection.GetMemberValue(block, "Tags")).Select(item => item.ToString() ?? string.Empty).Where(item => !string.IsNullOrEmpty(item)).ToList(),
                        ActionName = VrseReflection.GetString(block, "ActionName"),
                        TriggerName = VrseReflection.GetString(block, "TriggerName")
                    })
                    .ToList();

                return new VrseBuildingBlockListResult { Success = true, Count = blocks.Count, Blocks = blocks };
            });
        }

        [AiTool("vrse-scene-building-block-instantiate", Title = "VRseBuilder / Scene / Building Block Instantiate")]
        [Description("Instantiate a VRseBuilder building block prefab into the current scene.")]
        public VrseInstantiateBuildingBlockResult BuildingBlocksInstantiate(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName))
                throw new ArgumentException("blockName is required.", nameof(blockName));

            return MainThread.Instance.Run(() =>
            {
                object collection = LoadBuildingBlocksCollection();
                object block = VrseReflection.AsObjectList(VrseReflection.GetMemberValue(collection, "Blocks"))
                    .FirstOrDefault(candidate => string.Equals(VrseReflection.GetString(candidate, "BlockName"), blockName.Trim(), StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidOperationException($"Building block '{blockName}' was not found.");

                GameObject prefab = VrseReflection.GetMemberValue(block, "BlockPrefab") as GameObject
                    ?? throw new InvalidOperationException($"Building block '{blockName}' has no prefab assigned.");
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject
                    ?? throw new InvalidOperationException($"Failed to instantiate building block '{blockName}'.");

                if (SceneView.lastActiveSceneView != null)
                    instance.transform.position = SceneView.lastActiveSceneView.pivot;

                Selection.activeGameObject = instance;
                Undo.RegisterCreatedObjectUndo(instance, $"Instantiate Building Block: {blockName}");
                return new VrseInstantiateBuildingBlockResult { Success = true, InstanceName = instance.name, InstanceId = instance.GetInstanceID() };
            });
        }

        private static object LoadBuildingBlocksCollection()
        {
            string[] guids = AssetDatabase.FindAssets("t:VRseBlocksCollection");
            if (guids.Length == 0)
                guids = AssetDatabase.FindAssets("t:BuildingBlocksCollection");
            if (guids.Length == 0)
                throw new InvalidOperationException("No VRseBuilder building blocks collection was found in the project.");

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path)
                ?? throw new InvalidOperationException($"Failed to load building blocks collection at '{path}'.");
        }
    }
}
