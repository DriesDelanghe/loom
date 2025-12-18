import { memo } from 'react'
import { Handle, Position, type NodeProps } from '@xyflow/react'
import type { TriggerType } from '../../types'

const triggerTypeIcons: Record<TriggerType, string> = {
  Manual: '👆',
  Webhook: '🔗',
  Schedule: '⏰',
}

const triggerTypeColors: Record<TriggerType, string> = {
  Manual: 'border-green-400 bg-green-50',
  Webhook: 'border-blue-400 bg-blue-50',
  Schedule: 'border-purple-400 bg-purple-50',
}

const triggerTypeLabels: Record<TriggerType, string> = {
  Manual: 'Manual',
  Webhook: 'Webhook',
  Schedule: 'Schedule',
}

export interface TriggerNodeData extends Record<string, unknown> {
  id: string
  triggerBindingId: string
  triggerId: string
  type: TriggerType
  enabled: boolean
  priority: number | null
  config: Record<string, unknown> | null
  nodeKey: string // Used for layout persistence
}

export const TriggerNode = memo(function TriggerNode(props: NodeProps) {
  const data = props.data as TriggerNodeData
  const selected = props.selected

  return (
    <div
      className={`
        px-4 py-3 rounded-lg border-2 shadow-sm min-w-[140px]
        ${triggerTypeColors[data.type]}
        ${!data.enabled ? 'opacity-50' : ''}
        ${selected ? 'shadow-lg' : ''}
      `}
    >
      <div className="flex items-center gap-2">
        <span className="text-lg">{triggerTypeIcons[data.type]}</span>
        <div>
          <div className="font-medium text-gray-900 text-sm">
            {triggerTypeLabels[data.type]}
          </div>
          {data.priority !== null && (
            <div className="text-xs text-gray-500">Priority: {data.priority}</div>
          )}
          {!data.enabled && (
            <div className="text-xs text-gray-400">Disabled</div>
          )}
        </div>
      </div>

      {/* Source handle on the right - triggers only have outgoing connections */}
      <Handle
        type="source"
        position={Position.Right}
        id="source-Success"
        className="w-3 h-3 !bg-gray-400 border-2 border-white"
      />
    </div>
  )
})

