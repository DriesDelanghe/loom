import { useState, useMemo } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { DndProvider, useDrag, useDrop } from 'react-dnd'
import { HTML5Backend } from 'react-dnd-html5-backend'
import { keyDefinitionsApi } from '../../api/masterdata'
import type { KeyDefinitionSummary, FieldDefinitionSummary } from '../../types'

interface KeyDefinitionsEditorProps {
  schemaId: string
  keyDefinitions: KeyDefinitionSummary[]
  fields: FieldDefinitionSummary[]
  isReadOnly: boolean
  expertMode?: boolean
  validationErrors?: string[]
}

export function KeyDefinitionsEditor({
  schemaId,
  keyDefinitions,
  fields,
  isReadOnly,
  expertMode = false,
  validationErrors = [],
}: KeyDefinitionsEditorProps) {
  const queryClient = useQueryClient()
  const [selectedKeyId, setSelectedKeyId] = useState<string | null>(keyDefinitions[0]?.id || null)
  const [showAddKey, setShowAddKey] = useState(false)
  const [newKeyName, setNewKeyName] = useState('')
  const [newIsPrimary, setNewIsPrimary] = useState(false)

  const selectedKey = useMemo(
    () => keyDefinitions.find((k) => k.id === selectedKeyId) || null,
    [keyDefinitions, selectedKeyId]
  )

  // Available fields for key definition (scalar, required for primary)
  const availableFields = useMemo(() => {
    if (!selectedKey) return []
    return fields.filter((f) => {
      // Only scalar fields can be in keys
      if (f.fieldType !== 'Scalar') return false
      // Primary keys must use required fields
      if (selectedKey.isPrimary && !f.required) return false
      // Don't show fields already in this key
      if (selectedKey.keyFields.some((kf) => kf.fieldPath === f.path)) return false
      return true
    })
  }, [fields, selectedKey])

  const addKeyMutation = useMutation({
    mutationFn: () =>
      keyDefinitionsApi.addKeyDefinition(schemaId, newKeyName, newIsPrimary),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['schemaDetails', schemaId] })
      setShowAddKey(false)
      setNewKeyName('')
      setNewIsPrimary(false)
      setSelectedKeyId(data.id)
    },
  })

  const removeKeyFieldMutation = useMutation({
    mutationFn: (fieldId: string) => keyDefinitionsApi.removeKeyField(fieldId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schemaDetails', schemaId] })
    },
  })

  const reorderKeyFieldsMutation = useMutation({
    mutationFn: (keyFieldIdsInOrder: string[]) => {
      if (!selectedKeyId) throw new Error('No key selected')
      return keyDefinitionsApi.reorderKeyFields(selectedKeyId, keyFieldIdsInOrder)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schemaDetails', schemaId] })
    },
  })

  const removeKeyMutation = useMutation({
    mutationFn: (keyId: string) => keyDefinitionsApi.removeKeyDefinition(keyId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schemaDetails', schemaId] })
      if (selectedKeyId) {
        setSelectedKeyId(null)
      }
    },
  })

  const addKeyFieldMutation = useMutation({
    mutationFn: (fieldPath: string) => {
      if (!selectedKeyId) throw new Error('No key selected')
      const nextOrder = selectedKey?.keyFields.length || 0
      return keyDefinitionsApi.addKeyField(selectedKeyId, fieldPath, nextOrder)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schemaDetails', schemaId] })
    },
  })

  const handleAddField = (fieldPath: string) => {
    addKeyFieldMutation.mutate(fieldPath)
  }

  const handleRemoveField = (fieldId: string) => {
    if (confirm('Remove this field from the key?')) {
      removeKeyFieldMutation.mutate(fieldId)
    }
  }

  const handleReorder = (draggedFieldId: string, targetFieldId: string) => {
    if (!selectedKey) return

    const currentOrder = selectedKey.keyFields
      .sort((a, b) => a.order - b.order)
      .map((f) => f.id)

    const draggedIndex = currentOrder.indexOf(draggedFieldId)
    const targetIndex = currentOrder.indexOf(targetFieldId)

    if (draggedIndex === -1 || targetIndex === -1) return

    const newOrder = [...currentOrder]
    newOrder.splice(draggedIndex, 1)
    newOrder.splice(targetIndex, 0, draggedFieldId)

    reorderKeyFieldsMutation.mutate(newOrder)
  }

  const hasKeyErrors = (keyId: string) => {
    return validationErrors.some((err) => err.includes(keyId) || err.includes('KeyDefinitions'))
  }

  return (
    <DndProvider backend={HTML5Backend}>
      <div className="h-full flex">
        {/* Left: Keys Overview Panel */}
        <div className="w-80 bg-white border-r border-gray-200 flex flex-col">
          <div className="p-4 border-b border-gray-200">
            <h3 className="font-semibold text-gray-900 mb-2">Business Keys</h3>
            {!isReadOnly && (
              <button
                onClick={() => setShowAddKey(true)}
                className="w-full px-3 py-2 text-sm bg-loom-600 text-white rounded-lg hover:bg-loom-700"
              >
                ➕ Add Key
              </button>
            )}
          </div>

          {showAddKey && !isReadOnly && (
            <div className="p-4 border-b border-gray-200 bg-gray-50">
              <h4 className="text-sm font-medium mb-3">Add Key Definition</h4>
              <div className="space-y-3">
                <input
                  type="text"
                  value={newKeyName}
                  onChange={(e) => setNewKeyName(e.target.value)}
                  className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-loom-500"
                  placeholder="Key name"
                />
                <div className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    checked={newIsPrimary}
                    onChange={(e) => setNewIsPrimary(e.target.checked)}
                    className="rounded"
                    disabled={keyDefinitions.some((k) => k.isPrimary)}
                  />
                  <label className="text-xs text-gray-700">
                    Primary Key
                    {keyDefinitions.some((k) => k.isPrimary) && (
                      <span className="text-red-600 ml-1">(already exists)</span>
                    )}
                  </label>
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={() => addKeyMutation.mutate()}
                    disabled={!newKeyName.trim() || addKeyMutation.isPending}
                    className="flex-1 px-2 py-1 text-xs bg-loom-600 text-white rounded hover:bg-loom-700 disabled:opacity-50"
                  >
                    Add
                  </button>
                  <button
                    onClick={() => {
                      setShowAddKey(false)
                      setNewKeyName('')
                      setNewIsPrimary(false)
                    }}
                    className="flex-1 px-2 py-1 text-xs bg-gray-200 text-gray-700 rounded hover:bg-gray-300"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            </div>
          )}

          <div className="flex-1 overflow-y-auto p-2">
            {keyDefinitions.length === 0 ? (
              <div className="text-center py-8 text-sm text-gray-500">
                No keys defined
              </div>
            ) : (
              <div className="space-y-2">
                {keyDefinitions.map((keyDef) => (
                  <div
                    key={keyDef.id}
                    className={`p-3 rounded transition-colors ${
                      selectedKeyId === keyDef.id
                        ? 'bg-loom-100 border-2 border-loom-500'
                        : hasKeyErrors(keyDef.id)
                        ? 'bg-red-50 border-2 border-red-300'
                        : 'bg-gray-50 border-2 border-transparent hover:bg-gray-100'
                    }`}
                  >
                    <div className="flex items-center justify-between mb-1">
                      <div
                        onClick={() => setSelectedKeyId(keyDef.id)}
                        className="flex-1 cursor-pointer"
                      >
                        <div className="flex items-center gap-2">
                          <span className="font-medium text-sm text-gray-900">{keyDef.name}</span>
                          {keyDef.isPrimary && (
                            <span className="px-1.5 py-0.5 text-xs font-medium bg-blue-100 text-blue-800 rounded">
                              Primary
                            </span>
                          )}
                        </div>
                        <div className="text-xs text-gray-500">
                          {keyDef.keyFields.length} field{keyDef.keyFields.length !== 1 ? 's' : ''}
                        </div>
                        {hasKeyErrors(keyDef.id) && (
                          <div className="text-xs text-red-600 mt-1">⚠ Validation errors</div>
                        )}
                      </div>
                      {!isReadOnly && (
                        <button
                          onClick={(e) => {
                            e.stopPropagation()
                            if (confirm(`Remove key definition '${keyDef.name}'?`)) {
                              removeKeyMutation.mutate(keyDef.id)
                            }
                          }}
                          className="ml-2 px-2 py-1 text-xs text-red-600 hover:bg-red-50 rounded"
                          title="Remove key"
                        >
                          ✕
                        </button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Right: Key Editor */}
        <div className="flex-1 flex flex-col overflow-hidden">
          {selectedKey ? (
            <KeyEditor
              key={selectedKey.id}
              keyDefinition={selectedKey}
              availableFields={availableFields}
              isReadOnly={isReadOnly}
              expertMode={expertMode}
              onAddField={handleAddField}
              onRemoveField={handleRemoveField}
              onReorder={handleReorder}
              validationErrors={validationErrors}
            />
          ) : (
            <div className="flex-1 flex items-center justify-center text-gray-500">
              Select a key to edit
            </div>
          )}
        </div>
      </div>
    </DndProvider>
  )
}

interface KeyEditorProps {
  keyDefinition: KeyDefinitionSummary
  availableFields: FieldDefinitionSummary[]
  isReadOnly: boolean
  expertMode: boolean
  onAddField: (fieldPath: string) => void
  onRemoveField: (fieldId: string) => void
  onReorder: (draggedFieldId: string, targetFieldId: string) => void
  validationErrors: string[]
}

function KeyEditor({
  keyDefinition,
  availableFields,
  isReadOnly,
  expertMode,
  onAddField,
  onRemoveField,
  onReorder,
  validationErrors,
}: KeyEditorProps) {
  const sortedFields = useMemo(
    () => [...keyDefinition.keyFields].sort((a, b) => a.order - b.order),
    [keyDefinition.keyFields]
  )

  return (
    <div className="flex-1 flex flex-col">
      <div className="p-4 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="font-semibold text-gray-900">{keyDefinition.name}</h3>
            {keyDefinition.isPrimary && (
              <span className="inline-block mt-1 px-2 py-1 text-xs font-medium bg-blue-100 text-blue-800 rounded">
                Primary Key
              </span>
            )}
          </div>
        </div>
      </div>

      <div className="flex-1 flex overflow-hidden">
        {/* Left: Field Picker */}
        <div className="w-64 bg-gray-50 border-r border-gray-200 p-4 overflow-y-auto">
          <h4 className="text-sm font-medium text-gray-700 mb-3">Available Fields</h4>
          {availableFields.length === 0 ? (
            <div className="text-xs text-gray-500">
              {keyDefinition.keyFields.length === 0
                ? 'No scalar fields available'
                : 'All available fields are already in this key'}
            </div>
          ) : (
            <div className="space-y-1">
              {availableFields.map((field) => (
                <button
                  key={field.id}
                  onClick={() => onAddField(field.path)}
                  disabled={isReadOnly}
                  className="w-full text-left px-2 py-1.5 text-xs bg-white border border-gray-200 rounded hover:bg-loom-50 hover:border-loom-300 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <div className="font-mono">{field.path}</div>
                  <div className="text-gray-500">{field.scalarType}</div>
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Right: Key Field List */}
        <div className="flex-1 p-4 overflow-y-auto">
          <h4 className="text-sm font-medium text-gray-700 mb-3">Key Fields (in order)</h4>
          {sortedFields.length === 0 ? (
            <div className="text-center py-12 bg-white rounded-lg border border-gray-200">
              <p className="text-sm text-gray-500 mb-4">No fields in this key</p>
              <p className="text-xs text-gray-400">Select fields from the left panel to add them</p>
            </div>
          ) : (
            <div className="space-y-2">
              {sortedFields.map((keyField, index) => (
                <DraggableKeyField
                  key={keyField.id}
                  keyField={keyField}
                  index={index}
                  isReadOnly={isReadOnly}
                  expertMode={expertMode}
                  onRemove={onRemoveField}
                  onReorder={onReorder}
                  validationErrors={validationErrors}
                />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

interface DraggableKeyFieldProps {
  keyField: { id: string; fieldPath: string; order: number; normalization: string | null }
  index: number
  isReadOnly: boolean
  expertMode: boolean
  onRemove: (fieldId: string) => void
  onReorder: (draggedFieldId: string, targetFieldId: string) => void
  validationErrors: string[]
}

function DraggableKeyField({
  keyField,
  index,
  isReadOnly,
  expertMode,
  onRemove,
  onReorder,
  validationErrors,
}: DraggableKeyFieldProps) {
  const hasError = validationErrors.some((err) => err.includes(keyField.fieldPath))

  const [{ isDragging }, drag] = useDrag({
    type: 'keyField',
    item: { id: keyField.id, index },
    canDrag: !isReadOnly,
    collect: (monitor) => ({
      isDragging: monitor.isDragging(),
    }),
  })

  const [, drop] = useDrop({
    accept: 'keyField',
    hover: (draggedItem: { id: string; index: number }) => {
      if (draggedItem.id === keyField.id) return
      onReorder(draggedItem.id, keyField.id)
      draggedItem.index = index
    },
  })

  return (
    <div
      ref={(node) => drag(drop(node))}
      className={`p-3 bg-white border-2 rounded-lg flex items-center justify-between ${
        hasError
          ? 'border-red-300 bg-red-50'
          : isDragging
          ? 'border-loom-400 opacity-50'
          : 'border-gray-200 hover:border-gray-300'
      } ${isReadOnly ? 'cursor-default' : 'cursor-move'}`}
    >
      <div className="flex items-center gap-3 flex-1">
        <div className="text-gray-400 text-sm font-mono">{index + 1}.</div>
        <div className="flex-1">
          <div className="font-mono text-sm text-gray-900">{keyField.fieldPath}</div>
          {expertMode && keyField.normalization && (
            <div className="text-xs text-gray-500 mt-0.5">
              Normalization: {keyField.normalization}
            </div>
          )}
          {hasError && (
            <div className="text-xs text-red-600 mt-1">
              {validationErrors.find((err) => err.includes(keyField.fieldPath))}
            </div>
          )}
        </div>
      </div>
      {!isReadOnly && (
        <button
          onClick={() => onRemove(keyField.id)}
          className="ml-2 px-2 py-1 text-xs text-red-600 hover:bg-red-50 rounded"
        >
          ✕
        </button>
      )}
    </div>
  )
}
