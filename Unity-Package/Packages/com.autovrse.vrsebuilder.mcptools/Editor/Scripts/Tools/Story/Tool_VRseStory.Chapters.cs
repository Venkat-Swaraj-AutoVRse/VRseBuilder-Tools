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
        [AiTool("vrse-story-add-chapter", Title = "VRseBuilder / Story / Add Chapter")]
        [Description("Add a chapter to the active StoryCreator story.")]
        public VrseStoryChapterEditResult AddChapter(string name = "New Chapter", int index = -1, string? storyCreatorName = null)
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                List<object> chapters = VrseStoryDiscovery.GetChapters(story);
                object chapter = VrseStoryMutation.CreateChapter(string.IsNullOrWhiteSpace(name) ? "New Chapter" : name.Trim());

                if (index >= 0 && index <= chapters.Count)
                    chapters.Insert(index, chapter);
                else
                {
                    chapters.Add(chapter);
                    index = chapters.Count - 1;
                }

                VrseStoryMutation.SetChapters(story, chapters);
                VrseStoryMutation.MarkStoryChanged(storyCreator);
                return new VrseStoryChapterEditResult { Success = true, ChapterIndex = index, ChapterName = VrseReflection.GetString(chapter, "name") };
            });
        }

        [AiTool("vrse-story-rename-chapter", Title = "VRseBuilder / Story / Rename Chapter")]
        [Description("Rename a chapter in the active StoryCreator story.")]
        public VrseStoryChapterEditResult RenameChapter(int chapterIndex, string newName, string? storyCreatorName = null)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("newName is required.", nameof(newName));

            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                List<object> chapters = VrseStoryDiscovery.GetChapters(story);
                if (chapterIndex < 0 || chapterIndex >= chapters.Count)
                    throw new ArgumentOutOfRangeException(nameof(chapterIndex), $"chapterIndex must be within 0-{chapters.Count - 1}.");

                object chapter = chapters[chapterIndex];
                VrseReflection.SetMemberValue(chapter, "name", newName.Trim());
                VrseStoryMutation.MarkStoryChanged(storyCreator);
                return new VrseStoryChapterEditResult { Success = true, ChapterIndex = chapterIndex, ChapterName = newName.Trim() };
            });
        }

        [AiTool("vrse-story-remove-chapter", Title = "VRseBuilder / Story / Remove Chapter")]
        [Description("Remove a chapter from the active StoryCreator story.")]
        public VrseStoryChapterEditResult RemoveChapter(int chapterIndex, string? storyCreatorName = null)
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                List<object> chapters = VrseStoryDiscovery.GetChapters(story);
                if (chapterIndex < 0 || chapterIndex >= chapters.Count)
                    throw new ArgumentOutOfRangeException(nameof(chapterIndex), $"chapterIndex must be within 0-{chapters.Count - 1}.");

                string removedName = VrseReflection.GetString(chapters[chapterIndex], "name");
                chapters.RemoveAt(chapterIndex);
                VrseStoryMutation.SetChapters(story, chapters);
                VrseStoryMutation.MarkStoryChanged(storyCreator);
                return new VrseStoryChapterEditResult { Success = true, ChapterIndex = chapterIndex, RemovedChapterName = removedName, RemainingCount = chapters.Count };
            });
        }
    }
}
