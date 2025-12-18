import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'
import { schemasApi } from '../api/masterdata'
import type { SchemaRole, SchemaStatus } from '../types'

interface SchemaOverviewPageProps {
  role: SchemaRole
}

export function SchemaOverviewPage({ role }: SchemaOverviewPageProps) {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState<SchemaStatus | 'All'>('All')
  const [showCreate, setShowCreate] = useState(false)
  const [newKey, setNewKey] = useState('')
  const [newDescription, setNewDescription] = useState('')
  const [showDeleteModal, setShowDeleteModal] = useState(false)
  const [schemaToDelete, setSchemaToDelete] = useState<{ key: string; role: SchemaRole } | null>(null)

  const { data: schemas, isLoading, error } = useQuery({
    queryKey: ['schemas', role, statusFilter === 'All' ? undefined : statusFilter],
    queryFn: () => schemasApi.getSchemas(role, statusFilter === 'All' ? undefined : statusFilter),
  })

  const createMutation = useMutation({
    mutationFn: () =>
      schemasApi.createSchema(null, role, newKey, newDescription || undefined),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['schemas', role] })
      setShowCreate(false)
      // Navigate to the schema key overview, then to the version detail
      // The versionId is the schema ID returned from creation
      navigate(`/${role.toLowerCase()}/${newKey}/${data.id}`)
      setNewKey('')
      setNewDescription('')
    },
  })

  const deleteSchemaMutation = useMutation({
    mutationFn: (key: string) => schemasApi.deleteSchema(key, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schemas', role] })
      setShowDeleteModal(false)
      setSchemaToDelete(null)
    },
  })

  const filteredSchemas = schemas?.filter((schema) =>
    schema.key.toLowerCase().includes(searchTerm.toLowerCase()) ||
    schema.description?.toLowerCase().includes(searchTerm.toLowerCase())
  )

  if (isLoading) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="text-center">Loading...</div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="text-center text-red-600">Error loading schemas</div>
      </div>
    )
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 capitalize">{role} Data Schemas</h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage your {role.toLowerCase()} data schemas
          </p>
        </div>
        <button
          onClick={() => setShowCreate(true)}
          className="px-4 py-2 bg-loom-600 text-white rounded-lg hover:bg-loom-700 transition-colors"
        >
          ➕ Create Schema
        </button>
      </div>

      {showCreate && (
        <div className="mb-6 p-4 bg-white rounded-lg shadow border border-gray-200">
          <h3 className="text-lg font-semibold mb-4">Create New Schema</h3>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Schema Key
              </label>
              <input
                type="text"
                value={newKey}
                onChange={(e) => setNewKey(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-loom-500"
                placeholder="e.g., customer-schema"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Description (optional)
              </label>
              <textarea
                value={newDescription}
                onChange={(e) => setNewDescription(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-loom-500"
                rows={2}
                placeholder="Describe this schema..."
              />
            </div>
            <div className="flex gap-2">
              <button
                onClick={() => createMutation.mutate()}
                disabled={!newKey.trim() || createMutation.isPending}
                className="px-4 py-2 bg-loom-600 text-white rounded-lg hover:bg-loom-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Create
              </button>
              <button
                onClick={() => {
                  setShowCreate(false)
                  setNewKey('')
                  setNewDescription('')
                }}
                className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      <div className="mb-6 flex gap-4">
        <div className="flex-1">
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="🔍 Search schemas..."
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-loom-500"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value as SchemaStatus | 'All')}
          className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-loom-500"
        >
          <option value="All">All Status</option>
          <option value="Draft">Draft</option>
          <option value="Published">Published</option>
          <option value="Archived">Archived</option>
        </select>
      </div>

      {filteredSchemas && filteredSchemas.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-lg shadow border border-gray-200">
          <p className="text-gray-500">No schemas found</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredSchemas?.map((schema) => (
            <div
              key={schema.id}
              className="block p-6 bg-white rounded-lg shadow border border-gray-200 hover:shadow-lg transition-shadow"
            >
              <div className="flex items-start justify-between mb-2">
                <Link
                  to={`/${role.toLowerCase()}/${schema.key}`}
                  className="text-lg font-semibold text-gray-900 hover:text-loom-600"
                >
                  {schema.key}
                </Link>
                <div className="flex items-center gap-2">
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
                  <button
                    onClick={(e) => {
                      e.preventDefault()
                      e.stopPropagation()
                      setSchemaToDelete({ key: schema.key, role })
                      setShowDeleteModal(true)
                    }}
                    className="ml-2 px-2 py-1 text-xs text-red-600 hover:bg-red-50 rounded"
                    title="Delete schema"
                  >
                    ✕
                  </button>
                </div>
              </div>
              {schema.description && (
                <p className="text-sm text-gray-600 mb-3 line-clamp-2">{schema.description}</p>
              )}
              <div className="flex items-center gap-4 text-xs text-gray-500">
                <span>v{schema.version}</span>
                {schema.publishedAt && (
                  <span>Published {new Date(schema.publishedAt).toLocaleDateString()}</span>
                )}
              </div>
              {schema.tags && schema.tags.length > 0 && (
                <div className="mt-3 flex flex-wrap gap-1">
                  {schema.tags.map((tag: string) => (
                    <span
                      key={tag}
                      className="px-2 py-1 text-xs bg-loom-100 text-loom-700 rounded"
                    >
                      {tag}
                    </span>
                  ))}
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {showDeleteModal && schemaToDelete && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Delete Schema</h3>
            <p className="text-sm text-gray-600 mb-6">
              Are you sure you want to delete the schema "{schemaToDelete.key}"? This will delete all
              versions of this schema and cannot be undone.
            </p>
            <div className="flex gap-3 justify-end">
              <button
                onClick={() => {
                  setShowDeleteModal(false)
                  setSchemaToDelete(null)
                }}
                className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
              >
                Cancel
              </button>
              <button
                onClick={() => {
                  deleteSchemaMutation.mutate(schemaToDelete.key)
                }}
                disabled={deleteSchemaMutation.isPending}
                className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
              >
                {deleteSchemaMutation.isPending ? 'Deleting...' : 'Delete'}
              </button>
            </div>
            {deleteSchemaMutation.isError && (
              <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded text-sm text-red-800">
                {deleteSchemaMutation.error instanceof Error
                  ? deleteSchemaMutation.error.message
                  : 'Failed to delete schema'}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}

