# Configuration Service

The Configuration Service is the core service responsible for managing workflow definitions, versions, and their complete configuration including nodes, connections, triggers, variables, and labels.

## Table of Contents

1. [Overview](#overview)
2. [Core Concepts](#core-concepts)
3. [Functional Workflows](#functional-workflows)
4. [API Overview](#api-overview)
5. [Business Rules and Validations](#business-rules-and-validations)
6. [Data Model](#data-model)

## Overview

### Purpose

The Configuration Service provides a complete CQRS-based API for managing workflow configurations. It handles:

- Workflow lifecycle (creation, versioning, publishing)
- Workflow graph structure (nodes and connections)
- Trigger configuration and binding
- Workflow variables and labels
- Workflow validation and compilation

### Architecture

The service follows a **CQRS (Command Query Responsibility Segregation)** pattern:

- **Commands**: Mutations that change workflow state (create, update, delete)
- **Queries**: Read operations that retrieve workflow information
- **Domain Models**: Core business entities (Workflow, Node, Connection, Trigger, etc.)
- **Persistence Layer**: Database entities and mappings

### APIs

The service exposes two APIs:

1. **HTTP REST API**: Standard REST endpoints for web clients
2. **gRPC API**: Code-first gRPC service for high-performance inter-service communication

Both APIs provide the same functionality, just different transport protocols.

