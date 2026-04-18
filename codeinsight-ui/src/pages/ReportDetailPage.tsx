import { useEffect, useState, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getReport, getIssues } from '../api/client';
import type { AnalysisReport, CodeIssue, PagedResult } from '../types';
import ScoreRing from '../components/ScoreRing';
import StatCard from '../components/StatCard';
import SeverityBadge from '../components/SeverityBadge';
import ProgressBar from '../components/ProgressBar';
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer,
  PieChart, Pie, Cell, Legend
} from 'recharts';
import { FileCode, AlertTriangle, Layers, GitBranch, ChevronLeft, ChevronRight, ArrowLeft, Lightbulb } from 'lucide-react';

const SEVERITY_COLORS: Record<string, string> = {
  Critical: '#ef4444', High: '#f97316', Medium: '#eab308', Low: '#3b82f6'
};

const ISSUE_COLORS = ['#6366f1', '#22c55e', '#f59e0b', '#ef4444', '#8b5cf6'];

export default function ReportDetailPage() {
  const { id } = useParams<{ id: string }>();
  const reportId = Number(id);

  const [report, setReport] = useState<AnalysisReport | null>(null);
  const [issues, setIssues] = useState<PagedResult<CodeIssue> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [selectedIssue, setSelectedIssue] = useState<CodeIssue | null>(null);
  const [activeTab, setActiveTab] = useState<'overview' | 'issues'>('overview');

  const fetchReport = useCallback(async () => {
    const r = await getReport(reportId);
    setReport(r);
    return r;
  }, [reportId]);

  // Poll while running
  useEffect(() => {
    let interval: ReturnType<typeof setInterval>;
    const load = async () => {
      setLoading(true);
      const r = await fetchReport();
      setLoading(false);
      if (r && (r.status === 'Pending' || r.status === 'Running')) {
        interval = setInterval(async () => {
          const updated = await fetchReport();
          if (updated?.status === 'Completed' || updated?.status === 'Failed') {
            clearInterval(interval);
          }
        }, 1000);
      }
    };
    load();
    return () => clearInterval(interval);
  }, [fetchReport]);

  useEffect(() => {
    if (report?.status === 'Completed') {
      getIssues(reportId, page, 20).then(setIssues);
    }
  }, [report?.status, reportId, page]);

  if (loading) return (
    <div className="flex justify-center items-center min-h-[60vh]">
      <span className="w-10 h-10 border-2 border-indigo-500/30 border-t-indigo-500 rounded-full animate-spin" />
    </div>
  );

  if (!report) return (
    <div className="text-center py-20 text-gray-400">Report not found.</div>
  );

  const isRunning = report.status === 'Pending' || report.status === 'Running';

  const issueTypeData = [
    { name: 'Complexity', value: report.highComplexityCount },
    { name: 'Long Methods', value: report.longMethodCount },
    { name: 'Duplicates', value: report.duplicateBlockCount },
    { name: 'Deep Nesting', value: report.deepNestingCount },
  ].filter(d => d.value > 0);

  const severityData = ['Critical', 'High', 'Medium', 'Low'].map(s => ({
    name: s,
    value: report.issues.filter(i => i.severity === s).length
  })).filter(d => d.value > 0);

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="flex items-start justify-between mb-8">
        <div>
          <Link to="/reports" className="flex items-center gap-1 text-gray-500 hover:text-gray-300 text-sm mb-3 transition-colors">
            <ArrowLeft size={14} /> All Reports
          </Link>
          <h1 className="text-2xl font-bold text-white">{report.repositoryName}</h1>
          <a href={report.repositoryUrl} target="_blank" rel="noreferrer"
            className="text-indigo-400 hover:text-indigo-300 text-sm transition-colors">{report.repositoryUrl}</a>
        </div>
        {report.status === 'Completed' && report.totalFilesAnalyzed > 0 && (
          <div className="flex gap-4">
            <ScoreRing score={report.maintainabilityScore} label="Maintainability" />
            <ScoreRing score={report.complexityScore} label="Complexity" />
            <ScoreRing score={report.codeQualityScore} label="Quality" />
          </div>
        )}
      </div>

      {/* Progress */}
      {isRunning && (
        <div className="bg-gray-900 border border-gray-800 rounded-xl p-6 mb-6">
          <p className="text-white font-medium mb-3">Analysis in progress...</p>
          <ProgressBar percent={report.progressPercent} status={report.status} />
          <p className="text-gray-500 text-xs mt-2">This may take a minute depending on repository size.</p>
        </div>
      )}

      {report.status === 'Failed' && (
        <div className="bg-red-500/10 border border-red-500/30 rounded-xl p-4 mb-6 text-red-400">
          Analysis failed: {report.errorMessage}
        </div>
      )}

      {report.status === 'Completed' && report.totalFilesAnalyzed === 0 && (
        <div className="bg-yellow-500/10 border border-yellow-500/30 rounded-xl p-4 mb-6 text-yellow-400">
          ⚠ No C# (.cs) files found in this repository. CodeInsight only analyzes .NET/C# codebases. Try a C# repository like <a href="https://github.com/dotnet/aspnetcore" className="underline" target="_blank">dotnet/aspnetcore</a>.
        </div>
      )}

      {report.status === 'Completed' && report.totalFilesAnalyzed > 0 && (
        <>
          {/* Stat cards */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
            <StatCard label="Total Issues" value={report.totalIssuesFound} icon={<AlertTriangle size={18} />} color="text-red-400" />
            <StatCard label="Files Analyzed" value={report.totalFilesAnalyzed} icon={<FileCode size={18} />} color="text-indigo-400" />
            <StatCard label="High Complexity" value={report.highComplexityCount} icon={<GitBranch size={18} />} color="text-orange-400" />
            <StatCard label="Duplicate Blocks" value={report.duplicateBlockCount} icon={<Layers size={18} />} color="text-yellow-400" />
          </div>

          {/* Tabs */}
          <div className="flex gap-1 mb-6 bg-gray-900 border border-gray-800 rounded-xl p-1 w-fit">
            {(['overview', 'issues'] as const).map(tab => (
              <button key={tab} onClick={() => setActiveTab(tab)}
                className={`px-5 py-2 rounded-lg text-sm font-medium capitalize transition-colors ${
                  activeTab === tab ? 'bg-indigo-600 text-white' : 'text-gray-400 hover:text-white'
                }`}>
                {tab}
              </button>
            ))}
          </div>

          {activeTab === 'overview' && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Issue type bar chart */}
              <div className="bg-gray-900 border border-gray-800 rounded-xl p-5">
                <h3 className="text-white font-semibold mb-4">Issues by Type</h3>
                <ResponsiveContainer width="100%" height={220}>
                  <BarChart data={issueTypeData} barSize={32}>
                    <XAxis dataKey="name" tick={{ fill: '#9ca3af', fontSize: 12 }} axisLine={false} tickLine={false} />
                    <YAxis tick={{ fill: '#9ca3af', fontSize: 12 }} axisLine={false} tickLine={false} />
                    <Tooltip contentStyle={{ background: '#111827', border: '1px solid #374151', borderRadius: 8 }} />
                    <Bar dataKey="value" radius={[4, 4, 0, 0]}>
                      {issueTypeData.map((_, i) => <Cell key={i} fill={ISSUE_COLORS[i % ISSUE_COLORS.length]} />)}
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
              </div>

              {/* Severity pie chart */}
              <div className="bg-gray-900 border border-gray-800 rounded-xl p-5">
                <h3 className="text-white font-semibold mb-4">Issues by Severity</h3>
                <ResponsiveContainer width="100%" height={220}>
                  <PieChart>
                    <Pie data={severityData} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={80} label={({ name, percent }) => `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}>
                      {severityData.map((entry, i) => <Cell key={i} fill={SEVERITY_COLORS[entry.name]} />)}
                    </Pie>
                    <Legend wrapperStyle={{ fontSize: 12, color: '#9ca3af' }} />
                    <Tooltip contentStyle={{ background: '#111827', border: '1px solid #374151', borderRadius: 8 }} />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            </div>
          )}

          {activeTab === 'issues' && (
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              {/* Issues table */}
              <div className="bg-gray-900 border border-gray-800 rounded-xl overflow-hidden">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-gray-800 text-gray-400 text-xs uppercase tracking-wider">
                      <th className="text-left px-4 py-3">File / Method</th>
                      <th className="text-left px-4 py-3">Type</th>
                      <th className="text-left px-4 py-3">Severity</th>
                    </tr>
                  </thead>
                  <tbody>
                    {issues?.items.map(issue => (
                      <tr key={issue.id}
                        onClick={() => setSelectedIssue(issue)}
                        className={`border-b border-gray-800/50 cursor-pointer transition-colors ${
                          selectedIssue?.id === issue.id ? 'bg-indigo-600/10' : 'hover:bg-gray-800/40'
                        }`}>
                        <td className="px-4 py-3">
                          <p className="text-white font-medium truncate max-w-[180px]">{issue.fileName}</p>
                          {issue.methodName && <p className="text-gray-500 text-xs">{issue.methodName}</p>}
                        </td>
                        <td className="px-4 py-3 text-gray-300 text-xs">{issue.issueType}</td>
                        <td className="px-4 py-3"><SeverityBadge severity={issue.severity} /></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {/* Pagination */}
                {(issues?.totalPages ?? 0) > 1 && (
                  <div className="flex justify-center items-center gap-3 p-3 border-t border-gray-800">
                    <button onClick={() => setPage(p => p - 1)} disabled={page === 1}
                      className="p-1.5 rounded bg-gray-800 disabled:opacity-40 hover:bg-gray-700">
                      <ChevronLeft size={14} />
                    </button>
                    <span className="text-xs text-gray-400">{page} / {issues?.totalPages}</span>
                    <button onClick={() => setPage(p => p + 1)} disabled={page === issues?.totalPages}
                      className="p-1.5 rounded bg-gray-800 disabled:opacity-40 hover:bg-gray-700">
                      <ChevronRight size={14} />
                    </button>
                  </div>
                )}
              </div>

              {/* Issue detail panel */}
              <div className="bg-gray-900 border border-gray-800 rounded-xl p-5">
                {selectedIssue ? (
                  <div>
                    <div className="flex items-start justify-between mb-4">
                      <div>
                        <p className="text-white font-semibold">{selectedIssue.fileName}</p>
                        {selectedIssue.methodName && <p className="text-gray-400 text-sm">{selectedIssue.methodName}</p>}
                        {selectedIssue.lineStart && (
                          <p className="text-gray-500 text-xs">Lines {selectedIssue.lineStart}–{selectedIssue.lineEnd}</p>
                        )}
                      </div>
                      <SeverityBadge severity={selectedIssue.severity} />
                    </div>

                    <p className="text-gray-300 text-sm mb-4">{selectedIssue.description}</p>

                    {selectedIssue.metricValue !== undefined && (
                      <div className="bg-gray-800 rounded-lg px-3 py-2 text-xs text-gray-400 mb-4">
                        Metric value: <span className="text-white font-bold">{selectedIssue.metricValue}</span>
                      </div>
                    )}

                    <div className="flex items-start gap-2 bg-indigo-500/10 border border-indigo-500/20 rounded-lg p-3 mb-4">
                      <Lightbulb size={14} className="text-indigo-400 mt-0.5 shrink-0" />
                      <p className="text-indigo-300 text-sm">{selectedIssue.suggestion}</p>
                    </div>

                    {selectedIssue.codeSnippet && (
                      <pre className="bg-gray-950 border border-gray-800 rounded-lg p-3 text-xs text-gray-300 overflow-x-auto whitespace-pre-wrap font-mono">
                        {selectedIssue.codeSnippet}
                      </pre>
                    )}
                  </div>
                ) : (
                  <div className="flex flex-col items-center justify-center h-full text-gray-500 py-16">
                    <FileCode size={32} className="mb-3 opacity-30" />
                    <p className="text-sm">Select an issue to see details</p>
                  </div>
                )}
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
