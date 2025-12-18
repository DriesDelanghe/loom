import { useState, useMemo } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { schemasApi } from '../../api/masterdata'
import type { FieldDefinitionSummary, FieldType, ScalarType } from '../../types'

interface InspectorPanelProps {
  schemaId: string
  field: FieldDefinitionSummary | null
  isReadOnly: boolean
  onClose: () => void
}

export function InspectorPanel({ schemaId, field, isReadOnly, onClose }: InspectorPanelProps) {
  const queryClient = useQueryClient()
  const [isEditing, setIsEditing] = useState(false)
  const [editedPath, setEditedPath] = useState('')
  const [editedFieldType, setEditedFieldType] = useState<FieldType>('Scalar')
  const [editedScalarType, setEditedScalarType] = useState<ScalarType | null>(null)
  const [editedRequired, setEditedRequired] = useState(false)
  const [editedDescription, setEditedDescription] = useState('')

  // Initialize form when field changes or editing starts
  useMemo(() => {
    if (field) {
      setEditedPath(field.path)
      setEditedFieldType(field.fieldType)
      setEditedScalarType(field.scalarType)
      setEditedRequired(field.required)
      setEditedDescription(field.description || '')
    }
  }, [field, isEditing])

  const updateFieldMutation = useMutation({
    mutationFn: () => {
      if (!field) throw new Error('No field selected')
      return schemasApi.updateField(
        field.id,
        editedPath !== field.path ? editedPath : undefined,
        editedFieldType !== field.fieldType ? editedFieldType : undefined,
        editedScalarType !== field.scalarType ? editedScalarType : undefined,
        undefined, // elementSchemaId - not editable
        editedRequired !== field.required ? editedRequired : undefined,
        editedDescription !== (field.description || '') ? editedDescription : undefined
      )
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schemaDetails', schemaId] })
      setIsEditing(false)
    },
  })

  if (!field) {
    return (
      <div className="h-full flex items-center justify-center text-gray-500">
        Select a field to view details
      </div>
    )
  }

  const handleSave = () => {
    updateFieldMutation.mutate()
  }

  const handleCancel = () => {
    setIsEditing(false)
    // Reset to original values
    if (field) {
      setEditedPath(field.path)
      setEditedFieldType(field.fieldType)
      setEditedScalarType(field.scalarType)
      setEditedRequired(field.required)
      setEditedDescription(field.description || '')
    }
  }

  return (
    <div className="h-full flex flex-col">
      <div className="p-4 border-b border-gray-200 flex items-center justify-between">
        <h3 className="font-semibold text-gray-900">Field Details</h3>
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
                disabled={updateFieldMutation.isPending}
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
            Path
          </label>
          {isEditing ? (
            <input
              type="text"
              value={editedPath}
              onChange={(e) => setEditedPath(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
            />
          ) : (
            <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm text-gray-900">
              {field.path}
            </div>
          )}
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">
            Field Type
          </label>
          {isEditing ? (
            <select
              value={editedFieldType}
              onChange={(e) => setEditedFieldType(e.target.value as FieldType)}
              className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
            >
              <option value="Scalar">Scalar</option>
              <option value="Object">Object</option>
              <option value="Array">Array</option>
            </select>
          ) : (
            <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm text-gray-900">
              {field.fieldType}
            </div>
          )}
        </div>

        {field.fieldType === 'Scalar' && (
          <div>
            <label className="block text-xs font-medium text-gray-700 mb-1">
              Scalar Type
            </label>
            {isEditing ? (
              <select
                value={editedScalarType || ''}
                onChange={(e) => setEditedScalarType(e.target.value as ScalarType || null)}
                className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
              >
                <option value="">-- Select scalar type --</option>
                <option value="String">String</option>
                <option value="Integer">Integer</option>
                <option value="Decimal">Decimal</option>
                <option value="Boolean">Boolean</option>
                <option value="Date">Date</option>
                <option value="DateTime">DateTime</option>
                <option value="Time">Time</option>
                <option value="Guid">Guid</option>
              </select>
            ) : (
              <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm text-gray-900">
                {field.scalarType || 'Not set'}
              </div>
            )}
          </div>
        )}


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
              {field.required ? 'Yes' : 'No'}
            </div>
          )}
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">
            Description
          </label>
          {isEditing ? (
            <textarea
              value={editedDescription}
              onChange={(e) => setEditedDescription(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
              rows={3}
              placeholder="Field description (optional)"
            />
          ) : (
            <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm text-gray-700 min-h-[60px]">
              {field.description || 'No description'}
            </div>
          )}
        </div>

        {isReadOnly && (
          <div className="mt-4 p-3 bg-yellow-50 border border-yellow-200 rounded text-xs text-yellow-800">
            This schema is Published and cannot be modified.
          </div>
        )}
      </div>
    </div>
  )
}
