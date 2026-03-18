# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Top-down 2D Mech roguelike in Godot 4.6 / C# (.NET 8.0). Vampire Survivors-style auto-attacks with a Bio Prototype-inspired energy chain system — attacks charge other attacks on hit/kill.

## Build & Run

```
dotnet build Project187.csproj
```

Run: open in Godot Editor (v4.6.1+ with Mono/C# support), press F5. Main scene: `Scenes/Main/Main.tscn`.

## Architecture

### Attack & Energy Chain System
Each attack has an energy pool. It fires when `CurrentEnergy >= EnergyThreshold`. Generators fill the pool:
- `TimedEnergyGenerator` — adds energy per second
- `OnHitEnergyGenerator` — adds energy when a named attack hits an enemy
- `OnKillEnergyGenerator` — adds energy when a named attack kills an enemy

This enables chains: MachineGun (timed) → hits enemy → charges ShockPulse → fires AOE.

`AttackManager` owns all `AttackInstance` nodes and generators. It initializes attacks in two passes: first registers all attacks by ID, then wires generator subscriptions (so cross-attack references resolve correctly).

### Adaptation (Modifier) System
`IAdaptation` is a plain C# interface with three hooks called in order:
1. `OnFire(ref AttackFireParams)` — mutate spawn params before projectiles are created
2. `OnHitEnemy(ref HitResult)` — mutate hit result after collision
3. `ModifyStats(ref AttackRuntimeStats)` — pure stat multipliers via `GetComputedStats()`

Each `AttackInstance` has typed adaptation slots (`AdaptationCategory` mirrors `AttackType`). Slot enforcement is in `AttackManager.TryEquipAdaptation()`.

### Data vs Runtime
- **Data layer:** `AttackData`, `EnergyGeneratorData`, `AdaptationData` — Godot `Resource` subclasses, authored as `.tres` files
- **Runtime layer:** `AttackInstance`, `EnergyGeneratorBase`, `ProjectileNode`, `AreaEffectNode` — nodes in the scene tree

### Bootstrap
`Main.cs` creates attacks programmatically if `PlayerStats.StartingAttacks` is empty, so the game runs without `.tres` assets configured. To use data-driven attacks instead, create `.tres` files in the editor and assign them to `PlayerStats.StartingAttacks`.

### Key Gotchas
- C# `[GlobalClass]` Resources **cannot** be embedded as inline sub-resources in `.tscn` files — use external `.tres` files or create them in code
- `CharacterBody2D` must use `MotionMode = MotionModeEnum.Floating` for top-down movement
- Scene order in `Main.tscn` matters: Player must be before Enemies so the "Player" group is populated when enemies call `_Ready()`
- Use `Polygon2D` for placeholder visuals — `Sprite2D` without a texture renders nothing

## Folder Structure

```
Scripts/
  Core/           GameEnums.cs, IAdaptation.cs (+ 3 ref structs)
  Resources/      AttackData.cs, EnergyGeneratorData.cs (+ subclasses), AdaptationData.cs
  Attacks/        AttackInstance.cs (abstract), ProjectileAttack.cs, AreaAttack.cs, BeamAttack.cs, MeleeAttack.cs
  EnergyGenerators/  TimedEnergyGenerator.cs, OnHitEnergyGenerator.cs, OnKillEnergyGenerator.cs
  Projectiles/    ProjectileNode.cs, AreaEffectNode.cs
  Adaptations/    ProjectileAdaptations/, AreaAdaptations/, UniversalAdaptations/
  Player/         Player.cs, PlayerStats.cs, AttackManager.cs
  Enemies/        BasicEnemy.cs, EnemyStats.cs
  UI/             HUD.cs
Scenes/           Main/, Player/, Enemies/, Attacks/, UI/
Resources/        Attacks/, Adaptations/  (empty — populate via editor)
```

## Collision Layers
- Layer 2 (value 2): Player body
- Layer 4 (value 8): Enemy bodies
- Layer 5 (value 16): Projectiles/area effects — mask targets layer 4
