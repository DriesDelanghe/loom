import { api } from './client'

interface NodeLayout {
  nodeKey: string
  x: number
  y: number
  width?: number
  height?: number
}

interface WorkflowVersionLayoutResponse {
  nodes: NodeLayout[]
}

interface SuccessResponse {
  success: boolean
}

export const layoutApi = {
  getLayout: (workflowVersionId: string) =>
    api.get<WorkflowVersionLayoutResponse>(
      `/workflow-versions/${workflowVersionId}/layout`
    ),

  upsertNodeLayout: (
    workflowVersionId: string,
    nodeKey: string,
    x: number,
    y: number,
    width?: number,
    height?: number
  ) =>
    api.put<SuccessResponse>(
      `/workflow-versions/${workflowVersionId}/layout/nodes/${nodeKey}`,
      { x, y, width, height }
    ),

  upsertNodeLayoutsBatch: (workflowVersionId: string, nodes: NodeLayout[]) =>
    api.put<SuccessResponse>(
      `/workflow-versions/${workflowVersionId}/layout/nodes`,
      { nodes }
    ),

  copyLayoutFromVersion: (targetVersionId: string, sourceVersionId: string) =>
    api.post<SuccessResponse>(
      `/workflow-versions/${targetVersionId}/layout/copy-from/${sourceVersionId}`
    ),

  deleteNodeLayout: (workflowVersionId: string, nodeKey: string) =>
    api.delete<SuccessResponse>(
      `/workflow-versions/${workflowVersionId}/layout/nodes/${nodeKey}`
    ),
}

export type { NodeLayout, WorkflowVersionLayoutResponse }

