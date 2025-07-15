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

check_dependencies dotnet

cd ./src/Weaviate.Client

dotnet nswag run nswag.json