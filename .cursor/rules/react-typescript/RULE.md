---
alwaysApply: true
---

# Loom React + TypeScript Code Style Rules

## Project Structure

- **Pages**: `src/pages/` - Route-level components (e.g., `WorkflowListPage.tsx`, `WorkflowEditorPage.tsx`)
- **Components**: `src/components/` - Reusable UI components (e.g., `WorkflowNode.tsx`, `NodeConfigPanel.tsx`)
- **API Clients**: `src/api/` - API client modules (e.g., `workflows.ts`, `client.ts`, `layout.ts`)
- **Types**: `src/types/` - TypeScript type definitions (e.g., `index.ts`)
- **Utils**: `src/utils/` - Utility functions (e.g., `nodeMetadata.ts`)

## File Naming Conventions

- **Components**: PascalCase with `.tsx` extension (e.g., `WorkflowNode.tsx`, `NodeConfigPanel.tsx`)
- **Pages**: PascalCase with `.tsx` extension (e.g., `WorkflowListPage.tsx`, `WorkflowEditorPage.tsx`)
- **API Clients**: camelCase with `.ts` extension (e.g., `workflows.ts`, `client.ts`)
- **Types**: `index.ts` or descriptive name with `.ts` extension
- **Utils**: camelCase with `.ts` extension (e.g., `nodeMetadata.ts`)

## TypeScript Conventions

### Type Definitions

- **Union Types**: Use `type` for string literal unions and aliases
  ```typescript
  export type WorkflowStatus = 'Draft' | 'Published' | 'Archived'
  export type NodeType = 'Action' | 'Condition' | 'Validation' | 'Split' | 'Join'
  export type ConnectionOutcome = string
  ```

- **Interfaces**: Use `interface` for object shapes
  ```typescript
  export interface WorkflowDefinition {
    id: string
    name: string
    hasPublishedVersion: boolean
    latestVersion: number | null
  }
  ```

- **Component Props**: Define as `interface` with `Props` suffix
  ```typescript
  interface NodeConfigPanelProps {
    node: Node
    isReadOnly: boolean
    onClose: () => void
    onDelete: () => void
    versionId: string
  }
  ```

- **Component Data Props**: Extend `Record<string, unknown>` for React Flow and similar libraries
  ```typescript
  export interface WorkflowNodeData extends Record<string, unknown> {
    id: string
    workflowVersionId: string
    key: string
    // ...
  }
  ```

- **Nullable Types**: Use `| null` for nullable properties
  ```typescript
  name: string | null
  publishedAt: string | null
  ```

- **Optional Properties**: Use `?` for optional properties
  ```typescript
  isEndNode?: boolean
  description?: string
  ```

- **Record Types**: Use `Record<K, V>` for mapped types
  ```typescript
  const nodeTypeIcons: Record<NodeType, string> = {
    Action: '⚡',
    Condition: '❓',
    // ...
  }
  ```

- **Array Types**: Use `T[]` syntax (preferred over `Array<T>`)
  ```typescript
  nodes: Node[]
  errors: string[]
  ```

### Type Imports

- Use `type` keyword for type-only imports
  ```typescript
  import type { NodeType, NodeCategory } from '../types'
  import type { NodeProps } from '@xyflow/react'
  ```

- Import types from same file when exporting both component and types
  ```typescript
  import { WorkflowNode, type WorkflowNodeData } from '../components/editor/WorkflowNode'
  ```

### Type Assertions

- Use `as` for type assertions when necessary
  ```typescript
  const data = props.data as WorkflowNodeData
  setType(e.target.value as NodeType)
  ```

- Use non-null assertion `!` sparingly and only when certain
  ```typescript
  queryFn: () => workflowsApi.getVersionDetails(versionId!)
  ```

## React Component Patterns

### Functional Components

- Always use functional components (no class components)
- Use named exports for components
- Export both component and types from same file when appropriate

### Component Structure

```typescript
import { useState, useCallback } from 'react'
import type { NodeType } from '../types'

interface ComponentProps {
  // props
}

export function ComponentName({ prop1, prop2 }: ComponentProps) {
  // hooks
  // state
  // effects
  // callbacks
  // computed values
  
  return (
    // JSX
  )
}
```

### Memoization

