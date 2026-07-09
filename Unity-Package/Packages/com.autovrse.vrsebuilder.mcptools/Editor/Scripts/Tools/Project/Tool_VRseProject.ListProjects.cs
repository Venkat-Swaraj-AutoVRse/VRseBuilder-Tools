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

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Project
{
    public partial class Tool_VRseProject
    {
        [AiTool("vrse-project-list", Title = "VRseBuilder / Project / List")]
        [Description("List local VRseBuilder StudioProjects discovered under Assets/StudioProjects.")]
        public VrseListProjectsResult ListProjects()
        {
            return MainThread.Instance.Run(() =>
            {
                var authUtility = VrseTypeResolver.FindType("VRseBuilder.Backend.Editor.Auth.AuthUtility") ?? VrseTypeResolver.FindType("AuthUtility");
                return new VrseListProjectsResult
                {
                    LoggedIn = VrseReflection.InvokeStaticBool(authUtility, "IsLoggedIn"),
                    UserName = VrseReflection.InvokeStaticString(authUtility, "GetUserName"),
                    SelectedProject = VrseEditorPrefs.SelectedProject,
                    LocalProjects = VrseProjectDiscovery.ListLocalProjects()
                };
            });
        }
    }
}
