# VRseBuilder AI Tools

This package exposes VRseBuilder workflow tools as Ivan Unity-MCP `AiTool` methods.

## Project

- `vrse-project-status`
- `vrse-project-list`
- `vrse-project-select`
- `vrse-project-get-selected`
- `vrse-project-get-config`
- `vrse-project-open-room-manager-config`
- `vrse-project-open-menu-scene`
- `vrse-project-ensure-settings`
- `vrse-project-apply-settings`
- `vrse-project-open-studio-window`
- `vrse-project-open-config-window`

## Modules

- `vrse-module-list`
- `vrse-module-open`
- `vrse-module-open-art-scene`
- `vrse-module-experience-status`
- `vrse-module-create-experience`

## Story

- `vrse-story-info`
- `vrse-story-read`
- `vrse-story-save`
- `vrse-story-validate`
- `vrse-story-list-node-templates`
- `vrse-story-search-node-templates`
- `vrse-story-add-chapter`
- `vrse-story-rename-chapter`
- `vrse-story-remove-chapter`
- `vrse-story-add-moment`
- `vrse-story-rename-moment`
- `vrse-story-remove-moment`
- `vrse-story-add-trigger-set`
- `vrse-story-add-action`
- `vrse-story-update-node`
- `vrse-story-remove-action`
- `vrse-story-list-backups`
- `vrse-story-create-backup`
- `vrse-story-restore-backup`
- `vrse-story-apply-json`
- `vrse-story-patch`
- `vrse-story-undo-write`
- `vrse-story-move-action`
- `vrse-story-duplicate-action`
- `vrse-story-apply-action-to-multiple-moments`
- `vrse-story-apply-moment-weightage`

## Scene And Interactables

- `vrse-scene-query-objects-list`
- `vrse-scene-building-blocks-list`
- `vrse-scene-building-block-instantiate`
- `vrse-scene-hierarchy-checkup`
- `vrse-scene-list-loaded`
- `vrse-scene-get-components`
- `vrse-interactable-convert`
- `vrse-rotator-analyze`
- `vrse-rotator-create`

## Spatial

- `vrse-spatial-analyze-scene`
- `vrse-spatial-get-bounds`
- `vrse-spatial-get-surface`
- `vrse-spatial-check-placement`
- `vrse-spatial-list-probe-surfaces`

## Build And Evaluation

- `vrse-build-open-tool`
- `vrse-build-module-set-include`
- `vrse-build-start`
- `vrse-build-status`
- `vrse-evaluation-create-from-training`

## Dependency Strategy

The tools use reflection for VRseBuilder SDK-specific APIs. This keeps the package compilable in projects that do not include the SDK, while enabling full behavior when the SDK assemblies are present.