- Use `memo()` for React Flow nodes and frequently re-rendered components
  ```typescript
  export const WorkflowNode = memo(function WorkflowNode(props: NodeProps) {
    // component implementation
  })
  ```

- Use `useCallback` for event handlers passed to memoized children
- Use `useMemo` for expensive computations
- Include all dependencies in dependency arrays

### React Hooks

- **useState**: For local component state
  ```typescript
  const [name, setName] = useState(node.name || '')
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null)
  ```

- **useCallback**: For memoized callbacks
  ```typescript
  const onConnect = useCallback(
    (params: Connection) => {
      // handler logic
    },
    [dependencies]
  )
  ```

- **useMemo**: For computed values
  ```typescript
  const isReadOnly = useMemo(
    () => versionDetails?.version.status !== 'Draft',
    [versionDetails?.version.status]
  )
  ```

- **useEffect**: For side effects
  ```typescript
  useEffect(() => {
    // effect logic
  }, [dependencies])
  ```

- **useRef**: For mutable refs (e.g., debounce timers)
  ```typescript
  const saveLayoutDebounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  ```

### React Query Patterns

- **Query Keys**: Use arrays with descriptive keys
  ```typescript
  queryKey: ['versionDetails', versionId]
  queryKey: ['layout', versionId]
  queryKey: ['workflows']
  ```

- **Query Configuration**:
  ```typescript
  const { data, isLoading, error } = useQuery({
    queryKey: ['versionDetails', versionId],
    queryFn: () => workflowsApi.getVersionDetails(versionId!),
    enabled: !!versionId, // Conditional fetching
  })
  ```

- **Mutations**:
  ```typescript
  const mutation = useMutation({
    mutationFn: (data: { ... }) => api.method(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionId] })
      // Reset form state
    },
  })
  ```

- **Cache Invalidation**: Invalidate related queries in `onSuccess`
  ```typescript
  queryClient.invalidateQueries({ queryKey: ['versionDetails', versionId] })
  ```

## API Client Patterns

### Client Structure

- Group API methods by resource (e.g., `workflowsApi`, `nodesApi`, `connectionsApi`)
- Use arrow functions for methods
- Use generic type parameters for response types
- Use optional parameters with `?` and default values

```typescript
export const workflowsApi = {
  getWorkflows: () =>
    api.get<WorkflowDefinition[]>(`/workflows?tenantId=${TENANT_ID}`),

  createWorkflow: (name: string, description?: string) =>
    api.post<IdResponse>('/workflows', { tenantId: TENANT_ID, name, description }),

  getVersionDetails: (versionId: string) =>
    api.get<WorkflowVersionDetails>(`/workflows/versions/${versionId}`),
}
```

### API Client Base

- Use `import.meta.env` for Vite environment variables
- Create reusable `request` function with error handling
- Export typed API methods object

```typescript
const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api'

async function request<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${endpoint}`, {
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
    ...options,
  })

  if (!response.ok) {
    const error = await response.text()
    throw new ApiError(response.status, error || response.statusText)
  }

  if (response.status === 204) {
    return {} as T
  }

  return response.json()
}

export const api = {
  get: <T>(endpoint: string) => request<T>(endpoint),
  post: <T>(endpoint: string, body?: unknown) =>
    request<T>(endpoint, { method: 'POST', body: body ? JSON.stringify(body) : undefined }),
  put: <T>(endpoint: string, body?: unknown) =>
    request<T>(endpoint, { method: 'PUT', body: body ? JSON.stringify(body) : undefined }),
  delete: <T>(endpoint: string) => request<T>(endpoint, { method: 'DELETE' }),
}
```

## Styling Conventions

### Tailwind CSS

- Use Tailwind utility classes for styling
- Use template literals for conditional classes
- Use inline `style` prop for dynamic values (e.g., positions, colors from data)

```typescript
<div
  className={`
    px-4 py-3 rounded-lg border-2 shadow-sm min-w-[140px]
    ${nodeTypeColors[data.type]}
    ${selected ? 'shadow-lg' : ''}
    ${isControl ? 'border-orange-500 border-2' : ''}
    ${data.isEndNode ? 'ring-2 ring-green-300' : ''}
  `}
