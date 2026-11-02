#!/bin/bash
# Safety checks that the publish output contains the expected data directories.
set -uo pipefail

publishDir="${1:?usage: postbuild-checks.sh <publish dir>}"
failed=0

for dir in Configuration Messages Documentation; do
    if [[ ! -d "$publishDir/$dir" ]]; then
        echo "::error::$dir directory is missing from $publishDir"
        failed=1
    elif [[ -z $(find "$publishDir/$dir" -type f -print -quit) ]]; then
        echo "::error::$dir directory in $publishDir contains no files"
        failed=1
    else
        echo "$dir directory OK"
    fi
done

exit $failed
