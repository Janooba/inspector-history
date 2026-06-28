# WARNING: THIS REPO IS A WIP

# Inspector History Window

A Unity Editor package that provides comprehensive inspector selection history tracking with navigation capabilities.

## Overview

The Inspector History package logs all inspector selections and provides easy navigation through your selection history. It offers both a toolbar navigation interface and a dedicated window for detailed history management.

## Features

### 1. History Tracking
- Automatically logs all inspector selections in Unity Editor
- Maintains a chronological history of your selection actions
- Tracks selection frequency for frequently used objects
- Preserves object references across editor sessions using EditorPrefs

### 2. Navigation Controls
- **Inspector Toolbar Buttons**: Back and forward navigation buttons integrated directly into the Unity inspector toolbar
- **Quick Navigation**: Click through history directly to select objects
- **Smart Navigation**: Automatically manages history when navigating forward/backward

### 3. History Window
- Dedicated window accessible via `VoidState/Inspector History` menu
- Displays selection history in a scrollable list
- Shows frequently used objects for quick access
- Allows favorite selection management

### 4. Favorites System
- Mark important objects as favorites for quick access
- Favorites are displayed separately in the history window

### 5. Visual Indicators
- Object thumbnails in history list
- Type information and location indicators with tooltips
- Click visual indicators to ping the asset without selecting it

## Usage

### History Window
- Open via `VoidState/Inspector History` menu
- Browse through your selection history
- Use the "Favourites" section to quickly access your most important objects
- Use the "Frequent" section to see your most-used selections
- Click on any entry to select it in the inspector
- Entries from unloaded scenes are not shown

### Favorites
1. Click the star icon next to any history entry to favorite it
2. Favorites appear in the dedicated "Favourites" section
3. Toggle favorite status by clicking the star icon again

## Technical Details

### Data Persistence
- History is saved to EditorPrefs between editor sessions
- Uses Sirenix Serialization for data handling
- Maintains object references using Unity's new `GlobalObjectId` system

## Requirements

- Unity 6000 or later
- Sirenix Odin Serializer

## To Do

- Migrate Editorprefs to an in-project config file. This is to allow different projects to have their own history and settings
- Try to eliminate Odin Serializer dependency