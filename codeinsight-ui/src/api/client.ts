import axios from 'axios';
import type { AnalyzeRequest, AnalysisReport, AnalysisReportSummary, CodeIssue, PagedResult } from '../types';

const api = axios.create({ baseURL: '/api' });

export const analyzeRepo = (data: AnalyzeRequest) =>
  api.post<AnalysisReportSummary>('/analyze', data).then(r => r.data);

export const getReports = (page = 1, pageSize = 10) =>
  api.get<PagedResult<AnalysisReportSummary>>('/reports', { params: { page, pageSize } }).then(r => r.data);

export const getReport = (id: number) =>
  api.get<AnalysisReport>(`/reports/${id}`).then(r => r.data);

export const getIssues = (reportId: number, page = 1, pageSize = 20) =>
  api.get<PagedResult<CodeIssue>>(`/issues/${reportId}`, { params: { page, pageSize } }).then(r => r.data);
