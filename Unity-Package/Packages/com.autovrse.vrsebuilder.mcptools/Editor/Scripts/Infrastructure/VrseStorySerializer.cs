/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using UnityEngine;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure
{
    internal static class VrseStorySerializer
    {
        private static readonly string[] SimpleSections =
        {
            "onAwake",
            "onStart",
            "onFirstWarning",
            "onLastWarning",
            "onEnd"
        };

        public static VrseStoryNodeData SerializeNode(object? node)
        {
            if (node == null)
                return new VrseStoryNodeData();

            object? targetGameObject = VrseReflection.GetMemberValue(node, "TargetGameObject");
            return new VrseStoryNodeData
            {
                Name = VrseReflection.GetString(node, "Name"),
                Option = VrseReflection.GetString(node, "Option"),
                Query = VrseReflection.GetString(node, "Query"),
                Data = VrseReflection.GetString(node, "Data"),
                Type = VrseReflection.GetInt(node, "Type"),
                TargetGameObject = targetGameObject is GameObject go ? go.name : null
            };
        }

        public static VrseStoryInfoResult SerializeInfo(object storyCreator, object story)
        {
            var chapters = VrseStoryDiscovery.GetChapters(story);
            var result = new VrseStoryInfoResult
            {
                Success = true,
                StoryCreator = VrseStoryDiscovery.GetStoryCreatorName(storyCreator),
                FileName = VrseReflection.GetString(storyCreator, "_fileName"),
                FilePath = VrseReflection.GetString(storyCreator, "_FilePath"),
                IsDirty = !GetIsSavedToFile(storyCreator),
                TotalChapters = chapters.Count
            };

            for (int ci = 0; ci < chapters.Count; ci++)
            {
                object chapter = chapters[ci];
                var moments = VrseStoryDiscovery.GetMoments(chapter);
                var chapterInfo = new VrseStoryChapterInfo
                {
                    Index = ci,
                    Name = VrseReflection.GetString(chapter, "name"),
                    MomentCount = moments.Count
                };

                for (int mi = 0; mi < moments.Count; mi++)
                {
                    object moment = moments[mi];
                    chapterInfo.Moments.Add(new VrseStoryMomentInfo
                    {
                        Index = mi,
                        Name = VrseReflection.GetString(moment, "name"),
                        Defaults = VrseReflection.GetString(moment, "defaults")
                    });
                }

                result.Chapters.Add(chapterInfo);
            }

            return result;
        }

        public static VrseStoryReadResult SerializeRead(object storyCreator, object story, int filterChapter, int filterMoment, string? filterSection, int maxNodes)
        {
            maxNodes = maxNodes <= 0 ? 500 : maxNodes;
            string normalizedFilterSection = NormalizeSectionName(filterSection ?? string.Empty);
            int nodeCount = 0;
            var chapters = VrseStoryDiscovery.GetChapters(story);
            var result = new VrseStoryReadResult
            {
                Success = true,
                StoryCreator = VrseStoryDiscovery.GetStoryCreatorName(storyCreator),
                FileName = VrseReflection.GetString(storyCreator, "_fileName"),
                IsSavedToFile = GetIsSavedToFile(storyCreator),
                TotalChapters = chapters.Count,
                MaxNodes = maxNodes
            };

            for (int ci = 0; ci < chapters.Count; ci++)
            {
                if (filterChapter >= 0 && ci != filterChapter)
                    continue;

                object chapter = chapters[ci];
                var moments = VrseStoryDiscovery.GetMoments(chapter);
                var chapterRead = new VrseStoryChapterRead
                {
                    Index = ci,
                    Name = VrseReflection.GetString(chapter, "name"),
                    MomentCount = moments.Count
                };

                for (int mi = 0; mi < moments.Count; mi++)
                {
                    if (filterMoment >= 0 && mi != filterMoment)
                        continue;

                    object moment = moments[mi];
                    var momentRead = new VrseStoryMomentRead
                    {
                        Index = mi,
                        Name = VrseReflection.GetString(moment, "name"),
                        Defaults = VrseReflection.GetString(moment, "defaults")
                    };

                    foreach (string sectionName in SimpleSections)
                    {
                        if (!string.IsNullOrEmpty(normalizedFilterSection) && normalizedFilterSection != sectionName)
                            continue;

                        List<VrseStoryNodeData> nodes = SerializeNodeList(VrseStoryDiscovery.GetNodesFromActionSet(VrseReflection.GetMemberValue(moment, sectionName)), ref nodeCount, maxNodes);
                        if (nodes.Count > 0)
                            momentRead.Sections[sectionName] = nodes;
                    }

                    if (string.IsNullOrEmpty(normalizedFilterSection) || normalizedFilterSection == "onWrong")
                    {
                        List<VrseStoryTriggerActionSetData> wrongSets = SerializeTriggerActionSets(VrseStoryDiscovery.GetTriggerActionSets(VrseReflection.GetMemberValue(moment, "onWrong")), ref nodeCount, maxNodes);
                        if (wrongSets.Count > 0)
                            momentRead.Sections["onWrong"] = wrongSets;
                    }

                    if (string.IsNullOrEmpty(normalizedFilterSection) || normalizedFilterSection == "onRight")
                    {
                        object? onRight = VrseReflection.GetMemberValue(moment, "onRight");
                        List<VrseStoryTriggerActionSetData> rightSets = SerializeTriggerActionSets(VrseStoryDiscovery.GetTriggerActionSets(VrseReflection.GetMemberValue(onRight, "triggerActionSets")), ref nodeCount, maxNodes);
                        if (rightSets.Count > 0)
                        {
                            momentRead.Sections["onRight"] = new Dictionary<string, object>
                            {
                                { "mode", VrseReflection.GetString(onRight, "mode") },
                                { "triggerActionSets", rightSets }
                            };
                        }
                    }

                    chapterRead.Moments.Add(momentRead);
                }

                result.Chapters.Add(chapterRead);
            }

            result.TotalNodesReturned = nodeCount;
            return result;
        }

        private static List<VrseStoryNodeData> SerializeNodeList(List<object> nodes, ref int nodeCount, int maxNodes)
        {
            var result = new List<VrseStoryNodeData>();
            foreach (object node in nodes)
            {
                if (nodeCount >= maxNodes)
                    break;

                result.Add(SerializeNode(node));
                nodeCount++;
            }

            return result;
        }

        private static List<VrseStoryTriggerActionSetData> SerializeTriggerActionSets(List<object> sets, ref int nodeCount, int maxNodes)
        {
            var result = new List<VrseStoryTriggerActionSetData>();
            for (int i = 0; i < sets.Count; i++)
            {
                object set = sets[i];
                var setData = new VrseStoryTriggerActionSetData { Index = i };
                object? trigger = VrseReflection.GetMemberValue(set, "trigger");
                if (trigger != null && nodeCount < maxNodes)
                {
                    setData.Trigger = SerializeNode(trigger);
                    nodeCount++;
                }

                setData.Actions = SerializeNodeList(VrseStoryDiscovery.GetNodesFromActionSet(set), ref nodeCount, maxNodes);
                result.Add(setData);
            }

            return result;
        }

        public static bool GetIsSavedToFile(object storyCreator)
        {
            object? saved = VrseReflection.InvokeInstance(storyCreator, "GetIsStorySavedToFileCached");
            return saved is bool boolValue && boolValue;
        }

        public static string NormalizeSectionName(string section)
        {
            string normalized = section.Trim().Replace("-", string.Empty).Replace("_", string.Empty).ToLowerInvariant();
            return normalized switch
            {
                "onawake" => "onAwake",
                "onstart" => "onStart",
                "onwrong" => "onWrong",
                "onright" => "onRight",
                "onfirstwarning" => "onFirstWarning",
                "onlastwarning" => "onLastWarning",
                "onend" => "onEnd",
                _ => section
            };
        }

        public static List<string> StringListFromObject(object? value)
        {
            return value is IEnumerable enumerable
                ? enumerable.Cast<object>().Select(item => item?.ToString() ?? string.Empty).Where(item => !string.IsNullOrEmpty(item)).ToList()
                : new List<string>();
        }
    }
}
