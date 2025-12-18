# Technical Architecture

This document describes the technical implementation details of the frontend application.

## Technology Stack

### Core Framework

- **React 18**: UI library with hooks and functional components
- **TypeScript**: Type-safe JavaScript
- **Vite**: Fast build tool and dev server

### Routing

- **React Router v6**: Client-side routing
- **Routes**:
  - `/` → Redirects to `/workflows`
  - `/workflows` → Workflow list page
  - `/workflows/:workflowId` → Workflow versions page
  - `/workflows/:workflowId/versions/:versionId` → Workflow editor page

### State Management

- **React Query (TanStack Query)**: Server state management
  - Caching
  - Automatic refetching
  - Optimistic updates
  - Query invalidation
- **React Hooks**: Local component state
  - `useState` for local state
  - `useCallback` for memoized callbacks
  - `useMemo` for computed values
  - `useEffect` for side effects

### Graph Editor

- **React Flow (@xyflow/react)**: Visual graph editor
  - Custom node types (WorkflowNode, TriggerNode)
  - Custom edge styling
  - Drag and drop
  - Zoom and pan
  - Minimap
  - Controls

### Styling

- **Tailwind CSS**: Utility-first CSS framework
- **Custom Theme**: Loom brand colors
- **Responsive Design**: Mobile-friendly layouts

## Project Structure

```
src/
├── api/              # API client functions
│   ├── client.ts     # Base HTTP client
│   ├── workflows.ts  # Configuration service API
│   └── layout.ts     # Layout service API
├── components/       # React components
│   ├── editor/      # Editor-specific components
│   │   ├── WorkflowNode.tsx
│   │   ├── TriggerNode.tsx
│   │   ├── NodeConfigPanel.tsx
│   │   ├── TriggersPanel.tsx
│   │   ├── VariablesPanel.tsx
│   │   └── LabelsPanel.tsx
│   └── Layout.tsx   # Main layout wrapper
├── pages/            # Page components
│   ├── WorkflowListPage.tsx
│   ├── WorkflowVersionsPage.tsx
│   └── WorkflowEditorPage.tsx
├── types/            # TypeScript type definitions
│   └── index.ts
├── App.tsx           # Root component with routing
└── main.tsx          # Application entry point
```

## API Integration

### API Client

**Base Client** (`api/client.ts`):
- Wraps native Fetch API
- Handles errors
- Sets content-type headers
- Uses environment variables for base URL

**Configuration Service API** (`api/workflows.ts`):
- Workflow operations
- Node operations
- Connection operations
- Trigger operations
- Variable and label operations

**Layout Service API** (`api/layout.ts`):
- Get layout for workflow version
- Upsert node layout (single and batch)
- Copy layout from version
- Delete node layout

### Environment Variables

- `VITE_API_BASE_URL`: Base URL for Configuration Service API
- `VITE_LAYOUT_API_URL`: Base URL for Layout Service API
- Defaults to `/api` for both (proxied through Nginx)

## React Flow Integration

### Node Types

**Custom Node Components**:
- `WorkflowNode`: Regular workflow nodes
- `TriggerNode`: Trigger nodes (separate visual representation)

**Node Registration**:
```typescript
const nodeTypes = {
  workflow: WorkflowNode,
  trigger: TriggerNode,
}
```

### Edge Configuration

- **Connection Types**: Success (green), RecoverableFailure (red, animated)
- **Handles**: Source handles on right, target handles on left
- **Markers**: Arrow markers on edges
- **Labels**: Connection type labels

### Layout Direction

- **Left-to-Right**: Horizontal flow
- **Trigger Nodes**: Positioned on left
- **Workflow Nodes**: Flow from left to right
- **Default Positions**: Horizontal grid layout

## State Management Patterns

### Server State (React Query)

**Query Keys**:
- `['workflows']`: List of workflows
- `['versions', workflowId]`: Versions for workflow
- `['versionDetails', versionId]`: Complete workflow details
- `['layout', versionId]`: Layout data for version

**Mutations**:
- All mutations invalidate relevant queries
- Optimistic updates where appropriate
- Error handling with rollback

### Local State

**Component State**:
- Selected node ID
- Active panel type
- Validation results
- Form inputs

**React Flow State**:
- Nodes and edges managed by React Flow hooks
- `useNodesState` and `useEdgesState`
- Positions synced with layout service

## Layout Persistence

### Saving Positions

**Debounced Saves**:
- 500ms debounce during dragging
- Final save on drag stop
- Batch updates for multiple nodes

**Implementation**:
- `onNodeDrag`: Updates pending saves
- `onNodeDragStop`: Flushes pending saves
- Uses `useRef` for debounce timer
- Batch mutation for efficiency

### Loading Positions

**Race Condition Prevention**:
- Waits for both `versionDetails` and `layoutData`
- Checks `isLoadingLayout` before setting nodes
- Prioritizes layout positions on initial load

**Position Resolution**:
1. Check existing React Flow positions (user has dragged)
2. Check layout service positions
3. Fallback to default positions

## Error Handling

### API Errors

- **Network Errors**: Displayed in UI
- **Validation Errors**: Shown inline
- **404 Errors**: Handled gracefully
- **Retry Logic**: Manual retry for failed operations

### User Feedback

- **Loading States**: Spinners during operations
- **Success Feedback**: Visual confirmation
- **Error Messages**: Clear error descriptions
- **Confirmation Dialogs**: For destructive operations

## Performance Optimizations

### React Optimizations

- **Memoization**: `useMemo` for computed values
- **Callback Memoization**: `useCallback` for event handlers
- **Component Memoization**: `memo` for node components
- **Query Caching**: React Query automatic caching

### Graph Editor Optimizations

- **Debounced Saves**: Reduces API calls
- **Batch Updates**: Multiple changes in single request
- **Lazy Loading**: Load data only when needed
- **Virtual Scrolling**: For large lists (future)

## Build and Deployment

### Development

- **Vite Dev Server**: Fast HMR (Hot Module Replacement)
- **TypeScript**: Type checking
- **ESLint**: Code linting

### Production Build

- **Vite Build**: Optimized production bundle
- **Static Assets**: Served via Nginx
- **Environment Variables**: Injected at build time
- **Docker**: Containerized deployment

### Nginx Configuration

- **Static File Serving**: Serves React app
- **API Proxying**: Proxies to backend services
- **SPA Routing**: Fallback to index.html
- **Gzip Compression**: Enabled for assets

## Browser Compatibility

- **Modern Browsers**: Chrome, Firefox, Safari, Edge (latest versions)
- **ES6+ Features**: Uses modern JavaScript
- **Fetch API**: Native fetch (no polyfills)
- **CSS Grid/Flexbox**: Modern CSS layouts

