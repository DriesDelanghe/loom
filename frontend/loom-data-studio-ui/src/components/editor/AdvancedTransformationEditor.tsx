import { useState, useCallback, useEffect, useRef } from 'react'
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

import { transformationSpecsApi } from '../../api/masterdata'
import { TransformNode, type TransformNodeData } from './TransformNode'
import type { TransformNodeType, ValidationResult } from '../../types'
import type { NodeTypes } from '@xyflow/react'

const nodeTypes = {
  transform: TransformNode,
} as NodeTypes

interface AdvancedTransformationEditorProps {
  transformationSpecId: string
  isReadOnly: boolean
}

// Layout persistence key
const getLayoutKey = (specId: string) => `transform-layout-${specId}`

export function AdvancedTransformationEditor({
  transformationSpecId,
  isReadOnly,
}: AdvancedTransformationEditorProps) {
  const queryClient = useQueryClient()
  const [nodes, setNodes, onNodesChange] = useNodesState<FlowNode<TransformNodeData>>([])
  const [edges, setEdges, onEdgesChange] = useEdgesState<FlowEdge>([])
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null)
  const [showAddNode, setShowAddNode] = useState(false)
  const [validation, setValidation] = useState<ValidationResult | null>(null)
  const [showOutputBindings, setShowOutputBindings] = useState(true)
  const saveLayoutDebounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const pendingLayoutSaves = useRef<Map<string, { x: number; y: number }>>(new Map())

  const { data: transformationSpec, isLoading } = useQuery({
    queryKey: ['transformationSpec', transformationSpecId],
    queryFn: () => transformationSpecsApi.getTransformationSpecDetails(transformationSpecId),
    enabled: !!transformationSpecId,
  })

  const { data: validationResult } = useQuery({
    queryKey: ['transformationSpecValidation', transformationSpecId],
    queryFn: () => transformationSpecsApi.validateTransformationSpec(transformationSpecId),
    enabled: !!transformationSpecId && !isReadOnly,
    refetchInterval: 30000, // Refetch every 30 seconds
  })

  // Load saved layout
  const loadLayout = useCallback(() => {
    try {
      const saved = localStorage.getItem(getLayoutKey(transformationSpecId))
      if (saved) {
        return JSON.parse(saved) as Record<string, { x: number; y: number }>
      }
    } catch {
      // Ignore errors
    }
    return {}
  }, [transformationSpecId])

  // Save layout to localStorage
  const saveLayout = useCallback((nodeKey: string, position: { x: number; y: number }) => {
    const layouts = loadLayout()
    layouts[nodeKey] = position
    try {
      localStorage.setItem(getLayoutKey(transformationSpecId), JSON.stringify(layouts))
    } catch {
      // Ignore errors
    }
  }, [transformationSpecId, loadLayout])

  // Convert backend data to React Flow nodes and edges
  useEffect(() => {
    if (!transformationSpec || isLoading) return

    const savedLayout = loadLayout()
    const existingNodePositions = new Map(nodes.map(n => [n.id, n.position]))
    const isInitialLoad = nodes.length === 0

    const flowNodes: FlowNode<TransformNodeData>[] = transformationSpec.graphNodes.map((node, index) => {
      const nodeKey = node.key
      const savedPosition = savedLayout[nodeKey]
      const existingPosition = existingNodePositions.get(node.id)
      
      let position: { x: number; y: number }
      if (isInitialLoad) {
        position = savedPosition || existingPosition || { x: 100 + index * 250, y: 150 }
      } else {
        position = existingPosition || savedPosition || { x: 100 + index * 250, y: 150 }
      }

      // Check for validation errors on this node
      const hasError = validationResult?.errors.some(e => e.field.includes(nodeKey)) || false

      return {
        id: node.id,
        type: 'transform' as const,
        position,
        data: {
          id: node.id,
          key: node.key,
          nodeType: node.nodeType,
          outputType: node.outputType,
          config: node.config,
        },
        style: hasError ? {
          border: '2px solid #ef4444',
          backgroundColor: '#fee2e2',
        } : undefined,
      } as FlowNode<TransformNodeData>
    })

    // Create a map of node types for quick lookup
    const nodeTypeMap = new Map(flowNodes.map(n => [n.id, n.data.nodeType]))

    const flowEdges: FlowEdge[] = transformationSpec.graphEdges.map((edge) => {
      const sourceNodeType = nodeTypeMap.get(edge.fromNodeId)
      const isSplitOrJoin = sourceNodeType === 'Split' || sourceNodeType === 'Join'
      const sourceHandle = isSplitOrJoin ? `output-${edge.order}` : 'output'
      
      // Check for validation errors on this edge
      const hasError = validationResult?.errors.some(e => e.field.includes(edge.id)) || false

      return {
        id: edge.id,
        source: edge.fromNodeId,
        target: edge.toNodeId,
        sourceHandle: sourceHandle || 'output',
        targetHandle: 'input',
        label: edge.inputName,
        type: 'default',
        style: {
          stroke: hasError ? '#ef4444' : '#10b981',
          strokeWidth: hasError ? 3 : 2,
        },
        markerEnd: {
          type: 'arrowclosed',
          color: hasError ? '#ef4444' : '#10b981',
        },
        animated: hasError,
      } as FlowEdge
    })

    // Add output binding visualizations
    const outputBindingEdges: FlowEdge[] = transformationSpec.outputBindings
      .map((binding) => {
        const targetNode = flowNodes.find(n => n.id === binding.fromNodeId)
        if (!targetNode) return null

        return {
          id: `binding-${binding.id}`,
          source: binding.fromNodeId,
          target: `output-${binding.id}`,
          sourceHandle: 'output',
          targetHandle: 'input',
          label: binding.targetPath,
          type: 'default',
          style: {
            stroke: '#6b7280',
            strokeWidth: 1,
            strokeDasharray: '5,5',
          },
          markerEnd: {
            type: 'arrowclosed',
            color: '#6b7280',
          },
        } as FlowEdge
      })
      .filter((e): e is FlowEdge => e !== null)

    // Add output nodes if bindings are shown
    const outputNodes: FlowNode<TransformNodeData>[] = showOutputBindings
      ? transformationSpec.outputBindings
          .map((binding) => {
            const sourceNode = flowNodes.find(n => n.id === binding.fromNodeId)
            if (!sourceNode) return null

            return {
              id: `output-${binding.id}`,
              type: 'transform' as const,
              position: { x: sourceNode.position.x + 300, y: sourceNode.position.y },
              data: {
                id: `output-${binding.id}`,
                key: binding.targetPath,
                nodeType: 'Constant' as TransformNodeType,
                outputType: '{}',
                config: '{}',
              },
              style: {
                border: '2px dashed #6b7280',
                backgroundColor: '#f9fafb',
              },
            } as FlowNode<TransformNodeData>
          })
          .filter((n): n is FlowNode<TransformNodeData> => n !== null)
      : []

    setNodes([...flowNodes, ...outputNodes])
    setEdges([...flowEdges, ...outputBindingEdges])
  }, [transformationSpec, isLoading, validationResult, showOutputBindings, nodes, setNodes, setEdges, loadLayout])

  // Update validation when it changes
  useEffect(() => {
    if (validationResult) {
      setValidation(validationResult)
    }
  }, [validationResult])

  const addNodeMutation = useMutation({
    mutationFn: (data: { key: string; nodeType: TransformNodeType; outputType: string; config: string }) =>
      transformationSpecsApi.addTransformGraphNode(
        transformationSpecId,
        data.key,
        data.nodeType,
        data.outputType,
        data.config
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transformationSpec', transformationSpecId] })
      setShowAddNode(false)
    },
  })

  const removeNodeMutation = useMutation({
    mutationFn: (nodeId: string) => transformationSpecsApi.removeTransformGraphNode(nodeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transformationSpec', transformationSpecId] })
      setSelectedNodeId(null)
    },
  })

  const addEdgeMutation = useMutation({
    mutationFn: (data: { fromNodeId: string; toNodeId: string; inputName: string; order: number }) =>
      transformationSpecsApi.addTransformGraphEdge(
        transformationSpecId,
        data.fromNodeId,
        data.toNodeId,
        data.inputName,
        data.order
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transformationSpec', transformationSpecId] })
    },
  })

  const removeEdgeMutation = useMutation({
    mutationFn: (edgeId: string) => transformationSpecsApi.removeTransformGraphEdge(edgeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transformationSpec', transformationSpecId] })
    },
  })

  const onNodeDrag = useCallback((_: React.MouseEvent, node: FlowNode<TransformNodeData>) => {
    if (isReadOnly) return
    
    const nodeKey = node.data.key
    pendingLayoutSaves.current.set(nodeKey, { x: node.position.x, y: node.position.y })

    if (saveLayoutDebounceRef.current) {
      clearTimeout(saveLayoutDebounceRef.current)
    }

    saveLayoutDebounceRef.current = setTimeout(() => {
      const layouts = Array.from(pendingLayoutSaves.current.entries())
      layouts.forEach(([key, pos]) => {
        saveLayout(key, pos)
      })
      pendingLayoutSaves.current.clear()
    }, 500)
  }, [isReadOnly, saveLayout])

  const onNodeDragStop = useCallback((_: React.MouseEvent, node: FlowNode<TransformNodeData>) => {
    if (isReadOnly) return
    
    if (saveLayoutDebounceRef.current) {
      clearTimeout(saveLayoutDebounceRef.current)
      saveLayoutDebounceRef.current = null
    }

    const nodeKey = node.data.key
    saveLayout(nodeKey, { x: node.position.x, y: node.position.y })
    pendingLayoutSaves.current.delete(nodeKey)
  }, [isReadOnly, saveLayout])

  const onConnect: OnConnect = useCallback(
    (params: Connection) => {
      if (isReadOnly || !params.source || !params.target || !params.sourceHandle) return

      // Determine input name from handle
      const inputName = params.sourceHandle === 'output' ? 'input' : params.sourceHandle
      const order = params.sourceHandle.startsWith('output-') 
        ? parseInt(params.sourceHandle.split('-')[1]) || 0 
        : 0

      addEdgeMutation.mutate({
        fromNodeId: params.source,
        toNodeId: params.target,
        inputName,
        order,
      })
    },
    [isReadOnly, addEdgeMutation]
  )

  const onNodeClick = useCallback((_: React.MouseEvent, node: FlowNode<TransformNodeData>) => {
    // Don't select output binding nodes
    if (node.id.startsWith('output-')) return
    setSelectedNodeId(node.id)
  }, [])

  const onEdgeClick = useCallback(
    (_: React.MouseEvent, edge: FlowEdge) => {
      if (isReadOnly) return
      // Don't allow removing output binding edges
      if (edge.id.startsWith('binding-')) return
      if (confirm('Remove this connection?')) {
        removeEdgeMutation.mutate(edge.id)
      }
    },
    [isReadOnly, removeEdgeMutation]
  )

  const handleDeleteNode = useCallback(() => {
    if (!selectedNodeId) return
    if (confirm('Delete this node? This will also remove all connected edges.')) {
      removeNodeMutation.mutate(selectedNodeId)
    }
  }, [selectedNodeId, removeNodeMutation])

  if (isLoading) {
    return (
      <div className="h-full flex items-center justify-center">
        <div className="text-gray-500">Loading transformation graph...</div>
      </div>
    )
  }

  return (
    <div className="h-full relative">
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
        deleteKeyCode={isReadOnly ? null : 'Delete'}
      >
        <Background />
        <Controls />
        <MiniMap />

        <Panel position="top-right" className="flex gap-2">
          {!isReadOnly && (
            <>
              <button
                onClick={() => setShowAddNode(true)}
                className="bg-white px-3 py-2 rounded-lg shadow border border-gray-200 text-sm font-medium text-gray-700 hover:bg-gray-50"
              >
                + Add Node
              </button>
              <button
                onClick={() => setShowOutputBindings(!showOutputBindings)}
                className={`px-3 py-2 rounded-lg shadow border text-sm font-medium ${
                  showOutputBindings
                    ? 'bg-loom-600 text-white border-loom-600'
                    : 'bg-white text-gray-700 border-gray-200 hover:bg-gray-50'
                }`}
              >
                {showOutputBindings ? 'Hide' : 'Show'} Outputs
              </button>
            </>
          )}
        </Panel>
      </ReactFlow>

      {validation && !validation.isValid && (
        <div className="absolute bottom-4 right-4 max-w-md">
          <div className="bg-red-50 border border-red-200 rounded-lg shadow-lg p-4">
            <div className="flex items-center justify-between mb-2">
              <span className="font-medium text-red-800">✕ Validation Errors</span>
              <button onClick={() => setValidation(null)} className="text-gray-400 hover:text-gray-600">
                ✕
              </button>
            </div>
            <ul className="text-sm text-red-700 list-disc list-inside space-y-1">
              {validation.errors.map((err, i) => (
                <li key={i}>
                  <span className="font-medium">{err.field}:</span> {err.message}
                </li>
              ))}
            </ul>
          </div>
        </div>
      )}

      {showAddNode && !isReadOnly && (
        <AddNodeDialog
          onAdd={(nodeData) => addNodeMutation.mutate(nodeData)}
          onClose={() => setShowAddNode(false)}
          isPending={addNodeMutation.isPending}
        />
      )}

      {selectedNodeId && (
        <NodeConfigPanel
          node={nodes.find((n) => n.id === selectedNodeId)}
          onClose={() => setSelectedNodeId(null)}
          onDelete={handleDeleteNode}
          isReadOnly={isReadOnly}
          outputBindings={transformationSpec?.outputBindings.filter(b => b.fromNodeId === selectedNodeId) || []}
        />
      )}
    </div>
  )
}

