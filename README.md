# Stars, Planets and Moons

An interactive educational science toy about the Sun, Earth and Moon, and part of a wider
idea: a collection of exploratory science games sharing technology and design principles.

## Philosophy

Learning through exploration, experimentation and interaction — not quizzes. Experiences
should be enjoyable without constant text, instructions or achievement systems, accessible
to young children and interesting to older users.

## First experience: Sun, Earth and Moon

The first milestone, *Home → Earth → Night → Home*, has a working prototype (M1a, see below): start
at a stylised house in daylight, zoom out to see Earth and the Sun, rotate Earth or advance
time until that location is in darkness, then return and find it night. Seasons and the Moon
follow later. Scientific relationships stay correct while visual scale is compressed for clarity.

## Status

M0 (repository foundation) and **M1a (functional prototype)** are done. M1a is a placeholder-art prototype of the Home → Earth → Night → Home journey with a real day/night simulation; M1b will refine it. See [Docs/ProjectPlan.md](Docs/ProjectPlan.md) for what is done, the known limitations and what comes next.

## Running the prototype

1. Open the project (see *Opening the project*) and open `Assets/Scenes/HomeEarthNight.unity` (it is also the only scene in Build Settings).
2. Press Play. You start in front of a house at 10:00 local solar time, daytime, with time paused.

| Control | Action |
| --- | --- |
| Mouse wheel (scroll down = outward), `E`/`Q`, `PgUp`/`PgDn` | Travel out to space and back down |
| `End` or `X` / `Home` or `H` (or the buttons) | Go to space / go home |
| `Right`/`D` and `Left`/`A` (or `Time >>`/`Time <<`) | Faster / slower time; steps pass through paused into reverse |
| `Space` (or `Pause / Play`) | Pause or resume time |

To see the main journey: press `D` a couple of times, scroll out to space, watch the red home marker move from the lit side to the dark side, press `Home` and the house is lit by its windows and street lamp. Reverse time to bring the day back.

Rebuilding the scene from code is possible via *Stars, Planets and Moons > Rebuild Home-Earth-Night Scene*, but this overwrites the scene (see `Docs/ArchitectureNotes.md`). EditMode tests are in the Test Runner under `StarsPlanetsAndMoons.Tests.EditMode`.

## Requirements

- Unity **6000.6.7f1** (Universal Render Pipeline)
- [NotoriousPIG Framework](https://github.com/SamuelJay/NotoriousPIGFramework) **v0.3.0**, installed from Git via `Packages/manifest.json`:
  `https://github.com/SamuelJay/NotoriousPIGFramework.git?path=/Package#v0.3.0`
  The repository is **private**: Unity needs Git credentials with access to it.

## Opening the project

The Unity project is in the `StarsPlanetsAndMoons/` subfolder of this repository. Add that
folder in Unity Hub and open it with the version above. Unity resolves the framework package on first open.

## Bootstrap

The framework supports a project-owned profile at
`Assets/Resources/Framework/BootstrapData/BootstrapData.asset` (create with
*Assets > Create > Framework > Bootstrap Data*). **It has deliberately not been created**:
M1a registers no framework services, so on Play the framework just logs the harmless
`No BootstrapProfile found.` warning. Create it when a service is first needed.

## AI tooling: MCP for Unity (optional)

[MCP for Unity](https://github.com/CoplayDev/unity-mcp) **v10.3.0** (free, MIT) lets Claude Code and Codex
drive the open Unity Editor. The Unity package is pinned in `Packages/manifest.json`; the matching
server version (`mcpforunityserver==10.3.0`) is pinned in the repo-scoped client configs:

- Claude Code: `.mcp.json` (approve the project server when prompted)
- Codex: `.codex/config.toml` (project must be trusted in Codex)

Both launch the server with `uvx`, so [`uv`](https://docs.astral.sh/uv/) must be on `PATH`. The Unity
Editor must have this project open, with the package resolved, for tools to connect
(check *Window > MCP for Unity*). Keep the package tag and server version in step when upgrading.

## Documentation

- [Project plan and roadmap](Docs/ProjectPlan.md)
- [Architecture notes](Docs/ArchitectureNotes.md)
