# Loom Data Studio

A web-based configuration UI for managing data schemas, validations, and transformations in the Loom platform.

## Features

- **Schema Management**: Create and manage Incoming, Master, and Outgoing data schemas
- **Tree-Based Editor**: Visual tree editor for schema structure
- **Validation Rules**: Configure field and cross-field validation rules
- **Transformations**: Simple and advanced transformation modes
- **Key Definitions**: Define business keys for master data (Master schemas only)
- **Version Control**: Draft → Publish → Archive lifecycle management

## Navigation Structure

```
Data Studio
├── Incoming Data
│   ├── Schema Overview
│   ├── Schema Key Overview
│   └── Schema Version Detail
├── Master Data
│   ├── Schema Overview
│   ├── Schema Key Overview
│   └── Schema Version Detail
└── Outgoing Data
    ├── Schema Overview
    ├── Schema Key Overview
    └── Schema Version Detail
```

## Development

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

## Tech Stack

- **React 18** - UI framework
- **TypeScript** - Type safety
- **React Router** - Navigation
- **TanStack Query** - Data fetching
- **Tailwind CSS** - Styling
- **Vite** - Build tool

## API Integration

The frontend integrates with the `Loom.Services.MasterDataConfiguration` backend service. The API base URL can be configured via the `VITE_API_BASE_URL` environment variable (defaults to `/api`).

## Project Structure

```
src/
├── api/              # API client modules
├── components/       # Reusable UI components
│   ├── editor/      # Schema editor components
│   └── common/      # Common UI components
├── pages/            # Route-level page components
├── types/            # TypeScript type definitions
└── utils/            # Utility functions
```

## Key Concepts

### Beginner vs Expert Mode

- **Beginner Mode (Default)**: Simplified UI with only essential features
- **Expert Mode**: Unlocks advanced features like key definitions, advanced transformations, and reference visualization

### Transformation Modes

- **Simple Mode**: Table-based field mapping (default)
- **Advanced Mode**: Graph-based transformation editor (expert mode only, one-way upgrade)

### Schema Lifecycle

- **Draft**: Editable, can be modified
- **Published**: Immutable, read-only
- **Archived**: Historical record, read-only

## Environment Variables

- `VITE_API_BASE_URL`: API base URL (default: `/api`)

