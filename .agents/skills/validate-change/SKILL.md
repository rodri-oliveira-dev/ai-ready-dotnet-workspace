---
name: validate-change
description: Validate repository modifications before handoff by selecting and running the applicable static checks, build, and tests; use for implementation verification or pre-commit review.
---

# Validate a change

Verify the requested change without expanding its scope or modifying product code.

1. Read the applicable `AGENTS.md`, inspect `git status`, and identify the changed
   paths. Preserve unrelated work and do not clean or revert it.
2. Select checks from the changed surface:
   - Always run `git diff --check`, `./scripts/validate-ai-context.sh`, and
     `./scripts/validate-security.sh` when AI context, agents, skills, prompts,
     MCP, Dev Container, dependency, or workflow configuration changed.
   - For .NET changes, run the repository commands below in order.
   - Run focused tests first when they provide faster failure feedback, then run
     the full suite before handoff.
3. Stop after a failure that makes later results unreliable. Diagnose the failure
   from its output; fix it only when the active task authorizes implementation.
   Never weaken a check, delete evidence, or broaden permissions to obtain a pass.
4. Report commands exactly as executed, pass/fail status, relevant failure output,
   checks not run with reasons, and residual risks. Do not equate static validation
   with a real Copilot, Codex, or MCP client interaction.

## .NET validation commands

Run from the repository root:

```bash
dotnet restore AiReadyWorkspace.slnx --locked-mode
dotnet format AiReadyWorkspace.slnx --verify-no-changes --no-restore
dotnet build AiReadyWorkspace.slnx --configuration Release --no-restore
dotnet test AiReadyWorkspace.slnx --configuration Release --no-build
```

Use existing repository scripts rather than adding a skill-specific wrapper. Add a
script to this skill only if repeated deterministic logic cannot live in the
repository's shared validation script.
