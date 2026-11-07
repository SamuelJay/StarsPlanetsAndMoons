# Coding Standards

**Reusable engineering and code-review standards for first-party software, including AI-assisted work.**

**Status:** Consolidated draft for adoption across repositories.  
**Scope:** All first-party code written or modified by a developer or coding agent. The engineering principles are language-independent; the C#/Unity conventions apply where relevant.  
**Purpose:** Produce code that is correct, readable, maintainable, reusable where useful, consistent with its codebase, and supported by meaningful evidence.

## 1. Engineering priorities

**Priority order: correctness → maintainability → smallest technically sound change → implementation speed.**

1. **Understand before changing.** Examine the relevant current implementation, ownership, dependencies, and intended behaviour before making structural decisions. Use current code, configuration, tests, and logs as evidence; documentation alone is not proof of runtime state.
2. **Respect the existing architecture.** Integrate with established ownership, contracts, extension points, and engine/framework mechanisms. Do not invent competing pathways, bypass the framework for convenience, or replace a working system without justification and approval.
3. **Solve the underlying problem.** A small patch is not sound if it hides an invalid state, duplicates logic, bypasses ownership, or creates predictable follow-up work.
4. **Keep changes focused.** No speculative abstractions, unrelated refactors, wholesale rewrites, or cosmetic cleanup outside the requested scope. Prefer parameterising a suitable existing system to building a replacement.
5. **Build reusable systems where the reuse is real.** Prefer clear, independently usable components with narrow contracts. Do not make ordinary project-specific behaviour generic merely to appear extensible.
6. **Do not fight the engine.** Prefer the engine's or platform's intended mechanisms over brittle workarounds.
7. **Make correctness observable.** Test the affected behaviour and its realistic regression surface. Distinguish compile-time checks, automated tests, runtime observations, and unverified assumptions. Never weaken tests just to make them pass.

## 2. Architecture and design

### 2.1 Responsibilities and ownership

- Apply Single Responsibility: each class or module should have a coherent reason to change. Separate unrelated behaviour, presentation, persistence, authoring, and infrastructure concerns when they create meaningful coupling.
- Put state and behaviour with their rightful owner. Callers should ask a component to perform an intent-level operation, not reach deeply through its internals to manipulate subordinate components.
- Preserve one authoritative path for important decisions or state. Avoid parallel implementations of the same behaviour.
- Prefer explicit dependencies and contracts over hidden global state. Do not introduce arbitrary singleton patterns or new dependency-acquisition conventions where the project already defines one.
- Avoid unnecessary mutable static state; use it only when the architecture deliberately assigns that lifetime and ownership.

### 2.2 Reuse, duplication, inheritance, and composition

- **Do not copy and paste the same meaningful logic into multiple places.** If behaviour is semantically the same and should evolve together, give it one owner (a suitable existing method, component, service, utility, or base class).
- **Use relevant existing base classes.** Do not bypass a genuine Unity/framework hierarchy or duplicate behaviour already supplied by a base class.
- **Do not create artificial inheritance.** A new base class must express a real shared contract or reusable behaviour and a sensible *is-a* relationship. Plain data models and utility classes do not need a custom base class. Prefer composition/delegation where behaviours vary independently.
- Do not extract coincidentally similar code if its semantics, ownership, or likely evolution differ. Avoid trivial helper proliferation and premature abstraction.
- Follow SOLID and object-oriented design principles as practical reasoning tools, not as a mandate to create extra interfaces and layers.

### 2.3 Focus and complexity

- Prefer clear, cohesive methods; **approximately 25 lines is a review signal, not a hard limit**. Extract meaningful subordinate responsibilities rather than creating many one-line wrappers to satisfy a count.
- Reduce unnecessary nesting using guard clauses and early returns when they improve clarity.
- Keep interfaces and APIs as small and intentional as practical. Avoid long parameter lists when a coherent context object or better ownership would clarify the contract.
- Do not build for speculative future use cases. Extend only when an actual requirement needs it.

