# Computer Graphics

A small top-down action prototype built with C# and OpenTK, developed as a student project for the Computer Graphics course.
The goal is simple: survive long enough to find the exit in each level while enemies relentlessly hunt you down.
## Description

### Core Loop (MVP)

1. Spawn into a map that’s larger than the screen; the camera follows the player
2. Enemies spawn and chase the player; you must fight or evade them
3. Locate the exit somewhere in the level to advance
4. Difficulty scales with each level
   - More enemies and/or tougher stats
   - Procedurally randomized map border each level
   - Map size increases at higher levels

### Possible Extensions

1. Procedural obstacles scattered across the map
2. Loot chests with items or resources
3. Shop between levels for new weapons, upgrades, or utilities
4. Enemy variety: multiple species with distinct attack patterns and behaviors


## Authors

- Alexander Korn
- Silvan Schalkowski

## Requirements

- OpenTK 4.9.4
- .NET SDK 9.0+

## Assets in this project

- [Topdown map tileset by Cainos](https://cainos.itch.io/pixel-art-top-down-basic)
- [Moon-mage asset by RusKom](https://ruskom.itch.io/moon-mage-asset)
- [Fire/Explosion assets by ppeldo](https://ppeldo.itch.io/2d-pixel-art-game-spellmagic-fx)
- [Slime blob by the_game_house](https://the-game-house.itch.io/slime-assets-free-character-spritesheets)
- [Flavina Font](https://www.1001fonts.com/flavina-font.html)
