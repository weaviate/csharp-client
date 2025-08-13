#!/usr/bin/env bash

set -e

# Check for required dependencies
check_dependencies() {
  local commands=("$@")
  for cmd in "${commands[@]}"; do
    if ! command -v "$cmd" &> /dev/null; then
      echo "Error: $cmd is a required dependency and not found."
      exit 1
    fi
  done
}

check_dependencies dotnet realpath dirname

# Default configuration and version
configuration="Debug"
package_version="1.0.0-local"

# Parse arguments
for arg in "$@"; do
  if [[ "$arg" == "--release" ]]; then
    configuration="Release"
    package_version=""
  fi
done

dir="$(realpath "$( dirname "${BASH_SOURCE[0]}" )" )"
project_dir="${dir}/../src/Weaviate.Client"
output_dir="${HOME}/LocalNugetPackages"

mkdir -p "${output_dir}"

dotnet pack "${project_dir}/Weaviate.Client.csproj" \
  -c "${configuration}" \
  -o "${output_dir}" \
  /p:PackageVersion="${package_version}"

echo "NuGet package built with configuration ${configuration} and placed in ${output_dir}"