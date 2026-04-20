# 🚢 CRT Manager

> A Unity-based canal network management game inspired by the Canal & River Trust.  
> Players manage lock operations, boat traffic, water levels, and canal infrastructure across the UK's real waterway network.

---

## 📋 Table of Contents

- [Project Overview](#-project-overview)
- [Current State](#-current-state)
- [Architecture](#-architecture)
- [Systems Built](#-systems-built)
- [Data Pipeline](#-data-pipeline)
- [What's Left To Do](#-whats-left-to-do)
- [Known Issues & Technical Debt](#-known-issues--technical-debt)
- [File Structure](#-file-structure)

---
## 🗺 Live Roadmap

*This section will be automatically updated by GitHub Actions based on active Issues.*
## 🗺 Project Overview

CRT Manager is a real-time canal management simulation. The player takes the role of a Canal & River Trust network manager — scheduling boat movements, operating locks, balancing water levels, dispatching maintenance crews, and keeping the network running efficiently.

The game uses **real GeoJSON data** from the Canal & River Trust to generate the UK's actual canal network as playable spline-based paths in Unity.

---

## ✅ Current State

| System | Status | Notes |
|---|---|---|
| Canal Splines | ✅ Complete | All CRT routes generated from real data |
| Boat Movement | ✅ Functional | Physics-based spline following |
| Lock Handover | ✅ Functional | Inbound/outbound trigger system |
| Junction Routing | ✅ Functional | 3-branch junctions with route logic |
| Mooring System | ⚠️ Partial | Mooring logic works; speed control WIP |
| Camera System | ✅ Functional | WASD pan, Q/E rotate, scroll zoom |
| Boat UI | ✅ Functional | Inspector panel with live data binding |
| Game Time Cycle | ✅ Functional | Day/night time ticks via ScriptableObject |
| Lock Operations | 🔴 Not Started | No player-controlled lock gate logic yet |
| Water Levels | 🔴 Not Started | Planned system |
| Maintenance / Incidents | 🔴 Not Started | Planned system |
| Economy / Budget | 🔴 Not Started | Planned system |

---

## 🏗 Architecture

```
CRT Manager
├── Boats
│   ├── BoatController        — spline movement, physics, spline switching
│   ├── BoatData              — ScriptableObject: stats, state, flags
│   ├── BoatClickable         — raycast click → UI
│   └── BoatUiController      — UXML panel, live data binding
│
├── Canal Network
│   ├── SplineData            — per-segment metadata (name, region, nav status)
│   ├── CanalData             — per-canal metadata (segment count, name)
│   ├── HandoverTrigger       — segment-to-segment spline handoff
│   └── CanalPathConstraint   — locks camera/objects to spline XZ
│
├── Locks
│   ├── LockData              — identity + spline references
│   ├── LockManager           — scene-level lock management
│   ├── LockHandoverTrigger   — routes boats into/out of lock splines
│   ├── MooringSpot           — mooring state + inbound/outbound logic
│   └── MooringHandoverTrigger — trigger that calls MooringSpot.Trigger()
│
├── Junctions
│   ├── JunctionData          — spline refs, route/section dictionaries
│   ├── JunctionManager       — orchestrates trigger branch numbering
│   └── JunctionHandoverTrigger — per-branch collision → route selection
│
├── Camera
│   ├── CameraController      — WASD pan, Q/E rotate, scroll zoom
│   ├── CameraPathLock        — snaps camera to nearest canal spline
│   └── CanalBungee           — elastic pull toward spline (all 3 axes)
│
├── Game Management
│   ├── GameManager           — time tick, end-of-day handler
│   ├── GameManagerData       — ScriptableObject: time, date
│   └── NPCData               — ScriptableObject: crew identity, economy, skill
│
└── Editor Tools
    ├── CanalSplineTool       — bulk JSON → spline hierarchy generator
    ├── LockJsonToSpline      — lock point data → sorted spline
    ├── LockJsonToMap         — lock point data → spawned cube map
    ├── BoatControllerEditor  — custom inspector with progress bar
    ├── CanalBuilderLogic     — in-editor Catmull-Rom canal drawing tool
    └── XZSplineTool          — custom spline editor, Y-locked to XZ plane
```

---

## 🚤 Systems Built

### Boat Movement

Boats follow Unity Splines using a hybrid physics approach — a Rigidbody is pulled toward the spline by a gravity-like force, while a separate forward force pushes it along the tangent. This means boats have physical presence and can interact with each other.

- Boats maintain a `progress` float (0–1) along their current spline
- On reaching progress 0 or 1, `Splineswitch()` promotes `queuedSegment` → `currentSegment`
- A `pendingSegment` slot allows look-ahead routing (e.g., through a junction then onto the next canal)
- Boats that detect another boat ahead via SphereCast will shift to an offset lane to pass

### Lock & Mooring System

Locks are handled by a chain of triggers:

1. `LockHandoverTrigger` detects a boat approaching the lock and sets `NeedToMoor = true`, queuing the lock spline
2. `MooringHandoverTrigger` (at each end of the lock widening) calls `MooringSpot.Trigger()`
3. `MooringSpot` decides inbound vs outbound: inbound routes the boat to the mooring or straight spline; outbound clears state and restores speed

Lock prefabs (`Short Widening`, `Lock 5m change`) are self-contained with their own mooring splines, navigation splines, and trigger volumes baked in.

### Junction Routing

Three-branch junctions use a `JunctionData` ScriptableObject holding all six possible route splines (1→2, 1→3, 2→3) and the three next-section splines.

When a boat enters a `JunctionHandoverTrigger`, the branch number is looked up against the boat's route (currently hardcoded as "Ruby" for testing), and the correct junction + pending splines are queued.

### Canal Network Generation

All UK canal routes are generated from real Canal & River Trust GeoJSON data through a 5-stage Python pipeline:

| Stage | Script | Output |
|---|---|---|
| 1 | `Process1-dataflattening.py` | Flatten MultiLineString → LineString |
| 2 | `Process2-chordstometers.py` | Convert lon/lat → metric XZ coords |
| 3 | `Process3-Lockdata.py` | Enrich segments with lock counts |
| 4 | `Process4-Local starts.py` | Make each segment origin-relative |
| 5 | `Process5-Split.py` | Split into per-canal JSON files |

The Unity `CanalSplineTool` editor window then reads these files, generates a spline hierarchy in the scene, attaches `SplineData` metadata, links `previousSegment` and `nextSegment` references, and optionally spawns `HandoverTrigger` prefabs at each segment.

---

## 🗃 Data Pipeline

```
CRT GeoJSON (raw)
    ↓ Process1: flatten Multi types
    ↓ Process2: lon/lat → meters (Web Mercator, origin: Manchester)
    ↓ Process3: join lock count per segment from lock dataset
    ↓ Process4: make each segment local (first coord = [0, 0])
    ↓ Process5: split by canal name into individual JSON files
    ↓
Per-Canal JSON files  (e.g. "Wyrley & Essington Canal.json")
    ↓ CanalSplineTool (Unity Editor)
    ↓
Scene Hierarchy:
  Canal/
    Splines/
      Wyrley & Essington Canal/
        WF-001  [SplineContainer + SplineData + SplineExtrude]
        WF-002  [SplineContainer + SplineData + SplineExtrude]
        ...
```

---

## 🔜 What's Left To Do

### 🔴 Core Gameplay — Not Started

These are the primary pillars of the game that don't exist yet.

#### Lock Operations (Player-Controlled)
The lock system currently automates everything — boats just pass through. The actual gameplay of operating a lock needs to be built:
- Gate open/close actions (triggered by player input or assigned crew)
- Water fill/drain mechanics with a time delay
- Lock "state machine" — empty/filling/full/emptying/ready
- Boat cannot proceed until the lock cycle is complete
- Visual feedback — water level rising, gate animations

#### Water Level Management
Canal water levels are affected by usage, weather, and leakage. The player needs to:
- Monitor reservoir levels feeding each section
- Open/close sluice gates to balance levels
- React to drought warnings or overflow events
- Different sections operate at different heights (pounds) separated by locks

#### Maintenance & Incidents
- Random incidents: lock gate failure, embankment leak, debris blockage
- Maintenance crew dispatch — NPCs travel to the incident location
- Repair time + cost
- Backlog system if too many incidents stack up
- Prioritisation decisions (critical vs minor)

#### Economy & Budget
- Operating income from boat licenses and lock fees
- Staffing costs
- Maintenance spend
- Grant funding vs revenue balance
- Budget screen / end-of-period report

---

### 🟡 Systems — Partially Built, Need Completing

#### Route Planning (Boats)
Currently hardcoded to `"Ruby" → branch 2` for testing. Needs:
- Proper route data structure per boat (origin → destination → waypoints)
- Route lookup at each junction using the actual boat's destination
- UI for viewing/editing a boat's planned route

#### Mooring & Lock Speed Control
The speed halving/doubling in `MooringSpot` is a rough placeholder. Needs:
- Boats properly stop at a mooring point and wait for lock readiness
- A queue system if multiple boats are waiting at one lock
- Boats resume when signalled, not just when they hit the outbound trigger

#### NPC Crew System
`NPCData` ScriptableObject exists but isn't connected to anything. Needs:
- Crew assigned to boats and to lock stations
- Crew skill level affecting operation speed
- Fatigue / shift management

#### Camera System
The canal path lock (`CameraPathLock`, `CanalBungee`) is functional but the correct one for the game hasn't been finalised. Needs:
- One definitive camera controller (current setup has 3 competing scripts)
- Smooth transition when clicking between boats
- Overview zoom vs detail zoom modes

---

### 🟢 Polish & Infrastructure — Future Work

#### Saving & Loading
No persistence exists yet. Needs:
- ScriptableObject state serialisation
- Save file per playthrough
- Scene state preservation (boat positions, lock states, water levels)

#### UI
- Main HUD (time, date, budget summary, alert feed)
- Lock operation UI (gate controls, water gauge)
- Network overview map
- Incident notification system
- End-of-day summary screen

#### Audio
- Ambient water sounds
- Lock gate SFX
- Engine sounds per boat
- Alert sounds for incidents

#### Visual Polish
- Water shader with canal-appropriate flow
- Lock gate models and animation
- Boat models (current build uses placeholder geometry)
- Day/night lighting cycle connected to the game time system

#### Multiplayer / Async
Long-term stretch goal — shared canal network where multiple players manage different sections.

---

## ⚠️ Known Issues & Technical Debt

| Issue | Severity | Notes |
|---|---|---|
| `BoatUiController` uses `UnityEditor.SerializedObject` | High | This will break in a build — needs runtime binding |
| Route dictionary hardcoded to `"Ruby"` | High | Junction routing is test-only |
| `boat.speed *= 0.5f` / `*= 2f` in MooringSpot | Medium | Multiplying speed repeatedly will drift — needs absolute value |
| Three competing camera scripts on the rig | Medium | `CameraPathLock`, `CanalBungee`, `CanalPathConstraint` overlap |
| `SplineUtility.GetNearestPoint` passes local coords without transform in `CanalPathConstraint` | Medium | Bug — world pos passed as if it were spline-local |
| No null checks on `BoatController.GetComponent<BoatController>()` in triggers | Low | Will throw NullRef if non-boat colliders hit triggers |
| `JunctionManager.splineControl` duplicates logic from `JunctionHandoverTrigger.splineControl` | Low | Refactor into shared utility |

---

## 📁 File Structure

```
Assets/
├── Scripts/
│   ├── Boat/
│   │   ├── Boat Controller.cs
│   │   ├── Boat Clickable.cs
│   │   ├── Boat Ui Controller.cs
│   │   ├── BoatData.cs
│   │   └── NPC Data.cs
│   ├── Cam and player/
│   │   ├── CameraController.cs
│   │   ├── CameraPathLock.cs
│   │   ├── CanalBungee.cs
│   │   └── PlayerInteraction.cs
│   ├── Junctions/
│   │   ├── Juntion Data.cs
│   │   ├── Juntion Manager.cs
│   │   └── JuntionHandoverTrigger.cs
│   ├── Mooring Handover Trigger/
│   │   ├── MooringSpot.cs
│   │   └── Mooring Handover Trigger.cs
│   ├── Generated canals/
│   │   └── CanalData.cs
│   ├── Editor/
│   │   ├── CanalSplineTool.cs
│   │   ├── CanalBuilderLogic.cs
│   │   ├── LockJsonToSpline.cs
│   │   ├── Lock Json To Map.cs
│   │   ├── BoatControllerEditor.cs
│   │   └── XZSplineTool.cs
│   ├── Lock Data.cs
│   ├── LockManager.cs
│   ├── LockHandoverTrigger.cs
│   ├── CanalPathConstraint.cs
│   ├── HandoverTrigger.cs
│   ├── SplineData.cs
│   ├── Special Data.cs
│   ├── UIManager.cs
│   ├── gameManager.cs
│   └── GameManager Data.cs
│
├── Canals/
│   ├── Lengths/
│   │   ├── Process1-dataflattening.py
│   │   ├── Process2-chordstometers.py
│   │   ├── Process3-Lockdata.py
│   │   ├── Process4-Local starts.py
│   │   ├── Process5-Split.py
│   │   └── test1/           ← per-canal JSON files
│   └── Locks/
│       └── [per-canal lock JSON files]
│
├── Prefabs/
│   ├── Short Widening.prefab
│   └── Lock 5m change.prefab
│
├── Scripts/
│   ├── Canal 20m.prefab
│   └── MainGameData.asset
│
└── UI Toolkit/
    ├── Boat Dats.uxml
    ├── HUD.uxml
    ├── Boats.uss
    └── Standard.uss
```

---

*Built in Unity · Spline data derived from Canal & River Trust open datasets · Project by [Your Name]*
