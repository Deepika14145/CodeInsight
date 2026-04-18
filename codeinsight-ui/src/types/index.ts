export interface AnalyzeRequest {
  repositoryUrl: string;
  longMethodThreshold: number;
  complexityThreshold: number;
  nestingThreshold: number;
}

export interface AnalysisReportSummary {
  id: number;
  repositoryId: number;
  repositoryName: string;
  repositoryUrl: string;
  status: 'Pending' | 'Running' | 'Completed' | 'Failed';
  totalFilesAnalyzed: number;
  totalIssuesFound: number;
  maintainabilityScore: number;
  codeQualityScore: number;
  progressPercent: number;
  startedAt: string;
  completedAt?: string;
}

export interface AnalysisReport extends AnalysisReportSummary {
  highComplexityCount: number;
  longMethodCount: number;
  duplicateBlockCount: number;
  deepNestingCount: number;
  complexityScore: number;
  errorMessage?: string;
  issues: CodeIssue[];
}

export interface CodeIssue {
  id: number;
  reportId: number;
  filePath: string;
  fileName: string;
  issueType: string;
  severity: 'Critical' | 'High' | 'Medium' | 'Low';
  description: string;
  suggestion: string;
  methodName?: string;
  lineStart?: number;
  lineEnd?: number;
  metricValue?: number;
  codeSnippet?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
