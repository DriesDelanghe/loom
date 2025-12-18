import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { validationSpecsApi } from '../../api/masterdata'
import type { RuleType, Severity, ValidationSpecDetails } from '../../types'

interface ValidationEditorProps {
  schemaId: string
  isReadOnly: boolean
  expertMode: boolean
  onRuleSelect: (ruleId: string | null) => void
  selectedRuleId: string | null
}

export function ValidationEditor({
  schemaId,
  isReadOnly,
  expertMode: _expertMode,
  onRuleSelect,
  selectedRuleId,
}: ValidationEditorProps) {
  const queryClient = useQueryClient()
  const [showAddRule, setShowAddRule] = useState(false)

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

  const addRuleMutation = useMutation({
    mutationFn: (params: { ruleType: RuleType; severity: Severity; parameters: string }) =>
      validationSpecsApi.addValidationRule(
        validationSpec!.id,
        params.ruleType,
        params.severity,
        params.parameters
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['validationSpec', schemaId] })
      setShowAddRule(false)
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
            onClick={() => setShowAddRule(true)}
            className="w-full px-3 py-2 text-sm bg-loom-600 text-white rounded-lg hover:bg-loom-700 transition-colors"
          >
            ➕ Add Rule
          </button>
        )}
      </div>

      {showAddRule && !isReadOnly && (
        <div className="p-4 border-b border-gray-200 bg-gray-50">
          <h4 className="text-sm font-medium mb-3">Add Validation Rule</h4>
          <RuleForm
            onSubmit={(rule) => addRuleMutation.mutate(rule)}
            onCancel={() => setShowAddRule(false)}
          />
        </div>
      )}

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

interface RuleFormProps {
  onSubmit: (rule: { ruleType: RuleType; severity: Severity; parameters: string }) => void
  onCancel: () => void
}

function RuleForm({ onSubmit, onCancel }: RuleFormProps) {
  const [ruleType, setRuleType] = useState<RuleType>('Field')
  const [severity, setSeverity] = useState<Severity>('Error')
  const [fieldPath, setFieldPath] = useState('')

  const handleSubmit = () => {
    const parameters = JSON.stringify({ fieldPath })
    onSubmit({ ruleType, severity, parameters })
  }

  return (
    <div className="space-y-2">
      <div>
        <label className="block text-xs font-medium text-gray-700 mb-1">
          Rule Type
        </label>
        <select
          value={ruleType}
          onChange={(e) => setRuleType(e.target.value as RuleType)}
          className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-loom-500"
        >
          <option value="Field">Field</option>
          <option value="CrossField">Cross Field</option>
          <option value="Conditional">Conditional</option>
        </select>
      </div>
      <div>
        <label className="block text-xs font-medium text-gray-700 mb-1">
          Severity
        </label>
        <select
          value={severity}
          onChange={(e) => setSeverity(e.target.value as Severity)}
          className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-loom-500"
        >
          <option value="Error">Error</option>
          <option value="Warning">Warning</option>
        </select>
      </div>
      {ruleType === 'Field' && (
        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">
            Field Path
          </label>
          <input
            type="text"
            value={fieldPath}
            onChange={(e) => setFieldPath(e.target.value)}
            className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-loom-500"
            placeholder="e.g., email"
          />
        </div>
      )}
      <div className="flex gap-2">
        <button
          onClick={handleSubmit}
          className="flex-1 px-2 py-1 text-xs bg-loom-600 text-white rounded hover:bg-loom-700"
        >
          Add
        </button>
        <button
          onClick={onCancel}
          className="flex-1 px-2 py-1 text-xs bg-gray-200 text-gray-700 rounded hover:bg-gray-300"
        >
          Cancel
        </button>
      </div>
    </div>
  )
}
