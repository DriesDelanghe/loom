export type WorkflowStatus = 'Draft' | 'Published' | 'Archived'
export type NodeType = 'Action' | 'Condition' | 'Validation' | 'Split' | 'Join'
export type NodeCategory = 'Action' | 'Condition' | 'Validation' | 'Control'
export type ConnectionOutcome = string // Semantic outcomes: 'Completed', 'Failed', 'True', 'False', 'Valid', 'Invalid', 'Next', 'Joined'
export type TriggerType = 'Manual' | 'Webhook' | 'Schedule'
export type VariableType = 'String' | 'Number' | 'Boolean' | 'Object' | 'Array'

export interface WorkflowDefinition {
  id: string
  name: string
  hasPublishedVersion: boolean
  latestVersion: number | null
}

export interface WorkflowVersion {
  id: string
  definitionId: string
  version: number
  status: WorkflowStatus
  createdAt: string
  createdBy: string
  publishedAt: string | null
  publishedBy: string | null
}

export interface Node {
  id: string
  workflowVersionId: string
  key: string
  name: string | null
  type: NodeType
  config: Record<string, unknown> | null
  createdAt: string
}

export interface Connection {
  id: string
  workflowVersionId: string
  fromNodeId: string
  toNodeId: string
  outcome: ConnectionOutcome
  order: number | null
}

export interface WorkflowVariable {
  id: string
  workflowVersionId: string
  key: string
  type: VariableType
  initialValue: string | null
  description: string | null
}

export interface WorkflowLabelDefinition {
  id: string
  workflowVersionId: string
  key: string
  type: VariableType
  description: string | null
}

export interface TriggerNodeBinding {
  id: string
  entryNodeId: string
  order: number
}

export interface TriggerBinding {
  id: string
  triggerId: string
  workflowVersionId: string
  enabled: boolean
  priority: number | null
  nodeBindings: TriggerNodeBinding[]
  triggerType: TriggerType
  triggerConfig: Record<string, unknown> | null
}

export interface WorkflowVersionDetails {
  version: WorkflowVersion
  nodes: Node[]
  connections: Connection[]
  variables: WorkflowVariable[]
  labels: WorkflowLabelDefinition[]
  settings: WorkflowSettings | null
  triggerBindings: TriggerBinding[]
}

export interface WorkflowSettings {
  id: string
  workflowVersionId: string
  maxRetries: number
  retryDelaySeconds: number
  timeoutSeconds: number | null
}

export interface ValidationResult {
  isValid: boolean
  errors: string[]
  warnings: string[]
}

export interface Trigger {
  id: string
  tenantId: string
  type: TriggerType
  config: Record<string, unknown> | null
}

