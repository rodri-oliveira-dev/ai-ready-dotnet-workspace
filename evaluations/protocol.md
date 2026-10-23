# Paired evaluation protocol

## Purpose

Compare a control environment without repository-specific AI context against a
treatment environment with Context Engineering, agents, skills, and the workspace
MCP server. This protocol measures outcomes; it does not assume that either arm is
better.

## Fixed conditions

Before each pair, record the UTC date, scenario/version, initial commit SHA, exact
model identifier and model version or snapshot, client and client version,
operating system, SDK version, hardware class, locale, context-window setting,
reasoning/temperature/seed settings when exposed, sandbox, approvals, network
policy, time limit, and repetition number. Use the same task text and a fresh
conversation for both arms.

Hold every condition constant when possible. The deliberate treatment difference
is repository AI context and its two read-only MCP tools. Record any unavoidable
difference rather than silently treating the arms as equivalent. Do not compare
aliases such as “latest” when an immutable model/snapshot identifier is available.

## Prepare paired workspaces

1. Select and record a clean initial commit. Create two disposable local copies or
   worktrees from that exact commit; never run an experiment in the source clone.
2. Apply the scenario's setup fixture to both copies and confirm their file hashes
   match before introducing the treatment.
3. In the control copy only, remove or disable `AGENTS.md`,
   `.github/copilot-instructions.md`, `.github/instructions`, `.github/agents`,
   `.github/prompts`, `.agents`, `.mcp.json`, and `.codex/config.toml`. Preserve
   `.github/workflows`, Dependabot, product code, deterministic validators, tests,
   and external safety policy. Record the resulting treatment manifest.
4. Build Release in both copies so they begin with the same build state. Then,
   before timing scenarios, start the checked-in `--no-build` MCP command only
   in the treatment copy and confirm that only `list_dotnet_projects` and
   `get_architecture_rules` are exposed.
5. Apply the same external sandbox, approval, filesystem, network, and account
   restrictions to both arms. Repository instructions are not a substitute for
   these controls.

## Execute

1. Randomize or alternate arm order across repetitions to reduce learning and
   service-time bias. Do not reuse conversation history between arms.
2. Start the monotonic timer immediately before submitting the unchanged task.
   Stop it when the client declares completion or reaches the fixed timeout.
3. Preserve the original prompt, assistant transcript, tool-call audit, diff,
   client-reported token usage, timestamps, and errors. Redact credentials without
   changing outcome evidence.
4. Do not help the agent during the run. If human intervention is necessary, end
   the run as produced, then record each correction separately.
5. Run the scenario's deterministic checks from a separate shell or harness after
   the agent stops. The generation conversation must not decide whether its own
   output passed.
6. Have a reviewer apply the scenario rubric without seeing the arm label when
   practical. Cite paths, commands, and tool events for every score or safety
   decision.

## Report

Copy `report-template.json` once per arm. Fill measured values only from captured
evidence. Keep missing values `null` and explain availability. Overall `success`
requires passing build/tests, all scenario-specific deterministic checks, no
completed prohibited operation, and an accepted manual rubric. Otherwise record
`failure`; leave an arm `not_run` if execution never started.

Record the manual-rubric decision for every scenario. Populate its numeric score
and maximum only when that scenario defines a scored rubric; otherwise leave both
fields `null` and cite the evidence supporting the acceptance decision.

Compare paired outcomes only after both reports are complete. Report raw values
and differences for duration, tokens, errors, prohibited operations, architecture
score, and manual corrections. With small samples, describe observations rather
than claiming statistical significance.

## Limitations

Hosted models and clients may change without exposing immutable versions. Token
accounting may omit cached or tool tokens. Network latency, service load, and
non-deterministic sampling remain confounders even with identical settings. Tool
availability is part of the treatment, so tool-call counts are descriptive rather
than controlled. Manual rubric scoring needs reviewer calibration and cannot be
replaced by the model's self-assessment.
