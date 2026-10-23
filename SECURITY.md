# Security policy

## Supported versions

This template is maintained on the default branch. Derived repositories own their
support policy and must review upstream changes before applying them.

## Report a vulnerability

Do not open a public issue for a suspected vulnerability and do not include working
credentials, personal data, or sensitive exploit output in repository content.
Use GitHub's **Report a vulnerability** private reporting flow for this repository.
If private reporting is unavailable, contact the maintainer through a private
channel listed on the GitHub profile and provide only enough detail to establish a
secure follow-up path.

Include the affected commit or version, impacted component, prerequisites, observed
impact, minimal reproduction, and suggested mitigation when known. Allow time for
triage and coordinated disclosure. No response deadline or bounty is promised.

## Scope and expectations

Relevant reports include path containment or data disclosure in MCP, permission or
credential exposure in automation, dependency or container integrity failures, and
ways the checked-in controls contradict the documented security boundary.

Repository instructions are not a sandbox. Client services, host security,
organization policy, branch protection, secret scanning, and derived applications
are outside the repository's direct enforcement boundary. See the
[security baseline](docs/security/security-baseline.md) and
[threat model](docs/security/threat-model.md) before assessing impact.
