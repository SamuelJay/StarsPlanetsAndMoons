# Project Plan

Status: M0 and M1a are complete (see *Current status*). Everything else described here is planned, not implemented.

## Vision

A collection of interactive educational games and science toys, initially built as
**separate Unity projects** that share reusable technology and design principles. They
may eventually share a launcher or identity; no suite platform is designed or planned now.

Candidate experiences:

- Solar system, astronomy and planetary phenomena (this project).
- Physics: gravity, motion, forces, relativity and other experiments.
- Animals: explore environments by controlling animals and experiencing their behaviour.
- Evolution: eventually revisit the old ReptileX evolution-simulation concept.
- Dinosaur extinction and major events in Earth's history.
- Anatomy and how things work, such as the human ear.

### Philosophy

Learning through exploration, experimentation and interaction — not quizzes. Experiences
should be enjoyable without constant text, instructions or achievement systems. The
inspiration is a three-year-old's questions, but the concept is not limited to preschoolers.

## First project: Sun, Earth and Moon

Begins with how Earth's rotation causes day and night.

Eventual subjects (none are first-release requirements): day/night; seasons; Earth's orbit
and axial tilt; Moon phases and eclipses; the wider solar system and planet comparison;
scale, distance and gravity; major events such as asteroid impacts and dinosaur extinction.

### Initial playable milestone: Home → Earth → Night → Home

A child can:

1. Start at a recognisable, stylised surface location (house, road, car, simple surroundings) in daytime.
2. Zoom or transition outward until Earth is visible as a planet.
3. Continue far enough to see the direction of the Sun and which side of Earth is lit.
4. Interact with Earth's rotation, or advance time, until the original location passes into darkness.
5. Return to the same location and find it night.
6. Repeat and reverse to explore the relationship.

The concept must be communicated visually, without needing to read explanatory text. The
house and surroundings are a contextual anchor, not the seed of a city or open world.
Placeholder geometry is acceptable; art is addressed separately.

### Representation and scale

A previous attempt at a literally scaled solar system had serious readability and
navigation problems. Three concerns are kept distinct:

| Concern | Rule |
| --- | --- |
| Scientific state and relationships | Must be correct. |
| Visual scale and spatial representation | May be exaggerated or compressed for clarity. |
| Interaction and camera presentation | Designed for comprehension, not realism. |

Constraints:

- Day/night is caused by Earth's rotation relative to sunlight.
- Seasons (later) are explained by axial tilt and changing solar illumination, never by distance to the Sun.
- A surface location stays consistent across local and planetary representations.
- Returning from planetary view preserves the corresponding time and daylight state.
- Transitions never require literal travel through immense distances.
- No huge-world floating-point workarounds just to keep one continuous coordinate scale.

#### Plausible approaches (no decision made)

- **A. Single compressed-scale scene.** Earth, Sun and a local patch share one scene at artistic scales; the camera moves between them.
- **B. Separate local and orbital scenes/rigs** driven by shared state. A local surface view and a planetary view are different presentations; transitions cross-fade, dolly or morph between them.
- **C. Layered cameras / floating origin.** Multiple cameras render local and space layers; transition blends between them.
- **D. Hybrid.** Local patch rendered as a small object on a (visually compressed) Earth that the camera approaches, handing over to a local rig at close range.

#### Questions for an implementation spike

- Which approach makes "same location, same lighting state" easiest to guarantee?
- How is the surface location mapped onto the Earth sphere so it is recognisable from orbit (marker, glow, tiny house)?
- Can the Sun's direction/lighting be one source of truth for both views (one directional light vs. one per view)?
- How does the transition read to a child — continuous zoom, cut with cross-fade, or something else?
- What does time control look like (drag Earth, dial, scrub) and how does it behave while zoomed out vs. in?
- Do exaggerated sizes/distances in the planetary view mislead (e.g. Sun too close)? What cues keep it honest?
- Performance and precision on target devices.

### Interaction principles

- Intuitive for young children; still interesting for older users.
- Visual discovery leads; minimal reading.
- Simple, tactile interactions that invite experimentation and repetition.
- No unnecessary menus, objectives, rewards or artificial progression.
- Playful presentation without sacrificing accuracy.
- Input platforms and control schemes are not yet fixed.

