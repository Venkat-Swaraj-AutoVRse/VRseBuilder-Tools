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

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Project
{
    public partial class Tool_VRseProject
    {
        [AiTool("vrse-project-open-studio-window", Title = "VRseBuilder / Project / Open Studio Window")]
        [Description("Open the VRse Studio Projects editor window when the VRseBuilder SDK is installed.")]
        public VrseOpenWindowResult OpenStudioProjectWindow()
        {
            return MainThread.Instance.Run(() =>
            {
                Type windowType = VrseTypeResolver.FindType("VRseBuilder.Tools.Editor.VRseProjectWindowUI")
                    ?? throw new InvalidOperationException("VRseProjectWindowUI was not found. Ensure the VRseBuilder SDK is installed and compiled.");

                VrseReflection.InvokeStatic(windowType, "ShowWindow");
                return new VrseOpenWindowResult { Success = true, Window = "VRse Studio Projects" };
            });
        }

        [AiTool("vrse-project-open-config-window", Title = "VRseBuilder / Project / Open Config Window")]
        [Description("Open the VRse Project Config editor window when the VRseBuilder SDK is installed.")]
        public VrseOpenWindowResult OpenProjectConfigWindow()
        {
            return MainThread.Instance.Run(() =>
            {
                Type windowType = VrseTypeResolver.FindType("VRseBuilder.Tools.Editor.VRseProjectConfigWindow")
                    ?? throw new InvalidOperationException("VRseProjectConfigWindow was not found. Ensure the VRseBuilder SDK is installed and compiled.");

                VrseReflection.InvokeStatic(windowType, "ShowWindow");
                return new VrseOpenWindowResult { Success = true, Window = "VRse Project Config" };
            });
        }
    }
}
