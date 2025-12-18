import { api } from './client'
import type {
  WorkflowDefinition,
  WorkflowVersion,
  WorkflowVersionDetails,
  ValidationResult,
  NodeType,
  ConnectionOutcome,
  VariableType,
  TriggerType,
} from '../types'

interface IdResponse {
  id: string
}

interface SuccessResponse {
  success: boolean
}

const TENANT_ID = '00000000-0000-0000-0000-000000000001'

export const workflowsApi = {
  getWorkflows: () =>
    api.get<WorkflowDefinition[]>(`/workflows?tenantId=${TENANT_ID}`),

  createWorkflow: (name: string, description?: string) =>
    api.post<IdResponse>('/workflows', { tenantId: TENANT_ID, name, description }),

  getVersions: (workflowId: string) =>
    api.get<WorkflowVersion[]>(`/workflows/${workflowId}/versions`),

  getVersionDetails: (versionId: string) =>
    api.get<WorkflowVersionDetails>(`/workflows/versions/${versionId}`),

  createDraftVersion: (workflowId: string, createdBy: string) =>
    api.post<IdResponse>(`/workflows/${workflowId}/versions/draft`, { createdBy }),

  publishVersion: (versionId: string, publishedBy: string) =>
    api.post<SuccessResponse>(`/workflows/versions/${versionId}/publish`, { publishedBy }),

  deleteVersion: (versionId: string) =>
    api.delete<SuccessResponse>(`/workflows/versions/${versionId}`),

  validateVersion: (versionId: string) =>
    api.get<ValidationResult>(`/workflows/versions/${versionId}/validate`),
}

export const nodesApi = {
  addNode: (workflowVersionId: string, key: string, name: string, type: NodeType, config?: Record<string, unknown>) =>
    api.post<IdResponse>('/nodes', { workflowVersionId, key, name, type, config }),

  updateNodeMetadata: (nodeId: string, name: string | null, type?: NodeType) =>
    api.put<SuccessResponse>(`/nodes/${nodeId}`, { nodeId, name, type }),

  updateNodeConfig: (nodeId: string, config: Record<string, unknown>) =>
    api.put<SuccessResponse>(`/nodes/${nodeId}/config`, { config }),

  removeNode: (nodeId: string) =>
    api.delete<SuccessResponse>(`/nodes/${nodeId}`),
}

export const connectionsApi = {
  addConnection: (workflowVersionId: string, fromNodeId: string, toNodeId: string, outcome: ConnectionOutcome, order: number | null = null) =>
    api.post<IdResponse>('/connections', { workflowVersionId, fromNodeId, toNodeId, outcome, order }),

  removeConnection: (connectionId: string) =>
    api.delete<SuccessResponse>(`/connections/${connectionId}`),
}

export const variablesApi = {
  addVariable: (workflowVersionId: string, key: string, type: VariableType, initialValue?: string, description?: string) =>
    api.post<IdResponse>('/variables', { workflowVersionId, key, type, initialValue, description }),

  updateVariable: (variableId: string, type: VariableType, initialValue?: string, description?: string) =>
    api.put<SuccessResponse>(`/variables/${variableId}`, { type, initialValue, description }),

  removeVariable: (variableId: string) =>
    api.delete<SuccessResponse>(`/variables/${variableId}`),
}

export const labelsApi = {
  addLabel: (workflowVersionId: string, key: string, type: VariableType, description?: string) =>
    api.post<IdResponse>('/labels', { workflowVersionId, key, type, description }),

  removeLabel: (labelId: string) =>
    api.delete<SuccessResponse>(`/labels/${labelId}`),
}

export const triggersApi = {
  createTrigger: (type: TriggerType, config?: Record<string, unknown>) =>
    api.post<IdResponse>('/triggers', { tenantId: TENANT_ID, type, config }),

  bindTriggerToWorkflow: (triggerId: string, workflowVersionId: string, priority: number, enabled: boolean) =>
    api.post<IdResponse>('/triggers/bind', { triggerId, workflowVersionId, priority, enabled }),

  unbindTrigger: (bindingId: string) =>
    api.delete<SuccessResponse>(`/triggers/bindings/${bindingId}`),

  bindTriggerToNode: (triggerBindingId: string, entryNodeId: string, order?: number) =>
    api.post<IdResponse>('/triggers/bindings/nodes', { triggerBindingId, entryNodeId, order }),

  unbindTriggerFromNode: (nodeBindingId: string) =>
    api.delete<SuccessResponse>(`/triggers/bindings/nodes/${nodeBindingId}`),
}

