/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure
{
    internal static class VrseStoryMutation
    {
        private static Type StoryType => VrseTypeResolver.FindType("VRseBuilder.Core.Framework.Story") ?? throw Missing("Story");
        private static Type ChapterType => VrseTypeResolver.FindType("VRseBuilder.Core.Framework.Chapter") ?? throw Missing("Chapter");
        private static Type MomentType => VrseTypeResolver.FindType("VRseBuilder.Core.Framework.Moment") ?? throw Missing("Moment");
        private static Type NodeType => VrseTypeResolver.FindType("VRseBuilder.Core.Framework.Node") ?? throw Missing("Node");
        private static Type ActionSetType => VrseTypeResolver.FindType("VRseBuilder.Core.Framework.ActionSet") ?? throw Missing("ActionSet");
        private static Type TriggerActionSetType => VrseTypeResolver.FindType("VRseBuilder.Core.Framework.TriggerActionSet") ?? throw Missing("TriggerActionSet");
        private static Type TriggerActionSetsWithModeType => VrseTypeResolver.FindType("VRseBuilder.Core.Framework.TriggerActionSetsWithMode") ?? throw Missing("TriggerActionSetsWithMode");

        public static object CreateChapter(string name)
        {
            object chapter = Activator.CreateInstance(ChapterType) ?? throw Missing("Chapter instance");
            VrseReflection.SetMemberValue(chapter, "name", name);
            VrseReflection.SetMemberValue(chapter, "moments", Array.CreateInstance(MomentType, 0));
            return chapter;
        }

        public static object CreateMoment(string name)
        {
            object moment = Activator.CreateInstance(MomentType) ?? throw Missing("Moment instance");
            VrseReflection.SetMemberValue(moment, "name", name);
            return moment;
        }

        public static object CreateDefaultNode(bool isAction)
        {
            object node = Activator.CreateInstance(NodeType) ?? throw Missing("Node instance");
            VrseReflection.SetMemberValue(node, "Name", isAction ? "Objects" : "GrabbableTrigger");
            VrseReflection.SetMemberValue(node, "Type", (byte)(isAction ? 0 : 1));
            VrseReflection.SetMemberValue(node, "Query", string.Empty);
            VrseReflection.SetMemberValue(node, "Option", isAction ? "Spawn" : string.Empty);
            VrseReflection.SetMemberValue(node, "Data", "{}");
            return node;
        }

        public static object GetOrCreateActionSet(object moment, string section)
        {
            object? actionSet = VrseReflection.GetMemberValue(moment, section);
            if (actionSet != null)
                return actionSet;

            actionSet = Activator.CreateInstance(ActionSetType) ?? throw Missing("ActionSet instance");
            VrseReflection.SetMemberValue(actionSet, "actions", Array.CreateInstance(NodeType, 0));
            VrseReflection.SetMemberValue(moment, section, actionSet);
            return actionSet;
        }

        public static bool IsSimpleActionSection(string section)
        {
            return section == "onAwake" ||
                   section == "onStart" ||
                   section == "onFirstWarning" ||
                   section == "onLastWarning" ||
                   section == "onEnd";
        }

        public static object GetMoment(object story, int chapterIndex, int momentIndex, out object chapter)
        {
            List<object> chapters = VrseStoryDiscovery.GetChapters(story);
            if (chapterIndex < 0 || chapterIndex >= chapters.Count)
                throw new ArgumentOutOfRangeException(nameof(chapterIndex), $"chapterIndex must be within 0-{chapters.Count - 1}.");

            chapter = chapters[chapterIndex];
            List<object> moments = VrseStoryDiscovery.GetMoments(chapter);
            if (momentIndex < 0 || momentIndex >= moments.Count)
                throw new ArgumentOutOfRangeException(nameof(momentIndex), $"momentIndex must be within 0-{moments.Count - 1}.");

            return moments[momentIndex];
        }

        public static List<object> GetActionNodes(object moment, string section, int triggerSetIndex)
        {
            if (IsSimpleActionSection(section))
                return VrseStoryDiscovery.GetNodesFromActionSet(GetOrCreateActionSet(moment, section));

            object triggerActionSet = GetTriggerActionSet(moment, section, triggerSetIndex);
            return VrseStoryDiscovery.GetNodesFromActionSet(triggerActionSet);
        }

        public static void SetActionNodes(object moment, string section, int triggerSetIndex, List<object> nodes)
        {
            Array array = VrseReflection.ToTypedArray(NodeType, nodes);
            if (IsSimpleActionSection(section))
            {
                VrseReflection.SetMemberValue(GetOrCreateActionSet(moment, section), "actions", array);
                return;
            }

            VrseReflection.SetMemberValue(GetTriggerActionSet(moment, section, triggerSetIndex), "actions", array);
        }

        public static object GetTriggerActionSet(object moment, string section, int triggerSetIndex)
        {
            List<object> sets = GetTriggerActionSetsForSection(moment, section);
            if (triggerSetIndex < 0 || triggerSetIndex >= sets.Count)
                throw new ArgumentOutOfRangeException(nameof(triggerSetIndex), $"triggerSetIndex must be within 0-{sets.Count - 1} for {section}.");

            return sets[triggerSetIndex];
        }

        public static List<object> GetTriggerActionSetsForSection(object moment, string section)
        {
            if (section == "onWrong")
                return VrseStoryDiscovery.GetTriggerActionSets(VrseReflection.GetMemberValue(moment, "onWrong"));

            if (section == "onRight")
            {
                object? onRight = VrseReflection.GetMemberValue(moment, "onRight");
                return VrseStoryDiscovery.GetTriggerActionSets(VrseReflection.GetMemberValue(onRight, "triggerActionSets"));
            }

            throw new ArgumentException("section must be onWrong or onRight.", nameof(section));
        }

        public static void SetTriggerActionSetsForSection(object moment, string section, List<object> sets, string mode)
        {
            Array array = VrseReflection.ToTypedArray(TriggerActionSetType, sets);
            if (section == "onWrong")
            {
                VrseReflection.SetMemberValue(moment, "onWrong", array);
                return;
            }

            if (section == "onRight")
            {
                object? onRight = VrseReflection.GetMemberValue(moment, "onRight");
                if (onRight == null)
                {
                    onRight = Activator.CreateInstance(TriggerActionSetsWithModeType) ?? throw Missing("TriggerActionSetsWithMode instance");
                    VrseReflection.SetMemberValue(moment, "onRight", onRight);
                }

                if (string.IsNullOrEmpty(VrseReflection.GetString(onRight, "mode")))
                    VrseReflection.SetMemberValue(onRight, "mode", string.IsNullOrEmpty(mode) ? "InOrder" : mode);
                VrseReflection.SetMemberValue(onRight, "triggerActionSets", array);
                return;
            }

            throw new ArgumentException("section must be onWrong or onRight.", nameof(section));
        }

        public static object CreateTriggerActionSet()
        {
            object set = Activator.CreateInstance(TriggerActionSetType) ?? throw Missing("TriggerActionSet instance");
            VrseReflection.SetMemberValue(set, "trigger", CreateDefaultNode(isAction: false));
            VrseReflection.SetMemberValue(set, "actions", Array.CreateInstance(NodeType, 0));
            return set;
        }

        public static void SetChapters(object story, List<object> chapters)
        {
            VrseReflection.SetMemberValue(story, "chapters", VrseReflection.ToTypedArray(ChapterType, chapters));
        }

        public static void SetMoments(object chapter, List<object> moments)
        {
            VrseReflection.SetMemberValue(chapter, "moments", VrseReflection.ToTypedArray(MomentType, moments));
        }

        public static void MarkStoryChanged(object storyCreator)
        {
            object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
            VrseReflection.InvokeInstance(story, "AssignChapterAndMomentIndex");
            VrseReflection.InvokeInstance(storyCreator, "InvalidateIsStorySavedToFileCache");

            Type? referenceManagerType = VrseTypeResolver.FindType("VRseBuilder.Core.Framework.ReferenceManager");
            if (referenceManagerType != null)
            {
                object? referenceManager = Resources.FindObjectsOfTypeAll(referenceManagerType)
                    .Cast<object>()
                    .FirstOrDefault(obj => obj is Component component && component.gameObject.scene.IsValid() && component.gameObject.scene.isLoaded);
                VrseReflection.InvokeInstance(referenceManager, "OnStoryChangedFromInspector");
            }

            if (storyCreator is UnityEngine.Object unityObject)
                EditorUtility.SetDirty(unityObject);
            if (storyCreator is Component component && component.gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
        }

        private static InvalidOperationException Missing(string typeName)
        {
            return new InvalidOperationException($"VRseBuilder {typeName} type was not found. Ensure the VRseBuilder SDK is installed and compiled.");
        }
    }
}
