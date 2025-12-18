import { memo } from 'react'
import { Handle, Position, type NodeProps } from '@xyflow/react'
import type { TransformNodeType } from '../../types'

const nodeTypeIcons: Record<TransformNodeType, string> = {
  Source: '📥',
  Map: '🔄',
  Filter: '🔍',
  Aggregate: '📊',
  Join: '🔗',
  Split: '✂️',
  Constant: '📌',
  Expression: '⚡',
}

const nodeTypeColors: Record<TransformNodeType, string> = {
  Source: 'border-blue-400 bg-blue-50',
  Map: 'border-green-400 bg-green-50',
  Filter: 'border-yellow-400 bg-yellow-50',
  Aggregate: 'border-purple-400 bg-purple-50',
  Join: 'border-orange-400 bg-orange-50',
  Split: 'border-pink-400 bg-pink-50',
  Constant: 'border-gray-400 bg-gray-50',
  Expression: 'border-indigo-400 bg-indigo-50',
}

export interface TransformNodeData extends Record<string, unknown> {
  id: string
  key: string
  nodeType: TransformNodeType
  outputType: string
  config: string
}

export const TransformNode = memo(function TransformNode(props: NodeProps) {
  const data = props.data as TransformNodeData
  const selected = props.selected

  // Source nodes have no inputs, only outputs
  const isSource = data.nodeType === 'Source'
  
  // Output nodes (like Split) can have multiple outputs
  const hasMultipleOutputs = data.nodeType === 'Split' || data.nodeType === 'Join'

  return (
    <div
      className={`
        px-4 py-3 rounded-lg border-2 shadow-sm min-w-[160px]
        ${nodeTypeColors[data.nodeType]}
        ${selected ? 'shadow-lg ring-2 ring-loom-500' : ''}
      `}
    >
      {/* Input handles on the left - all nodes except Source can receive connections */}
      {!isSource && (
        <Handle
          type="target"
          position={Position.Left}
          id="input"
          className="w-3 h-3 !bg-gray-400 border-2 border-white"
        />
      )}

      <div className="flex items-center gap-2">
        <span className="text-lg">{nodeTypeIcons[data.nodeType]}</span>
        <div className="flex-1">
          <div className="font-medium text-gray-900 text-sm">
            {data.key}
          </div>
          <div className="text-xs text-gray-500">
            {data.nodeType}
          </div>
        </div>
      </div>

      {/* Output handles on the right */}
      {hasMultipleOutputs ? (
        // Split/Join can have multiple outputs
        <>
          <Handle
            type="source"
            position={Position.Right}
            id="output-0"
            className="w-3 h-3 !bg-green-400 border-2 border-white"
            style={{ top: '30%' }}
          />
          <Handle
            type="source"
            position={Position.Right}
            id="output-1"
            className="w-3 h-3 !bg-green-400 border-2 border-white"
            style={{ top: '70%' }}
          />
        </>
      ) : (
        // Single output
        <Handle
          type="source"
          position={Position.Right}
          id="output"
          className="w-3 h-3 !bg-green-400 border-2 border-white"
        />
      )}
    </div>
  )
})


