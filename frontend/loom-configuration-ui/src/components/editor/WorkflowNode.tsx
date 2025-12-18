import { memo } from 'react'
import { Handle, Position, type NodeProps } from '@xyflow/react'
import type { NodeType } from '../../types'
import { isControlNode, getPositiveOutcome, getNegativeOutcome } from '../../utils/nodeMetadata'

const nodeTypeIcons: Record<NodeType, string> = {
  Action: '⚡',
  Condition: '❓',
  Validation: '✓',
  Split: '⤢',
  Join: '⤣',
}

const nodeTypeColors: Record<NodeType, string> = {
  Action: 'border-blue-400 bg-blue-50',
  Condition: 'border-purple-400 bg-purple-50',
  Validation: 'border-green-400 bg-green-50',
  Split: 'border-orange-400 bg-orange-50',
  Join: 'border-orange-400 bg-orange-50',
}

export interface WorkflowNodeData extends Record<string, unknown> {
  id: string
  workflowVersionId: string
  key: string
  name: string | null
  type: NodeType
  config: Record<string, unknown> | null
  createdAt: string
  isEndNode?: boolean // Node with no outgoing connections
}

export const WorkflowNode = memo(function WorkflowNode(props: NodeProps) {
  const data = props.data as WorkflowNodeData
  const selected = props.selected
  const isControl = isControlNode(data.type)
  const positiveOutcome = getPositiveOutcome(data.type)
  const negativeOutcome = getNegativeOutcome(data.type)
  
  return (
    <div
      className={`
        px-4 py-3 rounded-lg border-2 shadow-sm min-w-[140px]
        ${nodeTypeColors[data.type]}
        ${selected ? 'shadow-lg' : ''}
        ${isControl ? 'border-orange-500 border-2' : ''}
        ${data.isEndNode ? 'ring-2 ring-green-300' : ''}
      `}
    >
      {/* Target handles on the left - all workflow nodes can receive connections */}
      <Handle
        type="target"
        position={Position.Left}
        id="target"
        className="w-3 h-3 !bg-gray-400 border-2 border-white"
      />
      
      <div className="flex items-center gap-2">
        <span className="text-lg">{nodeTypeIcons[data.type]}</span>
        <div className="flex-1">
          <div className="font-medium text-gray-900 text-sm">
            {data.name || data.key}
          </div>
          <div className="text-xs text-gray-500">
            {data.type}
            {isControl && ' (Control)'}
            {data.isEndNode && ' • End'}
          </div>
        </div>
      </div>
      
      {/* Source handles on the right - position-based for non-control nodes */}
      {isControl ? (
        // Control nodes: Split has multiple "Next" connectors, Join has one "Joined" connector
        data.type === 'Split' ? (
          // Split: multiple outgoing connectors, all "Next"
          <>
            <Handle
              type="source"
              position={Position.Right}
              id={`source-${positiveOutcome}`}
              className="w-3 h-3 !bg-orange-400 border-2 border-white"
              style={{ top: '50%' }}
              title={positiveOutcome}
            />
          </>
        ) : (
          // Join: single outgoing connector "Joined"
          <Handle
            type="source"
            position={Position.Right}
            id={`source-${positiveOutcome}`}
            className="w-3 h-3 !bg-orange-400 border-2 border-white"
            style={{ top: '50%' }}
            title={positiveOutcome}
          />
        )
      ) : (
        // Non-control nodes: top connector = positive outcome, bottom connector = negative outcome
        <>
          <Handle
            type="source"
            position={Position.Right}
            id={`source-${positiveOutcome}`}
            className="w-3 h-3 !bg-green-400 border-2 border-white"
            style={{ top: '25%' }}
            title={positiveOutcome}
          />
          <Handle
            type="source"
            position={Position.Right}
            id={`source-${negativeOutcome}`}
            className="w-3 h-3 !bg-red-400 border-2 border-white"
            style={{ top: '75%' }}
            title={negativeOutcome}
          />
        </>
      )}
    </div>
  )
})

