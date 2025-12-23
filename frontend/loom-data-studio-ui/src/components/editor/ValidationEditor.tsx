import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { validationSpecsApi } from '../../api/masterdata'
import type { ValidationSpecDetails } from '../../types'

interface ValidationEditorProps {
  schemaId: string
  isReadOnly: boolean
  expertMode: boolean
  onRuleSelect: (ruleId: string | null) => void
  selectedRuleId: string | null
  onAddRuleClick: () => void // Callback to trigger create mode in center panel
}

export function ValidationEditor({
  schemaId,
  isReadOnly,
  expertMode: _expertMode,
  onRuleSelect,
  selectedRuleId,
  onAddRuleClick,
}: ValidationEditorProps) {
  const queryClient = useQueryClient()

  const removeRuleMutation = useMutation({
    mutationFn: (ruleId: string) => validationSpecsApi.removeValidationRule(ruleId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['validationSpec', schemaId] })
      if (selectedRuleId) {
        onRuleSelect(null)
      }
    },
  })

  const { data: validationSpec, isLoading, refetch } = useQuery<ValidationSpecDetails | null>({
    queryKey: ['validationSpec', schemaId],
    queryFn: async () => {
      try {
        return await validationSpecsApi.getValidationSpecBySchemaId(schemaId)
      } catch (error: any) {
        if (error?.status === 404) {
          return null
        }
        throw error
      }
    },
  })


  const createSpecMutation = useMutation({
    mutationFn: () => validationSpecsApi.createValidationSpec(schemaId),
    onSuccess: () => {
      refetch()
    },
  })

  if (isLoading) {
    return <div className="p-6">Loading validation spec...</div>
  }

  if (!validationSpec && !isReadOnly) {
    return (
      <div className="p-6">
        <div className="text-center py-8">
          <p className="text-gray-500 mb-4">No validation spec found for this schema.</p>
          <button
            onClick={() => createSpecMutation.mutate()}
            disabled={createSpecMutation.isPending}
            className="px-4 py-2 bg-loom-600 text-white rounded-lg hover:bg-loom-700 disabled:opacity-50"
          >
            {createSpecMutation.isPending ? 'Creating...' : 'Create Validation Spec'}
          </button>
        </div>
      </div>
    )
  }

  if (!validationSpec) {
    return <div className="p-6">No validation spec available.</div>
  }


  return (
    <div className="h-full flex flex-col">
      <div className="p-4 border-b border-gray-200">
        <h3 className="font-semibold text-gray-900 mb-2">Validation Rules</h3>
        {!isReadOnly && (
          <button
            onClick={onAddRuleClick}
            className="w-full px-3 py-2 text-sm bg-loom-600 text-white rounded-lg hover:bg-loom-700 transition-colors"
          >
            ➕ Add Rule
          </button>
        )}
      </div>

      <div className="flex-1 overflow-y-auto p-2">
        {!validationSpec?.rules || validationSpec.rules.length === 0 ? (
          <div className="text-center py-8 text-sm text-gray-500">
            No rules defined
          </div>
        ) : (
          <div className="space-y-1">
            {validationSpec?.rules?.map((rule) => (
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
                    <div className="font-medium">{rule.ruleType}</div>
                    <div className="text-xs text-gray-500">{rule.severity}</div>
                  </div>
                  {!isReadOnly && (
                    <button
                      onClick={(e) => {
                        e.stopPropagation()
                        if (confirm(`Remove validation rule '${rule.ruleType}'?`)) {
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

