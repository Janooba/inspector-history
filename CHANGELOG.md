# Changelog

## [2.0.0] - 2026-11-25

### Summary:
Fixes history and favourites that were often wiped. History and settings now save to UserSettings without an asset import.

### Added:
- Settings page in `Edit/Preferences/Inspector History`. The gear button opens it.
- Skip Folders option, which ignores folders that are selected in the Project window.
- Tooltips on red entries that tell why the entry is not loaded.
- History is saved before an assembly reload and when the editor quits.

### Changed:
- History and settings are saved to `UserSettings/InspectorHistory.asset` by the new `HistoryData` ScriptableSingleton. It replaces `SerializedHistory`.
- The legacy `Assets/Resources/SelectionHistoryConfig.asset` is deleted on load. The package deletes it only if it references the old `SerializedHistory` script just to be safe.
- Favourites are stored in their own list now. Trimming the history no longer removes favourites.
- An entry is removed from the history only when its asset or scene is deleted. Favourites are never removed automatically.
- History loads on the first editor update instead of during InitializeOnLoad.
- Each selection change and each favourite toggle saves the history.

### Fixed:
- History and favourites were lost when the asset database was not ready at load time.
- Clicking a favourite that was not in the history disabled the back and forward buttons.
- Back and forward could select an entry from an unloaded scene and clear the selection.
- The location icon tooltip changed the tooltip of the same icon in other Unity windows.
- Each selection change resolved the global id of the selection once per history entry.
- The navigation bar created new GUI styles on every repaint.

## [1.1.2] - 2026-09-16

### Summary:
Fixes bloating history by trimming excess.

### Changed:
- SerializedHistory now has a Trim method, called by HistoryService, to remove excess entries.

## [1.1.1] - 2026-07-19

### Summary:
Reduce global id lookups to aid performance on large projects.

### Changed:
- Remove invalid global ids instead of trying to resolve them over and over
- Don't assign global id unless already empty or invalid

## [1.1.0] - 2026-07-17

- Initial non-wip release. See README.md