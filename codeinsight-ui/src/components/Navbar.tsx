import { Link, useLocation } from 'react-router-dom';
import { Code2, LayoutDashboard, Search } from 'lucide-react';

export default function Navbar() {
  const { pathname } = useLocation();
  const link = (to: string, label: string, Icon: React.ElementType) => (
    <Link
      to={to}
      className={`flex items-center gap-2 px-3 py-2 rounded-lg text-sm transition-colors ${
        pathname === to ? 'bg-indigo-600 text-white' : 'text-gray-400 hover:text-white hover:bg-gray-800'
      }`}
    >
      <Icon size={16} /> {label}
    </Link>
  );

  return (
    <nav className="border-b border-gray-800 bg-gray-950 sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 h-14 flex items-center justify-between">
        <Link to="/" className="flex items-center gap-2 font-bold text-white">
          <Code2 size={22} className="text-indigo-400" />
          <span>Code<span className="text-indigo-400">Insight</span></span>
          <span className="text-xs text-gray-500 font-normal ml-1">Technical Debt Analyzer</span>
        </Link>
        <div className="flex items-center gap-1">
          {link('/', 'Analyze', Search)}
          {link('/reports', 'Reports', LayoutDashboard)}
        </div>
      </div>
    </nav>
  );
}