interface AddNodeDialogProps {
  onAdd: (data: { key: string; nodeType: TransformNodeType; outputType: string; config: string }) => void
  onClose: () => void
  isPending: boolean
}

function AddNodeDialog({ onAdd, onClose, isPending }: AddNodeDialogProps) {
  const [key, setKey] = useState('')
  const [nodeType, setNodeType] = useState<TransformNodeType>('Map')
  const [outputType, setOutputType] = useState('{}')
  const [config, setConfig] = useState('{}')

  const handleSubmit = () => {
    if (!key.trim()) return
    onAdd({ key, nodeType, outputType, config })
  }

  return (
    <div className="absolute inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 max-w-md w-full shadow-xl">
        <h3 className="text-lg font-semibold mb-4">Add Transformation Node</h3>
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Node Key
            </label>
            <input
              type="text"
              value={key}
              onChange={(e) => setKey(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-loom-500"
              placeholder="e.g., map-customer-name"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Node Type
            </label>
            <select
              value={nodeType}
              onChange={(e) => setNodeType(e.target.value as TransformNodeType)}
              className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-loom-500"
            >
              <option value="Source">Source</option>
              <option value="Map">Map</option>
              <option value="Filter">Filter</option>
              <option value="Aggregate">Aggregate</option>
              <option value="Join">Join</option>
              <option value="Split">Split</option>
              <option value="Constant">Constant</option>
              <option value="Expression">Expression</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Output Type (JSON)
            </label>
            <textarea
              value={outputType}
              onChange={(e) => setOutputType(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-loom-500 font-mono text-sm"
              rows={3}
              placeholder='{"type": "object", ...}'
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Config (JSON)
            </label>
            <textarea
              value={config}
              onChange={(e) => setConfig(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-loom-500 font-mono text-sm"
              rows={3}
              placeholder='{"field": "value", ...}'
            />
          </div>
          <div className="flex gap-2">
            <button
              onClick={handleSubmit}
              disabled={!key.trim() || isPending}
              className="flex-1 px-4 py-2 bg-loom-600 text-white rounded-lg hover:bg-loom-700 disabled:opacity-50"
            >
              {isPending ? 'Adding...' : 'Add Node'}
            </button>
            <button
              onClick={onClose}
              className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
            >
              Cancel
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

interface NodeConfigPanelProps {
  node: FlowNode<TransformNodeData> | undefined
  onClose: () => void
  onDelete: () => void
  isReadOnly: boolean
  outputBindings: Array<{ id: string; targetPath: string; fromNodeId: string }>
}

function NodeConfigPanel({ node, onClose, onDelete, isReadOnly, outputBindings }: NodeConfigPanelProps) {
  if (!node) return null

  return (
    <div className="absolute right-0 top-0 bottom-0 w-80 bg-white border-l border-gray-200 shadow-lg z-40 overflow-y-auto">
      <div className="p-4 border-b border-gray-200 flex items-center justify-between">
        <h3 className="font-semibold text-gray-900">Node Configuration</h3>
        <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
          ✕
        </button>
      </div>
      <div className="p-4 space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Key</label>
          <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm">
            {node.data.key}
          </div>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
          <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm">
            {node.data.nodeType}
          </div>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Output Type</label>
          <pre className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-xs overflow-x-auto">
            {node.data.outputType}
          </pre>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Config</label>
          <pre className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-xs overflow-x-auto">
            {node.data.config}
          </pre>
        </div>
        {outputBindings.length > 0 && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Output Bindings</label>
            <div className="space-y-1">
              {outputBindings.map((binding) => (
                <div key={binding.id} className="px-3 py-2 bg-blue-50 border border-blue-200 rounded text-xs">
                  <span className="font-medium text-blue-900">{binding.targetPath}</span>
                </div>
              ))}
            </div>
          </div>
        )}
        {!isReadOnly && (
          <button
            onClick={onDelete}
            className="w-full px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
          >
            Delete Node
          </button>
        )}
      </div>
    </div>
  )
}
