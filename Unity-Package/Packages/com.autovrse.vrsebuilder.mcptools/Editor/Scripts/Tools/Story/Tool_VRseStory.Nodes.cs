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
using com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Story
{
    public partial class Tool_VRseStory
    {
        [AiTool("vrse-story-add-trigger-set", Title = "VRseBuilder / Story / Add Trigger Set")]
        [Description("Add a trigger/action set to an onWrong or onRight moment section.")]
        public VrseStoryTriggerSetEditResult AddTriggerSet(int chapterIndex, int momentIndex, string section, string mode = "InOrder", string? storyCreatorName = null)
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                object moment = VrseStoryMutation.GetMoment(story, chapterIndex, momentIndex, out _);
                string normalizedSection = VrseStorySerializer.NormalizeSectionName(section);
                if (normalizedSection != "onWrong" && normalizedSection != "onRight")
                    throw new ArgumentException("section must be onWrong or onRight.", nameof(section));

                List<object> sets = VrseStoryMutation.GetTriggerActionSetsForSection(moment, normalizedSection);
                object newSet = VrseStoryMutation.CreateTriggerActionSet();
                sets.Add(newSet);
                VrseStoryMutation.SetTriggerActionSetsForSection(moment, normalizedSection, sets, mode);
                VrseStoryMutation.MarkStoryChanged(storyCreator);

                return new VrseStoryTriggerSetEditResult
                {
                    Success = true,
                    ChapterIndex = chapterIndex,
                    MomentIndex = momentIndex,
                    Section = normalizedSection,
                    TriggerSetIndex = sets.Count - 1,
                    Trigger = VrseStorySerializer.SerializeNode(VrseReflection.GetMemberValue(newSet, "trigger")),
                    ActionCount = 0
                };
            });
        }

        [AiTool("vrse-story-add-action", Title = "VRseBuilder / Story / Add Action")]
        [Description("Add an action node to a moment section.")]
        public VrseStoryNodeEditResult AddAction(int chapterIndex, int momentIndex, string section, int triggerSetIndex = -1, string? storyCreatorName = null)
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                object moment = VrseStoryMutation.GetMoment(story, chapterIndex, momentIndex, out _);
                string normalizedSection = VrseStorySerializer.NormalizeSectionName(section);
                object newNode = VrseStoryMutation.CreateDefaultNode(isAction: true);
                List<object> nodes = VrseStoryMutation.GetActionNodes(moment, normalizedSection, triggerSetIndex);
                nodes.Add(newNode);
                VrseStoryMutation.SetActionNodes(moment, normalizedSection, triggerSetIndex, nodes);
                VrseStoryMutation.MarkStoryChanged(storyCreator);

                return new VrseStoryNodeEditResult
                {
                    Success = true,
                    ChapterIndex = chapterIndex,
                    MomentIndex = momentIndex,
                    Section = normalizedSection,
                    TriggerSetIndex = triggerSetIndex,
                    NodeIndex = nodes.Count - 1,
                    NodeKind = "action",
                    Node = VrseStorySerializer.SerializeNode(newNode)
                };
            });
        }

        [AiTool("vrse-story-update-node", Title = "VRseBuilder / Story / Update Node")]
        [Description("Update an action or trigger node in a moment section.")]
        public VrseStoryNodeEditResult UpdateNode(
            int chapterIndex,
            int momentIndex,
            string section,
            int nodeIndex = -1,
            int triggerSetIndex = -1,
            string nodeKind = "action",
            string? name = null,
            string? option = null,
            string? query = null,
            string? data = null,
            string? type = null,
            string? targetGameObjectPath = null,
            bool clearTargetGameObject = false,
            string? storyCreatorName = null)
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                object moment = VrseStoryMutation.GetMoment(story, chapterIndex, momentIndex, out _);
                string normalizedSection = VrseStorySerializer.NormalizeSectionName(section);
                string normalizedKind = string.IsNullOrWhiteSpace(nodeKind) ? "action" : nodeKind.Trim().ToLowerInvariant();
                object node = ResolveNode(moment, normalizedSection, normalizedKind, triggerSetIndex, nodeIndex);

                if (name != null) VrseReflection.SetMemberValue(node, "Name", name);
                if (option != null) VrseReflection.SetMemberValue(node, "Option", option);
                if (query != null) VrseReflection.SetMemberValue(node, "Query", query);
                if (data != null) VrseReflection.SetMemberValue(node, "Data", data);
                if (type != null) VrseReflection.SetMemberValue(node, "Type", ParseNodeType(type, VrseReflection.GetInt(node, "Type")));

                if (clearTargetGameObject)
                {
                    VrseReflection.SetMemberValue(node, "TargetGameObject", null);
                    VrseReflection.SetMemberValue(node, "Query", string.Empty);
                }

                if (!string.IsNullOrWhiteSpace(targetGameObjectPath))
                {
                    GameObject target = GameObject.Find(targetGameObjectPath.Trim())
                        ?? throw new InvalidOperationException($"Target GameObject not found in loaded scenes: {targetGameObjectPath}");
                    VrseReflection.SetMemberValue(node, "TargetGameObject", target);
                    VrseReflection.SetMemberValue(node, "Query", target.name);
                }

                VrseStoryMutation.MarkStoryChanged(storyCreator);
                return new VrseStoryNodeEditResult
                {
                    Success = true,
                    ChapterIndex = chapterIndex,
                    MomentIndex = momentIndex,
                    Section = normalizedSection,
                    TriggerSetIndex = triggerSetIndex,
                    NodeIndex = nodeIndex,
                    NodeKind = normalizedKind,
                    Node = VrseStorySerializer.SerializeNode(node)
                };
            });
        }

        [AiTool("vrse-story-remove-action", Title = "VRseBuilder / Story / Remove Action")]
        [Description("Remove an action node from a moment section.")]
        public VrseStoryNodeEditResult RemoveAction(int chapterIndex, int momentIndex, string section, int nodeIndex, int triggerSetIndex = -1, string? storyCreatorName = null)
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                object moment = VrseStoryMutation.GetMoment(story, chapterIndex, momentIndex, out _);
                string normalizedSection = VrseStorySerializer.NormalizeSectionName(section);
                List<object> nodes = VrseStoryMutation.GetActionNodes(moment, normalizedSection, triggerSetIndex);
                if (nodeIndex < 0 || nodeIndex >= nodes.Count)
                    throw new ArgumentOutOfRangeException(nameof(nodeIndex), $"nodeIndex must be within 0-{nodes.Count - 1}.");

                object removed = nodes[nodeIndex];
                nodes.RemoveAt(nodeIndex);
                VrseStoryMutation.SetActionNodes(moment, normalizedSection, triggerSetIndex, nodes);
                VrseStoryMutation.MarkStoryChanged(storyCreator);

                return new VrseStoryNodeEditResult
                {
                    Success = true,
                    ChapterIndex = chapterIndex,
                    MomentIndex = momentIndex,
                    Section = normalizedSection,
                    TriggerSetIndex = triggerSetIndex,
                    NodeIndex = nodeIndex,
                    NodeKind = "action",
                    RemovedNode = VrseStorySerializer.SerializeNode(removed),
                    RemainingCount = nodes.Count
                };
            });
        }

        private static object ResolveNode(object moment, string section, string nodeKind, int triggerSetIndex, int nodeIndex)
        {
            if (VrseStoryMutation.IsSimpleActionSection(section))
            {
                List<object> nodes = VrseStoryMutation.GetActionNodes(moment, section, triggerSetIndex);
                if (nodeIndex < 0 || nodeIndex >= nodes.Count)
                    throw new ArgumentOutOfRangeException(nameof(nodeIndex), $"nodeIndex must be within 0-{nodes.Count - 1} for {section}.");
                return nodes[nodeIndex];
            }

            object triggerActionSet = VrseStoryMutation.GetTriggerActionSet(moment, section, triggerSetIndex);
            if (nodeKind == "trigger")
                return VrseReflection.GetMemberValue(triggerActionSet, "trigger") ?? throw new InvalidOperationException("Trigger node is missing.");

            List<object> actionNodes = VrseStoryDiscovery.GetNodesFromActionSet(triggerActionSet);
            if (nodeIndex < 0 || nodeIndex >= actionNodes.Count)
                throw new ArgumentOutOfRangeException(nameof(nodeIndex), $"nodeIndex must be within 0-{actionNodes.Count - 1}.");
            return actionNodes[nodeIndex];
        }

        private static byte ParseNodeType(string type, int fallback)
        {
            if (byte.TryParse(type, out byte numeric))
                return numeric;
            if (string.Equals(type, "action", StringComparison.OrdinalIgnoreCase))
                return 0;
            if (string.Equals(type, "trigger", StringComparison.OrdinalIgnoreCase))
                return 1;
            return (byte)fallback;
        }
    }
}
