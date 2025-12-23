import { useState, useEffect, useMemo } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { transformationSpecsApi, schemasApi } from '../../api/masterdata'
import type { CompatibleTransformationSpecSummary, FieldDefinitionSummary } from '../../types'

interface NestedTransformationSelectorProps {
  transformationSpecId: string
  sourceFieldPath: string
  targetFieldPath: string
  sourceField: FieldDefinitionSummary | null
  targetField: FieldDefinitionSummary | null
  existingReferenceId: string | null
  isReadOnly: boolean
  expertMode: boolean
  onReferenceAdded: () => void
}

export function NestedTransformationSelector({
  transformationSpecId,
  sourceFieldPath,
  targetFieldPath,
  sourceField,
  targetField,
  existingReferenceId,
  isReadOnly,
  expertMode,
  onReferenceAdded,
}: NestedTransformationSelectorProps) {
  const queryClient = useQueryClient()
  const [showSelector, setShowSelector] = useState(false)
  const [selectedSpecId, setSelectedSpecId] = useState<string | null>(null)

  // Check if fields require nested transformation
  const requiresNestedTransformation = useMemo(() => {
    if (!sourceField || !targetField) return false
    const sourceIsComplex = sourceField.fieldType === 'Object' || sourceField.fieldType === 'Array'
    const targetIsComplex = targetField.fieldType === 'Object' || targetField.fieldType === 'Array'
    return sourceIsComplex && targetIsComplex
  }, [sourceField, targetField])

  // Fetch compatible transformation specs (try both Published and Draft)
  const { data: compatibleSpecs, isLoading: loadingSpecs } = useQuery<CompatibleTransformationSpecSummary[]>({
    queryKey: [
      'compatibleTransformationSpecs',
      sourceField?.elementSchemaId,
      targetField?.elementSchemaId,
    ],
    queryFn: async () => {
      if (!sourceField?.elementSchemaId || !targetField?.elementSchemaId) {
        return []
      }
      // First try Published transformations
      const published = await transformationSpecsApi.getCompatibleTransformationSpecs(
        sourceField.elementSchemaId,
        targetField.elementSchemaId,
        'Published'
      )
      // Also include Draft transformations (for development)
      const draft = await transformationSpecsApi.getCompatibleTransformationSpecs(
        sourceField.elementSchemaId,
        targetField.elementSchemaId,
        'Draft'
      )
      // Combine and deduplicate by ID
      const all = [...published, ...draft]
      const unique = all.filter((spec, index, self) => 
        index === self.findIndex(s => s.id === spec.id)
      )
      return unique
    },
    enabled: requiresNestedTransformation && !!sourceField?.elementSchemaId && !!targetField?.elementSchemaId,
  })

  // Fetch existing reference details if it exists
  const { data: transformationSpec } = useQuery({
    queryKey: ['transformationSpec', transformationSpecId],
    queryFn: () => transformationSpecsApi.getTransformationSpecDetails(transformationSpecId),
    enabled: !!transformationSpecId,
  })

  const existingReference = useMemo(() => {
    if (!transformationSpec || !existingReferenceId) return null
    return transformationSpec.references.find((r) => r.id === existingReferenceId)
  }, [transformationSpec, existingReferenceId])

  // Fetch child transformation spec details if reference exists
  const { data: childSpecDetails } = useQuery({
    queryKey: ['transformationSpec', existingReference?.childTransformationSpecId],
    queryFn: () => {
      if (!existingReference?.childTransformationSpecId) return null
      return transformationSpecsApi.getTransformationSpecDetails(existingReference.childTransformationSpecId)
    },
    enabled: !!existingReference?.childTransformationSpecId,
  })

  // Fetch source and target schema details for display
  const { data: sourceElementSchema } = useQuery({
    queryKey: ['schemaDetails', sourceField?.elementSchemaId],
    queryFn: () => {
      if (!sourceField?.elementSchemaId) return null
      return schemasApi.getSchemaDetails(sourceField.elementSchemaId)
    },
    enabled: !!sourceField?.elementSchemaId,
  })

  const { data: targetElementSchema } = useQuery({
    queryKey: ['schemaDetails', targetField?.elementSchemaId],
    queryFn: () => {
      if (!targetField?.elementSchemaId) return null
      return schemasApi.getSchemaDetails(targetField.elementSchemaId)
    },
    enabled: !!targetField?.elementSchemaId,
  })

  const addReferenceMutation = useMutation({
    mutationFn: (childSpecId: string) =>
      transformationSpecsApi.addTransformReference(transformationSpecId, sourceFieldPath, targetFieldPath, childSpecId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transformationSpec', transformationSpecId] })
      setShowSelector(false)
      setSelectedSpecId(null)
      onReferenceAdded()
    },
  })

  // Auto-suggest if exactly one compatible spec exists
  useEffect(() => {
    if (compatibleSpecs && compatibleSpecs.length === 1 && !existingReference && !isReadOnly) {
      // Auto-suggest but don't auto-select - let user confirm
      setSelectedSpecId(compatibleSpecs[0].id)
    }
  }, [compatibleSpecs, existingReference, isReadOnly])

  if (!requiresNestedTransformation) {
    return null
  }

  const fieldTypeLabel = sourceField?.fieldType === 'Array' ? 'array' : 'object'

  return (
    <div className="mt-3 p-3 bg-blue-50 border border-blue-200 rounded-lg">
      <div className="flex items-start justify-between mb-2">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-1">
            <span className="text-sm font-medium text-blue-900">
              {sourceField?.fieldType === 'Array' ? '📋' : '📦'} Nested {fieldTypeLabel.charAt(0).toUpperCase() + fieldTypeLabel.slice(1)} Transformation
            </span>
          </div>
          <p className="text-xs text-blue-700 mb-2">
            This field contains a {fieldTypeLabel}. A nested transformation is required.
          </p>

          {existingReference && childSpecDetails && sourceElementSchema && targetElementSchema ? (
            <div className="bg-white p-2 rounded border border-blue-300">
              <div className="text-xs font-medium text-gray-900 mb-1">
                Current Transformation:
              </div>
              <div className="text-xs text-gray-700">
                {expertMode ? (
                  <>
                    {sourceElementSchema.key} (v{sourceElementSchema.version}) →{' '}
                    {targetElementSchema.key} (v{targetElementSchema.version})
                    <br />
                    <span className="text-gray-500">
                      Spec ID: {childSpecDetails.id.substring(0, 8)}... | Version: {childSpecDetails.version} |{' '}
                      {childSpecDetails.cardinality} | {childSpecDetails.status}
                    </span>
                  </>
                ) : (
                  <>
                    {sourceElementSchema.key} → {targetElementSchema.key} (v{childSpecDetails.version})
                  </>
                )}
              </div>
              {!isReadOnly && (
                <button
                  onClick={() => setShowSelector(true)}
                  className="mt-2 text-xs text-blue-600 hover:text-blue-800 underline"
                >
                  Change transformation
                </button>
              )}
            </div>
          ) : (
            <div className="bg-yellow-50 p-2 rounded border border-yellow-300">
              <div className="text-xs font-medium text-yellow-800 mb-1">⚠ No nested transformation defined</div>
              {!isReadOnly && (
                <button
                  onClick={() => setShowSelector(true)}
                  className="mt-1 text-xs text-blue-600 hover:text-blue-800 underline"
                >
                  Select transformation
                </button>
              )}
            </div>
          )}
        </div>
      </div>

      {showSelector && !isReadOnly && (
        <div className="mt-3 p-3 bg-white border border-gray-300 rounded">
          <div className="text-xs font-medium text-gray-900 mb-2">Select Nested Transformation</div>

          {loadingSpecs ? (
            <div className="text-xs text-gray-500">Loading compatible transformations...</div>
          ) : compatibleSpecs && compatibleSpecs.length > 0 ? (
            <div className="space-y-2">
              {compatibleSpecs.map((spec) => {
                const isDraft = spec.status === 'Draft'
                const isSelectable = spec.status === 'Published' || spec.status === 'Draft'
                return (
                  <label
                    key={spec.id}
                    className={`flex items-start gap-2 p-2 rounded border ${
                      isSelectable ? 'cursor-pointer' : 'cursor-not-allowed opacity-50'
                    } ${
                      selectedSpecId === spec.id
                        ? 'border-blue-500 bg-blue-50'
                        : isDraft
                        ? 'border-yellow-300 bg-yellow-50'
                        : 'border-gray-200 hover:bg-gray-50'
                    }`}
                  >
                    <input
                      type="radio"
                      name="nestedTransformation"
                      value={spec.id}
                      checked={selectedSpecId === spec.id}
                      onChange={() => isSelectable && setSelectedSpecId(spec.id)}
                      disabled={!isSelectable}
                      className="mt-0.5"
                    />
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <div className="text-xs font-medium text-gray-900">
                          {spec.sourceSchemaKey} → {spec.targetSchemaKey}
                        </div>
                        {isDraft && (
                          <span className="text-xs px-1.5 py-0.5 bg-yellow-200 text-yellow-800 rounded">
                            Draft
                          </span>
                        )}
                        {spec.status === 'Published' && (
                          <span className="text-xs px-1.5 py-0.5 bg-green-200 text-green-800 rounded">
                            Published
                          </span>
                        )}
                      </div>
                      {expertMode && (
                        <div className="text-xs text-gray-500 mt-0.5">
                          v{spec.version} | {spec.cardinality} | {spec.status} | ID: {spec.id.substring(0, 8)}...
                        </div>
                      )}
                      {!expertMode && (
                        <div className="text-xs text-gray-500 mt-0.5">v{spec.version}</div>
                      )}
                      {isDraft && (
                        <div className="text-xs text-yellow-700 mt-1">
                          ⚠️ Draft transformation - will need to be Published before parent can be Published
                        </div>
                      )}
                    </div>
                  </label>
                )
              })}
              <div className="flex gap-2 mt-3">
                <button
                  onClick={() => {
                    if (selectedSpecId) {
                      addReferenceMutation.mutate(selectedSpecId)
                    }
                  }}
                  disabled={!selectedSpecId || addReferenceMutation.isPending}
                  className="flex-1 px-3 py-1.5 text-xs bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
                >
                  {addReferenceMutation.isPending ? 'Adding...' : 'Add Transformation'}
                </button>
                <button
                  onClick={() => {
                    setShowSelector(false)
                    setSelectedSpecId(null)
                  }}
                  className="flex-1 px-3 py-1.5 text-xs bg-gray-200 text-gray-700 rounded hover:bg-gray-300"
                >
                  Cancel
                </button>
              </div>
            </div>
          ) : (
            <div className="text-xs text-yellow-700 space-y-1">
              <div>
                No compatible transformations found for:
              </div>
              <div className="font-mono text-xs bg-yellow-100 p-1 rounded">
                Source: {sourceElementSchema?.key || sourceField?.elementSchemaId || 'unknown'} (v{sourceElementSchema?.version || '?'})
                <br />
                Target: {targetElementSchema?.key || targetField?.elementSchemaId || 'unknown'} (v{targetElementSchema?.version || '?'})
              </div>
              <div className="mt-2">
                Create a transformation from <strong>{sourceElementSchema?.key || 'source'}</strong> to <strong>{targetElementSchema?.key || 'target'}</strong> first.
                {sourceElementSchema?.status !== 'Published' || targetElementSchema?.status !== 'Published' ? (
                  <div className="mt-1 text-red-600">
                    Note: Element schemas must be Published for nested transformations.
                  </div>
                ) : null}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}

