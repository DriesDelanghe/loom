# MasterDataConfiguration Migrations

This directory contains Flyway migrations for the MasterDataConfiguration service database schema.

## Structure

- `flyway.conf`: Flyway configuration file
- `Dockerfile`: Docker image for running migrations
- `sql/`: SQL migration files (V{version}__{description}.sql)

## Migration Files

1. V1__Create_DataModels.sql - DataModels table
2. V2__Create_DataSchemas.sql - DataSchemas table
3. V3__Create_FieldDefinitions.sql - FieldDefinitions table
4. V4__Create_SchemaFlows.sql - SchemaFlows table
5. V5__Create_SchemaTags.sql - SchemaTags table
6. V6__Create_KeyDefinitions.sql - KeyDefinitions table
7. V7__Create_KeyFields.sql - KeyFields table
8. V8__Create_ValidationSpecs.sql - ValidationSpecs table
9. V9__Create_ValidationRules.sql - ValidationRules table
10. V10__Create_ValidationReferences.sql - ValidationReferences table
11. V11__Create_TransformationSpecs.sql - TransformationSpecs table
12. V12__Create_SimpleTransformRules.sql - SimpleTransformRules table
13. V13__Create_TransformGraphNodes.sql - TransformGraphNodes table
14. V14__Create_TransformGraphEdges.sql - TransformGraphEdges table
15. V15__Create_TransformOutputBindings.sql - TransformOutputBindings table
16. V16__Create_TransformReferences.sql - TransformReferences table

## Running Migrations

Migrations are automatically run as an init container in the Kubernetes deployment.

For local development:
```bash
docker build -t loom-masterdata-configuration-migrations .
docker run --rm \
  -e FLYWAY_DB_HOST=localhost \
  -e FLYWAY_DB_PORT=5432 \
  -e FLYWAY_DB_NAME=loom_masterdata_configuration \
  -e FLYWAY_DB_USER=postgres \
  -e FLYWAY_DB_PASSWORD=postgres \
  loom-masterdata-configuration-migrations
```