## 3. Naming and formatting — C# / Unity

These are deliberate house conventions, even when they differ from default Microsoft C# conventions. Do not rename external APIs, generated code, or unrelated existing code to enforce them.

| Element | Standard | Example |
| --- | --- | --- |
| Classes, structs, enums, interfaces | `PascalCase` | `TacticalController`, `IEvents` |
| Methods, including Unity callbacks and tests | `PascalCase` | `ResetAttackTimer`, `OnEnable`, `RequestRejectsInvalidAttack` |
| Fields, properties, local variables, parameters | `camelCase` | `attackCooldown`, `currentTarget` |
| `const` declarations | `UPPER_SNAKE_CASE` | `MAX_RETRY_COUNT` |
| Enum members | `PascalCase` | `AttackState.Recovering` |
| Private fields | `camelCase`, no obligatory underscore prefix | `activeEnemies` |

- **Use full, descriptive words.** No cryptic abbreviations or single-letter names, including loop indices (`enemyIndex`, not `i` or `idx`). The agreed acronym exceptions for your own identifiers are **AI, UI, ID, NPC**. External API names and language-defined identifiers retain their required spelling.
- **No `var` in C#.** Declare explicit types, including when the initializer makes the type obvious.
- **K&R braces:** opening `{` on the same line; `} else {` on one line. Use braces for conditional/loop bodies, including single statements.
- **Four spaces for indentation**, unless an existing repository-specific formatter explicitly overrides this.
- Keep conditions and method signatures on one line when readable. If they become unwieldy, first consider simplifying the condition or responsibility; wrap only when genuinely needed.
- Avoid redundant `else` after a branch that returns, throws, or otherwise conclusively exits.
- Keep fields, properties, constructors/initialisation, lifecycle methods, public methods, and private helpers in the established class layout. Do not scatter fields beside their first use or reorder unrelated members as incidental cleanup.
- Prefer one public top-level type per file, with the filename matching it. Small, closely related declarations may be kept together where the project already does so.
- Use `readonly` where a field is assigned once and should not be rebound.
- Preserve the repository's established namespace, assembly, line-ending, and encoding conventions. Prefer UTF-8 and plain-text source comments.
- Test names follow the same `PascalCase` method convention; do not introduce underscore-separated test-method names.

## 4. Encapsulation, configuration, and dependencies — C# / Unity

- **Prefer `[SerializeField] private`** for Inspector-authored fields; public mutable fields require a specific reason.
- For externally readable state, prefer a controlled property such as `{ get; private set; }` where appropriate. A serialized auto-property can use `[field: SerializeField]` if that is the project's established authoring pattern.
- Expose operations and stable contracts rather than mutable internals or long chains such as `character.characterCombat.comboRunner.DoSomething()`.
- **No magic numbers or magic strings in program logic.** Name meaningful constants, configuration values, gameplay tunables, identifiers, state keys, and lookups. Use ScriptableObjects or other project data/configuration mechanisms where suitable. Ordinary structural literals (`0`, `1`) do not need ceremonial constants.
- Player-facing dialogue, narrative text, UI labels, and informative log messages are not “magic strings.” Authored game content should live in data where practical rather than being dispersed through gameplay logic.
- Follow the **repository's existing** dependency-acquisition and lifecycle rules. Unity sibling references may be cached once during initialization; cross-system references should use the project's established service/injection pattern. Do not introduce `FindObjectOfType` runtime searches or arbitrary `.Instance` chains as shortcuts.
- Treat Unity component lifecycle, object destruction/fake-null semantics, serialization, and editor/runtime boundaries deliberately. Balance subscription and teardown, and keep editor-only dependencies out of player builds.

## 5. Nulls, errors, and diagnostics

