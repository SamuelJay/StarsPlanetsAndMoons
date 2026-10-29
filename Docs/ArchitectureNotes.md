# Architecture Notes

Current intentions and unresolved decisions. Nothing here is implemented, and nothing
should be built speculatively from it.

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

### Bootstrap requirement (not yet done)

The framework loads `Assets/Resources/Framework/BootstrapData/BootstrapData.asset`
(created via `Assets > Create > Framework > Bootstrap Data`). If missing, it logs
`No BootstrapProfile found.` and runs with zero app services. The asset is created in the
Editor and its content depends on which services M1 needs, so it is deferred until then.

## Simulation versus presentation

Intended separation:

- **Simulation state** — scientifically meaningful, view-independent: time/date, Earth rotation angle, axial tilt (later orbit position), Sun direction, the chosen surface location (latitude/longitude), derived illumination at that location.
- **Presentation** — how that state is drawn: apparent sizes, compressed distances, cameras, transitions, lighting rigs, placeholder art.

Principles:

- Earth's rotation and a location's illumination must not depend on how large Earth appears on screen.
- Local and planetary views are both *readers* of the same state, so they cannot disagree about whether it is day.
- Time is adjustable (forward/backward, scrubbed); views derive from time rather than accumulating their own.
- Transitions are a presentation concern and move no simulation state.

This is a design guideline, not authorisation to build a simulation framework. A plain
class or component holding a few values may be all M1 needs.

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

## Open decisions

- Local/planetary representation approach (see ProjectPlan.md, plausible approaches A–D).
- Input platforms and control scheme.
- Which framework services M1 registers (likely none beyond optionally `IEvents`/`IScenes`).
- Whether to introduce own assembly definitions (if so, reference `NotoriousPIG.Framework.Runtime` explicitly).
