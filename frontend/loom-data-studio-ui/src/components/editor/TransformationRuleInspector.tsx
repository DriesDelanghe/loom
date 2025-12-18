import { useState, useMemo } from 'react'
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query'
import { transformationSpecsApi, schemasApi } from '../../api/masterdata'
import type { SimpleTransformRuleSummary } from '../../types'

interface TransformationRuleInspectorProps {
  schemaId: string
  transformationSpecId: string // eslint-disable-line @typescript-eslint/no-unused-vars
  targetSchemaId: string
  rule: SimpleTransformRuleSummary | null
  isReadOnly: boolean
  onClose: () => void
}

export function TransformationRuleInspector({
  schemaId,
  transformationSpecId: _transformationSpecId,
  targetSchemaId,
  rule,
  isReadOnly,
  onClose,
}: TransformationRuleInspectorProps) {
  const queryClient = useQueryClient()
  const [isEditing, setIsEditing] = useState(false)
  const [editedSourcePath, setEditedSourcePath] = useState('')
  const [editedTargetPath, setEditedTargetPath] = useState('')
  const [editedConverterId, setEditedConverterId] = useState<string | null>(null)
  const [editedRequired, setEditedRequired] = useState(false)

  // Fetch source and target schema details for field dropdowns
  const { data: sourceSchemaDetails } = useQuery({
    queryKey: ['schemaDetails', schemaId],
    queryFn: () => schemasApi.getSchemaDetails(schemaId),
  })

  const { data: targetSchemaDetails } = useQuery({
    queryKey: ['schemaDetails', targetSchemaId],
    queryFn: () => schemasApi.getSchemaDetails(targetSchemaId),
    enabled: !!targetSchemaId,
  })

  const availableSourceFields = useMemo(() => {
    if (!sourceSchemaDetails) return []
    return sourceSchemaDetails.fields.map((f) => f.path)
  }, [sourceSchemaDetails])

  const availableTargetFields = useMemo(() => {
    if (!targetSchemaDetails) return []
    return targetSchemaDetails.fields.map((f) => f.path)
  }, [targetSchemaDetails])

  // Initialize form when rule changes or editing starts
  useMemo(() => {
    if (rule) {
      setEditedSourcePath(rule.sourcePath)
      setEditedTargetPath(rule.targetPath)
      setEditedConverterId(rule.converterId)
      setEditedRequired(rule.required)
    }
  }, [rule, isEditing])

  const updateRuleMutation = useMutation({
    mutationFn: () => {
      if (!rule) throw new Error('No rule selected')
      return transformationSpecsApi.updateSimpleTransformRule(
        rule.id,
        editedSourcePath !== rule.sourcePath ? editedSourcePath : undefined,
        editedTargetPath !== rule.targetPath ? editedTargetPath : undefined,
        editedConverterId !== rule.converterId ? editedConverterId : undefined,
        editedRequired !== rule.required ? editedRequired : undefined
      )
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transformationSpec', schemaId] })
      setIsEditing(false)
    },
  })

  if (!rule) {
    return (
      <div className="h-full flex items-center justify-center text-gray-500">
        Select a rule to view details
      </div>
    )
  }

  const handleSave = () => {
    updateRuleMutation.mutate()
  }

  const handleCancel = () => {
    setIsEditing(false)
    // Reset to original values
    if (rule) {
      setEditedSourcePath(rule.sourcePath)
      setEditedTargetPath(rule.targetPath)
      setEditedConverterId(rule.converterId)
      setEditedRequired(rule.required)
    }
  }

  return (
    <div className="h-full flex flex-col">
      <div className="p-4 border-b border-gray-200 flex items-center justify-between">
        <h3 className="font-semibold text-gray-900">Rule Details</h3>
        <div className="flex items-center gap-2">
          {!isReadOnly && !isEditing && (
            <button
              onClick={() => setIsEditing(true)}
              className="px-3 py-1 text-sm text-loom-600 hover:bg-loom-50 rounded"
            >
              Edit
            </button>
          )}
          {!isReadOnly && isEditing && (
            <>
              <button
                onClick={handleSave}
                disabled={updateRuleMutation.isPending}
                className="px-3 py-1 text-sm bg-loom-600 text-white rounded hover:bg-loom-700 disabled:opacity-50"
              >
                Save
              </button>
              <button
                onClick={handleCancel}
                className="px-3 py-1 text-sm bg-gray-200 text-gray-700 rounded hover:bg-gray-300"
              >
                Cancel
              </button>
            </>
          )}
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600"
          >
            ✕
          </button>
        </div>
      </div>

      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">
            Source Field
          </label>
          {isEditing ? (
            <select
              value={editedSourcePath}
              onChange={(e) => setEditedSourcePath(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
            >
              <option value="">-- Select source field --</option>
              {availableSourceFields.map((path) => (
                <option key={path} value={path}>
                  {path}
                </option>
              ))}
            </select>
          ) : (
            <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm font-mono text-gray-900">
              {rule.sourcePath}
            </div>
          )}
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">
            Target Field
          </label>
          {isEditing ? (
            <select
              value={editedTargetPath}
              onChange={(e) => setEditedTargetPath(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
            >
              <option value="">-- Select target field --</option>
              {availableTargetFields.map((path) => (
                <option key={path} value={path}>
                  {path}
                </option>
              ))}
            </select>
          ) : (
            <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm font-mono text-gray-900">
              {rule.targetPath}
            </div>
          )}
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">
            Converter
          </label>
          {isEditing ? (
            <input
              type="text"
              value={editedConverterId || ''}
              onChange={(e) => setEditedConverterId(e.target.value || null)}
              className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
              placeholder="Converter ID (optional)"
            />
          ) : (
            <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm text-gray-900">
              {rule.converterId || '-'}
            </div>
          )}
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">
            Required
          </label>
          {isEditing ? (
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={editedRequired}
                onChange={(e) => setEditedRequired(e.target.checked)}
                className="rounded"
              />
              <span className="text-sm text-gray-700">Required</span>
            </div>
          ) : (
            <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm text-gray-900">
              {rule.required ? 'Yes' : 'No'}
            </div>
          )}
        </div>

        {isReadOnly && (
          <div className="mt-4 p-3 bg-yellow-50 border border-yellow-200 rounded text-xs text-yellow-800">
            This transformation spec is Published and cannot be modified.
          </div>
        )}
      </div>
    </div>
  )
}

