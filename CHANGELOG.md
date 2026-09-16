# Changelog

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