- **Do not silently hide invalid state.** If a reference is required by contract, a null is a bug: allow a clear failure or report an actionable error rather than quietly skipping the behaviour.
- Do not add redundant defensive checks for references already guaranteed by construction, validation, or ownership.
- **Expected absence is different from an invariant violation.** Optional objects, destroyed Unity objects, absent configuration entries, and recoverable states may be handled intentionally without error when the surrounding contract says absence is valid.
- Avoid null-conditional operators such as `?.` when they would silently suppress required gameplay or system behaviour. They are fine where the reference is genuinely optional (for example, some UI/presentation paths).
- Diagnostics should name the failing owner/context and the missing or invalid value, not merely say that something failed. Use the project's logging mechanism and severity policy.
- Do not swallow exceptions, invent fallback behaviour, or continue in a misleading state merely to avoid visible failures.

## 6. Comments and documentation

**Default: self-explanatory code; no new code comments or XML documentation from agents unless explicitly requested by the repository owner.**

- Names, types, contracts, and structure should express what the code does. Do not narrate fields, methods, obvious control flow, or architecture already clear from identifiers.
- Do not write implementation history, refactor explanations, migration notes, investigation findings, debugging diaries, roadmap state, phase identifiers, or an agent's reasoning into source comments. Put that information in the appropriate project docs, tests, or commit history.
- If a genuinely hidden invariant, platform limitation, authored-data constraint, or workaround cannot be expressed through clearer code, **propose the comment and explain why it is necessary rather than adding it unasked**.
- When comments are explicitly requested, ordinary `//` comments should be concise and describe the surprising **why**, not the visible **what**. One or two short lines is a useful default, not a hard ceiling.
- When XML documentation is explicitly requested, use it for meaningful API contracts and guarantees; don't restate the method body. Prefer readable text over artificially minimising line count.
- Preserve existing comments unless the current change makes them incorrect or obsolete. Do not conduct a broad comment-cleanup sweep without explicit scope.
- No emoji, decorative symbols, or obscure jargon in comments. Do not commit commented-out dead code.
- Concise Unity Inspector `[Tooltip]` attributes are allowed when they genuinely help data authoring; they are not source-code explanation comments.

## 7. Testing, verification, and change discipline

For changes made by humans or agents:

1. **Audit the relevant path** only as far as needed to understand the current behaviour and choose a safe implementation. Reuse recent, trustworthy findings; don't re-audit the whole project by habit.
2. **Implement the minimum complete solution** with the current owner and extension path; do not introduce a second path or bypass a failed contract.
3. **Verify proportionately.** Compile and run relevant tests where available. For runtime-sensitive work, observe actual runtime behaviour using appropriate tools and logs. Check nearby consumers likely to regress.
4. **Review the diff.** Check logic, invariants, API compatibility, null/error handling, duplication, new dependencies, authoring data, and these conventions. Preserve unrelated code and pre-existing uncommitted changes.
5. **Be accurate about evidence.** “Tests passed,” “compiles,” and “runtime verified” are different claims. Report checks not run and risks that remain.
6. **Do not weaken tests**, change expected results without a justified behaviour change, or label unrelated pre-existing failures as regressions caused by the current task.
7. **Escalate** if a safe solution requires a material architectural departure, wider scope, breaking an established contract, or accepting a significant unresolved risk. Do not improvise new architecture to get an agent task over the line.

A useful implementation handoff states: changed files and why; significant design choices; actual verification and results; remaining risks/manual checks; and what was intentionally not changed. Use a real diff instead of reproducing complete files unless full files are necessary or requested.

## 8. Applying these standards across different projects

