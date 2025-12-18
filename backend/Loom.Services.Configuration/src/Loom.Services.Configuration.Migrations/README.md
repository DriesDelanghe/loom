# Loom Configuration Service Migrations

This folder contains Flyway migrations for setting up the Loom Configuration Service database schema.

## Structure

- `sql/` - Contains Flyway migration SQL files (V1__*.sql, V2__*.sql, etc.)
- `flyway.conf` - Flyway configuration file
- `Dockerfile` - Docker image for running migrations

## Running Migrations

### Using Docker

```bash
docker build -t loom-configuration-migrations -f src/Loom.Services.Configuration.Migrations/Dockerfile src/Loom.Services.Configuration.Migrations

docker run --rm \
  -e FLYWAY_DB_HOST=localhost \
  -e FLYWAY_DB_PORT=5432 \
  -e FLYWAY_DB_NAME=loom_configuration \
  -e FLYWAY_DB_USER=postgres \
  -e FLYWAY_DB_PASSWORD=postgres \
  loom-configuration-migrations
```

### Using Docker Compose

Add to your docker-compose.yml:

```yaml
services:
  migrations:
    build:
      context: .
      dockerfile: src/Loom.Services.Configuration.Migrations/Dockerfile
    environment:
      FLYWAY_DB_HOST: db
      FLYWAY_DB_PORT: 5432
      FLYWAY_DB_NAME: loom_configuration
      FLYWAY_DB_USER: postgres
      FLYWAY_DB_PASSWORD: postgres
    depends_on:
      - db
```

## Migration Files

- `V1__Create_WorkflowDefinitions.sql` - Creates WorkflowDefinitions table
- `V2__Create_WorkflowVersions.sql` - Creates WorkflowVersions table
- `V3__Create_Nodes.sql` - Creates Nodes table
- `V4__Create_Connections.sql` - Creates Connections table
- `V5__Create_Variables.sql` - Creates Variables table
- `V6__Create_Labels.sql` - Creates Labels table
- `V7__Create_Settings.sql` - Creates Settings table
- `V8__Create_Triggers.sql` - Creates Triggers table
- `V9__Create_TriggerBindings.sql` - Creates TriggerBindings table

