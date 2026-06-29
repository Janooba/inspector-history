# WARNING: THIS REPO IS A WIP

# Inspector History Window

A Unity Editor package that provides comprehensive inspector selection history tracking with navigation capabilities.

<img width="800" height="935" alt="example_2" src="https://github.com/user-attachments/assets/21f3f94d-bfd6-4502-b740-f0edba419cb8" />

## Overview

The Inspector History package logs all inspector selections and provides easy navigation through your selection history. It offers both a toolbar navigation interface and a dedicated window for detailed history management.

## Features

### 1. History Tracking
- Automatically logs all inspector selections in Unity Editor
- Maintains a chronological history of your selection actions
- Tracks selection frequency for frequently used objects
- Preserves object references across editor sessions using a ScriptableObject in Resources

<img width="263" height="185" alt="navigation" src="https://github.com/user-attachments/assets/28924597-a243-4779-b576-925ecfca5b1c" />

### 2. Navigation Controls
- **Inspector Toolbar Buttons**: Back and forward navigation buttons integrated directly into the Unity inspector toolbar
- **Quick Navigation**: Click through history directly to select objects
- **Smart Navigation**: Automatically manages history when navigating forward/backward

<img width="330" height="374" alt="Unity_KUWkF4vMQw" src="https://github.com/user-attachments/assets/4f21172a-e14a-4ac8-9bb1-88421f57b997" />

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
- History and settings are saved to a ScriptableObject in Resources between editor sessions
- Maintains object references using Unity's new `GlobalObjectId` system

## Requirements

- Unity 6000 or later
