# Changelog

## [1.1.1] - 2026-07-19

### Summary:
Reduce global id lookups to aid performance on large projects.

### Changed:
- Remove invalid global ids instead of trying to resolve them over and over
- Don't assign global id unless already empty or invalid

## [1.1.0] - 2026-07-17

- Initial non-wip release. See README.md