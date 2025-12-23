import { useState, useMemo, useEffect } from 'react'
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query'
import { transformationSpecsApi, schemasApi } from '../../api/masterdata'
import { NestedTransformationSelector } from './NestedTransformationSelector'
import type { SimpleTransformRuleSummary, TransformReferenceSummary } from '../../types'

interface TransformationRuleInspectorProps {
  schemaId: string
  transformationSpecId: string
  targetSchemaId: string
  rule: SimpleTransformRuleSummary | null
  isReadOnly: boolean
  expertMode: boolean
  onClose: () => void
}

export function TransformationRuleInspector({
  schemaId,
  transformationSpecId,
  targetSchemaId,
  rule,
  isReadOnly,
  expertMode,
  onClose,
}: TransformationRuleInspectorProps) {
  const queryClient = useQueryClient()
  const isCreateMode = rule === null
  
  // Create mode state
  const [newSourcePath, setNewSourcePath] = useState('')
  const [newTargetPath, setNewTargetPath] = useState('')
  const [newConverterId, setNewConverterId] = useState<string | null>(null)
  const [newRequired, setNewRequired] = useState(false)

  // Edit mode state
  const [isEditing, setIsEditing] = useState(false)
  const [editedSourcePath, setEditedSourcePath] = useState('')
  const [editedTargetPath, setEditedTargetPath] = useState('')
  const [editedConverterId, setEditedConverterId] = useState<string | null>(null)
  const [editedRequired, setEditedRequired] = useState(false)

  // Fetch source and target schema details for field dropdowns
  const { data: sourceSchemaDetails } = useQuery({
    queryKey: ['schemaDetails', schemaId],
    queryFn: () => schemasApi.getSchemaDetails(schemaId),
  })

  const { data: targetSchemaDetails } = useQuery({
    queryKey: ['schemaDetails', targetSchemaId],
    queryFn: () => schemasApi.getSchemaDetails(targetSchemaId),
    enabled: !!targetSchemaId,
  })

  // Fetch transformation spec to get references
  const { data: transformationSpec } = useQuery({
    queryKey: ['transformationSpec', transformationSpecId],
    queryFn: () => transformationSpecsApi.getTransformationSpecDetails(transformationSpecId),
    enabled: !!transformationSpecId,
  })

  const availableSourceFields = useMemo(() => {
    if (!sourceSchemaDetails) return []
    return sourceSchemaDetails.fields.map((f) => f.path)
  }, [sourceSchemaDetails])

  const availableTargetFields = useMemo(() => {
    if (!targetSchemaDetails) return []
    return targetSchemaDetails.fields.map((f) => f.path)
  }, [targetSchemaDetails])

  // Find source and target fields for the current rule (or create mode)
  const sourceField = useMemo(() => {
    if (!sourceSchemaDetails) return null
    if (isCreateMode) {
      return sourceSchemaDetails.fields.find((f) => f.path === newSourcePath) || null
    }
    if (!rule) return null
    return sourceSchemaDetails.fields.find((f) => f.path === rule.sourcePath) || null
  }, [sourceSchemaDetails, rule, isCreateMode, newSourcePath])

  const targetField = useMemo(() => {
    if (!targetSchemaDetails) return null
    if (isCreateMode) {
      return targetSchemaDetails.fields.find((f) => f.path === newTargetPath) || null
    }
    if (!rule) return null
    return targetSchemaDetails.fields.find((f) => f.path === rule.targetPath) || null
  }, [targetSchemaDetails, rule, isCreateMode, newTargetPath])

  // Find existing transform reference for this rule
  const ruleReference = useMemo(() => {
    if (!transformationSpec) return null
    if (isCreateMode) {
      return transformationSpec.references?.find(
        (r: TransformReferenceSummary) => r.sourceFieldPath === newSourcePath && r.targetFieldPath === newTargetPath
      ) || null
    }
    if (!rule) return null
    return transformationSpec.references?.find(
      (r: TransformReferenceSummary) => r.sourceFieldPath === rule.sourcePath && r.targetFieldPath === rule.targetPath
    ) || null
  }, [transformationSpec, rule, isCreateMode, newSourcePath, newTargetPath])

  // Determine if mapping is allowed in Simple mode
  const isAllowedInSimpleMode = useMemo(() => {
    if (!sourceField || !targetField) return true

    const sourceIsScalarArray = sourceField.fieldType === 'Array' && sourceField.scalarType
    const sourceIsObjectArray = sourceField.fieldType === 'Array' && sourceField.elementSchemaId
    const targetIsObjectArray = targetField.fieldType === 'Array' && targetField.elementSchemaId

    // Allowed in Simple mode:
    // - scalar[] → scalar[]
    // - object[] → scalar[] (field extraction)
    // - object[] → object[] (same schema)
    // - scalar → scalar
    // - object → object (with TransformReference)

    // Blocked: scalar[] → object[] or object
    if (sourceIsScalarArray && (targetIsObjectArray || targetField.fieldType === 'Object')) {
      return false
    }

    // Blocked: object[] → object[] (different schema) without reference
    if (sourceIsObjectArray && targetIsObjectArray) {
      if (sourceField.elementSchemaId !== targetField.elementSchemaId && !ruleReference) {
        return false
      }
    }

    return true
  }, [sourceField, targetField, ruleReference])

  // Initialize form when rule changes or editing starts
  useEffect(() => {
    if (rule && !isCreateMode) {
      setEditedSourcePath(rule.sourcePath)
      setEditedTargetPath(rule.targetPath)
      setEditedConverterId(rule.converterId)
      setEditedRequired(rule.required)
    } else if (isCreateMode) {
      // Reset create form
      setNewSourcePath('')
      setNewTargetPath('')
      setNewConverterId(null)
      setNewRequired(false)
    }
  }, [rule, isCreateMode])

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
      onClose() // Close create mode
    },
  })

  const updateRuleMutation = useMutation({
    mutationFn: () => {
      if (!rule) throw new Error('No rule selected')
      return transformationSpecsApi.updateSimpleTransformRule(
        rule.id,
        editedSourcePath !== rule.sourcePath ? editedSourcePath : undefined,
        editedTargetPath !== rule.targetPath ? editedTargetPath : undefined,
        editedConverterId !== rule.converterId ? editedConverterId : undefined,
        editedRequired !== rule.required ? editedRequired : undefined
      )
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transformationSpec', schemaId] })
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
        setEditedSourcePath(rule.sourcePath)
        setEditedTargetPath(rule.targetPath)
        setEditedConverterId(rule.converterId)
        setEditedRequired(rule.required)
      }
    }
  }

  return (
    <div className="h-full flex flex-col">
      <div className="p-4 border-b border-gray-200 flex items-center justify-between">
        <h3 className="font-semibold text-gray-900">
          {isCreateMode ? 'Add Transformation Rule' : 'Rule Details'}
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
                    ? !newSourcePath || !newTargetPath || addRuleMutation.isPending || !isAllowedInSimpleMode
                    : updateRuleMutation.isPending || !isAllowedInSimpleMode)
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
                Source Field
              </label>
              <select
                value={newSourcePath}
                onChange={(e) => setNewSourcePath(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
              >
                <option value="">-- Select source field --</option>
                {availableSourceFields.map((path) => {
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
                className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
              >
                <option value="">-- Select target field --</option>
                {availableTargetFields.map((path) => {
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
                className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
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
            {sourceField && targetField && (
              <>
                {!isAllowedInSimpleMode && (
                  <div className="mt-2 p-3 bg-yellow-50 border border-yellow-300 rounded text-xs text-yellow-900">
                    <div className="font-semibold mb-1">⚠️ This mapping requires an Advanced transformation</div>
                    <div className="mb-2 text-yellow-800">
                      {sourceField.fieldType === 'Array' && sourceField.scalarType && 
                       (targetField.fieldType === 'Array' && targetField.elementSchemaId || targetField.fieldType === 'Object') ? (
                        <span>Scalar arrays cannot be mapped to object arrays or objects in Simple mode. Use the Advanced editor to configure a TransformReference.</span>
                      ) : sourceField.fieldType === 'Array' && sourceField.elementSchemaId &&
                        targetField.fieldType === 'Array' && targetField.elementSchemaId &&
                        sourceField.elementSchemaId !== targetField.elementSchemaId ? (
                        <span>Object arrays with different schemas require a TransformReference. Use the Advanced editor to configure nested transformations.</span>
                      ) : null}
                    </div>
                    <button
                      type="button"
                      onClick={() => {
                        // TODO: Navigate to Advanced editor or show message
                        alert('Please switch to Advanced mode to configure this transformation.')
                      }}
                      className="px-3 py-1 bg-yellow-600 text-white rounded hover:bg-yellow-700 text-xs"
                    >
                      Open Advanced Editor
                    </button>
                  </div>
                )}
                {((sourceField.fieldType === 'Object' || sourceField.fieldType === 'Array') &&
                targetField.fieldType === 'Scalar' && !targetField.scalarType) ? (
                  <div className="mt-2 p-2 bg-red-50 border border-red-200 rounded text-xs text-red-800">
                    ⚠️ Invalid mapping: {sourceField.fieldType} field cannot map to Scalar field. Use a scalar array target for field extraction.
                  </div>
                ) : sourceField.fieldType === 'Scalar' &&
                  (targetField.fieldType === 'Object' || (targetField.fieldType === 'Array' && targetField.elementSchemaId)) ? (
                  <div className="mt-2 p-2 bg-red-50 border border-red-200 rounded text-xs text-red-800">
                    ⚠️ Invalid mapping: Scalar field cannot map to {targetField.fieldType} field. This requires an Advanced transformation.
                  </div>
                ) : null}
              </>
            )}
            {/* Nested Transformation Selector */}
            {sourceField && targetField && transformationSpec && (
              <NestedTransformationSelector
                transformationSpecId={transformationSpec.id}
                sourceFieldPath={newSourcePath}
                targetFieldPath={newTargetPath}
                sourceField={sourceField}
                targetField={targetField}
                existingReferenceId={ruleReference?.id || null}
                isReadOnly={isReadOnly}
                expertMode={expertMode}
                onReferenceAdded={() => {
                  queryClient.invalidateQueries({ queryKey: ['transformationSpec', schemaId] })
                }}
              />
            )}
          </>
        ) : (
          // Edit/view form
          <>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Source Field
              </label>
              {isEditing ? (
                <select
                  value={editedSourcePath}
                  onChange={(e) => setEditedSourcePath(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
                >
                  <option value="">-- Select source field --</option>
                  {availableSourceFields.map((path) => (
                    <option key={path} value={path}>
                      {path}
                    </option>
                  ))}
                </select>
              ) : (
                <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm font-mono text-gray-900">
                  {rule.sourcePath}
                </div>
              )}
            </div>

            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Target Field
              </label>
              {isEditing ? (
                <select
                  value={editedTargetPath}
                  onChange={(e) => setEditedTargetPath(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
                >
                  <option value="">-- Select target field --</option>
                  {availableTargetFields.map((path) => (
                    <option key={path} value={path}>
                      {path}
                    </option>
                  ))}
                </select>
              ) : (
                <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm font-mono text-gray-900">
                  {rule.targetPath}
                </div>
              )}
            </div>

            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Converter
              </label>
              {isEditing ? (
                <input
                  type="text"
                  value={editedConverterId || ''}
                  onChange={(e) => setEditedConverterId(e.target.value || null)}
                  className="w-full px-3 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-loom-500"
                  placeholder="Converter ID (optional)"
                />
              ) : (
                <div className="px-3 py-2 bg-gray-50 border border-gray-200 rounded text-sm text-gray-900">
                  {rule.converterId || '-'}
                </div>
              )}
            </div>

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
                  {rule.required ? 'Yes' : 'No'}
                </div>
              )}
            </div>

            {/* Nested Transformation Selector */}
            {sourceField && targetField && transformationSpec && (
              <div className="mt-4">
                <NestedTransformationSelector
                  transformationSpecId={transformationSpecId}
                  sourceFieldPath={rule.sourcePath}
                  targetFieldPath={rule.targetPath}
                  sourceField={sourceField}
                  targetField={targetField}
                  existingReferenceId={ruleReference?.id || null}
                  isReadOnly={isReadOnly}
                  expertMode={expertMode}
                  onReferenceAdded={() => {
                    queryClient.invalidateQueries({ queryKey: ['transformationSpec', transformationSpecId] })
                  }}
                />
              </div>
            )}

            {isReadOnly && (
              <div className="mt-4 p-3 bg-yellow-50 border border-yellow-200 rounded text-xs text-yellow-800">
                This transformation spec is Published and cannot be modified.
              </div>
            )}
          </>
        )}
      </div>
    </div>
  )
}

