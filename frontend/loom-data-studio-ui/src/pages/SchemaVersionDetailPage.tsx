import { useState, useMemo } from 'react'
import { useParams, Link, useNavigate } from 'react-router-dom'
import { useQuery, useQueryClient, useMutation } from '@tanstack/react-query'
import { schemasApi } from '../api/masterdata'
import { SchemaStructureEditor } from '../components/editor/SchemaStructureEditor'
import { InspectorPanel } from '../components/editor/InspectorPanel'
import { ValidationEditor } from '../components/editor/ValidationEditor'
import { ValidationRuleInspector } from '../components/editor/ValidationRuleInspector'
import { TransformationEditor } from '../components/editor/TransformationEditor'
import { TransformationRuleInspector } from '../components/editor/TransformationRuleInspector'
import { KeyDefinitionsEditor } from '../components/editor/KeyDefinitionsEditor'
import { SchemaReferenceGraph } from '../components/editor/SchemaReferenceGraph'
import { PublishDependenciesModal } from '../components/editor/PublishDependenciesModal'
import type { SchemaRole } from '../types'

interface SchemaVersionDetailPageProps {
  role: SchemaRole
}

type Tab = 'structure' | 'validations' | 'transformations' | 'keys' | 'references'

export function SchemaVersionDetailPage({ role }: SchemaVersionDetailPageProps) {
  const { schemaKey, versionId } = useParams<{ schemaKey: string; versionId: string }>()
  const queryClient = useQueryClient()
  const [activeTab, setActiveTab] = useState<Tab>('structure')
  const [selectedFieldId, setSelectedFieldId] = useState<string | null>(null)
  const [selectedValidationRuleId, setSelectedValidationRuleId] = useState<string | null>(null)
  const [selectedTransformationRuleId, setSelectedTransformationRuleId] = useState<string | null>(null)
  const [expertMode, setExpertMode] = useState(false)
  const [showDependenciesModal, setShowDependenciesModal] = useState(false)
  const [transformationMode, setTransformationMode] = useState<'Simple' | 'Advanced'>('Simple')
  const [showModeWarning, setShowModeWarning] = useState(false)
  const [showDeleteVersionModal, setShowDeleteVersionModal] = useState(false)
  const navigate = useNavigate()

  const { data: schemaDetails, isLoading, error } = useQuery({
    queryKey: ['schemaDetails', versionId],
    queryFn: () => schemasApi.getSchemaDetails(versionId!),
    enabled: !!versionId,
  })

  const isReadOnly = useMemo(
    () => schemaDetails?.status !== 'Draft',
    [schemaDetails?.status]
  )

  const { data: validationResult } = useQuery({
    queryKey: ['validateSchema', versionId],
    queryFn: () => schemasApi.validateSchema(versionId!),
    enabled: !!versionId && !isReadOnly,
    refetchInterval: 30000, // Poll for validation results
  })

  const validationErrors = useMemo(() => {
    if (!validationResult || validationResult.isValid) return []
    return validationResult.errors.map((e) => `${e.field}: ${e.message}`)
  }, [validationResult])

  const { data: unpublishedDependencies } = useQuery({
    queryKey: ['unpublishedDependencies', versionId],
    queryFn: () => schemasApi.getUnpublishedDependencies(versionId!),
    enabled: !!versionId && !isReadOnly,
  })

  // Check if this is the latest version
  const { data: allVersions } = useQuery({
    queryKey: ['schemas', role, schemaKey],
    queryFn: () => schemasApi.getSchemas(role),
    enabled: !!schemaKey,
  })

  const isLatestVersion = useMemo(() => {
    if (!schemaDetails || !allVersions) return false
    const versionsForKey = allVersions
      .filter((s) => s.key === schemaKey)
      .sort((a, b) => b.version - a.version)
    return versionsForKey.length > 0 && versionsForKey[0].id === schemaDetails.id
  }, [schemaDetails, allVersions, schemaKey])

  const deleteVersionMutation = useMutation({
    mutationFn: () => schemasApi.deleteSchemaVersion(versionId!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schemas', role, schemaKey] })
      navigate(`/${role.toLowerCase()}/${schemaKey}`)
    },
  })

  const selectedField = useMemo(() => {
    if (!selectedFieldId || !schemaDetails) return null
    return schemaDetails.fields.find((f) => f.id === selectedFieldId) || null
  }, [selectedFieldId, schemaDetails])

  // Fetch validation spec to get selected rule
  const { data: validationSpec } = useQuery({
    queryKey: ['validationSpec', versionId],
    queryFn: async () => {
      try {
        const { validationSpecsApi } = await import('../api/masterdata')
        return await validationSpecsApi.getValidationSpecBySchemaId(versionId!)
      } catch {
        return null
      }
    },
    enabled: activeTab === 'validations' && !!versionId && !!selectedValidationRuleId && selectedValidationRuleId !== '__create__',
  })

  const selectedValidationRule = useMemo(() => {
    if (!selectedValidationRuleId || selectedValidationRuleId === '__create__' || !validationSpec) return null
    return validationSpec.rules?.find((r) => r.id === selectedValidationRuleId) || null
  }, [selectedValidationRuleId, validationSpec])

  // Fetch transformation spec to get selected rule
  const { data: transformationSpec } = useQuery({
    queryKey: ['transformationSpec', versionId],
    queryFn: async () => {
      try {
        const { transformationSpecsApi } = await import('../api/masterdata')
        return await transformationSpecsApi.getTransformationSpecBySourceSchemaId(versionId!)
      } catch {
        return null
      }
    },
    enabled: activeTab === 'transformations' && !!versionId && !!selectedTransformationRuleId,
  })

  const selectedTransformationRule = useMemo(() => {
    if (!selectedTransformationRuleId || !transformationSpec) return null
    return transformationSpec.simpleRules?.find((r) => r.id === selectedTransformationRuleId) || null
  }, [selectedTransformationRuleId, transformationSpec])

  if (isLoading) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="text-center">Loading...</div>
      </div>
    )
  }

  if (error || !schemaDetails) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="text-center text-red-600">Schema not found</div>
      </div>
    )
  }

  return (
    <div className="h-screen flex flex-col">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 px-6 py-4">
        <div className="flex items-center justify-between">
          <div>
            <Link
              to={`/${role.toLowerCase()}/${schemaKey}`}
              className="text-sm text-loom-600 hover:text-loom-700 mb-1 inline-block"
            >
              ← Back to {schemaKey}
            </Link>
            <h1 className="text-2xl font-bold text-gray-900">
              {schemaKey} v{schemaDetails.version}
            </h1>
            <div className="flex items-center gap-3 mt-1">
              <span
                className={`px-2 py-1 text-xs font-medium rounded ${
                  schemaDetails.status === 'Published'
                    ? 'bg-green-100 text-green-800'
                    : schemaDetails.status === 'Draft'
                    ? 'bg-yellow-100 text-yellow-800'
                    : 'bg-gray-100 text-gray-800'
                }`}
              >
                {schemaDetails.status}
              </span>
              {!isReadOnly && (
                <label className="flex items-center gap-2 text-sm text-gray-600">
                  <input
                    type="checkbox"
                    checked={expertMode}
                    onChange={(e) => setExpertMode(e.target.checked)}
                    className="rounded"
                  />
                  Expert Mode
                </label>
              )}
            </div>
          </div>
          <div className="flex items-center gap-2">
            {!isReadOnly && (
              <button
                onClick={async () => {
                  try {
                    // Validate schema first (includes business keys validation)
                    const schemaValidation = await schemasApi.validateSchema(versionId!)
                    if (!schemaValidation.isValid) {
                      // Check if errors are due to unpublished dependencies
                      const hasUnpublishedRefs = schemaValidation.errors.some((e) =>
                        e.message.includes('must be Published')
                      )
                      if (hasUnpublishedRefs && unpublishedDependencies && unpublishedDependencies.length > 0) {
                        setShowDependenciesModal(true)
                        return
                      }
                      const errorMessages = schemaValidation.errors.map((e) => `${e.field}: ${e.message}`).join('\n')
                      alert(`Schema validation failed:\n\n${errorMessages}\n\nPlease fix the errors before publishing.`)
                      // Switch to relevant tab if key errors exist
                      if (schemaValidation.errors.some((e) => e.field.includes('KeyDefinitions'))) {
                        setActiveTab('keys')
                      }
                      return
                    }
                    await schemasApi.publishSchema(versionId!, 'user@example.com')
                    queryClient.invalidateQueries({ queryKey: ['schemaDetails', versionId] })
                    queryClient.invalidateQueries({ queryKey: ['validateSchema', versionId] })
                    alert('Schema published successfully!')
                  } catch (err) {
                    alert(`Failed to publish: ${err}`)
                  }
                }}
                className={`px-4 py-2 rounded-lg transition-colors ${
                  validationResult && !validationResult.isValid
                    ? 'bg-red-600 text-white hover:bg-red-700'
                    : 'bg-green-600 text-white hover:bg-green-700'
                }`}
              >
                {validationResult && !validationResult.isValid ? '⚠ Publish (Has Errors)' : 'Publish'}
              </button>
            )}
            {isLatestVersion && (
              <button
                onClick={() => setShowDeleteVersionModal(true)}
                className="px-3 py-1.5 text-sm text-red-600 border border-red-300 rounded hover:bg-red-50"
              >
                Delete Version
              </button>
            )}
          </div>
        </div>

        {/* Tabs */}
        <div className="mt-4 flex gap-1 border-b border-gray-200">
          <button
            onClick={() => setActiveTab('structure')}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
              activeTab === 'structure'
                ? 'border-loom-600 text-loom-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            Structure
          </button>
          <button
            onClick={() => setActiveTab('validations')}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
              activeTab === 'validations'
                ? 'border-loom-600 text-loom-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            Validations
          </button>
          <button
            onClick={() => setActiveTab('transformations')}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
              activeTab === 'transformations'
                ? 'border-loom-600 text-loom-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            Transformations
          </button>
          {role === 'Master' && schemaDetails?.status !== 'Archived' && (
            <button
              onClick={() => setActiveTab('keys')}
              className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
                activeTab === 'keys'
                  ? 'border-loom-600 text-loom-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              Keys
            </button>
          )}
          {expertMode && (
            <button
              onClick={() => setActiveTab('references')}
              className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
                activeTab === 'references'
                  ? 'border-loom-600 text-loom-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              References
            </button>
          )}
        </div>
      </div>

      {/* Main Content Area */}
      <div className="flex-1 flex overflow-hidden">
        {/* Left: Structure Tree */}
        {activeTab === 'structure' && (
          <>
            <div className="w-64 bg-white border-r border-gray-200 overflow-y-auto">
              <SchemaStructureEditor
                schemaId={versionId!}
                fields={schemaDetails.fields}
                isReadOnly={isReadOnly}
                onFieldSelect={setSelectedFieldId}
                selectedFieldId={selectedFieldId}
                schemaRole={schemaDetails.role}
                onAddFieldClick={() => setSelectedFieldId('__create__')}
              />
            </div>

            {/* Center: Field Details */}
            <div className="flex-1 bg-gray-50 overflow-hidden">
              {selectedFieldId === '__create__' ? (
                <InspectorPanel
                  schemaId={versionId!}
                  field={null}
                  isReadOnly={isReadOnly}
                  schemaRole={schemaDetails.role}
                  onClose={() => setSelectedFieldId(null)}
                />
              ) : selectedField ? (
                <InspectorPanel
                  schemaId={versionId!}
                  field={selectedField}
                  isReadOnly={isReadOnly}
                  schemaRole={schemaDetails.role}
                  onClose={() => setSelectedFieldId(null)}
                />
              ) : (
                <div className="h-full flex items-center justify-center text-gray-500">
                  Select a field to view details
                </div>
              )}
            </div>
          </>
        )}

        {/* Validations Tab */}
        {activeTab === 'validations' && (
          <>
            <div className="w-64 bg-white border-r border-gray-200 overflow-y-auto">
              <ValidationEditor
                schemaId={versionId!}
                isReadOnly={isReadOnly}
                expertMode={expertMode}
                onRuleSelect={setSelectedValidationRuleId}
                selectedRuleId={selectedValidationRuleId}
                onAddRuleClick={() => setSelectedValidationRuleId('__create__')}
              />
            </div>

            {/* Center: Validation Rule Details */}
            <div className="flex-1 bg-gray-50 overflow-hidden">
              {selectedValidationRuleId === '__create__' ? (
                <ValidationRuleInspector
                  schemaId={versionId!}
                  rule={null}
                  isReadOnly={isReadOnly}
                  onClose={() => setSelectedValidationRuleId(null)}
                />
              ) : selectedValidationRule ? (
                <ValidationRuleInspector
                  schemaId={versionId!}
                  rule={selectedValidationRule}
                  isReadOnly={isReadOnly}
                  onClose={() => setSelectedValidationRuleId(null)}
                />
              ) : (
                <div className="h-full flex items-center justify-center text-gray-500">
                  Select a rule to view details
                </div>
              )}
            </div>
          </>
        )}

        {/* Transformations Tab */}
        {activeTab === 'transformations' && (
          <div className="flex-1 flex flex-col overflow-hidden">
            {/* Transformation Header - Above both panels */}
            <div className="p-4 border-b border-gray-200 bg-white">
              <div className="flex items-center justify-between">
                <h3 className="font-semibold text-gray-900">Transformation</h3>
                {!isReadOnly && (
                  <div className="flex gap-2">
                    <button
                      onClick={() => {
                        if (transformationMode === 'Simple') return
                        setTransformationMode('Simple')
                      }}
                      className={`px-4 py-2 text-sm rounded-lg transition-colors ${
                        transformationMode === 'Simple'
                          ? 'bg-loom-600 text-white'
                          : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                      }`}
                    >
                      Simple Mode
                    </button>
                    {expertMode && (
                      <button
                        onClick={() => {
                          if (transformationMode === 'Advanced') return
                          if (transformationMode === 'Simple') {
                            setShowModeWarning(true)
                          } else {
                            setTransformationMode('Advanced')
                          }
                        }}
                        className={`px-4 py-2 text-sm rounded-lg transition-colors ${
                          transformationMode === 'Advanced'
                            ? 'bg-loom-600 text-white'
                            : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                        }`}
                      >
                        Advanced Mode
                      </button>
                    )}
                  </div>
                )}
              </div>
            </div>

            {showModeWarning && (
              <div className="p-4 bg-yellow-50 border-b border-yellow-200">
                <div className="flex items-start justify-between">
                  <div>
                    <h4 className="font-medium text-yellow-800 mb-1">
                      Switch to Advanced Mode?
                    </h4>
                    <p className="text-sm text-yellow-700">
                      Advanced mode uses a graph-based editor. This is a one-way upgrade.
                    </p>
                  </div>
                  <div className="flex gap-2">
                    <button
                      onClick={() => {
                        setTransformationMode('Advanced')
                        setShowModeWarning(false)
                      }}
                      className="px-3 py-1 text-sm bg-yellow-600 text-white rounded hover:bg-yellow-700"
                    >
                      Confirm
                    </button>
                    <button
                      onClick={() => setShowModeWarning(false)}
                      className="px-3 py-1 text-sm bg-gray-200 text-gray-700 rounded hover:bg-gray-300"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              </div>
            )}

            <div className="flex-1 flex overflow-hidden">
              <div className="w-64 bg-white border-r border-gray-200 overflow-y-auto">
                <TransformationEditor
                  schemaId={versionId!}
                  role={role}
                  isReadOnly={isReadOnly}
                  expertMode={expertMode}
                  onRuleSelect={setSelectedTransformationRuleId}
                  selectedRuleId={selectedTransformationRuleId}
                  mode={transformationMode}
                  onAddRuleClick={() => setSelectedTransformationRuleId('__create__')}
                />
              </div>

              {/* Center: Transformation Rule Details */}
              <div className="flex-1 bg-gray-50 overflow-hidden">
                {transformationSpec ? (
                  selectedTransformationRuleId === '__create__' ? (
                    <TransformationRuleInspector
                      schemaId={versionId!}
                      transformationSpecId={transformationSpec.id}
                      targetSchemaId={transformationSpec.targetSchemaId}
                      rule={null}
                      isReadOnly={isReadOnly}
                      expertMode={expertMode}
                      onClose={() => setSelectedTransformationRuleId(null)}
                    />
                  ) : selectedTransformationRule ? (
                    <TransformationRuleInspector
                      schemaId={versionId!}
                      transformationSpecId={transformationSpec.id}
                      targetSchemaId={transformationSpec.targetSchemaId}
                      rule={selectedTransformationRule}
                      isReadOnly={isReadOnly}
                      expertMode={expertMode}
                      onClose={() => setSelectedTransformationRuleId(null)}
                    />
                  ) : (
                    <div className="h-full flex items-center justify-center text-gray-500">
                      Select a rule to view details
                    </div>
                  )
                ) : (
                  <div className="h-full flex items-center justify-center text-gray-500">
                    {selectedTransformationRuleId === '__create__' 
                      ? 'Please create a transformation spec first'
                      : 'Select a rule to view details'}
                  </div>
                )}
              </div>
            </div>
          </div>
        )}

        {/* Keys Tab (Master only) */}
        {activeTab === 'keys' && role === 'Master' && (
          <div className="flex-1 overflow-hidden">
            <KeyDefinitionsEditor
              schemaId={versionId!}
              keyDefinitions={schemaDetails.keyDefinitions}
              fields={schemaDetails.fields}
              isReadOnly={isReadOnly}
              expertMode={expertMode}
              validationErrors={validationErrors}
            />
          </div>
        )}

        {/* References Tab */}
        {activeTab === 'references' && (
          <div className="flex-1 overflow-hidden">
            <SchemaReferenceGraph schemaId={versionId!} role={role} />
          </div>
        )}

      </div>

      {showDependenciesModal && unpublishedDependencies && unpublishedDependencies.length > 0 && (
        <PublishDependenciesModal
          schemaId={versionId!}
          dependencies={unpublishedDependencies}
          onClose={() => setShowDependenciesModal(false)}
          onSuccess={async () => {
            // After publishing dependencies, try to publish the main schema
            try {
              await schemasApi.publishSchema(versionId!, 'user@example.com')
              queryClient.invalidateQueries({ queryKey: ['schemaDetails', versionId] })
              queryClient.invalidateQueries({ queryKey: ['validateSchema', versionId] })
              alert('All schemas published successfully!')
            } catch (err) {
              alert(`Failed to publish main schema: ${err}`)
            }
          }}
        />
      )}

      {showDeleteVersionModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Delete Schema Version</h3>
            <p className="text-sm text-gray-600 mb-6">
              Are you sure you want to delete version {schemaDetails?.version} of schema "{schemaKey}"?
              This action cannot be undone.
            </p>
            <div className="flex gap-3 justify-end">
              <button
                onClick={() => setShowDeleteVersionModal(false)}
                className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
              >
                Cancel
              </button>
              <button
                onClick={() => {
                  deleteVersionMutation.mutate()
                  setShowDeleteVersionModal(false)
                }}
                disabled={deleteVersionMutation.isPending}
                className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
              >
                {deleteVersionMutation.isPending ? 'Deleting...' : 'Delete'}
              </button>
            </div>
            {deleteVersionMutation.isError && (
              <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded text-sm text-red-800">
                {deleteVersionMutation.error instanceof Error
                  ? deleteVersionMutation.error.message
                  : 'Failed to delete schema version'}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}

