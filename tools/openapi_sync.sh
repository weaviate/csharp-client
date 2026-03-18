#!/bin/bash

# Check if dependencies are installed
check_dependencies() {
    local commands=("$@")
    for cmd in "${commands[@]}"; do
        if ! command -v "$cmd" &> /dev/null; then
            echo "Error: $cmd is a required dependency and not found."
            exit 1
        fi
    done
}

check_dependencies curl tar realpath dirname

branchOrTag="${1:-main}"
dir="$(realpath "$( dirname "${BASH_SOURCE[0]}" )" )"
out="${dir}/../src/Weaviate.Client/Rest/Schema"

mkdir -p "${out}"

echo "Fetching OpenAPI spec from weaviate/${branchOrTag}..."
curl -vLkSs "https://api.github.com/repos/weaviate/weaviate/tarball/${branchOrTag}" -o "${dir}/weaviate.tar.gz"

# Extract openapi.json from the tarball
files="$(tar -tf "${dir}/weaviate.tar.gz" | grep 'openapi-specs/schema.json$' | tr '\n' ' ')"

if [ -z "$files" ]; then
    echo "Error: Could not find openapi-specs/schema.json in the tarball"
    rm "${dir}/weaviate.tar.gz"
    exit 1
fi

# Extract and rename to openapi.json
# shellcheck disable=SC2086 # we want to pass multiple arguments to tar
tar --strip-components=2 -C "${out}" -xvf "${dir}/weaviate.tar.gz" ${files}

# Rename schema.json to openapi.json
if [ -f "${out}/schema.json" ]; then
    mv "${out}/schema.json" "${out}/openapi.json"
    echo "Successfully updated ${out}/openapi.json"
else
    echo "Error: schema.json not found after extraction"
    rm "${dir}/weaviate.tar.gz"
    exit 1
fi

rm "${dir}/weaviate.tar.gz"

echo "done"
