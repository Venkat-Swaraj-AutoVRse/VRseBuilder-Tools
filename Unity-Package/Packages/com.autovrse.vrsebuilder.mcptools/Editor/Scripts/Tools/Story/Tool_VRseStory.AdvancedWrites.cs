/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using System.ComponentModel;
using System.IO;
using com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Story
{
    public partial class Tool_VRseStory
    {
        [AiTool("vrse-story-apply-json", Title = "VRseBuilder / Story / Apply JSON")]
        [Description("Replace the active StoryCreator story with a full Story JSON payload. Creates a backup first when possible.")]
        public VrseStoryApplyJsonResult ApplyJson(string json, string? storyCreatorName = null)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("json is required.", nameof(json));

            return MainThread.Instance.Run(() => ApplyJsonInternal(json, storyCreatorName, createBackupReason: "MCP Apply Story JSON"));
        }

        [AiTool("vrse-story-patch", Title = "VRseBuilder / Story / Patch")]
        [Description("Merge a JSON patch object into the current Story JSON when Newtonsoft.Json is available. Arrays are replaced by Newtonsoft's default merge behavior.")]
        public VrseStoryApplyJsonResult Patch(string patch, string? storyCreatorName = null)
        {
            if (string.IsNullOrWhiteSpace(patch))
                throw new ArgumentException("patch is required.", nameof(patch));

            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                string currentJson = JsonUtility.ToJson(story);
                string mergedJson = MergeJsonWithNewtonsoft(currentJson, patch);
                return ApplyJsonToStoryCreator(storyCreator, mergedJson, "MCP Patch Story");
            });
        }

        [AiTool("vrse-story-undo-write", Title = "VRseBuilder / Story / Undo Write")]
        [Description("Restore the most recent story backup and reload the StoryCreator.")]
        public VrseStoryBackupResult UndoWrite(string? storyCreatorName = null)
        {
            return RestoreStoryBackup(backupIndex: 0, backupPath: null, storyCreatorName: storyCreatorName);
        }

        private static VrseStoryApplyJsonResult ApplyJsonInternal(string json, string? storyCreatorName, string createBackupReason)
        {
            object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
            return ApplyJsonToStoryCreator(storyCreator, json, createBackupReason);
        }

        private static VrseStoryApplyJsonResult ApplyJsonToStoryCreator(object storyCreator, string json, string createBackupReason)
        {
            string filePath = VrseReflection.GetString(storyCreator, "_FilePath");
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                CreateBackup(filePath, createBackupReason);

            object story = JsonUtility.FromJson(json, VrseStoryMutation.GetStoryType())
                ?? throw new InvalidOperationException("Failed to deserialize story JSON.");
            VrseReflection.SetMemberValue(storyCreator, "_story", story);
            VrseStoryMutation.MarkStoryChanged(storyCreator);
            AssetDatabase.Refresh();

            return new VrseStoryApplyJsonResult
            {
                Success = true,
                StoryCreator = VrseStoryDiscovery.GetStoryCreatorName(storyCreator),
                ChapterCount = VrseStoryDiscovery.GetChapters(story).Count
            };
        }

        private static string MergeJsonWithNewtonsoft(string currentJson, string patchJson)
        {
            Type jObjectType = VrseTypeResolver.FindType("Newtonsoft.Json.Linq.JObject")
                ?? throw new InvalidOperationException("Newtonsoft.Json.Linq.JObject was not found. Install/enable Newtonsoft.Json or use vrse-story-apply-json with a complete Story JSON payload.");

            object current = VrseReflection.InvokeStatic(jObjectType, "Parse", currentJson)
                ?? throw new InvalidOperationException("Failed to parse current story JSON.");
            object patch = VrseReflection.InvokeStatic(jObjectType, "Parse", patchJson)
                ?? throw new InvalidOperationException("Failed to parse patch JSON.");
            VrseReflection.InvokeInstance(current, "Merge", patch);
            return current.ToString() ?? currentJson;
        }
    }
}
