#!/bin/bash

# Docker build script for play-app-api
# Usage: ./docker-build.sh [build|run|build-and-run|clean]

set -e

IMAGE_NAME="play-app-api"
TAG="dev"
CONTAINER_NAME="play-app-api-container"
HOST_PORT="3000"
CONTAINER_PORT="80"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to build the Docker image
build_image() {
    print_status "Building Docker image: ${IMAGE_NAME}:${TAG}"
    docker build -t "${IMAGE_NAME}:${TAG}" .
    print_success "Docker image built successfully!"
}

# Function to run the container
run_container() {
    print_status "Checking if container is already running..."
    if docker ps --format "table {{.Names}}" | grep -q "${CONTAINER_NAME}"; then
        print_warning "Container ${CONTAINER_NAME} is already running. Stopping it first..."
        docker stop "${CONTAINER_NAME}" > /dev/null 2>&1
        docker rm "${CONTAINER_NAME}" > /dev/null 2>&1
    fi
    
    print_status "Starting container: ${CONTAINER_NAME}"
    print_status "Mapping port ${HOST_PORT} -> ${CONTAINER_PORT}"
    docker run -d \
        --name "${CONTAINER_NAME}" \
        -p "${HOST_PORT}:${CONTAINER_PORT}" \
        "${IMAGE_NAME}:${TAG}"
    
    print_success "Container started successfully!"
    print_status "API is available at: http://localhost:${HOST_PORT}/ping"
    print_status "Container logs: docker logs ${CONTAINER_NAME}"
    print_status "Stop container: docker stop ${CONTAINER_NAME}"
}

# Function to build and run
build_and_run() {
    build_image
    run_container
}

# Function to clean up containers and images
clean() {
    print_status "Cleaning up containers and images..."
    
    # Stop and remove container if running
    if docker ps --format "table {{.Names}}" | grep -q "${CONTAINER_NAME}"; then
        print_status "Stopping container: ${CONTAINER_NAME}"
        docker stop "${CONTAINER_NAME}" > /dev/null 2>&1
        docker rm "${CONTAINER_NAME}" > /dev/null 2>&1
    fi
    
    # Remove image if exists
    if docker images --format "table {{.Repository}}:{{.Tag}}" | grep -q "${IMAGE_NAME}:${TAG}"; then
        print_status "Removing image: ${IMAGE_NAME}:${TAG}"
        docker rmi "${IMAGE_NAME}:${TAG}" > /dev/null 2>&1
    fi
    
    print_success "Cleanup completed!"
}

# Function to show usage
show_usage() {
    echo "Usage: $0 [build|run|build-and-run|clean]"
    echo ""
    echo "Commands:"
    echo "  build           - Build the Docker image"
    echo "  run             - Run the container (assumes image exists)"
    echo "  build-and-run   - Build the image and run the container"
    echo "  clean           - Stop and remove container, remove image"
    echo ""
    echo "Examples:"
    echo "  $0 build"
    echo "  $0 run"
    echo "  $0 build-and-run"
    echo "  $0 clean"
}

# Main script logic
case "${1:-build-and-run}" in
    "build")
        build_image
        ;;
    "run")
        run_container
        ;;
    "build-and-run")
        build_and_run
        ;;
    "clean")
        clean
        ;;
    "help"|"-h"|"--help")
        show_usage
        ;;
    *)
        print_error "Unknown command: $1"
        show_usage
        exit 1
        ;;
esac 