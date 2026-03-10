#!/usr/bin/env bash

function ls_compose {
  for file in *; do
    if [[ "$file" == *"docker-compose"* ]]; then
      echo "${file}"
    fi
  done
}

function exec_all {
  for file in $(ls_compose); do
    docker compose -f "${file}" "$@"
  done
}

function compose_up_all {
  exec_all up -d --quiet-pull
}

function compose_down_all {
  exec_all down --remove-orphans
}

function exec_dc {
	docker compose -f "docker-compose.yml" "$@"
}

function compose_up {
  echo "Pulling Weaviate image ${WEAVIATE_VERSION:-latest}..."
  echo "Images to be pulled:"
  exec_dc config --images
  echo ""

  if ! exec_dc pull; then
    echo "ERROR: Failed to pull Docker images"
    echo "Image pull failed for version ${WEAVIATE_VERSION}"
    return 1
  fi

  echo "Successfully pulled images. Starting containers..."
  if ! exec_dc up -d; then
    echo "ERROR: Failed to start containers"
    echo "Container status:"
    docker ps -a
    echo ""
    echo "Container logs:"
    exec_dc logs
    return 1
  fi

  echo "Containers started successfully"
  return 0
}

function compose_down {
  exec_dc down --remove-orphans
}

function all_weaviate_ports {
  # Include single-node default and multi-node cluster compose exposed REST ports
  echo "8080 8087 8088 8089"
}

function wait(){
  MAX_WAIT_SECONDS=60
  ALREADY_WAITING=0

  echo "Waiting for $1"
  while true; do
    # first check if weaviate already responds
    if ! curl -s "$1" > /dev/null; then
      echo "Weaviate port not responding yet. (waited for ${ALREADY_WAITING}s)"
      if [ $ALREADY_WAITING -gt $MAX_WAIT_SECONDS ]; then
        echo "======================================"
        echo "ERROR: Weaviate did not start up in $MAX_WAIT_SECONDS seconds"
        echo "======================================"
        echo ""
        echo "Docker container status:"
        docker ps -a --filter "name=weaviate"
        echo ""
        echo "Docker compose logs (last 50 lines):"
        exec_dc logs --tail=50
        echo ""
        echo "Checking port accessibility:"
        nc -zv localhost 8080 || echo "Port 8080 not accessible"
        echo "======================================"
        exit 1
      fi
      sleep 2
      (( ALREADY_WAITING+=2 )) || true
      continue
    fi

    # endpoint available, check if it is ready
    HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$1/v1/.well-known/ready")

    if [ "$HTTP_STATUS" -eq 200 ]; then
      break
    else
      echo "Weaviate responding but not ready yet. HTTP $HTTP_STATUS (waited for ${ALREADY_WAITING}s)"
      if [ $ALREADY_WAITING -gt $MAX_WAIT_SECONDS ]; then
        echo "======================================"
        echo "ERROR: Weaviate did not become ready in $MAX_WAIT_SECONDS seconds"
        echo "======================================"
        echo ""
        echo "Docker container status:"
        docker ps -a --filter "name=weaviate"
        echo ""
        echo "Docker compose logs (last 50 lines):"
        exec_dc logs --tail=50
        echo ""
        echo "Weaviate ready endpoint response:"
        curl -v "$1/v1/.well-known/ready" || true
        echo "======================================"
        exit 1
      else
        sleep 2
        (( ALREADY_WAITING+=2 )) || true
      fi
    fi
  done

  echo "Weaviate is up and running!"
}
