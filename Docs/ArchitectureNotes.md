# Architecture Notes

Current architecture (as implemented in M1a) plus unresolved decisions. Anything not
described as implemented is an intention only and should not be built speculatively.

## Framework

The project uses the NotoriousPIG Framework (`com.notoriouspig.framework`, v0.3.0) as its
common foundation, installed as a Git package pinned to a tag — never copied into the
project, never edited in `Library/PackageCache`.

What it provides: app/scene-scoped `Services`, a project-owned `BootstrapData` profile,
and optional `IEvents` and `IScenes` modules. It deliberately has no gameplay managers,
persistence, loading screens or base classes. Its docs say bootstrap setup is manual
(no editor tool yet).

Rules for this project:

- Respect framework boundaries; do not duplicate its functionality or modify it to suit this project.
- Register only services this project concretely needs. Events/Scenes are opt-in by registration; none are registered yet.
- Reusable code developed here should be written so it *could* be extracted later, but no new packages are created prematurely.
- The NotoriousPIG Character package is not used.

### Bootstrap (deliberately not done in M1a)

The framework loads `Assets/Resources/Framework/BootstrapData/BootstrapData.asset`
(created via `Assets > Create > Framework > Bootstrap Data`). If missing, it logs
`No BootstrapProfile found.` and runs with zero app services. M1a registers no framework services (the simulation and views are plain scene components referenced from the scene), so no profile exists and the framework only logs a harmless warning. Create it when a service is genuinely needed.

## Simulation versus presentation (implemented in M1a)

Code lives in `Assets/Scripts` (assembly `StarsPlanetsAndMoons.Runtime`).

**Simulation** (`Scripts/Simulation`)

- `EarthModel` — pure static maths. Simulation frame: +Y is the north pole, the Sun is at `EarthModel.SunDirection` (+Z), Earth rotates eastward once per 24 h, and at t = 12 h the prime meridian faces the Sun. It provides the Earth rotation, the surface direction and frame of a latitude/longitude, local solar time, and the Sun's direction and elevation at a surface point. Unit-tested.
- `SolarSimulation` — the single authoritative state: simulated hours, the time rate (hours per real second) and the home latitude/longitude. Everything else (Earth rotation, home direction, Sun elevation, local Sun direction) is derived on demand via `EarthModel`. It runs at execution order -100 so every view sees the new time in the same frame, and it knows nothing about cameras or views.

**Presentation** (`Scripts/Presentation`) — readers of the simulation only

- `LocalView` (on `LocalRoot`) — points the local sun light at the Sun's direction in the home's east/up/north frame, and sets light intensity, ambient, fog and the night lights from the Sun elevation.
- `EarthView` (on `PlanetRoot`) — rotates `Earth` by the simulation's Earth rotation, pins `HomeMarker` to the surface at the home latitude/longitude, orients the Sun rig and the planetary Sun light, and keeps the marker a readable size.
- `JourneyCamera` — owns travel only. It never writes to the simulation.

**Controls** (`Scripts/Controls`) — `PrototypeControls` maps Input System actions (`Assets/Input/Journey.inputactions`) onto `JourneyCamera` and `SolarSimulation` commands; `ControlsHud` shows status. The UI buttons call the same methods, so adding touch or controller bindings later means editing the actions asset, not the scene logic.

Invariants this keeps:

- Local and planetary illumination come from the same functions of the same state, so they cannot disagree about whether it is day (checked at runtime: both agree to ~1e-6).
- Time is a value, not something accumulated inside views, so reversing time reverses everything exactly.
- Transitions move only the camera and which root is active.

## Representation: hybrid with an obscured handoff (implemented in M1a)

This is approach D/B from the project plan in its simplest form.

- Two presentation roots in one scene, only one active at a time: `LocalRoot` (metres, flat ground disc, sky, fog, clouds) and `PlanetRoot` (Earth radius 100 units, parked at y = 20000 so the two never overlap in the Scene view). They are not scaled to one another. Earth is a compressed model, and the Sun is only 4 Earth radii away, with parallel beams as the honest direction cue.
- A single camera and a single travel parameter `Zoom` (0 = at the house, 1 = Earth from space) with a fixed handoff at 0.5.
  - Local half: an orbit around the house. The distance grows exponentially from 18 m to 1.8 km while the camera pitches from nearly horizontal to straight down, north up.
  - Planetary half: an orbit around Earth. The altitude grows exponentially from 0.2 to 2.2 Earth radii, and the camera direction swings from "straight above the home, north up" to a fixed viewpoint in the Sun-fixed frame, aiming slightly sunward so the Sun is in frame.
  - Both halves meet in the same situation (above the home, looking down, north up), so the heading stays coherent and a return trip arrives at exactly the same camera pose.
