import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { transformationSpecsApi, schemasApi } from '../../api/masterdata'
import type { SchemaRole, TransformationSpecDetails, DataSchemaSummary } from '../../types'

interface SimpleTransformationEditorProps {
  schemaId: string
  role: SchemaRole
  isReadOnly: boolean
  expertMode: boolean
  onRuleSelect: (ruleId: string | null) => void
  selectedRuleId: string | null
  onAddRuleClick: () => void // Callback to trigger create mode in center panel
}

export function SimpleTransformationEditor({
  schemaId,
  role,
  isReadOnly,
  expertMode: _expertMode,
  onRuleSelect,
  selectedRuleId,
  onAddRuleClick,
}: SimpleTransformationEditorProps) {
  const queryClient = useQueryClient()
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

  const removeRuleMutation = useMutation({
    mutationFn: (ruleId: string) => transformationSpecsApi.removeSimpleTransformRule(ruleId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transformationSpec', schemaId] })
      if (selectedRuleId) {
        onRuleSelect(null)
      }
    },
  })


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
            onClick={onAddRuleClick}
            className="w-full px-3 py-2 text-sm bg-loom-600 text-white rounded-lg hover:bg-loom-700 transition-colors"
          >
            ➕ Add Rule
          </button>
        )}
      </div>

      {/* Create form moved to center panel (TransformationRuleInspector) */}

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
