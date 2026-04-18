import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Zap, Settings, ChevronDown, ChevronUp } from 'lucide-react';
import { analyzeRepo } from '../api/client';

export default function HomePage() {
  const navigate = useNavigate();
  const [url, setUrl] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showAdvanced, setShowAdvanced] = useState(false);
  const [opts, setOpts] = useState({ longMethodThreshold: 30, complexityThreshold: 10, nestingThreshold: 4 });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!url.trim()) return;
    setError('');
    setLoading(true);
    try {
      const report = await analyzeRepo({ repositoryUrl: url.trim(), ...opts });
      navigate(`/reports/${report.id}`);
    } catch (err: any) {
      setError(err.response?.data?.message ?? 'Failed to start analysis. Check the URL and try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-[calc(100vh-56px)] flex flex-col items-center justify-center px-4 py-16">
      {/* Hero */}
      <div className="text-center mb-12 max-w-2xl">
        <div className="inline-flex items-center gap-2 bg-indigo-500/10 border border-indigo-500/20 rounded-full px-4 py-1.5 text-indigo-400 text-sm mb-6">
          <Zap size={14} /> Powered by Roslyn .NET Compiler Platform
        </div>
        <h1 className="text-5xl font-bold text-white mb-4 leading-tight">
          Analyze Technical Debt<br />
          <span className="text-indigo-400">in Your Codebase</span>
        </h1>
        <p className="text-gray-400 text-lg">
          Paste a GitHub repository URL and get a full report on cyclomatic complexity,
          long methods, deep nesting, duplicate code, and maintainability score.
        </p>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit} className="w-full max-w-2xl">
        <div className="bg-gray-900 border border-gray-800 rounded-2xl p-6 shadow-2xl">
          <label className="block text-sm text-gray-400 mb-2">GitHub Repository URL</label>
          <div className="flex gap-3">
            <div className="flex-1 flex items-center gap-2 bg-gray-800 border border-gray-700 rounded-xl px-4 focus-within:border-indigo-500 transition-colors">
              <svg viewBox="0 0 24 24" fill="currentColor" className="w-[18px] h-[18px] text-gray-500 shrink-0">
                <path d="M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0 0 24 12c0-6.63-5.37-12-12-12z"/>
              </svg>
              <input
                type="url"
                value={url}
                onChange={e => setUrl(e.target.value)}
                placeholder="https://github.com/owner/repository"
                className="flex-1 bg-transparent py-3 text-white placeholder-gray-600 outline-none text-sm"
                required
              />
            </div>
            <button
              type="submit"
              disabled={loading}
              className="px-6 py-3 bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed text-white font-semibold rounded-xl transition-colors flex items-center gap-2 whitespace-nowrap"
            >
              {loading ? (
                <><span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" /> Analyzing...</>
              ) : (
                <><Zap size={16} /> Analyze</>
              )}
            </button>
          </div>

          {error && <p className="mt-3 text-red-400 text-sm">{error}</p>}

          {/* Advanced options */}
          <button
            type="button"
            onClick={() => setShowAdvanced(v => !v)}
            className="mt-4 flex items-center gap-1 text-xs text-gray-500 hover:text-gray-300 transition-colors"
          >
            <Settings size={12} /> Advanced Options {showAdvanced ? <ChevronUp size={12} /> : <ChevronDown size={12} />}
          </button>

          {showAdvanced && (
            <div className="mt-4 grid grid-cols-3 gap-4 pt-4 border-t border-gray-800">
              {[
                { key: 'longMethodThreshold', label: 'Long Method (lines)', min: 10, max: 200 },
                { key: 'complexityThreshold', label: 'Complexity Threshold', min: 5, max: 50 },
                { key: 'nestingThreshold', label: 'Nesting Depth', min: 2, max: 10 },
              ].map(({ key, label, min, max }) => (
                <div key={key}>
                  <label className="text-xs text-gray-400 block mb-1">{label}</label>
                  <input
                    type="number"
                    min={min} max={max}
                    value={opts[key as keyof typeof opts]}
                    onChange={e => setOpts(o => ({ ...o, [key]: +e.target.value }))}
                    className="w-full bg-gray-800 border border-gray-700 rounded-lg px-3 py-2 text-white text-sm outline-none focus:border-indigo-500"
                  />
                </div>
              ))}
            </div>
          )}
        </div>
      </form>

      {/* Feature pills */}
      <div className="flex flex-wrap justify-center gap-3 mt-10">
        {['Cyclomatic Complexity', 'Long Methods', 'Deep Nesting', 'Duplicate Code', 'Naming Conventions', 'Maintainability Score'].map(f => (
          <span key={f} className="px-3 py-1.5 bg-gray-900 border border-gray-800 rounded-full text-xs text-gray-400">{f}</span>
        ))}
      </div>
    </div>
  );
}
