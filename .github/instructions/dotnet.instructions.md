---
description: 'Repository mechanics for .NET source and project files.'
applyTo: '**/*.cs,**/*.csproj,**/*.props,**/*.targets,**/*.slnx'
---

# .NET repository mechanics

- Keep the target framework centralized in `Directory.Build.props`; do not repeat it in individual projects.
- Declare package versions in `Directory.Packages.props` and keep project `PackageReference` entries versionless.
- Keep `Program.cs` focused on composition and endpoint registration; move reusable contracts and behavior to named types.
- Verify formatting with the solution-level `dotnet format` command before considering a .NET edit complete.