## Roadmap

These milestones are a direction, not a commitment to complete every feature.

**M0 — Repository foundation**
- Unity project recognised; Git configuration established.
- Framework dependency integrated.
- Vision and architectural questions documented.

**M1 — First playable day/night experience**
- Stylised surface location; Earth and Sun representation.
- Working day/night relationship.
- Movement between local and planetary views.
- Interactive time/rotation controls.
- Return to the same location in the correct lighting state.

**M2 — Experience refinement**
- Improve transitions, readability and presentation.
- Explore accessible interaction and input.
- Playtest whether the concept is actually understood.
- Replace placeholders selectively.

**M3 — Seasons**
- Earth orbit and axial tilt; changing sunlight across a year.
- Reuse established presentation and interaction where appropriate.

**M4 — Moon**
- Moon and lunar phases; consider eclipses and further interactions.

**Later possibilities**
- Other planets and astronomical phenomena.
- Gravity and orbital experiments.
- Zooming between planetary and surface scales (e.g. asteroid impacts).
- Connections to separate physics, animal and evolution projects.

## Current status

### M0 — Repository foundation (done)

- Unity project exists (Unity 6000.6.7f1, URP).
- NotoriousPIG Framework v0.3.0 declared in `Packages/manifest.json`, resolved and compiling.
- Framework bootstrap profile **intentionally not created**: nothing in M1a needs a framework service, so the framework logs `No BootstrapProfile found.` on play (harmless).

### M1a — Functional prototype of Home → Earth → Night → Home (done)

Entry scene: `Assets/Scenes/HomeEarthNight.unity`.

Implemented:

- One authoritative simulation state (`SolarSimulation`): simulated hours, Earth rotation (24 h per revolution, eastward), a fixed Sun direction, a home latitude/longitude (52°N, 0°E) and the derived illumination of that location. No tilt, orbit, seasons or Moon.
- Local view: placeholder house, road, cars, hedges, lamp, trees, neighbouring houses and cloud puffs. Sun, ambient light, fog and night lights (windows, lamp, porch) are derived from the simulation, not toggled.
- Planetary view: textured Earth sphere with a cloud shell, a home marker pinned to the surface, a Sun body and parallel sunbeams showing the Sun direction, and lit/dark hemispheres.
- One continuous zoom journey (mouse wheel, keys or buttons): house → rising above the neighbourhood → cloud layer → Earth from space, fully reversible. The local/planetary representation swap happens at the middle of a cloud white-out.
- Time controls: pause, forward and reverse at several rates (simulated hours per real second).
- 12 EditMode tests covering the Earth/Sun model.

Verified in the Unity 6000.6.7f1 Editor (play mode, driven through the Unity MCP; the wheel and keys were exercised with queued Input System events): compilation, scene load with no missing references, Earth rotation and marker pinned to the surface, local and planetary illumination agreeing to ~1e-6, home going day → night → day when time is advanced and reversed, travel not changing simulated time, and a round trip returning to the identical camera pose and daylight state.

Known issues and limitations to carry into M1b:

- The local/planetary handoff is a white-out swap, not a visually continuous blend: below the cloud layer the ground is a flat disc, above it a textured sphere with no matching terrain. A proper cloud layer or atmosphere would hide this better.
- The space camera is fixed in the inertial (Sun-fixed) frame, so the home marker rotates out of sight behind Earth late at night (hidden when checked at 22:30 local solar time). A follow-home or orbit control would fix this.
- No touch, controller or drag-to-rotate input. The Input System bindings live in `Assets/Input/Journey.inputactions`, so these can be added without changing the code that consumes them.
- Placeholder art only: no stars, moon, atmosphere glow or post-processing. The dark hemisphere is only faintly visible against black space.
- Not tested on a mobile device; performance is untested outside the Editor.
- Not hand-played with a real mouse and keyboard in a focused Editor; see the README for manual steps.

### Next milestone

M1b (refinement of M1a, before M2): improve the handoff and atmosphere, add a follow-home/orbit camera in space, add touch and controller input, improve the readability of the dark side, and do a first playtest of whether the day/night idea is understood.

See also: [ArchitectureNotes.md](ArchitectureNotes.md).
