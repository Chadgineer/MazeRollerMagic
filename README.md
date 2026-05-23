USE APK !!!!

# Maze Roller 2: Magic - Final Project

## 1. Project Overview
* [cite_start]**Game Name:** Maze Roller 2: Magic [cite: 1]
* **Platform:** Android (.apk build included)
* **UI Setup:** Canvas set to "Scale With Screen Size" (1080x1920 reference). Anchors and Safe Area are fully implemented.
* **Game Loop:** Main Menu -> Level Selection -> Gameplay -> Win/Lose -> Restart loop is fully working without crashes.

## 2. Core Mechanics & GDD Summary
* **Movement:** 2D physics-based ball movement controlled via mobile virtual joystick.
* **Jump Orb:** Collectible item tagged as `JumpOrb` that grants a temporary jump ability.
* **Win/Lose:** Reaching the `MazePortal` wins the level; falling into the `FallZone` triggers an instant respawn.

## 3. DOTween Animations & Juice Effects
* **Grid Pop-In:** All map blocks spawn with a randomized starting scale multiplier (0.0 to 0.4) and scale up using `Ease.OutBack`.
* **Respawn Sequence:** On fall, ball velocity is reset, position is set to spawn, and local scale plays a quick squash and `Ease.OutBack` expand animation.
* **Finish Transition:** Hitting the portal sets physics to kinematic and scales the ball down to zero. On complete, it loads the main menu.
* **Juice Effect:** The mobile Jump Button changes its active state dynamically; it appears only when an orb is collected and disappears when used or on death.

## 4. What Changed and Why
* **Implemented:** Added an instant coordinates-reset respawn system instead of reloading the full scene to keep the pacing smooth. Added a JSON validation check in the level menu to lock broken maps.
* **Cut:** The Dash mechanic was postponed to focus entirely on solid physics handling and jump button responsiveness.
* **Added:** Dual level loading system that reads maps from both local disk paths and compressed internal text assets inside `Resources/Levels/` for production stability.

## 5. AI Usage Explanation
1. **Architecture:** Used Gemini to decouple physics movement logic from collision triggers (`PlayerCollisionDetector`).
2. **API Updates:** Used AI to migrate obsolete velocity scripts to modern Unity 6 physics solver systems (`linearVelocity`).
3. **Debugging:** Resolved script execution order conflicts where the UI button initialized out of sync by re-routing logic into explicit initialization functions.
