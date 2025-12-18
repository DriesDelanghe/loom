import { useParams, Link, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { workflowsApi } from '../api/workflows'
import { layoutApi } from '../api/layout'
import type { WorkflowVersion, WorkflowStatus } from '../types'

const statusColors: Record<WorkflowStatus, string> = {
  Draft: 'bg-yellow-100 text-yellow-800',
  Published: 'bg-green-100 text-green-800',
  Archived: 'bg-gray-100 text-gray-600',
}

export function WorkflowVersionsPage() {
  const { workflowId } = useParams<{ workflowId: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const { data: versions, isLoading, error } = useQuery({
    queryKey: ['versions', workflowId],
    queryFn: () => workflowsApi.getVersions(workflowId!),
    enabled: !!workflowId,
  })

  const createDraftMutation = useMutation({
    mutationFn: async () => {
      const result = await workflowsApi.createDraftVersion(workflowId!, 'user@example.com')
      
      const publishedVersion = versions?.find((v: WorkflowVersion) => v.status === 'Published')
      if (publishedVersion) {
        try {
          await layoutApi.copyLayoutFromVersion(result.id, publishedVersion.id)
        } catch (error) {
          console.warn('Failed to copy layout:', error)
        }
      }
      
      return result
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['versions', workflowId] })
      navigate(`/workflows/${workflowId}/versions/${data.id}`)
    },
  })

  const deleteVersionMutation = useMutation({
    mutationFn: (versionId: string) => workflowsApi.deleteVersion(versionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['versions', workflowId] })
    },
  })

  const hasDraft = versions?.some((v: WorkflowVersion) => v.status === 'Draft')

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-loom-600" />
      </div>
    )
  }

  if (error) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-8">
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-700">
          Failed to load versions. Please try again.
        </div>
      </div>
    )
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-6">
        <Link to="/workflows" className="text-loom-600 hover:text-loom-700 text-sm font-medium flex items-center gap-1">
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
          Back to Workflows
        </Link>
      </div>

      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Workflow Versions</h1>
          <p className="text-gray-500 mt-1">Manage versions of your workflow</p>
        </div>
        <button
          onClick={() => createDraftMutation.mutate()}
          disabled={hasDraft || createDraftMutation.isPending}
          className="px-4 py-2 bg-loom-600 text-white rounded-lg hover:bg-loom-700 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed"
          title={hasDraft ? 'A draft version already exists' : undefined}
        >
          {createDraftMutation.isPending ? 'Creating...' : 'Create New Draft'}
        </button>
      </div>

      {versions?.length === 0 ? (
        <div className="text-center py-12 bg-gray-50 rounded-xl border-2 border-dashed border-gray-200">
          <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
          </svg>
          <h3 className="mt-4 text-lg font-medium text-gray-900">No versions yet</h3>
          <p className="mt-1 text-gray-500">Create a draft to start building your workflow.</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
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
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {versions?.map((version: WorkflowVersion) => (
                <tr key={version.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="text-sm font-medium text-gray-900">v{version.version}</span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusColors[version.status]}`}>
                      {version.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {new Date(version.createdAt).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {version.publishedAt ? new Date(version.publishedAt).toLocaleDateString() : '—'}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right space-x-3">
                    <Link
                      to={`/workflows/${workflowId}/versions/${version.id}`}
                      className="text-loom-600 hover:text-loom-800 font-medium text-sm"
                    >
                      {version.status === 'Draft' ? 'Edit' : 'View'}
                    </Link>
                    {version.status === 'Draft' && (
                      <button
                        type="button"
                        onClick={() => {
                          if (window.confirm('Delete this draft version? This cannot be undone.')) {
                            deleteVersionMutation.mutate(version.id)
                          }
                        }}
                        className="text-red-600 hover:text-red-800 font-medium text-sm"
                      >
                        Delete
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}

