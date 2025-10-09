#!/usr/bin/env bash

set -eou pipefail

export WEAVIATE_VERSION=$1

cd "$(dirname "${BASH_SOURCE[0]}")"

# shellcheck disable=SC1091
source compose.sh

compose_down_all
rm -rf weaviate-data || true