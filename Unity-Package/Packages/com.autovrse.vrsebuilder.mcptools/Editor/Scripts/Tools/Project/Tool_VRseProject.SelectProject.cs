/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using System.ComponentModel;
using System.Linq;
using com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Project
{
    public partial class Tool_VRseProject
    {
        [AiTool("vrse-project-select", Title = "VRseBuilder / Project / Select")]
        [Description("Select a VRseBuilder project for later module/story operations.")]
        public VrseSelectProjectResult SelectProject
        (
            [Description("Project name matching a folder under Assets/StudioProjects.")]
            string projectName
        )
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name is required.", nameof(projectName));

            return MainThread.Instance.Run(() =>
            {
                var project = VrseProjectDiscovery.ListLocalProjects()
                    .FirstOrDefault(candidate => string.Equals(candidate.Name, projectName.Trim(), StringComparison.OrdinalIgnoreCase));
                if (project == null)
                    throw new ArgumentException($"Project '{projectName}' was not found under Assets/StudioProjects.", nameof(projectName));

                VrseEditorPrefs.SelectedProject = project.Name;
                return new VrseSelectProjectResult
                {
                    Success = true,
                    SelectedProject = project.Name
                };
            });
        }
    }
}
