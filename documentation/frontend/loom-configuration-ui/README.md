# Loom Configuration UI

The Loom Configuration UI is a React-based web application for visually designing and managing workflow configurations.

## Table of Contents

1. [Overview](#overview)
2. [Core Concepts](#core-concepts)
3. [User Workflows](#user-workflows)
4. [Technical Architecture](#technical-architecture)
5. [API Integration](#api-integration)

## Overview

### Purpose

The frontend provides a visual, interactive interface for:

- Creating and managing workflow definitions
- Designing workflow graphs with nodes and connections
- Configuring triggers, variables, and labels
- Validating and publishing workflow versions
- Managing workflow version lifecycle

### Technology Stack

- **Framework**: React 18 with TypeScript
- **Routing**: React Router v6
- **State Management**: React Query (TanStack Query) for server state
- **Graph Editor**: React Flow (@xyflow/react) for visual workflow editing
- **Styling**: Tailwind CSS
- **Build Tool**: Vite
- **HTTP Client**: Native Fetch API

### Architecture

The application follows a **component-based architecture** with:

- **Pages**: Top-level route components (WorkflowList, WorkflowVersions, WorkflowEditor)
- **Components**: Reusable UI components (panels, nodes, etc.)
- **API Layer**: Client functions for backend communication
- **State Management**: React Query for server state, React hooks for local state

### Key Features

- **Visual Graph Editor**: Drag-and-drop workflow design with React Flow
- **Real-time Updates**: Optimistic UI updates with React Query
- **Layout Persistence**: Automatic saving of node positions
- **Validation Feedback**: Inline validation errors and warnings
- **Read-Only Mode**: Immutable view for published versions

