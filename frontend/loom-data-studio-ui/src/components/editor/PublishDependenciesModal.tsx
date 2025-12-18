import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { schemasApi } from '../../api/masterdata'
import type { UnpublishedDependency } from '../../api/masterdata'

interface PublishDependenciesModalProps {
  schemaId: string
  dependencies: UnpublishedDependency[]
  onClose: () => void
  onSuccess: () => void
}

export function PublishDependenciesModal({
  schemaId,
  dependencies,
  onClose,
  onSuccess,
}: PublishDependenciesModalProps) {
  const queryClient = useQueryClient()
  const [confirmed, setConfirmed] = useState(false)
  const [selectedDependencies, setSelectedDependencies] = useState<Set<string>>(
    new Set(dependencies.map((d) => d.schemaId))
  )

  const publishMutation = useMutation({
    mutationFn: () =>
      schemasApi.publishRelatedSchemas(
        schemaId,
        'user@example.com',
        Array.from(selectedDependencies)
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schemaDetails', schemaId] })
      queryClient.invalidateQueries({ queryKey: ['validateSchema', schemaId] })
      queryClient.invalidateQueries({ queryKey: ['unpublishedDependencies', schemaId] })
      onSuccess()
      onClose()
    },
  })

  const handleToggle = (dependencyId: string) => {
    const newSelected = new Set(selectedDependencies)
    if (newSelected.has(dependencyId)) {
      newSelected.delete(dependencyId)
    } else {
      newSelected.add(dependencyId)
    }
    setSelectedDependencies(newSelected)
  }

  const handlePublish = () => {
    if (!confirmed || selectedDependencies.size === 0) return
    publishMutation.mutate()
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 max-w-2xl w-full shadow-xl max-h-[80vh] flex flex-col">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-xl font-semibold text-gray-900">
            Publish Dependencies
          </h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600"
          >
            ✕
          </button>
        </div>

        <div className="mb-4 p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
          <p className="text-sm text-yellow-800">
            The following schemas need to be published before this schema can be published:
          </p>
        </div>

        <div className="flex-1 overflow-y-auto mb-4">
          <div className="space-y-2">
            {dependencies.map((dep) => (
              <label
                key={dep.schemaId}
                className="flex items-center gap-3 p-3 bg-gray-50 border border-gray-200 rounded-lg cursor-pointer hover:bg-gray-100"
              >
                <input
                  type="checkbox"
                  checked={selectedDependencies.has(dep.schemaId)}
                  onChange={() => handleToggle(dep.schemaId)}
                  className="rounded"
                />
                <div className="flex-1">
                  <div className="font-medium text-gray-900">
                    {dep.key} (v{dep.version})
                  </div>
                  <div className="text-sm text-gray-500">
                    {dep.role} - {dep.status}
                  </div>
                </div>
              </label>
            ))}
          </div>
        </div>

        <div className="border-t border-gray-200 pt-4">
          <label className="flex items-center gap-2 mb-4">
            <input
              type="checkbox"
              checked={confirmed}
              onChange={(e) => setConfirmed(e.target.checked)}
              className="rounded"
            />
            <span className="text-sm text-gray-700">
              I confirm that I want to publish {selectedDependencies.size} schema
              {selectedDependencies.size !== 1 ? 's' : ''} and understand this action cannot be undone.
            </span>
          </label>

          <div className="flex gap-2 justify-end">
            <button
              onClick={onClose}
              className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
            >
              Cancel
            </button>
            <button
              onClick={handlePublish}
              disabled={
                !confirmed ||
                selectedDependencies.size === 0 ||
                publishMutation.isPending
              }
              className="px-4 py-2 bg-loom-600 text-white rounded-lg hover:bg-loom-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {publishMutation.isPending
                ? 'Publishing...'
                : `Publish ${selectedDependencies.size} Schema${selectedDependencies.size !== 1 ? 's' : ''}`}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

