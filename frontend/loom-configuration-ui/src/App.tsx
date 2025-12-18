import { Routes, Route, Navigate } from 'react-router-dom'
import { Layout } from './components/Layout'
import { WorkflowListPage } from './pages/WorkflowListPage'
import { WorkflowVersionsPage } from './pages/WorkflowVersionsPage'
import { WorkflowEditorPage } from './pages/WorkflowEditorPage'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<Navigate to="/workflows" replace />} />
        <Route path="workflows" element={<WorkflowListPage />} />
        <Route path="workflows/:workflowId" element={<WorkflowVersionsPage />} />
        <Route path="workflows/:workflowId/versions/:versionId" element={<WorkflowEditorPage />} />
      </Route>
    </Routes>
  )
}

