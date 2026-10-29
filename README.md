# Stars, Planets and Moons

An interactive educational science toy about the Sun, Earth and Moon, and part of a wider
idea: a collection of exploratory science games sharing technology and design principles.

## Philosophy

Learning through exploration, experimentation and interaction — not quizzes. Experiences
should be enjoyable without constant text, instructions or achievement systems, accessible
to young children and interesting to older users.

## First experience: Sun, Earth and Moon

The first milestone (planned, **not yet implemented**) is *Home → Earth → Night → Home*: start
at a stylised house in daylight, zoom out to see Earth and the Sun, rotate Earth or advance
time until that location is in darkness, then return and find it night. Seasons and the Moon
follow later. Scientific relationships stay correct while visual scale is compressed for clarity.

## Status

Milestone M0 (repository foundation). No gameplay, scenes or simulation exist yet.

## Requirements

- Unity **6000.6.7f1** (Universal Render Pipeline)
- [NotoriousPIG Framework](https://github.com/SamuelJay/NotoriousPIGFramework) **v0.3.0**, installed from Git via `Packages/manifest.json`:
  `https://github.com/SamuelJay/NotoriousPIGFramework.git?path=/Package#v0.3.0`
  The repository is **private**: Unity needs Git credentials with access to it.

## Opening the project

The Unity project is in the `StarsPlanetsAndMoons/` subfolder of this repository. Add that
folder in Unity Hub and open it with the version above. Unity resolves the framework package on first open.

## Bootstrap

The framework requires a project-owned profile at
`Assets/Resources/Framework/BootstrapData/BootstrapData.asset` (create with
*Assets > Create > Framework > Bootstrap Data*). **It has not been created yet**; without
it the framework only logs a warning and registers no services. It will be added when the
first milestone determines which services are needed.

## Documentation

- [Project plan and roadmap](Docs/ProjectPlan.md)
- [Architecture notes](Docs/ArchitectureNotes.md)
