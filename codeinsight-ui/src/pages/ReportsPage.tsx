import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getReports } from '../api/client';
import type { AnalysisReportSummary, PagedResult } from '../types';
import ProgressBar from '../components/ProgressBar';
import { ExternalLink, ChevronLeft, ChevronRight } from 'lucide-react';

export default function ReportsPage() {
  const [data, setData] = useState<PagedResult<AnalysisReportSummary> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    getReports(page, 10).then(setData).finally(() => setLoading(false));
  }, [page]);

  const scoreColor = (s: number) => s >= 70 ? 'text-green-400' : s >= 40 ? 'text-yellow-400' : 'text-red-400';

  return (
    <div className="max-w-7xl mx-auto px-4 py-10">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-2xl font-bold text-white">Analysis Reports</h1>
          <p className="text-gray-400 text-sm mt-1">{data?.totalCount ?? 0} total reports</p>
        </div>
        <Link to="/" className="px-4 py-2 bg-indigo-600 hover:bg-indigo-500 text-white text-sm rounded-lg transition-colors">
          + New Analysis
        </Link>
      </div>

      {loading ? (
        <div className="flex justify-center py-20">
          <span className="w-8 h-8 border-2 border-indigo-500/30 border-t-indigo-500 rounded-full animate-spin" />
        </div>
      ) : (
        <>
          <div className="bg-gray-900 border border-gray-800 rounded-xl overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-800 text-gray-400 text-xs uppercase tracking-wider">
                  <th className="text-left px-5 py-3">Repository</th>
                  <th className="text-left px-5 py-3">Status</th>
                  <th className="text-right px-5 py-3">Files</th>
                  <th className="text-right px-5 py-3">Issues</th>
                  <th className="text-right px-5 py-3">Quality</th>
                  <th className="text-right px-5 py-3">Started</th>
                  <th className="px-5 py-3" />
                </tr>
              </thead>
              <tbody>
                {data?.items.map(r => (
                  <tr key={r.id} className="border-b border-gray-800/50 hover:bg-gray-800/30 transition-colors">
                    <td className="px-5 py-4">
                      <p className="font-medium text-white">{r.repositoryName}</p>
                      <p className="text-gray-500 text-xs truncate max-w-xs">{r.repositoryUrl}</p>
                    </td>
                    <td className="px-5 py-4 w-40">
                      <ProgressBar percent={r.progressPercent} status={r.status} />
                    </td>
                    <td className="px-5 py-4 text-right text-gray-300">{r.totalFilesAnalyzed}</td>
                    <td className="px-5 py-4 text-right text-gray-300">{r.totalIssuesFound}</td>
                    <td className={`px-5 py-4 text-right font-bold ${scoreColor(r.codeQualityScore)}`}>
                      {r.codeQualityScore > 0 ? `${Math.round(r.codeQualityScore)}/100` : '—'}
                    </td>
                    <td className="px-5 py-4 text-right text-gray-500 text-xs">
                      {new Date(r.startedAt).toLocaleDateString()}
                    </td>
                    <td className="px-5 py-4 text-right">
                      <Link to={`/reports/${r.id}`} className="text-indigo-400 hover:text-indigo-300 transition-colors">
                        <ExternalLink size={16} />
                      </Link>
                    </td>
                  </tr>
                ))}
                {data?.items.length === 0 && (
                  <tr><td colSpan={7} className="text-center py-16 text-gray-500">No reports yet. Start your first analysis.</td></tr>
                )}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {(data?.totalPages ?? 0) > 1 && (
            <div className="flex justify-center items-center gap-3 mt-6">
              <button onClick={() => setPage(p => p - 1)} disabled={page === 1}
                className="p-2 rounded-lg bg-gray-800 disabled:opacity-40 hover:bg-gray-700 transition-colors">
                <ChevronLeft size={16} />
              </button>
              <span className="text-sm text-gray-400">Page {page} of {data?.totalPages}</span>
              <button onClick={() => setPage(p => p + 1)} disabled={page === data?.totalPages}
                className="p-2 rounded-lg bg-gray-800 disabled:opacity-40 hover:bg-gray-700 transition-colors">
                <ChevronRight size={16} />
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
