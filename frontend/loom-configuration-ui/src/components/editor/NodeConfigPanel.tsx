import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { nodesApi } from '../../api/workflows'
import type { Node, NodeType } from '../../types'

interface NodeConfigPanelProps {
  node: Node
  isReadOnly: boolean
  onClose: () => void
  onDelete: () => void
  versionId: string
}

const nodeTypes: NodeType[] = ['Action', 'Condition', 'Validation', 'Split', 'Join']

export function NodeConfigPanel({ node, isReadOnly, onClose, onDelete, versionId }: NodeConfigPanelProps) {
  const queryClient = useQueryClient()
  const [name, setName] = useState(node.name || '')
  const [type, setType] = useState<NodeType>(node.type)
  const [config, setConfig] = useState(JSON.stringify(node.config || {}, null, 2))
  const [configError, setConfigError] = useState<string | null>(null)

  const updateMetadataMutation = useMutation({
    mutationFn: (data: { name: string | null; type?: NodeType }) => 
      nodesApi.updateNodeMetadata(node.id, data.name, data.type),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionId] })
    },
  })

  const updateConfigMutation = useMutation({
    mutationFn: (newConfig: Record<string, unknown>) => nodesApi.updateNodeConfig(node.id, newConfig),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionId] })
    },
  })

  const handleSaveConfig = () => {
    try {
      const parsed = JSON.parse(config)
      setConfigError(null)
      updateConfigMutation.mutate(parsed)
    } catch {
      setConfigError('Invalid JSON')
    }
  }

  return (
    <div className="w-80 bg-white border-l border-gray-200 flex flex-col">
      <div className="flex items-center justify-between px-4 py-3 border-b border-gray-200">
        <h3 className="font-semibold text-gray-900">Node Configuration</h3>
        <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Key</label>
          <input
            type="text"
            value={node.key}
            disabled
            className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-gray-50 text-gray-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            onBlur={() => {
              if (!isReadOnly && (name !== (node.name || '') || type !== node.type)) {
                updateMetadataMutation.mutate({ name: name || null, type })
              }
            }}
            disabled={isReadOnly}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-loom-500 focus:border-loom-500 disabled:bg-gray-50"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
          <select
            value={type}
            onChange={(e) => {
              setType(e.target.value as NodeType)
              if (!isReadOnly) {
                updateMetadataMutation.mutate({ name: name || null, type: e.target.value as NodeType })
              }
            }}
            disabled={isReadOnly}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-loom-500 focus:border-loom-500 disabled:bg-gray-50"
          >
            {nodeTypes.map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Configuration (JSON)</label>
          <textarea
            value={config}
            onChange={(e) => setConfig(e.target.value)}
            disabled={isReadOnly}
            rows={8}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg font-mono text-sm focus:ring-2 focus:ring-loom-500 focus:border-loom-500 disabled:bg-gray-50"
          />
          {configError && (
            <p className="mt-1 text-sm text-red-600">{configError}</p>
          )}
        </div>

        {!isReadOnly && (
          <button
            onClick={handleSaveConfig}
            disabled={updateConfigMutation.isPending}
            className="w-full px-4 py-2 bg-loom-600 text-white rounded-lg hover:bg-loom-700 disabled:opacity-50"
          >
            {updateConfigMutation.isPending ? 'Saving...' : 'Save Configuration'}
          </button>
        )}
      </div>

      {!isReadOnly && (
        <div className="p-4 border-t border-gray-200">
          <button
            onClick={onDelete}
            className="w-full px-4 py-2 bg-red-50 text-red-600 rounded-lg hover:bg-red-100 font-medium"
          >
            Delete Node
          </button>
        </div>
      )}
    </div>
  )
}

