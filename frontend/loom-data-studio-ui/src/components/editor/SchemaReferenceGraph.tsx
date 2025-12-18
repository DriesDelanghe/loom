import { useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import {
  ReactFlow,
  Background,
  Controls,
  MiniMap,
  useNodesState,
  useEdgesState,
  type Node as FlowNode,
  type Edge as FlowEdge,
} from '@xyflow/react'
import '@xyflow/react/dist/style.css'

import { schemasApi } from '../../api/masterdata'
import type { SchemaGraphNode, SchemaRole } from '../../types'

interface SchemaReferenceGraphProps {
  schemaId: string
  role: SchemaRole
}

interface SchemaNodeData extends Record<string, unknown> {
  schemaId: string
  key: string
  version: number
  role: SchemaRole
  status: string
}

const nodeColors: Record<SchemaRole, string> = {
  Incoming: 'border-blue-400 bg-blue-50',
  Master: 'border-green-400 bg-green-50',
  Outgoing: 'border-purple-400 bg-purple-50',
}

export function SchemaReferenceGraph({ schemaId }: SchemaReferenceGraphProps) {
  const [nodes, setNodes, onNodesChange] = useNodesState<FlowNode<SchemaNodeData>>([])
  const [edges, setEdges, onEdgesChange] = useEdgesState<FlowEdge>([])

  const { data: schemaGraph, isLoading } = useQuery({
    queryKey: ['schemaGraph', schemaId],
    queryFn: () => schemasApi.getSchemaGraph(schemaId),
    enabled: !!schemaId,
  })

  useEffect(() => {
    if (!schemaGraph || isLoading) return

    // Create nodes from graph
    const flowNodes: FlowNode<SchemaNodeData>[] = schemaGraph.nodes.map((node, index) => {
      // Layout nodes in a hierarchical tree structure
      const level = node.role === 'Incoming' ? 0 : node.role === 'Master' ? 1 : 2
      const x = 100 + level * 300
      const y = 100 + index * 150

      return {
        id: node.schemaId,
        type: 'default',
        position: { x, y },
        data: {
          schemaId: node.schemaId,
          key: node.key,
          version: node.version,
          role: node.role,
          status: node.status,
        },
        style: {
          background: node.role === 'Incoming' ? '#dbeafe' : node.role === 'Master' ? '#d1fae5' : '#e9d5ff',
          border: node.role === 'Incoming' ? '2px solid #60a5fa' : node.role === 'Master' ? '2px solid #34d399' : '2px solid #a78bfa',
          borderRadius: '8px',
          padding: '12px',
          minWidth: '180px',
        },
      }
    })

    // Create edges from graph
    const flowEdges: FlowEdge[] = schemaGraph.edges.map((edge, index) => ({
      id: `edge-${edge.fromSchemaId}-${edge.toSchemaId}-${index}`,
      source: edge.fromSchemaId,
      target: edge.toSchemaId,
      label: edge.fieldPath,
      type: 'default',
      style: {
        stroke: '#6b7280',
        strokeWidth: 2,
      },
      markerEnd: {
        type: 'arrowclosed',
        color: '#6b7280',
      },
      animated: false,
    }))

    setNodes(flowNodes)
    setEdges(flowEdges)
  }, [schemaGraph, isLoading, setNodes, setEdges])

  if (isLoading) {
    return (
      <div className="h-full flex items-center justify-center">
        <div className="text-gray-500">Loading schema graph...</div>
      </div>
    )
  }

  if (!schemaGraph || schemaGraph.nodes.length === 0) {
    return (
      <div className="h-full flex items-center justify-center">
        <div className="text-center text-gray-500">
          <p className="mb-2">No schema references found</p>
          <p className="text-sm">This schema doesn't reference or isn't referenced by other schemas.</p>
        </div>
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
        fitView
        nodesDraggable={true}
        nodesConnectable={false}
        elementsSelectable={true}
        deleteKeyCode={null}
      >
        <Background />
        <Controls />
        <MiniMap />

        {/* Legend */}
        <div className="absolute top-4 left-4 bg-white rounded-lg shadow-lg border border-gray-200 p-4 z-10">
          <h4 className="text-sm font-semibold text-gray-900 mb-2">Schema Roles</h4>
          <div className="space-y-1 text-xs">
            <div className="flex items-center gap-2">
              <div className="w-4 h-4 bg-blue-50 border-2 border-blue-400 rounded"></div>
              <span>Incoming</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-4 h-4 bg-green-50 border-2 border-green-400 rounded"></div>
              <span>Master</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-4 h-4 bg-purple-50 border-2 border-purple-400 rounded"></div>
              <span>Outgoing</span>
            </div>
          </div>
        </div>
      </ReactFlow>

      {/* Custom node renderer */}
      <div className="hidden">
        {nodes.map((node) => (
          <div key={node.id} className="schema-node">
            <div className="font-semibold text-gray-900">{node.data.key}</div>
            <div className="text-xs text-gray-500">
              v{node.data.version} • {node.data.role}
            </div>
            <div className="text-xs text-gray-400 mt-1">
              {node.data.status}
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}

// Custom node component for better visualization
export function SchemaGraphNode({ data }: { data: SchemaNodeData }) {
  return (
    <div
      className={`
        px-4 py-3 rounded-lg border-2 shadow-sm min-w-[180px]
        ${nodeColors[data.role]}
      `}
    >
      <div className="font-semibold text-gray-900 text-sm">{data.key}</div>
      <div className="text-xs text-gray-500 mt-1">
        v{data.version} • {data.role}
      </div>
      <div className="text-xs text-gray-400 mt-1">
        {data.status}
      </div>
    </div>
  )
}

