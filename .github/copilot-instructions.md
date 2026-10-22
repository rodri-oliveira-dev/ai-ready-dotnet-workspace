# GitHub Copilot instructions

- Use the root `AGENTS.md` as the shared repository contract when the active Copilot surface supports agent instructions.
- Apply matching `.github/instructions/*.instructions.md` files only to files covered by their `applyTo` globs.
- Because Copilot clients can combine instruction sources without a general precedence rule, surface a conflict instead of guessing which repository instruction wins.
- Treat files in `.github/prompts/` as manually invoked workflows, not always-on policy.
- In the final response, distinguish checks actually run from checks that are only recommended.
