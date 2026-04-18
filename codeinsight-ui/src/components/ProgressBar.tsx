export default function ProgressBar({ percent, status }: { percent: number; status: string }) {
  const color = status === 'Failed' ? 'bg-red-500' : status === 'Completed' ? 'bg-green-500' : 'bg-indigo-500';
  return (
    <div className="w-full">
      <div className="flex justify-between text-xs text-gray-400 mb-1">
        <span>{status}</span>
        <span>{percent}%</span>
      </div>
      <div className="h-2 bg-gray-800 rounded-full overflow-hidden">
        <div
          className={`h-full ${color} rounded-full transition-all duration-500`}
          style={{ width: `${percent}%` }}
        />
      </div>
    </div>
  );
}
