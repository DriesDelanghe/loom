import { useMutation, useQueryClient } from '@tanstack/react-query'
import { schemasApi } from '../../api/masterdata'
import type { FieldDefinitionSummary } from '../../types'

interface SchemaStructureEditorProps {
  schemaId: string
  fields: FieldDefinitionSummary[]
  isReadOnly: boolean
  onFieldSelect: (fieldId: string | null) => void
  selectedFieldId: string | null
  schemaRole: string // Role of the current schema to filter available schemas
  onAddFieldClick: () => void // Callback to trigger create mode in center panel
}

export function SchemaStructureEditor({
  schemaId,
  fields,
  isReadOnly,
  onFieldSelect,
  selectedFieldId,
  schemaRole: _schemaRole,
  onAddFieldClick,
}: SchemaStructureEditorProps) {
  const queryClient = useQueryClient()

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
            onClick={onAddFieldClick}
            className="w-full px-3 py-2 text-sm bg-loom-600 text-white rounded-lg hover:bg-loom-700 transition-colors"
          >
            ➕ Add Field
          </button>
        )}
      </div>

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
                      {field.fieldType === 'Array' && field.scalarType
                        ? `${field.scalarType}[]`
                        : field.fieldType === 'Array' && field.elementSchemaId
                        ? 'Object[]'
                        : field.fieldType === 'Scalar' && field.scalarType
                        ? `${field.fieldType} (${field.scalarType})`
                        : field.fieldType}
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

