.PHONY: build-images build-api-image build-migrations-image build-frontend-image deploy remove start-db stop-db frontend-install frontend-dev frontend-build help

IMAGE_REGISTRY ?=
IMAGE_TAG ?= latest
API_IMAGE_NAME = loom-configuration
MIGRATIONS_IMAGE_NAME = loom-configuration-migrations
LAYOUT_API_IMAGE_NAME = loom-layout
LAYOUT_MIGRATIONS_IMAGE_NAME = loom-layout-migrations
CONFIGURATION_UI_IMAGE_NAME = loom-configuration-ui

HELM_RELEASE_NAME ?= loom-configuration
HELM_NAMESPACE ?= loom
HELM_CHART_PATH = infrastructure/helm/loom-configuration

DB_ADMIN_PASSWORD ?= postgres
DB_SERVICE_PASSWORD ?= postgres1

API_BUILD_CONTEXT = backend/Loom.Services.Configuration
MIGRATIONS_BUILD_CONTEXT = backend/Loom.Services.Configuration/src/Loom.Services.Configuration.Migrations
LAYOUT_API_BUILD_CONTEXT = backend/Loom.Services.Layout
LAYOUT_MIGRATIONS_BUILD_CONTEXT = backend/Loom.Services.Layout/src/Loom.Services.Layout.Migrations

help:
	@echo 'Usage: make [target]'
	@echo ''
	@echo 'Docker:'
	@echo '  build-images          Build all Docker images'
	@echo '  build-api-image       Build the API image'
	@echo '  build-migrations-image Build the migrations image'
	@echo '  build-layout-api-image Build the layout API image'
	@echo '  build-layout-migrations-image Build the layout migrations image'
	@echo '  build-configuration-ui-image  Build the configuration UI image'
	@echo ''
	@echo 'Database:'
	@echo '  start-db              Start PostgreSQL via docker-compose'
	@echo '  stop-db               Stop PostgreSQL via docker-compose'
	@echo ''
	@echo 'Frontend:'
	@echo '  frontend-install      Install frontend dependencies'
	@echo '  frontend-dev          Start frontend dev server'
	@echo '  frontend-build        Build frontend for production'
	@echo ''
	@echo 'Kubernetes:'
	@echo '  deploy                Deploy the Helm chart'
	@echo '  remove                Remove the Helm deployment'

build-images: build-api-image build-migrations-image build-layout-api-image build-layout-migrations-image build-configuration-ui-image

build-api-image:
	@echo "Building $(API_IMAGE_NAME):$(IMAGE_TAG)"
	docker build \
		-t $(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(API_IMAGE_NAME):$(IMAGE_TAG) \
		-f $(API_BUILD_CONTEXT)/Dockerfile \
		$(API_BUILD_CONTEXT)

build-migrations-image:
	@echo "Building $(MIGRATIONS_IMAGE_NAME):$(IMAGE_TAG)"
	docker build \
		-t $(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(MIGRATIONS_IMAGE_NAME):$(IMAGE_TAG) \
		-f $(MIGRATIONS_BUILD_CONTEXT)/Dockerfile \
		$(MIGRATIONS_BUILD_CONTEXT)

build-layout-api-image:
	@echo "Building $(LAYOUT_API_IMAGE_NAME):$(IMAGE_TAG)"
	docker build \
		-t $(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(LAYOUT_API_IMAGE_NAME):$(IMAGE_TAG) \
		-f $(LAYOUT_API_BUILD_CONTEXT)/Dockerfile \
		$(LAYOUT_API_BUILD_CONTEXT)

build-layout-migrations-image:
	@echo "Building $(LAYOUT_MIGRATIONS_IMAGE_NAME):$(IMAGE_TAG)"
	docker build \
		-t $(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(LAYOUT_MIGRATIONS_IMAGE_NAME):$(IMAGE_TAG) \
		-f $(LAYOUT_MIGRATIONS_BUILD_CONTEXT)/Dockerfile \
		$(LAYOUT_MIGRATIONS_BUILD_CONTEXT)

build-configuration-ui-image:
	@echo "Building $(CONFIGURATION_UI_IMAGE_NAME):$(IMAGE_TAG)"
	docker build \
		--build-arg VITE_API_BASE_URL=$(or $(VITE_API_BASE_URL),/api) \
		--build-arg VITE_LAYOUT_API_URL=$(or $(VITE_LAYOUT_API_URL),/api/layout) \
		-t $(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(CONFIGURATION_UI_IMAGE_NAME):$(IMAGE_TAG) \
		-f frontend/dockerfiles/LoomConfigurationUI.Dockerfile \
		frontend

start-db:
	@echo "Starting PostgreSQL..."
	docker compose -f infrastructure/docker-compose/docker-compose.yaml up -d
	@echo "PostgreSQL is running on localhost:5432"

stop-db:
	@echo "Stopping PostgreSQL..."
	docker compose -f infrastructure/docker-compose/docker-compose.yaml down

frontend-install:
	cd frontend && npm install

frontend-dev:
	cd frontend && npm run dev

frontend-build:
	cd frontend && npm run build

deploy:
	@echo "Deploying $(HELM_RELEASE_NAME) to $(HELM_NAMESPACE)"
	helm upgrade --install $(HELM_RELEASE_NAME) $(HELM_CHART_PATH) \
		--namespace $(HELM_NAMESPACE) \
		--create-namespace \
		--set database.adminPassword=$(DB_ADMIN_PASSWORD) \
		--set database.servicePassword=$(DB_SERVICE_PASSWORD) \
		--set image.repository=$(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(API_IMAGE_NAME) \
		--set image.tag=$(IMAGE_TAG) \
		--set migrationImage.repository=$(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(MIGRATIONS_IMAGE_NAME) \
		--set migrationImage.tag=$(IMAGE_TAG) \
		--set frontend.image.repository=$(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(FRONTEND_IMAGE_NAME) \
		--set frontend.image.tag=$(IMAGE_TAG)

remove:
	@echo "Removing $(HELM_RELEASE_NAME)"
	helm uninstall $(HELM_RELEASE_NAME) --namespace $(HELM_NAMESPACE) || true
