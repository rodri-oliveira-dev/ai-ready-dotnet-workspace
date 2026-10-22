---
name: 'review-architecture'
description: 'Coordinate an evidence-based architecture review with the repository skill and MCP tools.'
argument-hint: 'Describe the change, comparison ref, or architecture question.'
agent: 'Architecture Reviewer'
---

# Review architecture

Review `${input:scope:the current changes or the supplied comparison ref}`.

Follow the [validate-change skill](../../.agents/skills/validate-change/SKILL.md)
for evidence and reporting discipline. Use the workspace MCP tools to inventory
projects and load architectural rules before assessing the requested scope.

Stay within the Architecture Reviewer's read-only tool set. If a validation command
cannot run with those tools, list the exact command as a manual verification step;
do not imply that it ran. Produce the agent's standard response format.
