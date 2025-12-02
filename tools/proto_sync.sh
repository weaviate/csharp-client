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

check_dependencies curl tar realpath dirname grep

branchOrTag="${1:-main}"
dir="$(realpath "$( dirname "${BASH_SOURCE[0]}" )" )"
out="${dir}/../src/Weaviate.Client/gRPC/proto"

rm -rf "${out}"
mkdir -p "${out}"

curl -vLkSs "https://api.github.com/repos/weaviate/weaviate/tarball/${branchOrTag}" -o "${dir}/weaviate.tar.gz"
files="$(tar -tf "${dir}/weaviate.tar.gz" | grep 'grpc/proto/v1/[^\.]\+.proto$' | tr '\n' ' ')"

# shellcheck disable=SC2086 # we want to pass multiple arguments to tar
tar --strip-components=3 -C "${out}" -xvf "${dir}/weaviate.tar.gz" ${files}

rm "${dir}/weaviate.tar.gz"

for f in "${out}"/v1/*.proto; do
    if ! grep -q 'option csharp_namespace = "Weaviate.Client.Grpc.Protobuf.V1";' "$f"; then
        # Find line after syntax and package
        syntax_line=$(grep -n '^syntax' "$f" | cut -d: -f1)
        package_line=$(grep -n '^package' "$f" | cut -d: -f1)
        insert_line=$((package_line > 0 ? package_line : syntax_line))
        insert_line=$((insert_line+1))
        tmpfile=$(mktemp)
        awk -v n=$insert_line 'NR==n{print "option csharp_namespace = \"Weaviate.Client.Grpc.Protobuf.V1\";"} 1' "$f" > "$tmpfile"
        mv "$tmpfile" "$f"
        echo "Added csharp_namespace option to $f"
    fi
done

echo "done"