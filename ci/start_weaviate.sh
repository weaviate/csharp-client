#!/usr/bin/env bash

set -eou pipefail

MIN_SUPPORTED="1.31.0"
REQUESTED_VERSION="${1:-$MIN_SUPPORTED}"

# Compare versions; ensure REQUESTED_VERSION >= MIN_SUPPORTED
if [[ $(printf '%s\n' "$MIN_SUPPORTED" "$REQUESTED_VERSION" | sort -V | head -n1) != "$MIN_SUPPORTED" ]]; then
  echo "Requested Weaviate version ($REQUESTED_VERSION) is lower than minimum supported ($MIN_SUPPORTED). Aborting."
  exit 1
fi

export WEAVIATE_VERSION="$REQUESTED_VERSION"

cd "$(dirname "${BASH_SOURCE[0]}")"

# shellcheck disable=SC1091
source compose.sh

echo "Stop existing session if running"
compose_down
rm -rf weaviate-data || true

echo "Run Docker compose (Weaviate $WEAVIATE_VERSION)"
compose_up

echo "Wait until all containers are up"

for port in $(all_weaviate_ports); do
  wait "http://localhost:$port"
done

echo "All containers running"
