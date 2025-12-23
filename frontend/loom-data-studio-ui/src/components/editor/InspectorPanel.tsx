import { useState, useMemo, useEffect } from 'react'
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query'
import { schemasApi } from '../../api/masterdata'
import type { FieldDefinitionSummary, FieldType, ScalarType, SchemaRole } from '../../types'

interface InspectorPanelProps {
  schemaId: string
  field: FieldDefinitionSummary | null
  isReadOnly: boolean
  schemaRole: SchemaRole
  onClose: () => void
}

export function InspectorPanel({ schemaId, field, isReadOnly, schemaRole, onClose }: InspectorPanelProps) {
  const queryClient = useQueryClient()
  const isCreateMode = field === null
  
  // Create mode state
  const [newPath, setNewPath] = useState('')
  const [newFieldType, setNewFieldType] = useState<'Scalar' | 'Object' | 'Array'>('Scalar')
  const [newScalarType, setNewScalarType] = useState<string>('String')
  const [newElementSchemaKey, setNewElementSchemaKey] = useState<string>('')
  const [newElementSchemaId, setNewElementSchemaId] = useState<string | null>(null)
  const [showSchemaAutocomplete, setShowSchemaAutocomplete] = useState(false)
  const [newRequired, setNewRequired] = useState(false)
  const [newDescription, setNewDescription] = useState('')

  // Edit mode state
  const [isEditing, setIsEditing] = useState(false)
  const [editedPath, setEditedPath] = useState('')
  const [editedFieldType, setEditedFieldType] = useState<FieldType>('Scalar')
  const [editedScalarType, setEditedScalarType] = useState<ScalarType | null>(null)
  const [editedElementSchemaKey, setEditedElementSchemaKey] = useState<string>('')
  const [editedElementSchemaId, setEditedElementSchemaId] = useState<string | null>(null)
  const [showEditSchemaAutocomplete, setShowEditSchemaAutocomplete] = useState(false)
  const [editedRequired, setEditedRequired] = useState(false)
  const [editedDescription, setEditedDescription] = useState('')

  // Fetch available schemas for autocomplete (for both create and edit modes)
  const { data: availableSchemas } = useQuery({
    queryKey: ['schemas', schemaRole],
    queryFn: async () => {
      const allSchemas = await schemasApi.getSchemas(schemaRole)
      return allSchemas.filter(s => s.status === 'Draft' || s.status === 'Published')
    },
    enabled: (isCreateMode && newFieldType !== 'Scalar' && showSchemaAutocomplete) || 
             (isEditing && editedFieldType !== 'Scalar' && showEditSchemaAutocomplete),
  })

  const filteredSchemas = useMemo(() => {
    if (!availableSchemas) return []
    const searchKey = isCreateMode ? newElementSchemaKey : editedElementSchemaKey
    if (!searchKey) return []
    const searchTerm = searchKey.toLowerCase()
    return availableSchemas
      .filter((s) => s.key.toLowerCase().includes(searchTerm))
      .slice(0, 10)
  }, [availableSchemas, newElementSchemaKey, editedElementSchemaKey, isCreateMode])

  // Helper to get schema key from ID
  const getSchemaKeyById = useMemo(() => {
    return (id: string | null) => {
      if (!id || !availableSchemas) return null
      return availableSchemas.find(s => s.id === id)?.key || null
    }
  }, [availableSchemas])

  // Initialize form when field changes or editing starts
  useEffect(() => {
    if (field && !isCreateMode) {
      setEditedPath(field.path)
      setEditedFieldType(field.fieldType)
      setEditedScalarType(field.scalarType)
      setEditedElementSchemaId(field.elementSchemaId)
      setEditedRequired(field.required)
      setEditedDescription(field.description || '')
    } else if (isCreateMode) {
      // Reset create form
      setNewPath('')
      setNewFieldType('Scalar')
      setNewScalarType('String')
      setNewElementSchemaKey('')
      setNewElementSchemaId(null)
      setShowSchemaAutocomplete(false)
      setNewRequired(false)
      setNewDescription('')
    }
  }, [field, isCreateMode])

  // Update schema key when schemas load or elementSchemaId changes
  useEffect(() => {
    if (field && !isCreateMode && field.elementSchemaId && availableSchemas) {
      const schema = availableSchemas.find(s => s.id === field.elementSchemaId)
      if (schema) {
        setEditedElementSchemaKey(schema.key)
      }
    }
  }, [field, isCreateMode, availableSchemas])

  // Handle field type changes - clear opposite type values
  useEffect(() => {
    if (isEditing && !isCreateMode) {
      if (editedFieldType === 'Scalar') {
        // Switching to Scalar - clear element schema
        setEditedElementSchemaId(null)
        setEditedElementSchemaKey('')
        setShowEditSchemaAutocomplete(false)
      } else {
        // Switching to Object/Array - clear scalar type
        setEditedScalarType(null)
      }
    }
  }, [editedFieldType, isEditing, isCreateMode])

  const addFieldMutation = useMutation({
    mutationFn: () =>
      schemasApi.addField(
        schemaId,
        newPath,
        newFieldType,
        newFieldType === 'Scalar' ? newScalarType : null,
        newFieldType !== 'Scalar' ? newElementSchemaId : null,
        newRequired,
        newDescription || undefined
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schemaDetails', schemaId] })
      onClose() // Close create mode
    },
  })

  const updateFieldMutation = useMutation({
    mutationFn: () => {
      if (!field) throw new Error('No field selected')
      return schemasApi.updateField(
        field.id,
        editedPath !== field.path ? editedPath : undefined,
        editedFieldType !== field.fieldType ? editedFieldType : undefined,
        editedFieldType === 'Scalar' && editedScalarType !== field.scalarType ? editedScalarType : (editedFieldType !== 'Scalar' ? null : undefined),
        editedFieldType !== 'Scalar' && editedElementSchemaId !== field.elementSchemaId ? editedElementSchemaId : (editedFieldType === 'Scalar' ? null : undefined),
        editedRequired !== field.required ? editedRequired : undefined,
        editedDescription !== (field.description || '') ? editedDescription : undefined
      )
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schemaDetails', schemaId] })
      setIsEditing(false)
    },
  })

  const handleSave = () => {
    if (isCreateMode) {
      addFieldMutation.mutate()
    } else {
      updateFieldMutation.mutate()
    }
  }

  const handleCancel = () => {
    if (isCreateMode) {
      onClose()
    } else {
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
  }

  return (
    <div className="h-full flex flex-col">
      <div className="p-4 border-b border-gray-200 flex items-center justify-between">
        <h3 className="font-semibold text-gray-900">
          {isCreateMode ? 'Add Field' : 'Field Details'}
        </h3>
        <div className="flex items-center gap-2">
          {!isReadOnly && !isCreateMode && !isEditing && (
            <button
              onClick={() => setIsEditing(true)}
              className="px-3 py-1 text-sm text-loom-600 hover:bg-loom-50 rounded"
            >
              Edit
            </button>
          )}
          {!isReadOnly && (isCreateMode || isEditing) && (
            <>
              <button
                onClick={handleSave}
                disabled={
                  (isCreateMode
                    ? !newPath.trim() || addFieldMutation.isPending || (newFieldType !== 'Scalar' && !newElementSchemaId)
                    : updateFieldMutation.isPending)
                }
                className="px-3 py-1 text-sm bg-loom-600 text-white rounded hover:bg-loom-700 disabled:opacity-50"
              >
                {isCreateMode ? 'Add' : 'Save'}
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
        {isCreateMode ? (
          // Create form
          <>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Path
              </label>
              <input
                type="text"
                value={newPath}
                onChange={(e) => setNewPath(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
                placeholder="fieldName"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Type
              </label>
              <select
                value={newFieldType}
                onChange={(e) => setNewFieldType(e.target.value as 'Scalar' | 'Object' | 'Array')}
                className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
              >
                <option value="Scalar">Scalar</option>
                <option value="Object">Object</option>
                <option value="Array">Array</option>
              </select>
            </div>
            {newFieldType === 'Scalar' && (
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Scalar Type
                </label>
                <select
                  value={newScalarType}
                  onChange={(e) => setNewScalarType(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
                >
                  <option value="String">String</option>
                  <option value="Integer">Integer</option>
                  <option value="Decimal">Decimal</option>
                  <option value="Boolean">Boolean</option>
                  <option value="Date">Date</option>
                  <option value="DateTime">DateTime</option>
                  <option value="Time">Time</option>
                  <option value="Guid">Guid</option>
                </select>
              </div>
            )}
            {newFieldType !== 'Scalar' && (
              <div className="relative">
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Referenced Schema
                </label>
                <input
                  type="text"
                  value={newElementSchemaKey}
                  onChange={(e) => {
                    setNewElementSchemaKey(e.target.value)
                    setShowSchemaAutocomplete(true)
                    const matchingSchema = availableSchemas?.find(
                      (s) => s.key.toLowerCase() === e.target.value.toLowerCase()
                    )
                    if (matchingSchema) {
                      setNewElementSchemaId(matchingSchema.id)
                    } else {
                      setNewElementSchemaId(null)
                    }
                  }}
                  onFocus={() => setShowSchemaAutocomplete(true)}
                  onBlur={() => {
                    setTimeout(() => setShowSchemaAutocomplete(false), 200)
                  }}
                  className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
                  placeholder="Type to search for schema key..."
                />
                {showSchemaAutocomplete && filteredSchemas.length > 0 && (
                  <div className="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-md shadow-lg max-h-48 overflow-y-auto">
                    {filteredSchemas.map((schema) => (
                      <div
                        key={schema.id}
                        onClick={() => {
                          setNewElementSchemaKey(schema.key)
                          setNewElementSchemaId(schema.id)
                          setShowSchemaAutocomplete(false)
                        }}
                        className="px-3 py-2 hover:bg-loom-50 cursor-pointer text-sm"
                      >
                        <div className="font-medium text-gray-900">{schema.key}</div>
                        {schema.description && (
                          <div className="text-xs text-gray-500 truncate">{schema.description}</div>
                        )}
                        <div className="text-xs text-gray-400">v{schema.version} - {schema.status}</div>
                      </div>
                    ))}
                  </div>
                )}
                {newElementSchemaKey && !newElementSchemaId && (
                  <div className="mt-1 text-xs text-red-600">
                    Schema not found. Please select from the dropdown.
                  </div>
                )}
              </div>
            )}
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={newRequired}
                onChange={(e) => setNewRequired(e.target.checked)}
                className="rounded"
              />
              <label className="text-xs text-gray-700">Required</label>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Description
              </label>
              <textarea
                value={newDescription}
                onChange={(e) => setNewDescription(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
                rows={3}
                placeholder="Field description (optional)"
              />
            </div>
          </>
        ) : (
          // Edit/view form
          <>
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

        {(isEditing ? editedFieldType : field.fieldType) === 'Scalar' && (
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

        {(isEditing ? editedFieldType : field.fieldType) !== 'Scalar' && (
          <div className="relative">
            <label className="block text-xs font-medium text-gray-700 mb-1">
              Referenced Schema
            </label>
            {isEditing ? (
              <>
                <input
                  type="text"
                  value={editedElementSchemaKey}
                  onChange={(e) => {
                    setEditedElementSchemaKey(e.target.value)
                    setShowEditSchemaAutocomplete(true)
                    const matchingSchema = availableSchemas?.find(
                      (s) => s.key.toLowerCase() === e.target.value.toLowerCase()
                    )
                    if (matchingSchema) {
                      setEditedElementSchemaId(matchingSchema.id)
                    } else {
                      setEditedElementSchemaId(null)
                    }
                  }}
                  onFocus={() => setShowEditSchemaAutocomplete(true)}
                  onBlur={() => {
                    setTimeout(() => setShowEditSchemaAutocomplete(false), 200)
                  }}
                  className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
                  placeholder="Type to search for schema key..."
                />
                {showEditSchemaAutocomplete && filteredSchemas.length > 0 && (
                  <div className="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-md shadow-lg max-h-48 overflow-y-auto">
                    {filteredSchemas.map((schema) => (
                      <div
                        key={schema.id}
                        onClick={() => {
                          setEditedElementSchemaKey(schema.key)
                          setEditedElementSchemaId(schema.id)
                          setShowEditSchemaAutocomplete(false)
                        }}
                        className="px-3 py-2 hover:bg-loom-50 cursor-pointer text-sm"
                      >
                        <div className="font-medium text-gray-900">{schema.key}</div>
                        {schema.description && (
                          <div className="text-xs text-gray-500 truncate">{schema.description}</div>
                        )}
                        <div className="text-xs text-gray-400">v{schema.version} - {schema.status}</div>
                      </div>
                    ))}
                  </div>
                )}
                {editedElementSchemaKey && !editedElementSchemaId && (
                  <div className="mt-1 text-xs text-red-600">
                    Schema not found. Please select from the dropdown.
                  </div>
                )}
              </>
            ) : (
              <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm text-gray-900">
                {field.elementSchemaId ? (getSchemaKeyById(field.elementSchemaId) || field.elementSchemaId) : 'Not set'}
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
          </>
        )}
      </div>
    </div>
  )
}
