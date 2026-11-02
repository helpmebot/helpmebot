#!/bin/bash
# Replaces the empty author dictionary in Credits.cs with commit counts from git history.
set -euo pipefail

file="${1:-src/Helpmebot.CoreServices/Credits.cs}"

[[ -f "$file" ]] || { echo "::error::$file not found"; exit 1; }

entries=$(git shortlog -sn HEAD | awk -F'\t' '
    {
        count = $1; gsub(/^ +/, "", count)
        name = $2; gsub(/\\/, "\\\\", name); gsub(/"/, "\\\"", name)
        printf "%s{\"%s\", %d}", (NR > 1 ? ", " : ""), name, count
    }')

# Use a non-printing delimiter-safe replacement: escape sed-special characters (& and \ and |)
escaped=$(printf '%s' "$entries" | sed -e 's/[\\&|]/\\&/g')

sed -i -e "s|{ };|{${escaped}};|" "$file"

grep -q 'Dictionary<string, uint> {{' "$file" || { echo "::error::Credits were not populated"; exit 1; }
