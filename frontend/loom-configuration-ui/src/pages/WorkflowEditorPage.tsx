import { useState, useCallback, useMemo, useEffect, useRef } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  ReactFlow,
  Background,
  Controls,
  MiniMap,
  useNodesState,
  useEdgesState,
  type Node as FlowNode,
  type Edge as FlowEdge,
  type Connection,
  type OnConnect,
  Panel,
} from '@xyflow/react'
import '@xyflow/react/dist/style.css'

import { workflowsApi, nodesApi, connectionsApi, triggersApi } from '../api/workflows'
import { layoutApi } from '../api/layout'
import { WorkflowNode, type WorkflowNodeData } from '../components/editor/WorkflowNode'
import { TriggerNode, type TriggerNodeData } from '../components/editor/TriggerNode'
import { NodeConfigPanel } from '../components/editor/NodeConfigPanel'
import { TriggersPanel } from '../components/editor/TriggersPanel'
import { VariablesPanel } from '../components/editor/VariablesPanel'
import { LabelsPanel } from '../components/editor/LabelsPanel'
import type { Node, Connection as WfConnection, ValidationResult } from '../types'
import { getOutcomeFromHandleId } from '../utils/nodeMetadata'
import type { NodeTypes } from '@xyflow/react'

const nodeTypes = {
  workflow: WorkflowNode,
  trigger: TriggerNode,
} as NodeTypes

type PanelType = 'node' | 'triggers' | 'variables' | 'labels' | null