>
```

### Conditional Rendering

- Use ternary operators for simple conditionals
- Use `&&` for conditional rendering
- Extract complex conditionals to variables or functions

```typescript
{isReadOnly && (
  <button onClick={onDelete}>Delete</button>
)}

{workflows?.length === 0 ? (
  <EmptyState />
) : (
  <WorkflowList />
)}
```

## Event Handlers

### Naming

- Use `handle` prefix for event handlers: `handleCreate`, `handleSave`, `handleDelete`
- Use `on` prefix for props: `onClose`, `onDelete`, `onSave`

### Form Handlers

```typescript
const handleCreate = (e: React.FormEvent) => {
  e.preventDefault()
  if (newName.trim()) {
    createMutation.mutate()
  }
}
```

### Click Handlers

```typescript
onClick={() => setShowCreate(true)}
onClick={() => mutation.mutate(id)}
onClick={onClose}
```

## Utility Functions

### File Organization

- Place utility functions in `src/utils/` directory
- Export as named exports
- Use descriptive function names

### Function Patterns

```typescript
export const isControlNode = (nodeType: NodeType): boolean => {
  return nodeCategoryMap[nodeType] === 'Control'
}

export const getPositiveOutcome = (nodeType: NodeType): string => {
  switch (nodeType) {
    case 'Action':
      return 'Completed'
    case 'Condition':
      return 'True'
    // ...
    default:
      return 'Completed'
  }
}
```

### Constants

- Use `const` for constants
- Use `Record` type for lookup maps
- Place constants at top of file

```typescript
const nodeTypeIcons: Record<NodeType, string> = {
  Action: '⚡',
  Condition: '❓',
  Validation: '✓',
  Split: '⤢',
  Join: '⤣',
}

const TENANT_ID = '00000000-0000-0000-0000-000000000001'
```

## React Flow Patterns

### Node Types

- Define node types object with `as NodeTypes` assertion
- Export node components and their data types

```typescript
const nodeTypes = {
  workflow: WorkflowNode,
  trigger: TriggerNode,
} as NodeTypes
```

### Node Data

- Extend `Record<string, unknown>` for node data interfaces
- Include all necessary properties for rendering and logic

### Edge Creation

- Use descriptive IDs for edges
- Include `sourceHandle` and `targetHandle` for proper connection points
- Add labels and styling for visual differentiation

```typescript
const edges: FlowEdge[] = connections.map((conn) => ({
  id: conn.id,
  source: conn.fromNodeId,
  target: conn.toNodeId,
  sourceHandle: `source-${conn.outcome}`,
  targetHandle: 'target',
  label: conn.outcome,
  style: {
    stroke: isPositive ? '#10b981' : '#ef4444',
  },
}))
```

## Import Organization

### Import Order

1. React and React-related imports
2. Third-party libraries (react-router-dom, @tanstack/react-query, @xyflow/react)
3. API clients
4. Components
5. Types (use `type` keyword)
6. Utils
7. Styles (CSS imports)

### Example

```typescript
import { useState, useCallback, useEffect, useRef } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ReactFlow, Background, Controls, type Node as FlowNode } from '@xyflow/react'
import '@xyflow/react/dist/style.css'

