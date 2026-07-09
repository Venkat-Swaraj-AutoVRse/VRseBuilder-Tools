/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using UnityEditor;
using UnityEngine;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure
{
    internal static class VrseStoryDiscovery
    {
        public static object FindStoryCreator(string? storyCreatorName = null)
        {
            Type storyCreatorType = VrseTypeResolver.FindType("VRseBuilder.Core.Framework.StoryCreator")
                ?? throw new InvalidOperationException("StoryCreator type was not found. Ensure the VRseBuilder SDK is installed and compiled.");

            List<object> storyCreators = VrseReflection.FindSceneObjectsOfType(storyCreatorType);
            if (!string.IsNullOrWhiteSpace(storyCreatorName))
            {
                object? named = storyCreators.FirstOrDefault(candidate =>
                    candidate is Component component && string.Equals(component.gameObject.name, storyCreatorName.Trim(), StringComparison.OrdinalIgnoreCase));
                if (named != null)
                    return named;
            }

            return storyCreators.FirstOrDefault()
                ?? throw new InvalidOperationException("No StoryCreator found in the loaded scenes.");
        }

        public static object GetLoadedStory(object storyCreator)
        {
            return VrseReflection.GetMemberValue(storyCreator, "_story")
                ?? throw new InvalidOperationException("The active StoryCreator does not have a story loaded.");
        }

        public static string GetStoryCreatorName(object storyCreator)
        {
            return storyCreator is Component component ? component.gameObject.name : string.Empty;
        }

        public static object LoadNodeTemplatesData()
        {
            string[] guids = AssetDatabase.FindAssets("t:NodeTemplatesData");
            if (guids == null || guids.Length == 0)
                throw new InvalidOperationException("NodeTemplatesData ScriptableObject was not found in the project.");

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path)
                ?? throw new InvalidOperationException($"Failed to load NodeTemplatesData at '{path}'.");

            VrseReflection.InvokeInstance(asset, "OnValidate");
            return asset;
        }

        public static List<object> GetChapters(object story)
        {
            return VrseReflection.AsObjectList(VrseReflection.GetMemberValue(story, "chapters"));
        }

        public static List<object> GetMoments(object chapter)
        {
            return VrseReflection.AsObjectList(VrseReflection.GetMemberValue(chapter, "moments"));
        }

        public static List<object> GetNodesFromActionSet(object? actionSet)
        {
            return VrseReflection.AsObjectList(VrseReflection.GetMemberValue(actionSet, "actions"));
        }

        public static List<object> GetTriggerActionSets(object? value)
        {
            return VrseReflection.AsObjectList(value);
        }

        public static Dictionary<object, (int chapterIndex, string chapterName, int momentIndex, string momentName)> BuildMomentLookup(object story)
        {
            var lookup = new Dictionary<object, (int, string, int, string)>();
            List<object> chapters = GetChapters(story);
            for (int ci = 0; ci < chapters.Count; ci++)
            {
                object chapter = chapters[ci];
                string chapterName = VrseReflection.GetString(chapter, "name");
                List<object> moments = GetMoments(chapter);
                for (int mi = 0; mi < moments.Count; mi++)
                {
                    object moment = moments[mi];
                    lookup[moment] = (ci, chapterName, mi, VrseReflection.GetString(moment, "name"));
                }
            }

            return lookup;
        }

        public static List<object> DictionaryKeys(object? dictionary)
        {
            if (dictionary is IDictionary idictionary)
                return idictionary.Keys.Cast<object>().ToList();

            return new List<object>();
        }
    }
}
