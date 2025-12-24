# 🕷️ Spider Cave: 2D Action Platformer

A classic 2D action-platformer built with **Unity**. In this game, players take control of a spider navigating through a dangerous, trap-filled cave. The project features unique movement mechanics, custom hand-drawn animations, and a final battle with an enemy knight. The spider's appearance was inspired by the anime "Kumo desu ga, Nani ka?", but the sprites were hand-drawn.

## 🕹️ Gameplay Mechanics

- **Unique Movement:** Beyond basic running and jumping, the spider can **climb walls** (up and down), **slide** down vertical surfaces and jump off the wall while sliding.
- **Combat System:** Defend yourself by shooting **venomous orbs** at distant threats.
- **Health System:** You have 3 HP (displayed in the top-right corner).
  - **Spike Pits:** Deadly traps that cause instant death (3 damage).
  - **Wall Spikes:** Static hazards requiring precision (1 damage).
- **Checkpoints:** Ancient trees serve as save points. Running past them saves your progress.
- **Boss Encounter:** A Knight guards the end of the level, attacking with a sword (1 damage). You must use your ranged attacks to defeat him.

## 🛠️ Technical Implementation

- **Engine:** Unity
- **Programming:** C# scripts for character controllers, health management, and AI behavior.
- **Art & Animation:** The spider character, venom effects, and animations were hand-drawn in **Krita**. Animations were implemented using frame-by-frame sprite swapping in Unity.
- **Assets:**
  - **Custom-made:** Main character sprites, combat animations.
  - **Asset Store:** Backgrounds, the Knight enemy, traps, and trees (Free licenses).
  - **Audio:** Royalty-free sound effects and ambient tracks.

## 📺 Project Preview

**[Watch Gameplay Video on Google Drive](https://drive.google.com/file/d/1VEHodL7nP8SQ6iNGi6FTqVXojKOXnBrE/view?usp=sharing)**

## 📂 Project Structure

The repository includes the essential Unity folders:

- `Assets/`: Scripts, sprites, audio, and scenes.
- `Packages/`: Project dependencies.
- `ProjectSettings/`: Unity engine configurations.

## 🚀 How to Run

1.  Clone the repository to your local machine.
2.  Open the project using **Unity Hub**.
3.  Navigate to `Assets/Scenes` and open the main scene.
4.  Press **Play** in the Unity Editor.

---

_Developed as a personal learning project to master 2D physics, character controllers, and the Unity animation pipeline._
