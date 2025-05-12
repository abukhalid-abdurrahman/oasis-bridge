# Docker Build Guide for OASIS Bridge

This document provides full instructions to build, run, and push Docker images for the **OASIS Bridge** project, including file structure, ignore rules, and useful commands.

---

## Prerequisites

Ensure the following tools are installed:

- Docker (Desktop or CLI)
- Stable internet connection (for pulling base images)
- Docker Hub account (for pushing the image)

---
### Navigate to Project Root
Before running any Docker commands, navigate to the root of the project:
```sh
cd oasis-bridge
```

### Building the Docker Image
To build the Docker image for the backend service using a specific Dockerfile, use the following command:
```sh
docker build -f backend/deployments/Dockerfile.oasis_bridge -t username/oasis-bridge:vn .
```
### Running the Docker Container
To run the built image locally, use:
```sh
docker run -d -p 8080:80 --name oasis-bridge-backend username/oasis-bridge:vn
```
### Pushing the Image to Docker Hub
Once built and tested, push the image to Docker Hub:
```sh
docker push username/oasis-bridge:vn
```

# Docker Build Guide for OASIS Bridge Db Migrator

### Navigate to Project Root
Before running any Docker commands, navigate to the root of the project:
```sh
cd oasis-bridge
```
### Building the Docker Image
To build the Docker image for the backend service using a specific Dockerfile, use the following command:
```sh
docker build -f backend/deployments/Dockerfile.oasis_bridge_migration_db -t username/oasis-bridge-db-migrator:vn .
```
### Running the Docker Container
To run the built image locally, use:
```sh
docker run -d -p 8080:80 --name oasis-bridge-backend-db-migrator username/oasis-bridge-db-migrator:vn
```
### Pushing the Image to Docker Hub
Once built and tested, push the image to Docker Hub:
```sh
docker push username/oasis-bridge-db-migrator:vn
```
