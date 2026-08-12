import { apiRequest } from "./client";
import type { AuthUserDto } from "./auth";

export type UpdateProfileRequestDto = {
  firstName: string;
  lastName: string;
  email: string;
  profileImageUrl: string;
  expertise: string;
  interests: string;
};

export function fetchMyProfile(token: string, signal?: AbortSignal): Promise<AuthUserDto> {
  return apiRequest<AuthUserDto>({
    path: "/profile/me",
    token,
    signal,
    cache: "no-store",
  });
}

export function updateMyProfile(
  token: string,
  request: UpdateProfileRequestDto,
  signal?: AbortSignal,
): Promise<AuthUserDto> {
  return apiRequest<AuthUserDto>({
    method: "PUT",
    path: "/profile/me",
    token,
    body: request,
    signal,
  });
}
