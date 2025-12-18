import { useState, useMemo } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { schemasApi } from '../../api/masterdata'
import type { FieldDefinitionSummary, SchemaRole } from '../../types'

interface SchemaStructureEditorProps {
  schemaId: string
  fields: FieldDefinitionSummary[]
  isReadOnly: boolean
  onFieldSelect: (fieldId: string | null) => void
  selectedFieldId: string | null
  schemaRole: string // Role of the current schema to filter available schemas
}

export function SchemaStructureEditor({
  schemaId,
  fields,
  isReadOnly,
  onFieldSelect,
  selectedFieldId,
  schemaRole,
}: SchemaStructureEditorProps) {
  const queryClient = useQueryClient()
  const [showAddField, setShowAddField] = useState(false)
  const [newPath, setNewPath] = useState('')
  const [newFieldType, setNewFieldType] = useState<'Scalar' | 'Object' | 'Array'>('Scalar')
  const [newScalarType, setNewScalarType] = useState<string>('String')
  const [newElementSchemaKey, setNewElementSchemaKey] = useState<string>('')
  const [newElementSchemaId, setNewElementSchemaId] = useState<string | null>(null)
  const [showSchemaAutocomplete, setShowSchemaAutocomplete] = useState(false)
  const [newRequired, setNewRequired] = useState(false)
  const [newDescription, setNewDescription] = useState('')

  // Fetch available schemas for autocomplete (Draft and Published schemas of the same role)
  // When editing, we can reference both Draft and Published schemas
  const { data: availableSchemas } = useQuery({
    queryKey: ['schemas', schemaRole],
    queryFn: async () => {
      const allSchemas = await schemasApi.getSchemas(schemaRole as SchemaRole)
      // Filter to only Draft and Published (exclude Archived)
      return allSchemas.filter(s => s.status === 'Draft' || s.status === 'Published')
    },
    enabled: newFieldType !== 'Scalar' && showSchemaAutocomplete,
  })

  const filteredSchemas = useMemo(() => {
    if (!availableSchemas || !newElementSchemaKey) return []
    const searchTerm = newElementSchemaKey.toLowerCase()
    return availableSchemas
      .filter((s) => s.key.toLowerCase().includes(searchTerm))
      .slice(0, 10) // Limit to 10 results
  }, [availableSchemas, newElementSchemaKey])

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
      setShowAddField(false)
      setNewPath('')
      setNewFieldType('Scalar')
      setNewScalarType('String')
      setNewElementSchemaKey('')
      setNewElementSchemaId(null)
      setShowSchemaAutocomplete(false)
      setNewRequired(false)
      setNewDescription('')
    },
  })

  const removeFieldMutation = useMutation({
    mutationFn: (fieldId: string) => schemasApi.removeField(fieldId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schemaDetails', schemaId] })
      if (selectedFieldId) {
        onFieldSelect(null)
      }
    },
    onError: (error) => {
      console.error('Error removing field:', error)
      alert(`Failed to remove field: ${error.message}`)
    },
  })

  const buildTree = (fields: FieldDefinitionSummary[]) => {
    const tree: Array<{ field: FieldDefinitionSummary; children: Array<{ field: FieldDefinitionSummary; children: any[] }> }> = []
    const fieldMap = new Map<string, FieldDefinitionSummary>()
    const childrenMap = new Map<string, FieldDefinitionSummary[]>()

    fields.forEach((field) => {
      fieldMap.set(field.path, field)
      const parts = field.path.split('.')
      if (parts.length === 1) {
        if (!childrenMap.has(field.path)) {
          childrenMap.set(field.path, [])
        }
        tree.push({ field, children: [] })
      } else {
        const parentPath = parts.slice(0, -1).join('.')
        if (!childrenMap.has(parentPath)) {
          childrenMap.set(parentPath, [])
        }
        childrenMap.get(parentPath)!.push(field)
      }
    })

    return tree
  }

  buildTree(fields)

  return (
    <div className="h-full flex flex-col">
      <div className="p-4 border-b border-gray-200">
        <h3 className="font-semibold text-gray-900 mb-2">Schema Structure</h3>
        {!isReadOnly && (
          <button
            onClick={() => setShowAddField(true)}
            className="w-full px-3 py-2 text-sm bg-loom-600 text-white rounded-lg hover:bg-loom-700 transition-colors"
          >
            ➕ Add Field
          </button>
        )}
      </div>

      {showAddField && !isReadOnly && (
        <div className="p-4 border-b border-gray-200 bg-gray-50">
          <h4 className="text-sm font-medium mb-3">Add Field</h4>
          <div className="space-y-2">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Path
              </label>
              <input
                type="text"
                value={newPath}
                onChange={(e) => setNewPath(e.target.value)}
                className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-loom-500"
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
                className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-loom-500"
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
                  className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-loom-500"
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
                    // Find matching schema and set its ID
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
                    // Delay hiding to allow click on dropdown item
                    setTimeout(() => setShowSchemaAutocomplete(false), 200)
                  }}
                  className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-loom-500"
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
            <div className="flex gap-2">
              <button
                onClick={() => addFieldMutation.mutate()}
                disabled={!newPath.trim() || addFieldMutation.isPending || (newFieldType !== 'Scalar' && !newElementSchemaId)}
                className="flex-1 px-2 py-1 text-xs bg-loom-600 text-white rounded hover:bg-loom-700 disabled:opacity-50"
              >
                Add
              </button>
              <button
                onClick={() => {
                  setShowAddField(false)
                  setNewPath('')
                  setNewFieldType('Scalar')
                  setNewScalarType('String')
                  setNewElementSchemaKey('')
                  setNewElementSchemaId(null)
                  setShowSchemaAutocomplete(false)
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
        {fields.length === 0 ? (
          <div className="text-center py-8 text-sm text-gray-500">
            No fields defined
          </div>
        ) : (
          <div className="space-y-1">
            {fields.map((field) => (
              <div
                key={field.id}
                className={`p-2 rounded text-sm transition-colors ${
                  selectedFieldId === field.id
                    ? 'bg-loom-100 text-loom-900'
                    : 'hover:bg-gray-100 text-gray-700'
                }`}
              >
                <div className="flex items-center justify-between">
                  <div
                    onClick={() => onFieldSelect(field.id)}
                    className="flex-1 cursor-pointer"
                  >
                    <div className="flex items-center gap-2">
                      <span className="font-medium">{field.path}</span>
                      {field.required && (
                        <span className="text-xs text-red-600">*</span>
                      )}
                    </div>
                    <div className="text-xs text-gray-500 mt-0.5">
                      {field.fieldType}
                      {field.fieldType === 'Scalar' && field.scalarType && ` (${field.scalarType})`}
                    </div>
                  </div>
                  {!isReadOnly && (
                    <button
                      onClick={(e) => {
                        e.stopPropagation()
                        if (confirm(`Remove field '${field.path}'?`)) {
                          removeFieldMutation.mutate(field.id)
                        }
                      }}
                      className="ml-2 px-2 py-1 text-xs text-red-600 hover:bg-red-50 rounded"
                      title="Remove field"
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
  )
}

