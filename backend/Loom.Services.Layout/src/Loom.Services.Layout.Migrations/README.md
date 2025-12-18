# Layout Service Migrations

This project contains Flyway migrations for the Layout service database.

## Usage

Run migrations using the Docker image:

```bash
docker run --rm \
  -e FLYWAY_DB_HOST=localhost \
  -e FLYWAY_DB_PORT=5432 \
  -e FLYWAY_DB_NAME=loom_layout \
  -e FLYWAY_DB_USER=loom_layout_user \
  -e FLYWAY_DB_PASSWORD=loom_layout_password \
  loom-layout-migrations:latest
```

