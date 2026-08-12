import { apiRequest } from "@/lib/api/client";

export type AiProviderStatusDto = {
  name: string;
  configured: boolean;
  healthStatus: string;
  lastSuccessfulCallAtUtc: string | null;
};

export type AiFailureBucketDto = {
  errorCode: string;
  count: number;
};

export type AiUsagePointDto = {
  hourUtc: string;
  requests: number;
  successes: number;
  failures: number;
};

export type AiOperationCountDto = {
  operation: string;
  count: number;
  successes: number;
};

export type AiDashboardDto = {
  requestsToday: number;
  successRate: number;
  averageLatencyMs: number;
  provider: AiProviderStatusDto;
  failures: AiFailureBucketDto[];
  usageByHour: AiUsagePointDto[];
  byOperation: AiOperationCountDto[];
  generatedAtUtc: string;
};

export type AiPolicyDto = {
  title: string;
  rules: string[];
};

export async function fetchAiDashboard(
  token: string,
  signal?: AbortSignal,
): Promise<AiDashboardDto> {
  return apiRequest<AiDashboardDto>({
    token,
    method: "GET",
    path: "/admin/ai",
    signal,
  });
}

export async function fetchAiPolicy(
  token: string,
  signal?: AbortSignal,
): Promise<AiPolicyDto> {
  return apiRequest<AiPolicyDto>({
    token,
    method: "GET",
    path: "/admin/ai/policy",
    signal,
  });
}
