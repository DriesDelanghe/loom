import { Routes, Route, Navigate } from 'react-router-dom'
import { Layout } from './components/Layout'
import { SchemaOverviewPage } from './pages/SchemaOverviewPage'
import { SchemaKeyOverviewPage } from './pages/SchemaKeyOverviewPage'
import { SchemaVersionDetailPage } from './pages/SchemaVersionDetailPage'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<Navigate to="/incoming" replace />} />
        
        {/* Incoming Data Routes */}
        <Route path="incoming" element={<SchemaOverviewPage role="Incoming" />} />
        <Route path="incoming/:schemaKey" element={<SchemaKeyOverviewPage role="Incoming" />} />
        <Route
          path="incoming/:schemaKey/:versionId"
          element={<SchemaVersionDetailPage role="Incoming" />}
        />
        
        {/* Master Data Routes */}
        <Route path="master" element={<SchemaOverviewPage role="Master" />} />
        <Route path="master/:schemaKey" element={<SchemaKeyOverviewPage role="Master" />} />
        <Route
          path="master/:schemaKey/:versionId"
          element={<SchemaVersionDetailPage role="Master" />}
        />
        
        {/* Outgoing Data Routes */}
        <Route path="outgoing" element={<SchemaOverviewPage role="Outgoing" />} />
        <Route path="outgoing/:schemaKey" element={<SchemaKeyOverviewPage role="Outgoing" />} />
        <Route
          path="outgoing/:schemaKey/:versionId"
          element={<SchemaVersionDetailPage role="Outgoing" />}
        />
      </Route>
    </Routes>
  )
}

