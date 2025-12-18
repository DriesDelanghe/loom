import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { triggersApi } from '../../api/workflows'
import type { WorkflowVersionDetails, TriggerType, Node } from '../../types'

interface TriggersPanelProps {
  versionDetails: WorkflowVersionDetails
  isReadOnly: boolean
  onClose: () => void
}

const triggerTypes: TriggerType[] = ['Manual', 'Webhook', 'Schedule']

export function TriggersPanel({ versionDetails, isReadOnly, onClose }: TriggersPanelProps) {
  const queryClient = useQueryClient()
  const [showCreate, setShowCreate] = useState(false)
  const [newTriggerType, setNewTriggerType] = useState<TriggerType>('Manual')
  const [selectedBindingId, setSelectedBindingId] = useState<string | null>(null)
  const [selectedEntryNode, setSelectedEntryNode] = useState<string>('')

  const createAndBindMutation = useMutation({
    mutationFn: async () => {
      const trigger = await triggersApi.createTrigger(newTriggerType)
      await triggersApi.bindTriggerToWorkflow(trigger.id, versionDetails.version.id, 1, true)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionDetails.version.id] })
      setShowCreate(false)
    },
  })

  const unbindMutation = useMutation({
    mutationFn: (bindingId: string) => triggersApi.unbindTrigger(bindingId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionDetails.version.id] })
    },
  })

  const bindToNodeMutation = useMutation({
    mutationFn: (data: { triggerBindingId: string; entryNodeId: string }) =>
      triggersApi.bindTriggerToNode(data.triggerBindingId, data.entryNodeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionDetails.version.id] })
      setSelectedEntryNode('')
    },
  })

  const unbindFromNodeMutation = useMutation({
    mutationFn: (nodeBindingId: string) => triggersApi.unbindTriggerFromNode(nodeBindingId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versionDetails', versionDetails.version.id] })
    },
  })

  const handleAddEntryNode = (bindingId: string) => {
    if (!selectedEntryNode) return
    bindToNodeMutation.mutate({ triggerBindingId: bindingId, entryNodeId: selectedEntryNode })
  }

  return (
    <div className="w-96 bg-white border-l border-gray-200 flex flex-col">
      <div className="flex items-center justify-between px-4 py-3 border-b border-gray-200">
        <h3 className="font-semibold text-gray-900">Triggers</h3>
        <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <div className="flex-1 overflow-y-auto p-4">
        {versionDetails.triggerBindings.length === 0 ? (
          <div className="text-center py-8 text-gray-500">
            <p>No triggers configured</p>
            {!isReadOnly && (
              <button
                onClick={() => setShowCreate(true)}
                className="mt-2 text-loom-600 hover:text-loom-700 font-medium"
              >
                Add a trigger
              </button>
            )}
          </div>
        ) : (
          <div className="space-y-4">
            {versionDetails.triggerBindings.map((binding) => (
              <div key={binding.id} className="border border-gray-200 rounded-lg p-4">
                <div className="flex items-center justify-between mb-3">
                  <div className="flex items-center gap-2">
                    <span className={`w-2 h-2 rounded-full ${binding.enabled ? 'bg-green-500' : 'bg-gray-400'}`} />
                    <span className="font-medium text-gray-900">Trigger</span>
                  </div>
                  {!isReadOnly && (
                    <button
                      onClick={() => unbindMutation.mutate(binding.id)}
                      className="text-red-500 hover:text-red-700 text-sm"
                    >
                      Remove
                    </button>
                  )}
                </div>

                <div className="text-sm text-gray-500 mb-3">
                  Priority: {binding.priority ?? 'Default'}
                </div>

                <div className="border-t border-gray-100 pt-3">
                  <div className="text-sm font-medium text-gray-700 mb-2">Entry Nodes</div>
                  
                  {binding.nodeBindings.length === 0 ? (
                    <p className="text-sm text-yellow-600 bg-yellow-50 px-2 py-1 rounded">
                      ⚠ No entry nodes configured
                    </p>
                  ) : (
                    <ul className="space-y-1 mb-2">
                      {binding.nodeBindings.map((nb) => {
                        const node = versionDetails.nodes.find((n: Node) => n.id === nb.entryNodeId)
                        return (
                          <li key={nb.id} className="flex items-center justify-between text-sm bg-gray-50 px-2 py-1 rounded">
                            <span>{node?.name || node?.key || nb.entryNodeId}</span>
                            {!isReadOnly && (
                              <button
                                onClick={() => unbindFromNodeMutation.mutate(nb.id)}
                                className="text-red-500 hover:text-red-700"
                              >
                                ×
                              </button>
                            )}
                          </li>
                        )
                      })}
                    </ul>
                  )}

                  {!isReadOnly && (
                    <div className="mt-2">
                      {selectedBindingId === binding.id ? (
                        <div className="flex gap-2">
                          <select
                            value={selectedEntryNode}
                            onChange={(e) => setSelectedEntryNode(e.target.value)}
                            className="flex-1 text-sm border border-gray-300 rounded px-2 py-1"
                          >
                            <option value="">Select node...</option>
                            {versionDetails.nodes
                              .filter((n: Node) => !binding.nodeBindings.some(nb => nb.entryNodeId === n.id))
                              .map((node: Node) => (
                                <option key={node.id} value={node.id}>
                                  {node.name || node.key}
                                </option>
                              ))}
                          </select>
                          <button
                            onClick={() => handleAddEntryNode(binding.id)}
                            disabled={!selectedEntryNode}
                            className="px-2 py-1 bg-loom-600 text-white text-sm rounded hover:bg-loom-700 disabled:opacity-50"
                          >
                            Add
                          </button>
                          <button
                            onClick={() => setSelectedBindingId(null)}
                            className="px-2 py-1 text-gray-500 text-sm"
                          >
                            Cancel
                          </button>
                        </div>
                      ) : (
                        <button
                          onClick={() => setSelectedBindingId(binding.id)}
                          className="text-sm text-loom-600 hover:text-loom-700"
                        >
                          + Add entry node
                        </button>
                      )}
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}

        {showCreate && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
            <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6">
              <h3 className="text-lg font-semibold mb-4">Add Trigger</h3>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
                  <select
                    value={newTriggerType}
                    onChange={(e) => setNewTriggerType(e.target.value as TriggerType)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                  >
                    {triggerTypes.map((type) => (
                      <option key={type} value={type}>{type}</option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="flex justify-end gap-3 mt-6">
                <button
                  onClick={() => setShowCreate(false)}
                  className="px-4 py-2 text-gray-700 hover:bg-gray-100 rounded-lg"
                >
                  Cancel
                </button>
                <button
                  onClick={() => createAndBindMutation.mutate()}
                  disabled={createAndBindMutation.isPending}
                  className="px-4 py-2 bg-loom-600 text-white rounded-lg hover:bg-loom-700 disabled:opacity-50"
                >
                  {createAndBindMutation.isPending ? 'Adding...' : 'Add Trigger'}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>

      {!isReadOnly && versionDetails.triggerBindings.length > 0 && (
        <div className="p-4 border-t border-gray-200">
          <button
            onClick={() => setShowCreate(true)}
            className="w-full px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50"
          >
            + Add Another Trigger
          </button>
        </div>
      )}
    </div>
  )
}

