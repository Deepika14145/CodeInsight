const colors: Record<string, string> = {
  Critical: 'bg-red-500/20 text-red-400 border border-red-500/30',
  High: 'bg-orange-500/20 text-orange-400 border border-orange-500/30',
  Medium: 'bg-yellow-500/20 text-yellow-400 border border-yellow-500/30',
  Low: 'bg-blue-500/20 text-blue-400 border border-blue-500/30',
};

export default function SeverityBadge({ severity }: { severity: string }) {
  return (
    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${colors[severity] ?? 'bg-gray-700 text-gray-300'}`}>
      {severity}
    </span>
  );
}
