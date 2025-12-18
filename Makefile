.PHONY: build-images build-api-image build-migrations-image build-frontend-image deploy remove start-db stop-db frontend-install frontend-dev frontend-build help build-platform-deps deploy-platform remove-platform rebuild

IMAGE_REGISTRY ?=
IMAGE_TAG ?= latest
API_IMAGE_NAME = loom-configuration
MIGRATIONS_IMAGE_NAME = loom-configuration-migrations
LAYOUT_API_IMAGE_NAME = loom-layout
LAYOUT_MIGRATIONS_IMAGE_NAME = loom-layout-migrations
CONFIGURATION_UI_IMAGE_NAME = loom-configuration-ui

# MasterData Configuration
MASTERDATA_API_IMAGE_NAME = loom-masterdata-configuration
MASTERDATA_MIGRATIONS_IMAGE_NAME = loom-masterdata-configuration-migrations
DATA_STUDIO_UI_IMAGE_NAME = loom-data-studio-ui

HELM_RELEASE_NAME ?= loom-configuration
HELM_NAMESPACE ?= loom
HELM_CHART_PATH = infrastructure/helm/loom-configuration

# MasterData Configuration Helm
MASTERDATA_HELM_RELEASE_NAME ?= loom-masterdata-configuration
MASTERDATA_HELM_CHART_PATH = infrastructure/helm/loom-masterdata-configuration

# Platform (parent chart with all services)
PLATFORM_HELM_RELEASE_NAME ?= loom-platform
PLATFORM_HELM_CHART_PATH = infrastructure/helm/loom-platform

DB_ADMIN_PASSWORD ?= postgres
DB_SERVICE_PASSWORD ?= postgres1

API_BUILD_CONTEXT = backend/Loom.Services.Configuration
MIGRATIONS_BUILD_CONTEXT = backend/Loom.Services.Configuration/src/Loom.Services.Configuration.Migrations
LAYOUT_API_BUILD_CONTEXT = backend/Loom.Services.Layout
LAYOUT_MIGRATIONS_BUILD_CONTEXT = backend/Loom.Services.Layout/src/Loom.Services.Layout.Migrations

# MasterData Configuration
MASTERDATA_API_BUILD_CONTEXT = backend/Loom.Services.MasterDataConfiguration
MASTERDATA_MIGRATIONS_BUILD_CONTEXT = backend/Loom.Services.MasterDataConfiguration/src/Loom.Services.MasterDataConfiguration.Migrations

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
	@echo '  build-masterdata-api-image Build the masterdata API image'
	@echo '  build-masterdata-migrations-image Build the masterdata migrations image'
	@echo '  build-data-studio-ui-image Build the data studio UI image'
	@echo '  build-platform-images Build all images for the platform'
	@echo '  rebuild               Rebuild the platform'
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
	@echo '  deploy                Deploy the configuration Helm chart'
	@echo '  remove                Remove the configuration Helm deployment'
	@echo '  deploy-masterdata     Deploy the masterdata Helm chart'
	@echo '  remove-masterdata     Remove the masterdata Helm deployment'
	@echo '  deploy-platform      Deploy the complete platform (all services)'
	@echo '  remove-platform       Remove the platform deployment'
	@echo '  build-platform-deps   Build Helm chart dependencies'

build-images: build-configuration-images build-masterdata-images

build-configuration-images: build-api-image build-migrations-image build-layout-api-image build-layout-migrations-image build-configuration-ui-image

build-masterdata-images: build-masterdata-api-image build-masterdata-migrations-image build-data-studio-ui-image

build-platform-images: build-images build-masterdata-images

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

