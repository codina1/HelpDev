import { apiRequest } from "@/lib/api/client";
import type { SeoDashboardRawDto } from "@/lib/admin/seo/seo-types";

export async function fetchSeoDashboard(
  token: string,
  signal?: AbortSignal,
): Promise<SeoDashboardRawDto> {
  return apiRequest<SeoDashboardRawDto>({
    token,
    method: "GET",
    path: "/admin/seo/dashboard",
    signal,
  });
}
