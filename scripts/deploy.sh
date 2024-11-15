#!/bin/bash

# Variables
REGISTRY_NAME="cs526registrymswamina"
IMAGE_NAME="cs526/imagesharingcloud"
ACR_IMAGE_NAME="$REGISTRY_NAME.azurecr.io/imagesharingcloud"

echo "🔑 Logging into Azure..."
az login || { echo "❌ Azure login failed"; exit 1; }

echo "🔑 Logging into Azure Container Registry..."
az acr login --name $REGISTRY_NAME || { echo "❌ ACR login failed"; exit 1; }

echo "🏷️ Tagging Docker image..."
docker tag $IMAGE_NAME $ACR_IMAGE_NAME || { echo "❌ Docker tag failed"; exit 1; }

echo "⬆️ Pushing image to ACR..."
docker push $ACR_IMAGE_NAME || { echo "❌ Docker push failed"; exit 1; }

echo "✅ Successfully pushed image to ACR"