build-masterdata-api-image:
	@echo "Building $(MASTERDATA_API_IMAGE_NAME):$(IMAGE_TAG)"
	docker build \
		-t $(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(MASTERDATA_API_IMAGE_NAME):$(IMAGE_TAG) \
		-f $(MASTERDATA_API_BUILD_CONTEXT)/Dockerfile \
		$(MASTERDATA_API_BUILD_CONTEXT)

build-masterdata-migrations-image:
	@echo "Building $(MASTERDATA_MIGRATIONS_IMAGE_NAME):$(IMAGE_TAG)"
	docker build \
		-t $(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(MASTERDATA_MIGRATIONS_IMAGE_NAME):$(IMAGE_TAG) \
		-f $(MASTERDATA_MIGRATIONS_BUILD_CONTEXT)/Dockerfile \
		$(MASTERDATA_MIGRATIONS_BUILD_CONTEXT)

build-data-studio-ui-image:
	@echo "Building $(DATA_STUDIO_UI_IMAGE_NAME):$(IMAGE_TAG)"
	docker build \
		--build-arg VITE_API_BASE_URL=$(or $(VITE_API_BASE_URL),/api) \
		-t $(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(DATA_STUDIO_UI_IMAGE_NAME):$(IMAGE_TAG) \
		-f frontend/dockerfiles/LoomDataStudioUI.Dockerfile \
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

deploy-masterdata:
	@echo "Deploying $(MASTERDATA_HELM_RELEASE_NAME) to $(HELM_NAMESPACE)"
	helm upgrade --install $(MASTERDATA_HELM_RELEASE_NAME) $(MASTERDATA_HELM_CHART_PATH) \
		--namespace $(HELM_NAMESPACE) \
		--create-namespace \
		--set database.adminPassword=$(DB_ADMIN_PASSWORD) \
		--set database.servicePassword=$(DB_SERVICE_PASSWORD) \
		--set image.repository=$(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(MASTERDATA_API_IMAGE_NAME) \
		--set image.tag=$(IMAGE_TAG) \
		--set migrationImage.repository=$(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(MASTERDATA_MIGRATIONS_IMAGE_NAME) \
		--set migrationImage.tag=$(IMAGE_TAG) \
		--set frontend.image.repository=$(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(DATA_STUDIO_UI_IMAGE_NAME) \
		--set frontend.image.tag=$(IMAGE_TAG)

remove-masterdata:
	@echo "Removing $(MASTERDATA_HELM_RELEASE_NAME)"
	helm uninstall $(MASTERDATA_HELM_RELEASE_NAME) --namespace $(HELM_NAMESPACE) || true

build-platform-deps:
	@echo "Building Helm chart dependencies for $(PLATFORM_HELM_RELEASE_NAME)"
	cd $(PLATFORM_HELM_CHART_PATH) && helm dependency build

deploy-platform: build-platform-deps
	@echo "Deploying $(PLATFORM_HELM_RELEASE_NAME) to $(HELM_NAMESPACE)"
	helm upgrade --install $(PLATFORM_HELM_RELEASE_NAME) $(PLATFORM_HELM_CHART_PATH) \
		--namespace $(HELM_NAMESPACE) \
		--create-namespace \
		--set loom-configuration.database.adminPassword=$(DB_ADMIN_PASSWORD) \
		--set loom-configuration.database.servicePassword=$(DB_SERVICE_PASSWORD) \
		--set loom-configuration.image.repository=$(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(API_IMAGE_NAME) \
		--set loom-configuration.image.tag=$(IMAGE_TAG) \
		--set loom-configuration.migrationImage.repository=$(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(MIGRATIONS_IMAGE_NAME) \
		--set loom-configuration.migrationImage.tag=$(IMAGE_TAG) \
		--set loom-configuration.frontend.loomConfigurationUi.image.repository=$(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(CONFIGURATION_UI_IMAGE_NAME) \
		--set loom-configuration.frontend.loomConfigurationUi.image.tag=$(IMAGE_TAG) \
		--set loom-configuration.layout.image.repository=$(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(LAYOUT_API_IMAGE_NAME) \
		--set loom-configuration.layout.image.tag=$(IMAGE_TAG) \
		--set loom-configuration.layout.migrationImage.repository=$(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(LAYOUT_MIGRATIONS_IMAGE_NAME) \
		--set loom-configuration.layout.migrationImage.tag=$(IMAGE_TAG) \
		--set loom-masterdata-configuration.database.adminPassword=$(DB_ADMIN_PASSWORD) \
		--set loom-masterdata-configuration.database.servicePassword=$(DB_SERVICE_PASSWORD) \
		--set loom-masterdata-configuration.image.repository=$(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(MASTERDATA_API_IMAGE_NAME) \
		--set loom-masterdata-configuration.image.tag=$(IMAGE_TAG) \
		--set loom-masterdata-configuration.migrationImage.repository=$(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(MASTERDATA_MIGRATIONS_IMAGE_NAME) \
		--set loom-masterdata-configuration.migrationImage.tag=$(IMAGE_TAG) \
		--set loom-masterdata-configuration.frontend.image.repository=$(if $(IMAGE_REGISTRY),$(IMAGE_REGISTRY)/)$(DATA_STUDIO_UI_IMAGE_NAME) \
		--set loom-masterdata-configuration.frontend.image.tag=$(IMAGE_TAG)

remove-platform:
	@echo "Removing $(PLATFORM_HELM_RELEASE_NAME)"
	helm uninstall $(PLATFORM_HELM_RELEASE_NAME) --namespace $(HELM_NAMESPACE) || true

rebuild:
	make build-platform-images
	make remove-platform
	make deploy-platform