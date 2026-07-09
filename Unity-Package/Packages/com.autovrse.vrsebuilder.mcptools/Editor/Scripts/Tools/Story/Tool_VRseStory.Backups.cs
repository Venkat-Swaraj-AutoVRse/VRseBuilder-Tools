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
using System.IO;
using System.Linq;
using com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Story
{
    public partial class Tool_VRseStory
    {
        [AiTool("vrse-story-list-backups", Title = "VRseBuilder / Story / List Backups")]
        [Description("List available backup versions for the active StoryCreator story JSON.")]
        public VrseStoryBackupListResult ListStoryBackups(string? storyCreatorName = null)
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                string filePath = VrseReflection.GetString(storyCreator, "_FilePath");
                List<VrseStoryBackupInfo> backups = GetBackupHistory(filePath);
                return new VrseStoryBackupListResult
                {
                    StoryCreator = VrseStoryDiscovery.GetStoryCreatorName(storyCreator),
                    FilePath = filePath,
                    BackupCount = backups.Count,
                    Backups = backups
                };
            });
        }

        [AiTool("vrse-story-create-backup", Title = "VRseBuilder / Story / Create Backup")]
        [Description("Create a backup version of the active StoryCreator story JSON.")]
        public VrseStoryBackupResult CreateStoryBackup(string reason = "Manual MCP Backup", string? storyCreatorName = null)
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                string filePath = VrseReflection.GetString(storyCreator, "_FilePath");
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    throw new FileNotFoundException("Story JSON file does not exist.", filePath);

                CreateBackup(filePath, string.IsNullOrWhiteSpace(reason) ? "Manual MCP Backup" : reason.Trim());
                VrseStoryBackupInfo? latest = GetBackupHistory(filePath).FirstOrDefault();
                return new VrseStoryBackupResult
                {
                    Success = latest != null,
                    StoryCreator = VrseStoryDiscovery.GetStoryCreatorName(storyCreator),
                    FilePath = filePath,
                    LatestBackup = latest
                };
            });
        }

        [AiTool("vrse-story-restore-backup", Title = "VRseBuilder / Story / Restore Backup")]
        [Description("Restore a backup version into the active StoryCreator story JSON and reload the story.")]
        public VrseStoryBackupResult RestoreStoryBackup(int backupIndex = -1, string? backupPath = null, string? storyCreatorName = null)
        {
            return MainThread.Instance.Run(() =>
            {
                object storyCreator = VrseStoryDiscovery.FindStoryCreator(storyCreatorName);
                string filePath = VrseReflection.GetString(storyCreator, "_FilePath");
                string resolvedBackupPath = ResolveBackupPath(filePath, backupIndex, backupPath);
                RestoreBackup(filePath, resolvedBackupPath);
                VrseReflection.InvokeInstance(storyCreator, "SetStoryFromFile");
                VrseReflection.InvokeInstance(storyCreator, "InvalidateIsStorySavedToFileCache");
                VrseStoryMutation.MarkStoryChanged(storyCreator);

                return new VrseStoryBackupResult
                {
                    Success = true,
                    StoryCreator = VrseStoryDiscovery.GetStoryCreatorName(storyCreator),
                    FilePath = filePath,
                    RestoredBackupPath = resolvedBackupPath
                };
            });
        }

        private static void CreateBackup(string filePath, string reason)
        {
            Type storyVersioningType = VrseTypeResolver.FindType("VRseBuilder.Tools.Editor.StoryVersioning")
                ?? throw new InvalidOperationException("StoryVersioning was not found. Ensure VRseBuilder SDK editor tools are installed and compiled.");
            VrseReflection.InvokeStatic(storyVersioningType, "CreateVersion", filePath, reason);
        }

        private static List<VrseStoryBackupInfo> GetBackupHistory(string filePath)
        {
            Type storyVersioningType = VrseTypeResolver.FindType("VRseBuilder.Tools.Editor.StoryVersioning")
                ?? throw new InvalidOperationException("StoryVersioning was not found. Ensure VRseBuilder SDK editor tools are installed and compiled.");
            object? raw = VrseReflection.InvokeStatic(storyVersioningType, "GetHistory", filePath);
            return VrseReflection.AsObjectList(raw)
                .Select((backup, index) => new VrseStoryBackupInfo
                {
                    Index = index,
                    FilePath = VrseReflection.GetString(backup, "filePath"),
                    Timestamp = VrseReflection.GetString(backup, "timestamp"),
                    DisplayDate = VrseReflection.GetString(backup, "displayDate"),
                    Reason = VrseReflection.GetString(backup, "reason")
                })
                .ToList();
        }

        private static string ResolveBackupPath(string filePath, int backupIndex, string? backupPath)
        {
            if (!string.IsNullOrWhiteSpace(backupPath))
            {
                if (!File.Exists(backupPath))
                    throw new FileNotFoundException("Backup file does not exist.", backupPath);
                return backupPath;
            }

            List<VrseStoryBackupInfo> history = GetBackupHistory(filePath);
            if (backupIndex < 0 || backupIndex >= history.Count)
                throw new ArgumentOutOfRangeException(nameof(backupIndex), $"backupIndex must be within 0-{Math.Max(history.Count - 1, 0)}.");
            return history[backupIndex].FilePath;
        }

        private static void RestoreBackup(string filePath, string backupPath)
        {
            Type storyVersioningType = VrseTypeResolver.FindType("VRseBuilder.Tools.Editor.StoryVersioning")
                ?? throw new InvalidOperationException("StoryVersioning was not found. Ensure VRseBuilder SDK editor tools are installed and compiled.");
            VrseReflection.InvokeStatic(storyVersioningType, "RestoreVersion", filePath, backupPath);
        }
    }
}
