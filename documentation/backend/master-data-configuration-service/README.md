# Master Data Configuration Service

The Master Data Configuration Service is responsible for managing data schemas, their structure, validation rules, transformation specifications, and business keys for the Loom data platform.

## Table of Contents

1. [Overview](#overview)
2. [Core Concepts](#core-concepts)
3. [Functional Workflows](#functional-workflows)
4. [API Overview](#api-overview)
5. [Business Rules and Validations](#business-rules-and-validations)
6. [Data Model](#data-model)

## Overview

### Purpose

The Master Data Configuration Service provides a complete CQRS-based API for managing data schema configurations. It handles:

- Data schema lifecycle (creation, versioning, publishing)
- Schema structure definition (fields, types, references)
  - **Scalar fields**: Primitive values (string, int, boolean, etc.)
  - **Object fields**: References to other schemas
  - **Array fields**: Arrays of scalars (e.g., `string[]`, `int[]`) or arrays of objects (e.g., `OrderItem[]`)
- Validation rule configuration
- Transformation specification (simple and advanced graph-based)
  - **Simple mode**: Direct field mappings with support for scalar arrays and field extraction
  - **Advanced mode**: Graph-based transformations with explicit nested transformations
- Business key definitions for master data
- Schema tagging and organization

### Architecture

The service follows a **CQRS (Command Query Responsibility Segregation)** pattern:

- **Commands**: Mutations that change schema state (create, update, delete, publish)
- **Queries**: Read operations that retrieve schema information
- **Domain Models**: Core business entities (DataSchema, FieldDefinition, ValidationSpec, TransformationSpec, etc.)
- **Persistence Layer**: Database entities and mappings
- **Static Validation Engine**: Validates all configurations at publish time

### Key Principles

1. **Configuration-Only**: No runtime execution logic, all behavior is declarative
2. **Publish-Time Validation**: All correctness checks performed before publishing
3. **Immutable Published Objects**: Published schemas cannot be modified
4. **Draft → Published → Archived Lifecycle**: Clear state transitions
5. **Schema Roles**: Schemas are categorized as Incoming, Master, or Outgoing
6. **Versioning**: Each schema can have multiple versions, unique by (Key, Role)

### Database

- **PostgreSQL** with JSONB support for flexible configuration storage
- **Flyway** migrations for schema management
- **Entity Framework Core** for ORM


