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
using UnityEngine.SceneManagement;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Project
{
    public partial class Tool_VRseProject
    {
        [AiTool("vrse-project-status", Title = "VRseBuilder / Project / Status")]
        [Description("Get VRseBuilder auth, selected project, and active scene status.")]
        public VrseProjectStatusResult Status()
        {
            return MainThread.Instance.Run(() =>
            {
                string selectedProject = VrseEditorPrefs.SelectedProject;
                var activeScene = SceneManager.GetActiveScene();
                var authUtility = VrseTypeResolver.FindType("VRseBuilder.Backend.Editor.Auth.AuthUtility") ?? VrseTypeResolver.FindType("AuthUtility");

                return new VrseProjectStatusResult
                {
                    LoggedIn = VrseReflection.InvokeStaticBool(authUtility, "IsLoggedIn"),
                    UserName = VrseReflection.InvokeStaticString(authUtility, "GetUserName"),
                    BaseUrl = VrseReflection.InvokeStaticString(authUtility, "GetBaseUrl"),
                    SelectedProject = selectedProject,
                    ActiveSceneName = activeScene.name,
                    ActiveScenePath = activeScene.path,
                    HasSelectedProjectConfig = !string.IsNullOrEmpty(selectedProject) && VrseProjectDiscovery.LoadRoomManagerConfig(selectedProject) != null
                };
            });
        }
    }
}
