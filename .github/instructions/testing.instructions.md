---
applyTo: "**/*Tests*/**/*.{cs,csproj}" 
---

# Testing Instructions

- Prefer deterministic tests.
- Validate behavior through public contracts.
- When hardware is unavailable, use the Fake sensor implementation.
- Do not bypass Correlation, Application services or Domain behavior in integration-style validation.
- The Fake should simulate external input, not replace application behavior.
- Do not claim tests were executed unless they actually ran.
