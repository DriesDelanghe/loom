import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { validationSpecsApi } from '../../api/masterdata'
import type { ValidationRuleSummary, RuleType, Severity } from '../../types'

interface ValidationRuleInspectorProps {
  schemaId: string
  rule: ValidationRuleSummary | null
  isReadOnly: boolean
  onClose: () => void
}

export function ValidationRuleInspector({
  schemaId,
  rule,
  isReadOnly,
  onClose,
}: ValidationRuleInspectorProps) {
  const queryClient = useQueryClient()
  const isCreateMode = rule === null
  
  // Create mode state
  const [newRuleType, setNewRuleType] = useState<RuleType>('Field')
  const [newSeverity, setNewSeverity] = useState<Severity>('Error')
  const [newFieldPath, setNewFieldPath] = useState('')

  // Edit mode state
  const [isEditing, setIsEditing] = useState(false)
  const [editedRuleType, setEditedRuleType] = useState<RuleType>('Field')
  const [editedSeverity, setEditedSeverity] = useState<Severity>('Error')
  const [editedFieldPath, setEditedFieldPath] = useState('')

  // Initialize form when rule changes or editing starts
  useEffect(() => {
    if (rule && !isCreateMode) {
      setEditedRuleType(rule.ruleType)
      setEditedSeverity(rule.severity)
      try {
        const params = JSON.parse(rule.parameters)
        setEditedFieldPath(params.fieldPath || '')
      } catch {
        setEditedFieldPath('')
      }
    } else if (isCreateMode) {
      // Reset create form
      setNewRuleType('Field')
      setNewSeverity('Error')
      setNewFieldPath('')
    }
  }, [rule, isCreateMode])

  const addRuleMutation = useMutation({
    mutationFn: () => {
      const parameters = JSON.stringify({ fieldPath: newFieldPath })
      return validationSpecsApi.addValidationRule(schemaId, newRuleType, newSeverity, parameters)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['validationSpec', schemaId] })
      onClose() // Close create mode
    },
  })

  const updateRuleMutation = useMutation({
    mutationFn: () => {
      if (!rule) throw new Error('No rule selected')
      const parameters = JSON.stringify({ fieldPath: editedFieldPath })
      return validationSpecsApi.updateValidationRule(
        rule.id,
        editedRuleType !== rule.ruleType ? editedRuleType : undefined,
        editedSeverity !== rule.severity ? editedSeverity : undefined,
        parameters !== rule.parameters ? parameters : undefined
      )
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['validationSpec', schemaId] })
      setIsEditing(false)
    },
  })

  const handleSave = () => {
    if (isCreateMode) {
      addRuleMutation.mutate()
    } else {
      updateRuleMutation.mutate()
    }
  }

  const handleCancel = () => {
    if (isCreateMode) {
      onClose()
    } else {
      setIsEditing(false)
      // Reset to original values
      if (rule) {
        setEditedRuleType(rule.ruleType)
        setEditedSeverity(rule.severity)
        try {
          const params = JSON.parse(rule.parameters)
          setEditedFieldPath(params.fieldPath || '')
        } catch {
          setEditedFieldPath('')
        }
      }
    }
  }

  let parametersObj: any = {}
  if (rule && !isCreateMode) {
    try {
      parametersObj = JSON.parse(rule.parameters)
    } catch {
      // Invalid JSON
    }
  }

  return (
    <div className="h-full flex flex-col">
      <div className="p-4 border-b border-gray-200 flex items-center justify-between">
        <h3 className="font-semibold text-gray-900">
          {isCreateMode ? 'Add Validation Rule' : 'Rule Details'}
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
                    ? !newFieldPath.trim() || addRuleMutation.isPending
                    : updateRuleMutation.isPending)
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
                Rule Type
              </label>
              <select
                value={newRuleType}
                onChange={(e) => setNewRuleType(e.target.value as RuleType)}
                className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
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
                value={newSeverity}
                onChange={(e) => setNewSeverity(e.target.value as Severity)}
                className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
              >
                <option value="Error">Error</option>
                <option value="Warning">Warning</option>
              </select>
            </div>
            {newRuleType === 'Field' && (
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Field Path
                </label>
                <input
                  type="text"
                  value={newFieldPath}
                  onChange={(e) => setNewFieldPath(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
                  placeholder="e.g., email"
                />
              </div>
            )}
            {newRuleType !== 'Field' && (
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Parameters (JSON)
                </label>
                <textarea
                  value={JSON.stringify({ fieldPath: newFieldPath }, null, 2)}
                  onChange={(e) => {
                    try {
                      const parsed = JSON.parse(e.target.value)
                      setNewFieldPath(parsed.fieldPath || '')
                    } catch {
                      // Invalid JSON, keep as is
                    }
                  }}
                  className="w-full px-3 py-2 border border-gray-300 rounded text-xs font-mono focus:outline-none focus:ring-2 focus:ring-loom-500"
                  rows={6}
                />
              </div>
            )}
          </>
        ) : (
          // Edit/view form
          <>
        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">
            Rule Type
          </label>
          {isEditing ? (
            <select
              value={editedRuleType}
              onChange={(e) => setEditedRuleType(e.target.value as RuleType)}
              className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
            >
              <option value="Field">Field</option>
              <option value="CrossField">Cross Field</option>
              <option value="Conditional">Conditional</option>
            </select>
          ) : (
            <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm text-gray-900">
              {rule.ruleType}
            </div>
          )}
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">
            Severity
          </label>
          {isEditing ? (
            <select
              value={editedSeverity}
              onChange={(e) => setEditedSeverity(e.target.value as Severity)}
              className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
            >
              <option value="Error">Error</option>
              <option value="Warning">Warning</option>
            </select>
          ) : (
            <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm text-gray-900">
              {rule.severity}
            </div>
          )}
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">
            Parameters
          </label>
          {isEditing ? (
            <div className="space-y-2">
              {editedRuleType === 'Field' && (
                <input
                  type="text"
                  value={editedFieldPath}
                  onChange={(e) => setEditedFieldPath(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
                  placeholder="Field path (e.g., email)"
                />
              )}
              {editedRuleType !== 'Field' && (
                <textarea
                  value={JSON.stringify(parametersObj, null, 2)}
                  onChange={(e) => {
                    try {
                      JSON.parse(e.target.value)
                      setEditedFieldPath('')
                    } catch {
                      // Invalid JSON, keep as is
                    }
                  }}
                  className="w-full px-3 py-2 border border-gray-300 rounded text-xs font-mono focus:outline-none focus:ring-2 focus:ring-loom-500"
                  rows={6}
                />
              )}
            </div>
          ) : (
            <pre className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-xs overflow-x-auto">
              {JSON.stringify(parametersObj, null, 2)}
            </pre>
          )}
        </div>

            {isReadOnly && (
              <div className="mt-4 p-3 bg-yellow-50 border border-yellow-200 rounded text-xs text-yellow-800">
                This validation spec is Published and cannot be modified.
              </div>
            )}
          </>
        )}
      </div>
    </div>
  )
}

