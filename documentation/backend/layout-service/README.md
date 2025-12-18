# Layout Service

The Layout Service is a Backend For Frontend (BFF) service responsible for persisting and retrieving node positions on the workflow editor canvas.

## Table of Contents

1. [Overview](#overview)
2. [Core Concepts](#core-concepts)
3. [Functional Workflows](#functional-workflows)
4. [API Overview](#api-overview)
5. [Data Model](#data-model)

## Overview

### Purpose

The Layout Service provides a simple, focused API for managing the visual layout of workflow nodes in the editor. It stores:

- Node positions (X, Y coordinates)
- Optional node dimensions (width, height)
- Layout metadata per workflow version

### Architecture

The service follows a **CQRS (Command Query Responsibility Segregation)** pattern:

- **Commands**: Mutations that save or update layout data
- **Queries**: Read operations that retrieve layout data
- **Persistence Layer**: Database entities for layout storage

### Design Philosophy

The Layout Service is intentionally **decoupled** from the Configuration Service:

- **Separation of Concerns**: Layout is a UI concern, not a workflow configuration concern
- **Independent Scaling**: Layout service can scale independently
- **Simplified Data Model**: Uses node keys (strings) instead of node IDs (UUIDs) for stability across versions

### Node Keys

The service uses **node keys** (string identifiers) rather than node IDs (UUIDs) because:

- Keys are stable across workflow version copies
- When a draft is created from a published version, nodes keep the same keys but get new IDs
- This allows layout positions to be easily copied between versions

### API

The service exposes a **HTTP REST API** only (no gRPC):

- Base path: `/api/workflow-versions/{workflowVersionId}/layout`
- All operations are scoped to a workflow version
- Tenant isolation via `X-Tenant-Id` header

