import { useState } from 'react'
import { useParams, Link, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { schemasApi } from '../api/masterdata'
import type { SchemaRole } from '../types'

interface SchemaKeyOverviewPageProps {
  role: SchemaRole
}

export function SchemaKeyOverviewPage({ role }: SchemaKeyOverviewPageProps) {
  const { schemaKey } = useParams<{ schemaKey: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const { data: schemas, isLoading, error } = useQuery({
    queryKey: ['schemas', role, schemaKey],
    queryFn: () => schemasApi.getSchemas(role),
    enabled: !!schemaKey,
  })

  const schemaVersions = schemas?.filter((s) => s.key === schemaKey).sort((a, b) => b.version - a.version)
  const latestPublished = schemaVersions?.find((s) => s.status === 'Published')
  const latestDraft = schemaVersions?.find((s) => s.status === 'Draft')

  const [showDeleteVersionModal, setShowDeleteVersionModal] = useState(false)
  const [versionToDelete, setVersionToDelete] = useState<string | null>(null)
  const [showTagInput, setShowTagInput] = useState(false)
  const [newTag, setNewTag] = useState('')
  const [editingVersionId, setEditingVersionId] = useState<string | null>(null)

  const createDraftMutation = useMutation({
    mutationFn: () => {
      if (!latestPublished) {
        throw new Error('No published version to create draft from')
      }
      return schemasApi.createSchema(null, role, schemaKey!, latestPublished.description || undefined)
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['schemas', role, schemaKey] })
      navigate(`/${role.toLowerCase()}/${schemaKey}/${data.id}`)
    },
  })

  const deleteVersionMutation = useMutation({
    mutationFn: (versionId: string) => schemasApi.deleteSchemaVersion(versionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schemas', role, schemaKey] })
      setShowDeleteVersionModal(false)
      setVersionToDelete(null)
    },
  })

  // Determine which version is the latest
  const latestVersion = schemaVersions && schemaVersions.length > 0 ? schemaVersions[0] : null

  // Fetch schema details for the latest version to get tags with IDs
  const { data: latestVersionDetails } = useQuery({
    queryKey: ['schemaDetails', latestVersion?.id],
    queryFn: () => schemasApi.getSchemaDetails(latestVersion!.id),
    enabled: !!latestVersion,
  })

  const addTagMutation = useMutation({
    mutationFn: (tag: string) => {
      if (!latestVersion) throw new Error('No version selected')
      return schemasApi.addSchemaTag(latestVersion.id, tag)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schemas', role, schemaKey] })
      queryClient.invalidateQueries({ queryKey: ['schemaDetails', latestVersion?.id] })
      setNewTag('')
      setShowTagInput(false)
    },
  })

  const removeTagMutation = useMutation({
    mutationFn: (tag: string) => {
      if (!latestVersion) throw new Error('No version selected')
      return schemasApi.removeSchemaTagByValue(latestVersion.id, tag)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schemas', role, schemaKey] })
      queryClient.invalidateQueries({ queryKey: ['schemaDetails', latestVersion?.id] })
    },
  })

  if (isLoading) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="text-center">Loading...</div>
      </div>
    )
  }

  if (error || !schemaVersions || schemaVersions.length === 0) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="text-center text-red-600">Schema not found</div>
      </div>
    )
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-6">
        <Link
          to={`/${role.toLowerCase()}`}
          className="text-sm text-loom-600 hover:text-loom-700 mb-2 inline-block"
        >
          ← Back to {role} Data
        </Link>
        <h1 className="text-3xl font-bold text-gray-900">{schemaKey}</h1>
        <p className="mt-1 text-sm text-gray-500">Version history</p>
      </div>

      <div className="mb-6 flex gap-4 items-start">
        {latestDraft ? (
          <Link
            to={`/${role.toLowerCase()}/${schemaKey}/${latestDraft.id}`}
            className="px-4 py-2 bg-loom-600 text-white rounded-lg hover:bg-loom-700 transition-colors"
          >
            Open Draft (v{latestDraft.version})
          </Link>
        ) : (
          <button
            onClick={() => createDraftMutation.mutate()}
            disabled={!latestPublished || createDraftMutation.isPending}
            className="px-4 py-2 bg-loom-600 text-white rounded-lg hover:bg-loom-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            ➕ Create New Draft
          </button>
        )}
      </div>

      {/* Tags Section */}
      {latestVersion && (
        <div className="mb-6 bg-white rounded-lg shadow border border-gray-200 p-4">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-lg font-semibold text-gray-900">Tags</h2>
            <button
              onClick={() => {
                setEditingVersionId(latestVersion.id)
                setShowTagInput(true)
              }}
              className="px-3 py-1 text-sm bg-loom-600 text-white rounded hover:bg-loom-700"
            >
              ➕ Add Tag
            </button>
          </div>

          {showTagInput && editingVersionId === latestVersion.id && (
            <div className="mb-3 flex gap-2">
              <input
                type="text"
                value={newTag}
                onChange={(e) => setNewTag(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' && newTag.trim()) {
                    addTagMutation.mutate(newTag.trim())
                  } else if (e.key === 'Escape') {
                    setShowTagInput(false)
                    setNewTag('')
                  }
                }}
                placeholder="Enter tag name..."
                className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-loom-500"
                autoFocus
              />
              <button
                onClick={() => {
                  if (newTag.trim()) {
                    addTagMutation.mutate(newTag.trim())
                  }
                }}
                disabled={!newTag.trim() || addTagMutation.isPending}
                className="px-4 py-2 bg-loom-600 text-white rounded-lg hover:bg-loom-700 disabled:opacity-50"
              >
                Add
              </button>
              <button
                onClick={() => {
                  setShowTagInput(false)
                  setNewTag('')
                }}
                className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
              >
                Cancel
              </button>
            </div>
          )}

          {latestVersionDetails?.tags && latestVersionDetails.tags.length > 0 ? (
            <div className="flex flex-wrap gap-2">
              {latestVersionDetails.tags.map((tag) => (
                <span
                  key={tag}
                  className="inline-flex items-center gap-2 px-3 py-1 bg-loom-100 text-loom-700 rounded-lg text-sm"
                >
                  {tag}
                  <button
                    onClick={() => removeTagMutation.mutate(tag)}
                    disabled={removeTagMutation.isPending}
                    className="text-loom-700 hover:text-red-600 text-xs"
                    title="Remove tag"
                  >
                    ✕
                  </button>
                </span>
              ))}
            </div>
          ) : (
            <p className="text-sm text-gray-500">No tags added yet</p>
          )}

          {addTagMutation.isError && (
            <div className="mt-2 p-2 bg-red-50 border border-red-200 rounded text-sm text-red-800">
              {addTagMutation.error instanceof Error
                ? addTagMutation.error.message
                : 'Failed to add tag'}
            </div>
          )}

          {removeTagMutation.isError && (
            <div className="mt-2 p-2 bg-red-50 border border-red-200 rounded text-sm text-red-800">
              {removeTagMutation.error instanceof Error
                ? removeTagMutation.error.message
                : 'Failed to remove tag'}
            </div>
          )}
        </div>
      )}

      <div className="bg-white rounded-lg shadow border border-gray-200 overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Version
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Created
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Published
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {schemaVersions.map((schema) => (
              <tr key={schema.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                  v{schema.version}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span
                    className={`px-2 py-1 text-xs font-medium rounded ${
                      schema.status === 'Published'
                        ? 'bg-green-100 text-green-800'
                        : schema.status === 'Draft'
                        ? 'bg-yellow-100 text-yellow-800'
                        : 'bg-gray-100 text-gray-800'
                    }`}
                  >
                    {schema.status}
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {new Date(schema.createdAt).toLocaleDateString()}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {schema.publishedAt
                    ? new Date(schema.publishedAt).toLocaleDateString()
                    : '-'}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm">
                  <div className="flex items-center gap-3">
                    <Link
                      to={`/${role.toLowerCase()}/${schemaKey}/${schema.id}`}
                      className="text-loom-600 hover:text-loom-700"
                    >
                      Open
                    </Link>
                    {latestVersion && latestVersion.id === schema.id && (
                      <button
                        onClick={() => {
                          setVersionToDelete(schema.id)
                          setShowDeleteVersionModal(true)
                        }}
                        className="text-red-600 hover:text-red-700 text-sm"
                        title="Delete version"
                      >
                        ✕
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showDeleteVersionModal && versionToDelete && latestVersion && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Delete Schema Version</h3>
            <p className="text-sm text-gray-600 mb-6">
              Are you sure you want to delete version {latestVersion.version} of schema "{schemaKey}"?
              This action cannot be undone.
            </p>
            <div className="flex gap-3 justify-end">
              <button
                onClick={() => {
                  setShowDeleteVersionModal(false)
                  setVersionToDelete(null)
                }}
                className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
              >
                Cancel
              </button>
              <button
                onClick={() => {
                  deleteVersionMutation.mutate(versionToDelete)
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

