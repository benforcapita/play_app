#!/bin/bash

# Complete deployment script for play-app-api
# This script builds, pushes the Docker image, and updates the Azure Container App
# Usage: ./deploy.sh [amd64|arm64]

set -e

# Configuration
REGISTRY="ghcr.io"
USERNAME="benforcapita"
IMAGE_NAME="play-app-api"
TAG="amd64"  # Default to amd64
RESOURCE_GROUP="play-app-rg"
CONTAINER_APP_NAME="play-api"

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

# Function to check prerequisites
check_prerequisites() {
    print_status "Checking prerequisites..."
    
    # Check if CR_PAT is set
    if [ -z "$CR_PAT" ]; then
        print_error "CR_PAT environment variable is not set!"
        print_error "Please add your GitHub Container Registry token to ~/.zshrc:"
        print_error "echo 'export CR_PAT=your_github_token_here' >> ~/.zshrc"
        print_error "Then reload with: source ~/.zshrc"
        exit 1
    fi
    
    # Check if Azure CLI is installed
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI is not installed or not in PATH!"
        print_error "Please install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    fi
    
    # Check if Docker is installed
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed or not in PATH!"
        print_error "Please install Docker: https://docs.docker.com/get-docker/"
        exit 1
    fi
    
    # Check if logged into Azure
    if ! az account show &> /dev/null; then
        print_error "Not logged into Azure CLI!"
        print_error "Please run: az login"
        exit 1
    fi
    
    print_success "All prerequisites met!"
}

# Function to build and push the Docker image
build_and_push() {
    print_status "Building and pushing Docker image..."
    
    FULL_IMAGE_NAME="${REGISTRY}/${USERNAME}/${IMAGE_NAME}:${TAG}"
    
    # Build the image
    print_status "Building Docker image..."
    docker build --platform linux/amd64 -t "${FULL_IMAGE_NAME}" .
    
    # Login to GitHub Container Registry
    print_status "Logging in to GitHub Container Registry..."
    echo "$CR_PAT" | docker login ghcr.io -u "$USERNAME" --password-stdin
    
    # Push the image
    print_status "Pushing image to registry..."
    docker push "${FULL_IMAGE_NAME}"
    
    print_success "Image successfully built and pushed: ${FULL_IMAGE_NAME}"
}

# Function to update the Azure Container App
update_container_app() {
    print_status "Updating Azure Container App..."
    
    FULL_IMAGE_NAME="${REGISTRY}/${USERNAME}/${IMAGE_NAME}:${TAG}"
    
    # Update the container app
    print_status "Updating container app '${CONTAINER_APP_NAME}' in resource group '${RESOURCE_GROUP}'..."
    az containerapp update \
        -n "${CONTAINER_APP_NAME}" \
        -g "${RESOURCE_GROUP}" \
        --image "${FULL_IMAGE_NAME}"
    
    print_success "Container app updated successfully!"
}

# Function to show usage
show_usage() {
    echo "Usage: $0 [amd64|arm64]"
    echo ""
    echo "This script will:"
    echo "  1. Check prerequisites (Docker, Azure CLI, GitHub token)"
    echo "  2. Build the Docker image"
    echo "  3. Push the image to GitHub Container Registry"
    echo "  4. Update the Azure Container App"
    echo ""
    echo "Arguments:"
    echo "  amd64  - Build for AMD64 architecture (default)"
    echo "  arm64  - Build for ARM64 architecture"
    echo ""
    echo "Environment Variables:"
    echo "  CR_PAT - GitHub Container Registry token (required)"
    echo ""
    echo "Examples:"
    echo "  $0        # Deploy AMD64 version"
    echo "  $0 amd64  # Deploy AMD64 version"
    echo "  $0 arm64  # Deploy ARM64 version"
}

# Use command line argument if provided
if [ -n "$1" ]; then
    TAG="$1"
fi

# Validate tag
if [[ "$TAG" != "amd64" && "$TAG" != "arm64" ]]; then
    print_error "Invalid tag: $TAG. Must be 'amd64' or 'arm64'"
    show_usage
    exit 1
fi

print_status "Starting deployment process for ${TAG} architecture..."

# Check prerequisites
check_prerequisites

# Build and push the image
build_and_push

# Update the container app
update_container_app

print_success "Deployment completed successfully!"
print_status "Your API should now be running with the latest changes." 