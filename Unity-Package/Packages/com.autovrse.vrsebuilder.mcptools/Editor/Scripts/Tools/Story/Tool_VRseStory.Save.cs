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
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Story
{
    public partial class Tool_VRseStory
    {
        [AiTool("vrse-story-save", Title = "VRseBuilder / Story / Save")]
        [Description("Save the active StoryCreator story to its configured JSON file.")]
        public VrseStorySaveResult Save
        (
            [Description("Optional StoryCreator GameObject name. Uses the first loaded StoryCreator when omitted.")]
            string? storyCreatorName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                VrseReflection.InvokeInstance(storyCreator, "SaveToJSONFile");
                AssetDatabase.Refresh();

                return new VrseStorySaveResult
                {
                    Success = true,
                    StoryCreator = VrseStoryDiscovery.GetStoryCreatorName(storyCreator),
                    FileName = VrseReflection.GetString(storyCreator, "_fileName"),
                    FilePath = VrseReflection.GetString(storyCreator, "_FilePath"),
                    IsSavedToFile = VrseStorySerializer.GetIsSavedToFile(storyCreator)
                };
            });
        }
    }
}
