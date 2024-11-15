#!/bin/bash
set -e

# Build and publish
echo "Building and publishing..."
dotnet publish ImageSharingWithCloud.csproj -c Release --no-self-contained -o ~/tmp/cs526/ImageSharingWithCloud/publish

# Go to the tmp directory
cd ~/tmp/cs526/ImageSharingWithCloud/

# Build Docker image
echo "Building Docker image..."
docker build --platform linux/amd64 -t cs526/imagesharingcloud .
