# Loom Platform Helm Chart

This is the parent Helm chart that deploys the complete Loom platform, including:
- Configuration Service (API + Layout Service + Frontend)
- MasterData Configuration Service (API + Frontend)

## Installation

```bash
# Build dependencies first
helm dependency build

# Deploy everything
helm upgrade --install loom-platform . \
  --namespace loom \
  --create-namespace \
  --set loom-configuration.image.repository=myregistry/loom-configuration \
  --set loom-configuration.image.tag=v1.0.0 \
  --set loom-masterdata-configuration.image.repository=myregistry/loom-masterdata-configuration \
  --set loom-masterdata-configuration.image.tag=v1.0.0
```

## Subcharts

- `loom-configuration`: Configuration service with workflow management
- `loom-masterdata-configuration`: MasterData configuration service with data schema management

## Values

All values from the subcharts can be overridden using the subchart name as a prefix:

```yaml
loom-configuration:
  replicaCount: 2
  image:
    repository: myregistry/loom-configuration
    tag: v1.0.0

loom-masterdata-configuration:
  replicaCount: 2
  image:
    repository: myregistry/loom-masterdata-configuration
    tag: v1.0.0
```

## Disabling Services

To disable a service:

```yaml
loom-configuration:
  enabled: false

loom-masterdata-configuration:
  enabled: false
```