- The swap happens at `Zoom == 0.5`, hidden by a white-out overlay (alpha is 1 there and falls off over ±0.1 of travel). The overlay colour follows daylight at the home, so it is cloud-white by day and dark by night. The swap is a pure function of `Zoom`, so repeated travel is consistent and there is no hysteresis state.
- The background (procedural skybox vs. black) and clip planes are set per half.
- In the planetary half the camera is fixed in the inertial frame, so Earth truly rotates under it. The early part of the planetary approach rides with the home location.

Why this and not a single literally scaled world: there are no huge-world precision problems and no second representation to keep in step. The cost is the visual handoff (see the limitations in `ProjectPlan.md`).

## Zoom mode versus Fly mode

`JourneyCamera` is a guided path (Zoom mode), and the only thing coupled to it is the camera pose. A free-flight mode would be a different camera controller that writes the same camera and chooses which root is active; the simulation and both views would not change. Fly mode is not implemented.

## Generic entity system — open question

Whether the wider framework could benefit from a general entity abstraction beneath or
alongside a Character system. **Undecided; not assumed required; not implemented.**

### 1. What already covers it

- Unity's `GameObject` + components already are a generic entity/composition model.
- Prefabs, ScriptableObjects, tags/layers and `Transform` hierarchy cover identity, data, grouping and placement.
- The framework's `Services` covers discovery of singletons/scoped systems; `IEvents` covers decoupled notification.

### 2. Possibly genuine shared responsibilities

Candidates (celestial bodies, inspectable objects, simulated physical objects, vehicles,
animals/characters, evolution organisms): a stable identity/id; a display name or
inspection hook; spawn/despawn lifecycle; being a target of selection or camera focus;
save/restore of state. Most of these may be handled adequately by components.

### 3. Why a base class or hierarchy could hurt if premature

- These objects share little behaviour; a common base becomes either empty or a grab-bag.
- Inheritance couples unrelated projects (planets vs. organisms) to one shape and one release cadence.
- Early abstraction guesses requirements wrongly and is costly to unwind across projects.
- It risks duplicating Unity's component model and fighting the framework's "no convenience base classes" stance.

### 4. Triggers to revisit

- Two or more projects independently re-implement the same non-trivial identity/selection/lifecycle code.
- A concrete feature (e.g. one inspector, one focus/select system, shared save) needs to treat different object kinds uniformly.
- Duplication is observed, not predicted.

### 5. Composition and narrow interfaces

If shared needs emerge, prefer small single-purpose pieces: e.g. `IInspectable`, `IFocusTarget`,
or an identity component — each adopted independently, discovered via `GetComponent` or the
framework's services, with no mandatory common root type. Objects opt into only what they need.

## Decisions taken in M1a

- Local/planetary representation: hybrid with an obscured handoff (above).
- Framework services: none registered; no bootstrap profile.
- Assembly definitions: `StarsPlanetsAndMoons.Runtime`, `StarsPlanetsAndMoons.Editor` and `StarsPlanetsAndMoons.Tests.EditMode`. They do not reference the framework assembly because nothing uses it yet.
- Input: the new Input System with a project-owned actions asset; desktop bindings only.

## Open decisions

- Input platforms and the final control scheme (touch, controller).
- Whether and how a later Fly mode shares the camera.
- How M3 (axial tilt, orbit) extends `EarthModel`; the model is intentionally only the M1a subset.

## Editor tooling

`Assets/Editor/HomeEarthNightSceneBuilder.cs` (menu *Stars, Planets and Moons > Rebuild Home-Earth-Night Scene*) generated the scene, the placeholder materials, the gable-roof mesh and the input actions, and `PlaceholderTextures.cs` generated the placeholder Earth and cloud textures. They are kept so the placeholder content is reproducible. Running the menu item **overwrites** `Assets/Scenes/HomeEarthNight.unity`, so do not use it after editing the scene by hand. Delete both once the scene is authored by hand.
