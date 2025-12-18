import { Outlet, Link, useLocation } from 'react-router-dom'

export function Layout() {
  const location = useLocation()
  
  return (
    <div className="min-h-screen flex flex-col">
      <header className="bg-loom-900 text-white shadow-lg">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            <Link to="/" className="flex items-center gap-3">
              <div className="w-8 h-8 bg-loom-500 rounded-lg flex items-center justify-center">
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                </svg>
              </div>
              <span className="text-xl font-semibold tracking-tight">Loom</span>
            </Link>
            <nav className="flex items-center gap-6">
              <Link
                to="/workflows"
                className={`text-sm font-medium transition-colors ${
                  location.pathname.startsWith('/workflows')
                    ? 'text-white'
                    : 'text-loom-300 hover:text-white'
                }`}
              >
                Workflows
              </Link>
            </nav>
          </div>
        </div>
      </header>
      <main className="flex-1">
        <Outlet />
      </main>
    </div>
  )
}