export function WorkflowEditorPage() {
  const { workflowId, versionId } = useParams<{ workflowId: string; versionId: string }>()
  const queryClient = useQueryClient()
  
  const [nodes, setNodes, onNodesChange] = useNodesState<FlowNode<WorkflowNodeData | TriggerNodeData>>([])
  const [edges, setEdges, onEdgesChange] = useEdgesState<FlowEdge>([])
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null)
  const [activePanel, setActivePanel] = useState<PanelType>(null)
  const [validation, setValidation] = useState<ValidationResult | null>(null)

  const { data: versionDetails, isLoading } = useQuery({
    queryKey: ['versionDetails', versionId],
    queryFn: () => workflowsApi.getVersionDetails(versionId!),
    enabled: !!versionId,
  })

  const { data: layoutData, isLoading: isLoadingLayout } = useQuery({
    queryKey: ['layout', versionId],
    queryFn: () => layoutApi.getLayout(versionId!),
    enabled: !!versionId,
  })

  const isReadOnly = useMemo(() => versionDetails?.version.status !== 'Draft', [versionDetails?.version.status])

  const entryNodeIds = useMemo(() => {
    const ids = new Set<string>()
    versionDetails?.triggerBindings.forEach(tb => {
      tb.nodeBindings.forEach(nb => ids.add(nb.entryNodeId))
    })
    return ids
  }, [versionDetails?.triggerBindings])

  useEffect(() => {
    // Wait for both versionDetails and layoutData to be loaded (or confirmed as not loading)
    if (!versionDetails || isLoadingLayout) return

    const existingNodePositions = new Map(nodes.map(n => [n.id, n.position]))
    const layoutPositions = new Map(
      layoutData?.nodes.map(n => [n.nodeKey, { x: n.x, y: n.y }]) || []
    )

    const isInitialLoad = nodes.length === 0

    // Create trigger nodes
    const triggerNodes: FlowNode<TriggerNodeData>[] = versionDetails.triggerBindings.map((tb, index: number) => {
      const triggerNodeId = `trigger-${tb.id}`
      const triggerNodeKey = `trigger-${tb.id}` // Use same format for layout key
      const existingPosition = existingNodePositions.get(triggerNodeId)
      const layoutPosition = layoutPositions.get(triggerNodeKey)
      
      // Prioritize layout position on initial load, otherwise use existing or default
      let position: { x: number; y: number }
      if (isInitialLoad) {
        position = layoutPosition || { x: 50, y: 50 + index * 120 }
      } else {
        position = existingPosition || layoutPosition || { x: 50, y: 50 + index * 120 }
      }
      
      return {
        id: triggerNodeId,
        type: 'trigger',
        position,
        data: {
          id: triggerNodeId,
          triggerBindingId: tb.id,
          triggerId: tb.triggerId,
          type: tb.triggerType as any,
          enabled: tb.enabled,
          priority: tb.priority,
          config: tb.triggerConfig,
          nodeKey: triggerNodeKey, // Add nodeKey for layout persistence
        },
      }
    })

    // Determine end nodes (nodes with no outgoing connections)
    const nodesWithOutgoing = new Set(versionDetails.connections.map((c: WfConnection) => c.fromNodeId))

    // Create workflow nodes
    const workflowNodes: FlowNode<WorkflowNodeData>[] = versionDetails.nodes.map((node: Node, index: number) => {
      const existingPosition = existingNodePositions.get(node.id)
      const layoutPosition = layoutPositions.get(node.key)
      
      // On initial load, prioritize layout positions
      // Otherwise, preserve existing positions (user has dragged nodes)
      // Default positions are horizontal (left to right layout)
      let position: { x: number; y: number }
      if (isInitialLoad) {
        position = layoutPosition || { x: 100 + index * 250, y: 150 }
      } else {
        position = existingPosition || layoutPosition || { x: 100 + index * 250, y: 150 }
      }
      
      return {
        id: node.id,
        type: 'workflow',
        position,
        data: {
          id: node.id,
          workflowVersionId: node.workflowVersionId,
          key: node.key,
          name: node.name,
          type: node.type,
          config: node.config,
          createdAt: node.createdAt,
          isEndNode: !nodesWithOutgoing.has(node.id),
        },
      }
    })

    // Combine all nodes
    const flowNodes = [...triggerNodes, ...workflowNodes]

    // Create edges from trigger nodes to entry nodes
    const triggerEdges: FlowEdge[] = versionDetails.triggerBindings.flatMap(tb => 
      tb.nodeBindings.map(nb => ({
        id: `trigger-edge-${tb.id}-${nb.id}`,
        source: `trigger-${tb.id}`,
        target: nb.entryNodeId,
        sourceHandle: 'source-Success',
        targetHandle: 'target', // WorkflowNode uses 'target' as the handle ID
        label: 'Start',
        type: 'default',
        className: 'trigger-edge',
        style: {
          stroke: '#8b5cf6',
          strokeWidth: 2,
        },
        markerEnd: {
          type: 'arrowclosed',
          color: '#8b5cf6',
        },
        data: {
          triggerBindingId: tb.id,
          nodeBindingId: nb.id,
        },
      }))
    )

    // Create edges from workflow connections
    const workflowEdges: FlowEdge[] = versionDetails.connections.map((conn: WfConnection) => {
      const sourceHandle = `source-${conn.outcome}`
      const isPositive = ['Completed', 'True', 'Valid', 'Next', 'Joined'].includes(conn.outcome)
      
      return {
      id: conn.id,
      source: conn.fromNodeId,
      target: conn.toNodeId,
        sourceHandle,
        targetHandle: 'target',
        label: conn.outcome,
        type: 'default',
        className: isPositive ? 'success' : 'failure',
        animated: !isPositive,
        style: {
          stroke: isPositive ? '#10b981' : '#ef4444',
        },
        data: {
          outcome: conn.outcome,
          connectionId: conn.id,
        },
        markerEnd: {
          type: 'arrowclosed',
          color: isPositive ? '#10b981' : '#ef4444',
        },
      }
    })

    // Combine all edges
    const flowEdges = [...triggerEdges, ...workflowEdges]

    const newNodeIds = new Set(flowNodes.map(n => n.id))
    const nodesChanged = flowNodes.length !== nodes.length || 
      flowNodes.some(n => {
        const existing = nodes.find(en => en.id === n.id)
        if (!existing) return true
        
        // Check if data changed
        if (existing.data.name !== n.data.name || 
          existing.data.type !== n.data.type || 
          existing.data.isEntry !== n.data.isEntry ||
            existing.data.config !== n.data.config) {
          return true
        }
        
        // On initial load, check position changes
        if (isInitialLoad) {
          if (existing.position.x !== n.position.x || existing.position.y !== n.position.y) {
            return true
          }
        }
        
        return false
      }) ||
      nodes.some(n => !newNodeIds.has(n.id))

    // Check if edges need updating
    const newEdgeIds = new Set(flowEdges.map(e => e.id))
    const edgesChanged = flowEdges.length !== edges.length ||
      flowEdges.some(e => {
        const existing = edges.find(ee => ee.id === e.id)
        return !existing || 
          existing.source !== e.source ||
          existing.target !== e.target ||
          existing.label !== e.label
      }) ||
      edges.some(e => !newEdgeIds.has(e.id))

    if (nodesChanged) {
      setNodes(flowNodes)
    }

    // Always update edges when versionDetails changes to ensure all connections are shown
    // This is necessary because React Flow may deduplicate edges with same source/target
    if (edgesChanged) {
      setEdges(flowEdges)
    }
      }, [versionDetails, entryNodeIds, layoutData, isLoadingLayout, nodes, edges, setNodes, setEdges, nodes.length])

  const addNodeMutation = useMutation({
    mutationFn: (data: { key: string; name: string; type: Node['type'] }) =>
      nodesApi.addNode(versionId!, data.key, data.name, data.type),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionId] })
    },
  })

  const removeNodeMutation = useMutation({
    mutationFn: (nodeId: string) => nodesApi.removeNode(nodeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionId] })
      setSelectedNodeId(null)
    },
  })

  const addConnectionMutation = useMutation({
    mutationFn: (data: { fromNodeId: string; toNodeId: string; outcome: string }) =>
      connectionsApi.addConnection(versionId!, data.fromNodeId, data.toNodeId, data.outcome),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionId] })
    },
  })

  const removeConnectionMutation = useMutation({
    mutationFn: (connectionId: string) => connectionsApi.removeConnection(connectionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionId] })
    },
  })

  const publishMutation = useMutation({
    mutationFn: () => workflowsApi.publishVersion(versionId!, 'user@example.com'),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionId] })
      queryClient.invalidateQueries({ queryKey: ['versions', workflowId] })
    },
  })

  const validateMutation = useMutation({
    mutationFn: () => workflowsApi.validateVersion(versionId!),
    onSuccess: (result) => {
      setValidation(result)
    },
  })

  const saveLayoutDebounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const pendingLayoutSaves = useRef<Map<string, { x: number; y: number }>>(new Map())

  const saveLayoutMutation = useMutation({
    mutationFn: async (data: { nodeKey: string; x: number; y: number }) => {
      return layoutApi.upsertNodeLayout(versionId!, data.nodeKey, data.x, data.y)
    },
  })

  const saveLayoutBatchMutation = useMutation({
    mutationFn: async (layouts: Array<{ nodeKey: string; x: number; y: number }>) => {
      return layoutApi.upsertNodeLayoutsBatch(
        versionId!,
        layouts.map(l => ({ nodeKey: l.nodeKey, x: l.x, y: l.y }))
      )
    },
  })

  const onNodeDrag = useCallback((_: React.MouseEvent, node: FlowNode<WorkflowNodeData | TriggerNodeData>) => {
    if (isReadOnly) return
    
    let nodeKey: string | undefined
    
    if (node.type === 'trigger') {
      // For trigger nodes, use the nodeKey from data
      nodeKey = (node.data as TriggerNodeData).nodeKey
    } else {
      // For workflow nodes, find the node data
      const nodeData = versionDetails?.nodes.find((n: Node) => n.id === node.id)
      nodeKey = nodeData?.key
    }
    
    if (!nodeKey) return

    pendingLayoutSaves.current.set(nodeKey, { x: node.position.x, y: node.position.y })

    if (saveLayoutDebounceRef.current) {
      clearTimeout(saveLayoutDebounceRef.current)
    }

    saveLayoutDebounceRef.current = setTimeout(() => {
      const layouts = Array.from(pendingLayoutSaves.current.entries()).map(([key, pos]) => ({
        nodeKey: key,
        x: pos.x,
        y: pos.y,
      }))
      
      if (layouts.length > 0) {
        saveLayoutBatchMutation.mutate(layouts)
        pendingLayoutSaves.current.clear()
      }
    }, 500)
  }, [isReadOnly, versionDetails, versionId, saveLayoutBatchMutation])

  const onNodeDragStop = useCallback((_: React.MouseEvent, node: FlowNode<WorkflowNodeData | TriggerNodeData>) => {
    if (isReadOnly) return
    
    let nodeKey: string | undefined
    
    if (node.type === 'trigger') {
      // For trigger nodes, use the nodeKey from data
      nodeKey = (node.data as TriggerNodeData).nodeKey
    } else {
      // For workflow nodes, find the node data
      const nodeData = versionDetails?.nodes.find((n: Node) => n.id === node.id)
      nodeKey = nodeData?.key
    }
    
    if (!nodeKey) return
    
    if (saveLayoutDebounceRef.current) {
      clearTimeout(saveLayoutDebounceRef.current)
      saveLayoutDebounceRef.current = null
    }

    const layouts = Array.from(pendingLayoutSaves.current.entries()).map(([key, pos]) => ({
      nodeKey: key,
      x: pos.x,
      y: pos.y,
    }))
    
    if (layouts.length > 0) {
      saveLayoutBatchMutation.mutate(layouts)
      pendingLayoutSaves.current.clear()
    } else {
      saveLayoutMutation.mutate({ nodeKey, x: node.position.x, y: node.position.y })
    }
  }, [isReadOnly, versionDetails, versionId, saveLayoutMutation, saveLayoutBatchMutation])

  const bindTriggerToNodeMutation = useMutation({
    mutationFn: (data: { triggerBindingId: string; entryNodeId: string }) =>
      triggersApi.bindTriggerToNode(data.triggerBindingId, data.entryNodeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionId] })
    },
  })

  const onConnect: OnConnect = useCallback(
    (params: Connection) => {
      if (isReadOnly || !params.source || !params.target || !params.sourceHandle) return
      
      // Handle connections from trigger nodes
      if (params.source.startsWith('trigger-')) {
        const triggerBindingId = params.source.replace('trigger-', '')
        bindTriggerToNodeMutation.mutate({
          triggerBindingId,
          entryNodeId: params.target,
        })
        return
      }
      
      // Regular workflow node connections - determine outcome from handle position
      const sourceNode = versionDetails?.nodes.find((n: Node) => n.id === params.source)
      if (!sourceNode) return

      const outcome = getOutcomeFromHandleId(params.sourceHandle, sourceNode.type)
      
      // Check for duplicate connections with same outcome
      const existingConnection = versionDetails?.connections.find(
        (c: WfConnection) => c.fromNodeId === params.source && 
                           c.toNodeId === params.target &&
                           c.outcome === outcome
      )
      
      if (existingConnection) {
        alert(`A connection with outcome '${outcome}' already exists between these nodes`)
        return
      }
      
      addConnectionMutation.mutate({ 
        fromNodeId: params.source, 
        toNodeId: params.target,
        outcome
      })
    },
    [isReadOnly, addConnectionMutation, versionDetails, bindTriggerToNodeMutation]
  )

  const onNodeClick = useCallback((_: React.MouseEvent, node: FlowNode<WorkflowNodeData | TriggerNodeData>) => {
    setSelectedNodeId(node.id)
    if (node.type === 'trigger') {
      setActivePanel('triggers')
    } else {
    setActivePanel('node')
    }
  }, [])

  const unbindTriggerFromNodeMutation = useMutation({
    mutationFn: (nodeBindingId: string) => triggersApi.unbindTriggerFromNode(nodeBindingId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionId] })
    },
  })

  const onEdgeClick = useCallback(
    (_: React.MouseEvent, edge: FlowEdge) => {
      if (isReadOnly) return
      if (confirm('Remove this connection?')) {
        // Check if this is a trigger edge
        if (edge.id.startsWith('trigger-edge-')) {
          const nodeBindingId = edge.data?.nodeBindingId as string | undefined
          if (nodeBindingId) {
            unbindTriggerFromNodeMutation.mutate(nodeBindingId)
          }
        } else {
        removeConnectionMutation.mutate(edge.id)
        }
      }
    },
    [isReadOnly, removeConnectionMutation, unbindTriggerFromNodeMutation]
  )

  const handleAddNode = () => {
    if (isReadOnly) return
    const key = `node-${Date.now()}`
    addNodeMutation.mutate({ key, name: 'New Node', type: 'Action' })
  }

  const selectedNode = versionDetails?.nodes.find((n: Node) => n.id === selectedNodeId)

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-loom-600" />
      </div>
    )
  }

  return (
    <div className="h-[calc(100vh-64px)] flex">
      <div className="flex-1 relative">
        <ReactFlow
          nodes={nodes}
          edges={edges}
          onNodesChange={onNodesChange}
          onEdgesChange={onEdgesChange}
          onConnect={onConnect}
          onNodeClick={onNodeClick}
          onEdgeClick={onEdgeClick}
          onNodeDrag={onNodeDrag}
          onNodeDragStop={onNodeDragStop}
          nodeTypes={nodeTypes}
          fitView
          nodesDraggable={!isReadOnly}
          nodesConnectable={!isReadOnly}
          elementsSelectable={true}
          deleteKeyCode={null}
          multiSelectionKeyCode={null}
          
        >
          <Background />
          <Controls />
          <MiniMap />
          
          <Panel position="top-left" className="flex flex-col gap-2">
            <Link
              to={`/workflows/${workflowId}`}
              className="bg-white px-3 py-2 rounded-lg shadow border border-gray-200 text-sm font-medium text-gray-700 hover:bg-gray-50 flex items-center gap-2"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
              </svg>
              Back
            </Link>
            
            <div className="bg-white px-3 py-2 rounded-lg shadow border border-gray-200">
              <span className={`text-xs font-medium px-2 py-1 rounded ${
                versionDetails?.version.status === 'Draft' ? 'bg-yellow-100 text-yellow-800' :
                versionDetails?.version.status === 'Published' ? 'bg-green-100 text-green-800' :
                'bg-gray-100 text-gray-600'
              }`}>
                {versionDetails?.version.status}
              </span>
              <span className="ml-2 text-sm text-gray-600">
                v{versionDetails?.version.version}
              </span>
            </div>
          </Panel>

          <Panel position="top-right" className="flex gap-2">
            {!isReadOnly && (
              <>
                <button
                  onClick={handleAddNode}
                  className="bg-white px-3 py-2 rounded-lg shadow border border-gray-200 text-sm font-medium text-gray-700 hover:bg-gray-50"
                >
                  + Add Node
                </button>
                <button
                  onClick={() => validateMutation.mutate()}
                  disabled={validateMutation.isPending}
                  className="bg-white px-3 py-2 rounded-lg shadow border border-gray-200 text-sm font-medium text-gray-700 hover:bg-gray-50"
                >
                  Validate
                </button>
                <button
                  onClick={() => publishMutation.mutate()}
                  disabled={publishMutation.isPending}
                  className="bg-loom-600 px-3 py-2 rounded-lg shadow text-sm font-medium text-white hover:bg-loom-700"
                >
                  Publish
                </button>
              </>
            )}
          </Panel>

          <Panel position="bottom-left" className="flex gap-2">
            <button
              onClick={() => setActivePanel(activePanel === 'triggers' ? null : 'triggers')}
              className={`px-3 py-2 rounded-lg shadow border text-sm font-medium ${
                activePanel === 'triggers' ? 'bg-loom-600 text-white border-loom-600' : 'bg-white text-gray-700 border-gray-200 hover:bg-gray-50'
              }`}
            >
              Triggers
            </button>
            <button
              onClick={() => setActivePanel(activePanel === 'variables' ? null : 'variables')}
              className={`px-3 py-2 rounded-lg shadow border text-sm font-medium ${
                activePanel === 'variables' ? 'bg-loom-600 text-white border-loom-600' : 'bg-white text-gray-700 border-gray-200 hover:bg-gray-50'
              }`}
            >
              Variables
            </button>
            <button
              onClick={() => setActivePanel(activePanel === 'labels' ? null : 'labels')}
              className={`px-3 py-2 rounded-lg shadow border text-sm font-medium ${
                activePanel === 'labels' ? 'bg-loom-600 text-white border-loom-600' : 'bg-white text-gray-700 border-gray-200 hover:bg-gray-50'
              }`}
            >
              Labels
            </button>
          </Panel>
        </ReactFlow>

        {validation && (
          <div className="absolute bottom-4 right-4 max-w-md">
            <div className={`rounded-lg shadow-lg p-4 ${validation.isValid ? 'bg-green-50 border border-green-200' : 'bg-red-50 border border-red-200'}`}>
              <div className="flex items-center justify-between mb-2">
                <span className={`font-medium ${validation.isValid ? 'text-green-800' : 'text-red-800'}`}>
                  {validation.isValid ? '✓ Validation passed' : '✕ Validation failed'}
                </span>
                <button onClick={() => setValidation(null)} className="text-gray-400 hover:text-gray-600">
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
              {validation.errors.length > 0 && (
                <ul className="text-sm text-red-700 list-disc list-inside">
                  {validation.errors.map((err, i) => <li key={i}>{err}</li>)}
                </ul>
              )}
              {validation.warnings.length > 0 && (
                <ul className="text-sm text-yellow-700 list-disc list-inside mt-2">
                  {validation.warnings.map((warn, i) => <li key={i}>{warn}</li>)}
                </ul>
              )}
            </div>
          </div>
        )}
      </div>

      {activePanel === 'node' && selectedNode && (
        <NodeConfigPanel
          node={selectedNode}
          isReadOnly={isReadOnly}
          onClose={() => { setActivePanel(null); setSelectedNodeId(null) }}
          onDelete={() => removeNodeMutation.mutate(selectedNode.id)}
          versionId={versionId!}
        />
      )}

      {activePanel === 'triggers' && versionDetails && (
        <TriggersPanel
          versionDetails={versionDetails}
          isReadOnly={isReadOnly}
          onClose={() => setActivePanel(null)}
        />
      )}

      {activePanel === 'variables' && versionDetails && (
        <VariablesPanel
          variables={versionDetails.variables}
          versionId={versionId!}
          isReadOnly={isReadOnly}
          onClose={() => setActivePanel(null)}
        />
      )}

      {activePanel === 'labels' && versionDetails && (
        <LabelsPanel
          labels={versionDetails.labels}
          versionId={versionId!}
          isReadOnly={isReadOnly}
          onClose={() => setActivePanel(null)}
        />
      )}
    </div>
  )
}

