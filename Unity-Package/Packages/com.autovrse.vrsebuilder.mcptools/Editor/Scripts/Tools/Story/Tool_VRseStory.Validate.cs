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
using System.ComponentModel;
using System.Reflection;
using com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Story
{
    public partial class Tool_VRseStory
    {
        [AiTool("vrse-story-validate", Title = "VRseBuilder / Story / Validate")]
        [Description("Validate the active StoryCreator story through VRseBuilder's StoryFlowValidator when available.")]
        public VrseStoryValidateResult Validate
        (
            [Description("Optional StoryCreator GameObject name. Uses the first loaded StoryCreator when omitted.")]
            string? storyCreatorName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                Type storyReportType = VrseTypeResolver.FindType("VRseBuilder.Tools.Editor.StoryReportEditor")
                    ?? throw new InvalidOperationException("StoryReportEditor was not found. Ensure the VRseBuilder SDK editor tools are installed and compiled.");

                Type validatorType = storyReportType.GetNestedType("StoryFlowValidator", BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException("StoryFlowValidator was not found in StoryReportEditor.");

                object validator = Activator.CreateInstance(validatorType, nonPublic: true)
                    ?? throw new InvalidOperationException("Could not create StoryFlowValidator.");

                object? rawResult = VrseReflection.InvokeInstance(validator, "Validate", story);
                return BuildValidationResult(storyCreator, story, rawResult);
            });
        }

        private static VrseStoryValidateResult BuildValidationResult(object storyCreator, object story, object? rawResult)
        {
            var result = new VrseStoryValidateResult
            {
                Success = true,
                StoryCreator = VrseStoryDiscovery.GetStoryCreatorName(storyCreator)
            };

            var momentLookup = VrseStoryDiscovery.BuildMomentLookup(story);
            if (rawResult is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    object moment = entry.Key;
                    var issue = new VrseStoryValidationIssue();
                    if (momentLookup.TryGetValue(moment, out var momentInfo))
                    {
                        issue.ChapterIndex = momentInfo.chapterIndex;
                        issue.ChapterName = momentInfo.chapterName;
                        issue.MomentIndex = momentInfo.momentIndex;
                        issue.MomentName = momentInfo.momentName;
                    }
                    else
                    {
                        issue.MomentName = VrseReflection.GetString(moment, "name");
                    }

                    if (entry.Value is IDictionary sectionDictionary)
                    {
                        foreach (DictionaryEntry sectionEntry in sectionDictionary)
                        {
                            string sectionName = sectionEntry.Key?.ToString() ?? string.Empty;
                            List<string> messages = VrseStorySerializer.StringListFromObject(sectionEntry.Value);
                            if (messages.Count == 0)
                                continue;

                            issue.Sections[sectionName] = messages;
                            issue.IssueCount += messages.Count;
                        }
                    }

                    if (issue.IssueCount > 0)
                    {
                        result.Issues.Add(issue);
                        result.TotalIssueCount += issue.IssueCount;
                    }
                }
            }

            result.TotalMomentsWithIssues = result.Issues.Count;
            result.IsValid = result.TotalIssueCount == 0;
            return result;
        }
    }
}
