#!/bin/bash

# Build and push script for play-app-api
# Usage: ./build-and-push.sh [amd64|arm64]

set -e

# Configuration
REGISTRY="ghcr.io"
USERNAME="benforcapita"
IMAGE_NAME="play-app-api"
TAG="amd64"  # Default to amd64

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

# Check if CR_PAT is set
if [ -z "$CR_PAT" ]; then
    print_error "CR_PAT environment variable is not set!"
    print_error "Please add your GitHub Container Registry token to ~/.zshrc:"
    print_error "echo 'export CR_PAT=your_github_token_here' >> ~/.zshrc"
    print_error "Then reload with: source ~/.zshrc"
    exit 1
fi

# Use command line argument if provided
if [ -n "$1" ]; then
    TAG="$1"
fi

FULL_IMAGE_NAME="${REGISTRY}/${USERNAME}/${IMAGE_NAME}:${TAG}"

print_status "Building and pushing image: ${FULL_IMAGE_NAME}"

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
print_status "You can now update your Azure Container App with this image." 