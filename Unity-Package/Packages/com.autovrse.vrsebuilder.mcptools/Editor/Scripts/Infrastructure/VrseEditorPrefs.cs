/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using UnityEditor;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure
{
    internal static class VrseEditorPrefs
    {
        private const string SelectedProjectKey = "VRSEBUILDER_LAST_SELECTED_PROJECT";

        public static string SelectedProject
        {
            get => EditorPrefs.GetString(SelectedProjectKey, string.Empty);
            set => EditorPrefs.SetString(SelectedProjectKey, value ?? string.Empty);
        }
    }
}
