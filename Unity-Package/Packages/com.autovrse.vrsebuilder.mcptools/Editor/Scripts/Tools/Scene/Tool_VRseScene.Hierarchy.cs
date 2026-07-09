/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using System.ComponentModel;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Scene
{
    public partial class Tool_VRseScene
    {
        [AiTool("vrse-scene-hierarchy-checkup", Title = "VRseBuilder / Scene / Hierarchy Checkup")]
        [Description("Run VRseBuilder SceneHierarchyCheckup to move query objects under the QueryObjects parent.")]
        public VrseSceneActionResult SceneHierarchyCheckup()
        {
            return MainThread.Instance.Run(() =>
            {
                Type type = VrseTypeResolver.FindType("VRseBuilder.Core.Editor.SceneHierarchyCheckup")
                    ?? throw new InvalidOperationException("SceneHierarchyCheckup type was not found. Ensure VRseBuilder SDK editor tools are installed and compiled.");
                VrseReflection.InvokeStatic(type, "MoveQueryObjects");
                return new VrseSceneActionResult
                {
                    Success = true,
                    Message = "Scene hierarchy checkup complete. Query objects moved under QueryObjects when needed."
                };
            });
        }
    }
}