- **Core rules:** Sections 1, 2, 5, 6, and 7 apply across languages where their concepts make sense.
- **C#/Unity-specific rules:** Sections 3 and 4 apply to owned Unity/C# code. For other languages and engines, follow their local language conventions unless a project explicitly adopts an equivalent rule.
- **Project architecture wins on specifics:** A repository may define concrete owners, base classes, dependency patterns, logging, serialization, or layout conventions. Those specifics override a generic example in this document; they do not authorize bypassing the underlying engineering principles.
- **Third-party/generated code is excluded** from automatic restyling. Do not churn legacy first-party code outside the requested change just to make it consistent.
- **Adopt incrementally:** New and materially modified code should meet the standards. Existing violations are review findings to prioritise, not automatic permission for a repository-wide rewrite.
- **Agent instruction:** Link to this document from `AGENTS.md` or `CLAUDE.md` in each repository, and require the agent to read it before code changes or standards reviews. Maintain one master copy and sync it deliberately; avoid independently evolving conflicting copies.

## 9. Standards review protocol for coding agents

A standards review is an **evidence-based code review**, not a refactor mandate and not a checklist to maximise findings.

### Review scope

- By default review the **current diff and its necessary integration seams** against the standards. For a whole-repository audit, the request must explicitly say so.
- Read relevant repository instructions and project architecture first, then inspect real code. Do not assume documents describe current implementation perfectly.
- Identify which files are first-party and which rules apply. Do not report local/project exceptions as violations.
- Look for semantic issues **as well as** formatting: duplicated ownership, misplaced responsibilities, bypassed framework mechanisms, unjustified inheritance, hidden invalid state, magic values, comments, missing verification, and regression risk.
- Distinguish objectively verifiable violations from design judgments. Give the smallest useful remedy; do not recommend speculative rewrites.

### Finding severity

| Severity | Meaning |
| --- | --- |
| **Blocking** | Correctness or safety issue, broken architectural invariant, or serious maintainability/ownership issue introduced by the change. |
| **Should fix** | Clear convention violation, meaningful duplication, unclear contract, or avoidable complexity in the changed scope. |
| **Consider** | Reasonable improvement with trade-offs; not a requirement or blocker. |
| **Pre-existing / out of scope** | Existing issue not introduced or materially worsened by this change; record only if relevant to risk. |

For each finding give **file and line(s)**, the precise rule or principle, why it matters in this instance, the narrowest sound fix, and whether it was introduced by the current change. A finding without evidence should be labelled as a question, not asserted as fact. Avoid duplicate findings that describe the same root cause.

### Review result

Return:

1. **Verdict:** meets standards / meets with non-blocking findings / changes required / unable to verify.
2. **Findings:** ordered by severity, with evidence and precise fixes; explicitly say when none were found.
3. **Verification:** checks actually run, results, and relevant gaps.
4. **Scope limits:** major areas not inspected and any pre-existing issues relevant to the change.
5. **Minimal next action:** what needs fixing now versus what can remain deferred.

**Review-only means no edits, commits, pushes, branch changes, test weakening, or cleanup.** An agent may implement fixes only if the task explicitly authorises implementation and defines its scope.

### Copy/paste review request

```text
REVIEW ONLY — do not edit files, commit, push, or change branches.

Read CODING_STANDARDS.md and relevant repository instructions (AGENTS.md,
CLAUDE.md, and task-relevant project architecture/testing docs). Inspect
current code rather than relying on docs as proof of implementation.

Scope: [current diff by default / specified paths / explicitly whole repo].
Review first-party code against applicable standards, including real design
and ownership issues, not just naming and formatting. Preserve established
project-specific conventions and do not demand unrelated refactors.

For each actionable finding, give file:line, severity, precise violated rule,
evidence and impact, the smallest technically sound fix, and whether the issue
was introduced by this change. Separate objective violations from judgment
calls and pre-existing debt; avoid duplicate findings for one root cause.

Return a concise verdict, ordered findings, verification actually performed,
material unverified risks, and the smallest required next action. Do not claim
runtime verification unless you actually observed runtime behaviour.
```

---

**Working principle:** AI-written code is not held to a lower standard. Quality comes from ownership, coherent design, understandable diffs, and verified behaviour — not from whether a human or an agent typed the code.
