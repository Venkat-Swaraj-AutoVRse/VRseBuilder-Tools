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
        [AiTool("vrse-story-move-action", Title = "VRseBuilder / Story / Move Action")]
        [Description("Move an action node within a moment section.")]
        public VrseStoryNodeEditResult MoveAction(int chapterIndex, int momentIndex, string section, int fromIndex, int toIndex, int triggerSetIndex = -1, string? storyCreatorName = null)
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                object moment = VrseStoryMutation.GetMoment(story, chapterIndex, momentIndex, out _);
                string normalizedSection = VrseStorySerializer.NormalizeSectionName(section);
                List<object> nodes = VrseStoryMutation.GetActionNodes(moment, normalizedSection, triggerSetIndex);
                if (fromIndex < 0 || fromIndex >= nodes.Count || toIndex < 0 || toIndex >= nodes.Count)
                    throw new ArgumentOutOfRangeException(nameof(fromIndex), $"fromIndex and toIndex must be within 0-{nodes.Count - 1}.");

                object node = nodes[fromIndex];
                nodes.RemoveAt(fromIndex);
                nodes.Insert(toIndex, node);
                VrseStoryMutation.SetActionNodes(moment, normalizedSection, triggerSetIndex, nodes);
                VrseStoryMutation.MarkStoryChanged(storyCreator);

                return new VrseStoryNodeEditResult
                {
                    Success = true,
                    ChapterIndex = chapterIndex,
                    MomentIndex = momentIndex,
                    Section = normalizedSection,
                    TriggerSetIndex = triggerSetIndex,
                    FromIndex = fromIndex,
                    ToIndex = toIndex,
                    Node = VrseStorySerializer.SerializeNode(node)
                };
            });
        }

        [AiTool("vrse-story-duplicate-action", Title = "VRseBuilder / Story / Duplicate Action")]
        [Description("Duplicate an action node in a moment section.")]
        public VrseStoryNodeEditResult DuplicateAction(int chapterIndex, int momentIndex, string section, int nodeIndex, int triggerSetIndex = -1, string? storyCreatorName = null)
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

                object clone = VrseStoryMutation.CloneNode(nodes[nodeIndex]);
                int newIndex = nodeIndex + 1;
                nodes.Insert(newIndex, clone);
                VrseStoryMutation.SetActionNodes(moment, normalizedSection, triggerSetIndex, nodes);
                VrseStoryMutation.MarkStoryChanged(storyCreator);

                return new VrseStoryNodeEditResult
                {
                    Success = true,
                    ChapterIndex = chapterIndex,
                    MomentIndex = momentIndex,
                    Section = normalizedSection,
                    TriggerSetIndex = triggerSetIndex,
                    SourceNodeIndex = nodeIndex,
                    NewNodeIndex = newIndex,
                    Node = VrseStorySerializer.SerializeNode(clone)
                };
            });
        }

        [AiTool("vrse-story-apply-action-to-multiple-moments", Title = "VRseBuilder / Story / Apply Action To Multiple Moments")]
        [Description("Clone one source action node into multiple target moments.")]
        public VrseStoryApplyActionMultipleResult ApplyActionToMultipleMoments(
            int chapterIndex,
            int momentIndex,
            string section,
            int nodeIndex,
            VrseStoryApplyActionTarget[] targets,
            int triggerSetIndex = -1,
            string? storyCreatorName = null)
        {
            if (targets == null || targets.Length == 0)
                throw new ArgumentException("targets must contain at least one target moment.", nameof(targets));

            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                object sourceMoment = VrseStoryMutation.GetMoment(story, chapterIndex, momentIndex, out _);
                string sourceSection = VrseStorySerializer.NormalizeSectionName(section);
                List<object> sourceNodes = VrseStoryMutation.GetActionNodes(sourceMoment, sourceSection, triggerSetIndex);
                if (nodeIndex < 0 || nodeIndex >= sourceNodes.Count)
                    throw new ArgumentOutOfRangeException(nameof(nodeIndex), $"nodeIndex must be within 0-{sourceNodes.Count - 1}.");

                object sourceNode = sourceNodes[nodeIndex];
                var result = new VrseStoryApplyActionMultipleResult
                {
                    Success = true,
                    SourceChapterIndex = chapterIndex,
                    SourceMomentIndex = momentIndex,
                    SourceSection = sourceSection,
                    SourceTriggerSetIndex = triggerSetIndex,
                    SourceNodeIndex = nodeIndex,
                    SourceNode = VrseStorySerializer.SerializeNode(sourceNode)
                };

                foreach (VrseStoryApplyActionTarget target in targets)
                {
                    string targetSection = string.IsNullOrWhiteSpace(target.Section) ? sourceSection : VrseStorySerializer.NormalizeSectionName(target.Section);
                    int targetTriggerSetIndex = target.TriggerSetIndex >= 0 ? target.TriggerSetIndex : triggerSetIndex;
                    var targetResult = new VrseStoryApplyActionTargetResult
                    {
                        ChapterIndex = target.ChapterIndex,
                        MomentIndex = target.MomentIndex,
                        Section = targetSection,
                        TriggerSetIndex = targetTriggerSetIndex
                    };

                    try
                    {
                        object targetMoment = VrseStoryMutation.GetMoment(story, target.ChapterIndex, target.MomentIndex, out _);
                        List<object> targetNodes = VrseStoryMutation.GetActionNodes(targetMoment, targetSection, targetTriggerSetIndex);
                        object clone = VrseStoryMutation.CloneNode(sourceNode);
                        targetNodes.Add(clone);
                        VrseStoryMutation.SetActionNodes(targetMoment, targetSection, targetTriggerSetIndex, targetNodes);
                        targetResult.Success = true;
                        targetResult.NewNodeIndex = targetNodes.Count - 1;
                    }
                    catch (Exception ex)
                    {
                        targetResult.Success = false;
                        targetResult.Error = ex.Message;
                    }

                    result.Targets.Add(targetResult);
                }

                VrseStoryMutation.MarkStoryChanged(storyCreator);
                return result;
            });
        }

        [AiTool("vrse-story-apply-moment-weightage", Title = "VRseBuilder / Story / Apply Moment Weightage")]
        [Description("Update weightage and wrongReduction values in a moment's defaults JSON.")]
        public VrseMomentWeightageResult ApplyMomentWeightage(int chapterIndex, int momentIndex, float weightage, float wrongReduction, string? storyCreatorName = null)
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                object moment = VrseStoryMutation.GetMoment(story, chapterIndex, momentIndex, out _);
                var defaults = LoadMomentDefaults(VrseReflection.GetString(moment, "defaults"));
                defaults.weightage = weightage;
                defaults.wrongReduction = wrongReduction;
                VrseReflection.SetMemberValue(moment, "defaults", JsonUtility.ToJson(defaults));
                VrseStoryMutation.MarkStoryChanged(storyCreator);
                return new VrseMomentWeightageResult
                {
                    Success = true,
                    ChapterIndex = chapterIndex,
                    MomentIndex = momentIndex,
                    Weightage = defaults.weightage,
                    WrongReduction = defaults.wrongReduction
                };
            });
        }

        private static MomentDefaults LoadMomentDefaults(string defaultsJson)
        {
            if (string.IsNullOrWhiteSpace(defaultsJson))
                return new MomentDefaults();

            try
            {
                return JsonUtility.FromJson<MomentDefaults>(defaultsJson) ?? new MomentDefaults();
            }
            catch
            {
                return new MomentDefaults();
            }
        }

        [Serializable]
        private sealed class MomentDefaults
        {
            public float weightage;
            public float wrongReduction;
        }
    }
}
