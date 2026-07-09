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

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Story
{
    public partial class Tool_VRseStory
    {
        [AiTool("vrse-story-add-moment", Title = "VRseBuilder / Story / Add Moment")]
        [Description("Add a moment to a chapter in the active StoryCreator story.")]
        public VrseStoryMomentEditResult AddMoment(int chapterIndex, string name = "New Moment", int index = -1, string? storyCreatorName = null)
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                List<object> chapters = VrseStoryDiscovery.GetChapters(story);
                if (chapterIndex < 0 || chapterIndex >= chapters.Count)
                    throw new ArgumentOutOfRangeException(nameof(chapterIndex), $"chapterIndex must be within 0-{chapters.Count - 1}.");

                object chapter = chapters[chapterIndex];
                List<object> moments = VrseStoryDiscovery.GetMoments(chapter);
                object moment = VrseStoryMutation.CreateMoment(string.IsNullOrWhiteSpace(name) ? "New Moment" : name.Trim());
                if (index >= 0 && index <= moments.Count)
                    moments.Insert(index, moment);
                else
                {
                    moments.Add(moment);
                    index = moments.Count - 1;
                }

                VrseStoryMutation.SetMoments(chapter, moments);
                VrseStoryMutation.MarkStoryChanged(storyCreator);
                return new VrseStoryMomentEditResult { Success = true, ChapterIndex = chapterIndex, MomentIndex = index, MomentName = VrseReflection.GetString(moment, "name") };
            });
        }

        [AiTool("vrse-story-rename-moment", Title = "VRseBuilder / Story / Rename Moment")]
        [Description("Rename a moment in the active StoryCreator story.")]
        public VrseStoryMomentEditResult RenameMoment(int chapterIndex, int momentIndex, string newName, string? storyCreatorName = null)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("newName is required.", nameof(newName));

            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                object moment = VrseStoryMutation.GetMoment(story, chapterIndex, momentIndex, out _);
                VrseReflection.SetMemberValue(moment, "name", newName.Trim());
                VrseStoryMutation.MarkStoryChanged(storyCreator);
                return new VrseStoryMomentEditResult { Success = true, ChapterIndex = chapterIndex, MomentIndex = momentIndex, MomentName = newName.Trim() };
            });
        }

        [AiTool("vrse-story-remove-moment", Title = "VRseBuilder / Story / Remove Moment")]
        [Description("Remove a moment from a chapter in the active StoryCreator story.")]
        public VrseStoryMomentEditResult RemoveMoment(int chapterIndex, int momentIndex, string? storyCreatorName = null)
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                object moment = VrseStoryMutation.GetMoment(story, chapterIndex, momentIndex, out object chapter);
                List<object> moments = VrseStoryDiscovery.GetMoments(chapter);
                string removedName = VrseReflection.GetString(moment, "name");
                moments.RemoveAt(momentIndex);
                VrseStoryMutation.SetMoments(chapter, moments);
                VrseStoryMutation.MarkStoryChanged(storyCreator);
                return new VrseStoryMomentEditResult { Success = true, ChapterIndex = chapterIndex, MomentIndex = momentIndex, RemovedMomentName = removedName, RemainingCount = moments.Count };
            });
        }
    }
}
