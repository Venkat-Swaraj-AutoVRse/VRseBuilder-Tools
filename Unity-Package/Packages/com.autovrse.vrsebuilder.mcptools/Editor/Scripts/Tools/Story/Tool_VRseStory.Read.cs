/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System.ComponentModel;
using com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Story
{
    public partial class Tool_VRseStory
    {
        [AiTool("vrse-story-read", Title = "VRseBuilder / Story / Read")]
        [Description("Read StoryCreator story sections and nodes with optional chapter/moment/section filters.")]
        public VrseStoryReadResult Read
        (
            [Description("Optional StoryCreator GameObject name. Uses the first loaded StoryCreator when omitted.")]
            string? storyCreatorName = null,
            [Description("Optional zero-based chapter index filter. Use -1 for all chapters.")]
            int chapterIndex = -1,
            [Description("Optional zero-based moment index filter. Use -1 for all moments.")]
            int momentIndex = -1,
            [Description("Optional section filter: onAwake, onStart, onFirstWarning, onLastWarning, onEnd, onWrong, or onRight.")]
            string? section = null,
            [Description("Maximum number of nodes to return.")]
            int maxNodes = 500
        )
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                object story = VrseStoryDiscovery.GetLoadedStory(storyCreator);
                return VrseStorySerializer.SerializeRead(storyCreator, story, chapterIndex, momentIndex, section, maxNodes);
            });
        }
    }
}
