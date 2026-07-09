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
        [AiTool("vrse-story-list-node-templates", Title = "VRseBuilder / Story / List Node Templates")]
        [Description("List VRseBuilder story action and trigger node templates.")]
        public VrseNodeTemplateListResult ListNodeTemplates()
        {
            return MainThread.Instance.Run(() =>
            {
                object templatesData = VrseStoryDiscovery.LoadNodeTemplatesData();
                List<VrseNodeTemplateData> actions = SerializeTemplateList(VrseReflection.GetMemberValue(templatesData, "actionTemplates"), "action");
                List<VrseNodeTemplateData> triggers = SerializeTemplateList(VrseReflection.GetMemberValue(templatesData, "triggerTemplates"), "trigger");

                return new VrseNodeTemplateListResult
                {
                    Success = true,
                    ActionTemplateCount = actions.Count,
                    TriggerTemplateCount = triggers.Count,
                    ActionTemplates = actions,
                    TriggerTemplates = triggers,
                    DefaultParameters = SerializeParameters(VrseReflection.GetMemberValue(templatesData, "DefaultParameters"))
                };
            });
        }

        [AiTool("vrse-story-search-node-templates", Title = "VRseBuilder / Story / Search Node Templates")]
        [Description("Search VRseBuilder story action and trigger node templates by name, backend ID, description, or option text.")]
        public VrseNodeTemplateSearchResult SearchNodeTemplates
        (
            [Description("Search query.")]
            string query,
            [Description("Optional filter: action, trigger, or empty for both.")]
            string? type = null,
            [Description("Maximum results to return.")]
            int maxResults = 20
        )
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query is required.", nameof(query));

            return MainThread.Instance.Run(() =>
            {
                object templatesData = VrseStoryDiscovery.LoadNodeTemplatesData();
                var all = new List<VrseNodeTemplateData>();
                if (string.IsNullOrEmpty(type) || string.Equals(type, "action", StringComparison.OrdinalIgnoreCase))
                    all.AddRange(SerializeTemplateList(VrseReflection.GetMemberValue(templatesData, "actionTemplates"), "action"));
                if (string.IsNullOrEmpty(type) || string.Equals(type, "trigger", StringComparison.OrdinalIgnoreCase))
                    all.AddRange(SerializeTemplateList(VrseReflection.GetMemberValue(templatesData, "triggerTemplates"), "trigger"));

                string queryLower = query.ToLowerInvariant();
                string[] tokens = queryLower.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
                List<VrseNodeTemplateData> results = all
                    .Select(template =>
                    {
                        template.MatchScore = ScoreTemplate(template, queryLower, tokens);
                        return template;
                    })
                    .Where(template => template.MatchScore > 0)
                    .OrderByDescending(template => template.MatchScore)
                    .Take(maxResults <= 0 ? 20 : maxResults)
                    .ToList();

                return new VrseNodeTemplateSearchResult
                {
                    Success = true,
                    Query = query,
                    ResultCount = results.Count,
                    Results = results
                };
            });
        }

        private static List<VrseNodeTemplateData> SerializeTemplateList(object? templates, string category)
        {
            return VrseReflection.AsObjectList(templates)
                .Select(template => SerializeTemplate(template, category))
                .ToList();
        }

        private static VrseNodeTemplateData SerializeTemplate(object template, string category)
        {
            return new VrseNodeTemplateData
            {
                Name = VrseReflection.GetString(template, "Name"),
                BackendId = VrseReflection.GetString(template, "BackendId"),
                Type = VrseReflection.GetString(template, "Type"),
                Description = VrseReflection.GetString(template, "Description"),
                Category = category,
                Options = VrseReflection.AsObjectList(VrseReflection.GetMemberValue(template, "Options"))
                    .Select(SerializeOption)
                    .ToList()
            };
        }

        private static VrseNodeTemplateOptionData SerializeOption(object option)
        {
            return new VrseNodeTemplateOptionData
            {
                Name = VrseReflection.GetString(option, "Name"),
                Description = VrseReflection.GetString(option, "Description"),
                Parameters = SerializeParameters(VrseReflection.GetMemberValue(option, "Parameters")),
                NestedParameters = VrseReflection.AsObjectList(VrseReflection.GetMemberValue(option, "NestedParameters"))
                    .Select(SerializeNestedParameter)
                    .ToList()
            };
        }

        private static VrseNodeTemplateNestedParameterData SerializeNestedParameter(object nested)
        {
            return new VrseNodeTemplateNestedParameterData
            {
                Key = VrseReflection.GetString(nested, "Key"),
                Description = VrseReflection.GetString(nested, "Description"),
                Parameters = SerializeParameters(VrseReflection.GetMemberValue(nested, "Parameters"))
            };
        }

        private static List<VrseNodeTemplateParameterData> SerializeParameters(object? parameters)
        {
            return VrseReflection.AsObjectList(parameters)
                .Select(parameter => new VrseNodeTemplateParameterData
                {
                    Key = VrseReflection.GetString(parameter, "Key"),
                    Type = VrseReflection.GetString(parameter, "Type"),
                    DefaultValue = VrseReflection.GetString(parameter, "DefaultValue"),
                    Description = VrseReflection.GetString(parameter, "Description")
                })
                .ToList();
        }

        private static int ScoreTemplate(VrseNodeTemplateData template, string queryLower, string[] tokens)
        {
            int score = 0;
            string nameLower = template.Name.ToLowerInvariant();
            string descLower = template.Description.ToLowerInvariant();
            string backendIdLower = template.BackendId.ToLowerInvariant();

            if (nameLower == queryLower) score += 100;
            else if (nameLower.Contains(queryLower)) score += 50;
            if (backendIdLower.Contains(queryLower)) score += 30;
            if (descLower.Contains(queryLower)) score += 20;

            foreach (string token in tokens)
            {
                if (token.Length < 2) continue;
                if (nameLower.Contains(token)) score += 10;
                if (descLower.Contains(token)) score += 5;
            }

            foreach (VrseNodeTemplateOptionData option in template.Options)
            {
                string optionName = option.Name.ToLowerInvariant();
                string optionDescription = option.Description.ToLowerInvariant();
                if (optionName.Contains(queryLower)) score += 15;
                if (optionDescription.Contains(queryLower)) score += 5;
                foreach (string token in tokens)
                {
                    if (token.Length >= 2 && optionName.Contains(token))
                        score += 3;
                }
            }

            return score;
        }
    }
}
