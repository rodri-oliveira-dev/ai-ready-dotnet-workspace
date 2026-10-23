#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

for dependency in dotnet git; do
  if ! command -v "$dependency" >/dev/null 2>&1; then
    echo "Required dependency not found: $dependency" >&2
    exit 1
  fi
done

sdk_version="$(dotnet --version)"
if [[ "$sdk_version" != 10.* ]]; then
  echo "The selected SDK must be .NET 10; found $sdk_version." >&2
  exit 1
fi

echo "Using .NET SDK $sdk_version"
git --version
dotnet restore AiReadyWorkspace.slnx --locked-mode
dotnet build AiReadyWorkspace.slnx --configuration Release --no-restore
dotnet test AiReadyWorkspace.slnx --configuration Release --no-build --logger "console;verbosity=normal"

echo "Environment verification completed successfully."
