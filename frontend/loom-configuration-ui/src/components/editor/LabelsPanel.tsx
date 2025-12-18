import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { labelsApi } from '../../api/workflows'
import type { WorkflowLabelDefinition, VariableType } from '../../types'

interface LabelsPanelProps {
  labels: WorkflowLabelDefinition[]
  versionId: string
  isReadOnly: boolean
  onClose: () => void
}

const labelTypes: VariableType[] = ['String', 'Number', 'Boolean']

export function LabelsPanel({ labels, versionId, isReadOnly, onClose }: LabelsPanelProps) {
  const queryClient = useQueryClient()
  const [showAdd, setShowAdd] = useState(false)
  const [newKey, setNewKey] = useState('')
  const [newType, setNewType] = useState<VariableType>('String')
  const [newDescription, setNewDescription] = useState('')

  const addMutation = useMutation({
    mutationFn: () => labelsApi.addLabel(versionId, newKey, newType, newDescription || undefined),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionId] })
      setShowAdd(false)
      setNewKey('')
      setNewType('String')
      setNewDescription('')
    },
  })

  const removeMutation = useMutation({
    mutationFn: (id: string) => labelsApi.removeLabel(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionId] })
    },
  })

  return (
    <div className="w-80 bg-white border-l border-gray-200 flex flex-col">
      <div className="flex items-center justify-between px-4 py-3 border-b border-gray-200">
        <h3 className="font-semibold text-gray-900">Labels</h3>
        <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <div className="flex-1 overflow-y-auto p-4">
        <p className="text-xs text-gray-500 mb-4">
          Labels are metadata attached to workflow instances for filtering and grouping.
        </p>

        {labels.length === 0 ? (
          <div className="text-center py-8 text-gray-500">
            <p>No labels defined</p>
          </div>
        ) : (
          <div className="space-y-3">
            {labels.map((label) => (
              <div key={label.id} className="border border-gray-200 rounded-lg p-3">
                <div className="flex items-center justify-between mb-1">
                  <span className="font-medium text-gray-900 font-mono text-sm">{label.key}</span>
                  {!isReadOnly && (
                    <button
                      onClick={() => removeMutation.mutate(label.id)}
                      className="text-red-500 hover:text-red-700 text-sm"
                    >
                      ×
                    </button>
                  )}
                </div>
                <span className="text-xs px-1.5 py-0.5 bg-gray-100 rounded text-gray-600">
                  {label.type}
                </span>
                {label.description && (
                  <p className="text-xs text-gray-500 mt-1">{label.description}</p>
                )}
              </div>
            ))}
          </div>
        )}

        {showAdd && (
          <div className="mt-4 border border-loom-200 rounded-lg p-4 bg-loom-50">
            <h4 className="font-medium text-gray-900 mb-3">Add Label</h4>
            <div className="space-y-3">
              <input
                type="text"
                placeholder="Key"
                value={newKey}
                onChange={(e) => setNewKey(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
              />
              <select
                value={newType}
                onChange={(e) => setNewType(e.target.value as VariableType)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
              >
                {labelTypes.map((type) => (
                  <option key={type} value={type}>{type}</option>
                ))}
              </select>
              <input
                type="text"
                placeholder="Description (optional)"
                value={newDescription}
                onChange={(e) => setNewDescription(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
              />
              <div className="flex gap-2">
                <button
                  onClick={() => addMutation.mutate()}
                  disabled={!newKey || addMutation.isPending}
                  className="flex-1 px-3 py-2 bg-loom-600 text-white rounded-lg text-sm hover:bg-loom-700 disabled:opacity-50"
                >
                  Add
                </button>
                <button
                  onClick={() => setShowAdd(false)}
                  className="px-3 py-2 text-gray-600 text-sm hover:bg-gray-100 rounded-lg"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        )}
      </div>

      {!isReadOnly && !showAdd && (
        <div className="p-4 border-t border-gray-200">
          <button
            onClick={() => setShowAdd(true)}
            className="w-full px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50"
          >
            + Add Label
          </button>
        </div>
      )}
    </div>
  )
}

