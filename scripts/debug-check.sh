#!/bin/bash
set -eo pipefail

SCRIPT_PATH="${BASH_SOURCE[0]:-$0}"
SCRIPT_DIR="$(cd "$(dirname "$SCRIPT_PATH")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
ADR_DIR="$REPO_ROOT/docs/adr"
OUTPUT_FORMAT="text"

>&2 echo "DEBUG: Starting script"
>&2 echo "DEBUG: ADR_DIR=$ADR_DIR"

if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "🔍 检查 ADR 关系双向一致性..."
    echo ""
fi

>&2 echo "DEBUG: After output check"

# Enable debug if DEBUG env var is set
if [ "${DEBUG:-}" = "1" ]; then
    set -x
fi

>&2 echo "DEBUG: After DEBUG check"

# 临时文件
TEMP_DIR=$(mktemp -d)
>&2 echo "DEBUG: Created TEMP_DIR=$TEMP_DIR"

trap "rm -rf $TEMP_DIR" EXIT
>&2 echo "DEBUG: Set trap"

DEPENDENCIES_FILE="$TEMP_DIR/dependencies.txt"
SUPERSEDES_FILE="$TEMP_DIR/supersedes.txt"

errors=0

>&2 echo "DEBUG: Starting while loop"
>&2 echo "DEBUG: Running find command..."

counter=0
while IFS= read -r adr_file; do
    counter=$((counter + 1))
    if [ $counter -le 3 ]; then
        >&2 echo "DEBUG: Processing file $counter: $adr_file"
    fi
    if [ $counter -ge 3 ]; then
        break
    fi
done < <(find "$ADR_DIR" -name "ADR-*.md" -not -name "README.md" -not -path "*/proposals/*" | sort)

>&2 echo "DEBUG: Processed $counter files"
>&2 echo "DEBUG: Script completed"

echo "✅ Test completed"
