#!/usr/bin/env bash

set -eou pipefail

export WEAVIATE_VERSION=$1

source ./ci/compose.sh

echo "Stop existing session if running"
compose_down_all
rm -rf weaviate-data || true

echo "Run Docker compose"
compose_up_all

echo "Wait until all containers are up"

for port in $(all_weaviate_ports); do
  wait "http://localhost:$port"
done

echo "All containers running"
