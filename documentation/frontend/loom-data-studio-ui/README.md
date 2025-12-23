# Loom Data Studio UI

The Loom Data Studio UI is a React-based web application for visually designing and managing data schema configurations.

## Table of Contents

1. [Overview](#overview)
2. [Core Concepts](#core-concepts)
3. [User Workflows](#user-workflows)
4. [API Integration](#api-integration)
5. [Technical Architecture](#technical-architecture)

## Overview

### Purpose

The frontend provides a visual, interactive interface for:

- Creating and managing data schemas
- Defining schema structure with fields and types
  - **Scalar fields**: Primitive values (string, int, boolean, etc.)
  - **Object fields**: References to other schemas
  - **Array fields**: Arrays of scalars (e.g., `string[]`, `int[]`) or arrays of objects (e.g., `OrderItem[]`)
- Configuring validation rules
- Designing data transformations (simple and advanced)
  - **Simple mode**: Direct field mappings with UI guards for type compatibility
  - **Advanced mode**: Graph-based transformations with explicit nested transformations
- Defining business keys for master data
- Managing schema version lifecycle
- Organizing schemas with tags

### Technology Stack

- **Framework**: React 18 with TypeScript
- **Routing**: React Router v6
- **State Management**: React Query (TanStack Query) for server state
- **Graph Editor**: React Flow (@xyflow/react) for visual transformation graph editing
- **Styling**: Tailwind CSS
- **Build Tool**: Vite
- **HTTP Client**: Native Fetch API

### Architecture

The application follows a **component-based architecture** with:

- **Pages**: Top-level route components (SchemaOverview, SchemaKeyOverview, SchemaVersionDetail)
- **Components**: Reusable UI components (editors, panels, inspectors)
- **API Layer**: Client functions for backend communication
- **State Management**: React Query for server state, React hooks for local state

### Key Features

- **Visual Schema Editor**: Tree-based structure editor for schema fields
  - Element type selector for arrays (scalar vs object)
  - Field type display shows array element types (e.g., `string[]`, `OrderItem[]`)
- **Validation Editor**: Rule-based validation configuration
- **Transformation Editor**: Simple table-based and advanced graph-based transformation design
  - Type compatibility validation with helpful error messages
  - UI guards to prevent invalid mappings in Simple mode
  - Nested transformation selector for Object/Object-Array fields
- **Business Key Editor**: Visual key definition with field ordering
- **Real-time Updates**: Optimistic UI updates with React Query
- **Validation Feedback**: Inline validation errors and warnings
- **Read-Only Mode**: Immutable view for published versions
- **Expert Mode**: Advanced features for power users


