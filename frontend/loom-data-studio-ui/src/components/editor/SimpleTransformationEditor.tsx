import { useState, useMemo } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { transformationSpecsApi, schemasApi } from '../../api/masterdata'
import { NestedTransformationSelector } from './NestedTransformationSelector'
import type { SchemaRole, TransformationSpecDetails, DataSchemaSummary } from '../../types'

interface SimpleTransformationEditorProps {
  schemaId: string
  role: SchemaRole
  isReadOnly: boolean
  expertMode: boolean
  onRuleSelect: (ruleId: string | null) => void
  selectedRuleId: string | null
}

export function SimpleTransformationEditor({
  schemaId,
  role,
  isReadOnly,
  expertMode,
  onRuleSelect,
  selectedRuleId,
}: SimpleTransformationEditorProps) {
  const queryClient = useQueryClient()
  const [showAddRule, setShowAddRule] = useState(false)
  const [newSourcePath, setNewSourcePath] = useState('')
  const [newTargetPath, setNewTargetPath] = useState('')
  const [newConverterId, setNewConverterId] = useState<string | null>(null)
  const [newRequired, setNewRequired] = useState(false)
  const [targetSchemaId, setTargetSchemaId] = useState<string | null>(null)
  const [showTargetSchemaSelect, setShowTargetSchemaSelect] = useState(false)

  // Fetch transformation spec by source schema ID
  const { data: transformationSpec, isLoading, refetch } = useQuery<TransformationSpecDetails | null>({
    queryKey: ['transformationSpec', schemaId],
    queryFn: async () => {
      try {
        return await transformationSpecsApi.getTransformationSpecBySourceSchemaId(schemaId)
      } catch {
        return null
      }
    },
  })

  // Fetch available target schemas (opposite role)
  const targetRole: SchemaRole = role === 'Incoming' ? 'Master' : role === 'Master' ? 'Outgoing' : 'Master'
  const { data: availableTargetSchemas } = useQuery<DataSchemaSummary[]>({
    queryKey: ['schemas', targetRole],
    queryFn: () => schemasApi.getSchemas(targetRole),
    enabled: showTargetSchemaSelect || !transformationSpec,
  })

  // Fetch source schema details to get available fields
  const { data: sourceSchemaDetails } = useQuery({
    queryKey: ['schemaDetails', schemaId],
    queryFn: () => schemasApi.getSchemaDetails(schemaId),
  })

  // Fetch target schema details to get available fields
  const { data: targetSchemaDetails } = useQuery({
    queryKey: ['schemaDetails', targetSchemaId || transformationSpec?.targetSchemaId],
    queryFn: () => schemasApi.getSchemaDetails(targetSchemaId || transformationSpec?.targetSchemaId!),
    enabled: !!(targetSchemaId || transformationSpec?.targetSchemaId),
  })

  const createSpecMutation = useMutation({
    mutationFn: (targetId: string) =>
      transformationSpecsApi.createTransformationSpec(
        schemaId,
        targetId,
        'Simple',
        'OneToOne'
      ),
    onSuccess: () => {
      refetch()
      setShowTargetSchemaSelect(false)
    },
  })

  const addRuleMutation = useMutation({
    mutationFn: () => {
      if (!transformationSpec) throw new Error('Transformation spec not found')
      const nextOrder = transformationSpec.simpleRules.length
      return transformationSpecsApi.addSimpleTransformRule(
        transformationSpec.id,
        newSourcePath,
        newTargetPath,
        newConverterId,
        newRequired,
        nextOrder
      )
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transformationSpec', schemaId] })
      setShowAddRule(false)
      setNewSourcePath('')
      setNewTargetPath('')
      setNewConverterId(null)
      setNewRequired(false)
    },
  })

  const removeRuleMutation = useMutation({
    mutationFn: (ruleId: string) => transformationSpecsApi.removeSimpleTransformRule(ruleId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transformationSpec', schemaId] })
      if (selectedRuleId) {
        onRuleSelect(null)
      }
    },
  })

  const availableSourceFields = useMemo(() => {
    if (!sourceSchemaDetails) return []
    return sourceSchemaDetails.fields.map((f) => f.path)
  }, [sourceSchemaDetails])

  const availableTargetFields = useMemo(() => {
    if (!targetSchemaDetails) return []
    return targetSchemaDetails.fields.map((f) => f.path)
  }, [targetSchemaDetails])

  // Get field details for selected source/target paths
  const selectedSourceField = useMemo(() => {
    if (!sourceSchemaDetails || !newSourcePath) return null
    return sourceSchemaDetails.fields.find((f) => f.path === newSourcePath) || null
  }, [sourceSchemaDetails, newSourcePath])

  const selectedTargetField = useMemo(() => {
    if (!targetSchemaDetails || !newTargetPath) return null
    return targetSchemaDetails.fields.find((f) => f.path === newTargetPath) || null
  }, [targetSchemaDetails, newTargetPath])

  // Check if current rule has nested transformation reference
  const currentRuleReference = useMemo(() => {
    if (!transformationSpec || !newSourcePath || !newTargetPath) return null
    return transformationSpec.references.find(
      (r) => r.sourceFieldPath === newSourcePath && r.targetFieldPath === newTargetPath
    ) || null
  }, [transformationSpec, newSourcePath, newTargetPath])

  if (isLoading) {
    return <div className="p-6">Loading transformation spec...</div>
  }

  if (!transformationSpec && !isReadOnly) {
    return (
      <div className="p-6">
        <div className="text-center py-8">
          <p className="text-gray-500 mb-4">No transformation spec found for this schema.</p>
          {!showTargetSchemaSelect ? (
            <button
              onClick={() => setShowTargetSchemaSelect(true)}
              className="px-4 py-2 bg-loom-600 text-white rounded-lg hover:bg-loom-700"
            >
              Create Transformation Spec
            </button>
          ) : (
            <div className="max-w-md mx-auto">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Select Target Schema ({targetRole})
              </label>
              <select
                value={targetSchemaId || ''}
                onChange={(e) => setTargetSchemaId(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-1 focus:ring-loom-500"
              >
                <option value="">-- Select target schema --</option>
                {availableTargetSchemas?.map((schema) => (
                  <option key={schema.id} value={schema.id}>
                    {schema.key} (v{schema.version}) - {schema.status}
                  </option>
                ))}
              </select>
              <div className="flex gap-2 mt-4">
                <button
                  onClick={() => {
                    if (targetSchemaId) {
                      createSpecMutation.mutate(targetSchemaId)
                    }
                  }}
                  disabled={!targetSchemaId || createSpecMutation.isPending}
                  className="flex-1 px-4 py-2 bg-loom-600 text-white rounded-lg hover:bg-loom-700 disabled:opacity-50"
                >
                  {createSpecMutation.isPending ? 'Creating...' : 'Create'}
                </button>
                <button
                  onClick={() => {
                    setShowTargetSchemaSelect(false)
                    setTargetSchemaId(null)
                  }}
                  className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
                >
                  Cancel
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    )
  }

  if (!transformationSpec) {
    return <div className="p-6">No transformation spec available.</div>
  }


  return (
    <div className="h-full flex flex-col">
      <div className="p-4 border-b border-gray-200">
        <h3 className="font-semibold text-gray-900 mb-2">Transformation Rules</h3>
        {!isReadOnly && (
          <button
            onClick={() => setShowAddRule(true)}
            className="w-full px-3 py-2 text-sm bg-loom-600 text-white rounded-lg hover:bg-loom-700 transition-colors"
          >
            ➕ Add Rule
          </button>
        )}
      </div>

      {showAddRule && !isReadOnly && (
        <div className="p-4 border-b border-gray-200 bg-gray-50">
          <h4 className="text-sm font-medium mb-3">Add Transformation Rule</h4>
          <div className="space-y-2">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Source Field
              </label>
              <select
                value={newSourcePath}
                onChange={(e) => setNewSourcePath(e.target.value)}
                className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-loom-500"
              >
                <option value="">-- Select source field --</option>
                {availableSourceFields.map((path: string) => {
                  const field = sourceSchemaDetails?.fields.find(f => f.path === path)
                  const typeLabel = field ? ` (${field.fieldType})` : ''
                  return (
                    <option key={path} value={path}>
                      {path}{typeLabel}
                    </option>
                  )
                })}
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Target Field
              </label>
              <select
                value={newTargetPath}
                onChange={(e) => setNewTargetPath(e.target.value)}
                className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-loom-500"
              >
                <option value="">-- Select target field --</option>
                {availableTargetFields.map((path: string) => {
                  const field = targetSchemaDetails?.fields.find(f => f.path === path)
                  const typeLabel = field ? ` (${field.fieldType})` : ''
                  return (
                    <option key={path} value={path}>
                      {path}{typeLabel}
                    </option>
                  )
                })}
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Converter (optional)
              </label>
              <input
                type="text"
                value={newConverterId || ''}
                onChange={(e) => setNewConverterId(e.target.value || null)}
                className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-loom-500"
                placeholder="Converter ID"
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={newRequired}
                onChange={(e) => setNewRequired(e.target.checked)}
                className="rounded"
              />
              <label className="text-xs text-gray-700">Required</label>
            </div>

            {/* Type Compatibility Warning */}
            {selectedSourceField && selectedTargetField && (
              (selectedSourceField.fieldType === 'Object' || selectedSourceField.fieldType === 'Array') &&
              selectedTargetField.fieldType === 'Scalar' ? (
                <div className="mt-2 p-2 bg-red-50 border border-red-200 rounded text-xs text-red-800">
                  ⚠️ Invalid mapping: {selectedSourceField.fieldType} field cannot map to Scalar field. 
                  {selectedSourceField.fieldType} fields must map to {selectedSourceField.fieldType} fields with a nested transformation.
                </div>
              ) : selectedSourceField.fieldType === 'Scalar' &&
                (selectedTargetField.fieldType === 'Object' || selectedTargetField.fieldType === 'Array') ? (
                <div className="mt-2 p-2 bg-red-50 border border-red-200 rounded text-xs text-red-800">
                  ⚠️ Invalid mapping: Scalar field cannot map to {selectedTargetField.fieldType} field. 
                  Scalar fields can only map to Scalar fields.
                </div>
              ) : null
            )}

            {/* Nested Transformation Selector */}
            {selectedSourceField && selectedTargetField && transformationSpec ? (
              <NestedTransformationSelector
                transformationSpecId={transformationSpec.id}
                sourceFieldPath={newSourcePath}
                targetFieldPath={newTargetPath}
                sourceField={selectedSourceField}
                targetField={selectedTargetField}
                existingReferenceId={currentRuleReference?.id || null}
                isReadOnly={isReadOnly}
                expertMode={expertMode}
                onReferenceAdded={() => {
                  queryClient.invalidateQueries({ queryKey: ['transformationSpec', schemaId] })
                }}
              />
            ) : (
              <div className="text-xs text-gray-400 mt-2">
                {!selectedSourceField && 'Select source field'}
                {!selectedTargetField && 'Select target field'}
                {!transformationSpec && 'Loading transformation spec...'}
              </div>
            )}

            <div className="flex gap-2">
              <button
                onClick={() => addRuleMutation.mutate()}
                disabled={!newSourcePath || !newTargetPath || addRuleMutation.isPending}
                className="flex-1 px-2 py-1 text-xs bg-loom-600 text-white rounded hover:bg-loom-700 disabled:opacity-50"
              >
                Add
              </button>
              <button
                onClick={() => {
                  setShowAddRule(false)
                  setNewSourcePath('')
                  setNewTargetPath('')
                  setNewConverterId(null)
                  setNewRequired(false)
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
        {transformationSpec.simpleRules.length === 0 ? (
          <div className="text-center py-8 text-sm text-gray-500">
            No rules defined
          </div>
        ) : (
          <div className="space-y-1">
            {transformationSpec.simpleRules.map((rule) => (
              <div
                key={rule.id}
                className={`p-2 rounded text-sm transition-colors ${
                  selectedRuleId === rule.id
                    ? 'bg-loom-100 text-loom-900'
                    : 'hover:bg-gray-100 text-gray-700'
                }`}
              >
                <div className="flex items-center justify-between">
                  <div
                    onClick={() => onRuleSelect(rule.id)}
                    className="flex-1 cursor-pointer"
                  >
                    <div className="font-medium font-mono text-xs">{rule.sourcePath}</div>
                    <div className="text-xs text-gray-500">→ {rule.targetPath}</div>
                  </div>
                  {!isReadOnly && (
                    <button
                      onClick={(e) => {
                        e.stopPropagation()
                        if (confirm(`Remove transformation rule '${rule.sourcePath} → ${rule.targetPath}'?`)) {
                          removeRuleMutation.mutate(rule.id)
                        }
                      }}
                      className="ml-2 px-2 py-1 text-xs text-red-600 hover:bg-red-50 rounded"
                      title="Remove rule"
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