import { workflowsApi, nodesApi } from '../api/workflows'
import { layoutApi } from '../api/layout'
import { WorkflowNode, type WorkflowNodeData } from '../components/editor/WorkflowNode'
import type { Node, Connection } from '../types'
import { getOutcomeFromHandleId } from '../utils/nodeMetadata'
```

## Code Style

### Variable Naming

- Use camelCase for variables and functions
- Use PascalCase for components and types
- Use descriptive names: `selectedNodeId`, `isReadOnly`, `versionDetails`

### Optional Chaining

- Use optional chaining for safe property access
  ```typescript
  versionDetails?.version.status
  layoutData?.nodes.map(...)
  ```

### Nullish Coalescing

- Use `??` for default values
  ```typescript
  const name = node.name || node.key
  const position = layoutPosition || { x: 50, y: 50 }
  ```

### Array Methods

- Use `.map()`, `.filter()`, `.find()`, `.some()`, `.forEach()` as appropriate
- Use `.find()` for single item lookup
- Use `.some()` for existence checks
- Use `.filter()` for filtering arrays

### Object Destructuring

- Use destructuring for props and object properties
  ```typescript
  export function Component({ prop1, prop2, onClose }: ComponentProps) {
    const { workflowId, versionId } = useParams<{ workflowId: string; versionId: string }>()
  }
  ```

### Template Literals

- Use template literals for string interpolation
  ```typescript
  const triggerNodeId = `trigger-${tb.id}`
  const edgeId = `trigger-edge-${tb.id}-${nb.id}`
  ```

## Error Handling

### React Query Errors

- Handle loading and error states in components
  ```typescript
  if (isLoading) {
    return <LoadingSpinner />
  }

  if (error) {
    return <ErrorMessage />
  }
  ```

### Try-Catch

- Use try-catch for JSON parsing and other operations that can throw
  ```typescript
  try {
    const parsed = JSON.parse(config)
    setConfigError(null)
    updateConfigMutation.mutate(parsed)
  } catch {
    setConfigError('Invalid JSON')
  }
  ```

## Comments

- Use comments sparingly - code should be self-documenting
- Use comments for complex logic or non-obvious behavior
- Use JSX comments for section markers: `{/* Section comment */}`

## Formatting

- Use 2 spaces for indentation
- Use single quotes for strings (if consistent with project)
- Use trailing commas in objects and arrays
- Use semicolons (if consistent with project)
- Remove trailing whitespace

## Example Patterns

### Page Component

```typescript
import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { workflowsApi } from '../api/workflows'
import type { WorkflowDefinition } from '../types'

export function WorkflowListPage() {
  const queryClient = useQueryClient()
  const [showCreate, setShowCreate] = useState(false)
  const [newName, setNewName] = useState('')

  const { data: workflows, isLoading, error } = useQuery({
    queryKey: ['workflows'],
    queryFn: workflowsApi.getWorkflows,
  })

  const createMutation = useMutation({
    mutationFn: () => workflowsApi.createWorkflow(newName),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workflows'] })
      setShowCreate(false)
      setNewName('')
    },
  })

  if (isLoading) return <LoadingSpinner />
  if (error) return <ErrorMessage />

  return (
    <div className="container">
      {/* JSX */}
    </div>
  )
}
```

### Memoized Component

```typescript
import { memo } from 'react'
import type { NodeType } from '../types'

export interface ComponentData extends Record<string, unknown> {
  id: string
  name: string
  type: NodeType
}

export const Component = memo(function Component(props: NodeProps) {
  const data = props.data as ComponentData
  
  return (
    <div className="...">
      {/* JSX */}
    </div>
  )
})
```

### API Client Module

```typescript
import { api } from './client'
import type { WorkflowDefinition, WorkflowVersion } from '../types'

interface IdResponse {
  id: string
}

export const workflowsApi = {
  getWorkflows: () =>
    api.get<WorkflowDefinition[]>(`/workflows?tenantId=${TENANT_ID}`),

  getVersionDetails: (versionId: string) =>
    api.get<WorkflowVersionDetails>(`/workflows/versions/${versionId}`),

  createWorkflow: (name: string, description?: string) =>
    api.post<IdResponse>('/workflows', { tenantId: TENANT_ID, name, description }),
}
```

### Utility Functions

```typescript
import type { NodeType, NodeCategory } from '../types'

export const nodeCategoryMap: Record<NodeType, NodeCategory> = {
  Action: 'Action',
  Condition: 'Condition',
  // ...
}

export const isControlNode = (nodeType: NodeType): boolean => {
  return nodeCategoryMap[nodeType] === 'Control'
}

export const getPositiveOutcome = (nodeType: NodeType): string => {
  switch (nodeType) {
    case 'Action':
      return 'Completed'
    case 'Condition':
      return 'True'
    default:
      return 'Completed'
  }
}
```

## When Adding New Features

1. **Pages**: Create page component in `src/pages/`
2. **Components**: Create reusable component in `src/components/`
3. **API Methods**: Add methods to appropriate API client in `src/api/`
4. **Types**: Add types to `src/types/index.ts` or create new type file
5. **Utils**: Add utility functions to `src/utils/` if needed
6. **Routes**: Add route in `App.tsx` if new page
7. **React Query**: Use `useQuery` for data fetching, `useMutation` for mutations

