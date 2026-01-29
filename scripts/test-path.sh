#!/bin/bash
set -eo pipefail

SCRIPT_PATH="${BASH_SOURCE[0]:-$0}"
SCRIPT_DIR="$(cd "$(dirname "$SCRIPT_PATH")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
ADR_DIR="$REPO_ROOT/docs/adr"

echo "SCRIPT_PATH: $SCRIPT_PATH"
echo "SCRIPT_DIR: $SCRIPT_DIR"
echo "REPO_ROOT: $REPO_ROOT"
echo "ADR_DIR: $ADR_DIR"

if [ -d "$ADR_DIR" ]; then
    echo "✅ Directory exists"
    file_count=$(find "$ADR_DIR" -name "ADR-*.md" -not -name "README.md" -not -path "*/proposals/*" | wc -l)
    echo "Found $file_count ADR files"
else
    echo "❌ ERROR: Directory does not exist!"
    exit 1
fi
