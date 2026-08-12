import { apiRequest } from "./client";

export type SendOtpResponseDto = {
  message: string;
  expiresInSeconds: number;
  otp?: string | null;
};

export type AuthUserDto = {
  id: string;
  mobile: string;
  role: string;
  firstName?: string;
  lastName?: string;
  displayName?: string;
  email?: string;
  profileImageUrl?: string;
  expertise?: string;
  interests?: string;
  profileCompletionPercent?: number;
};

export type VerifyOtpResponseDto = {
  accessToken: string;
  expiresIn: number;
  user: AuthUserDto;
};

export function sendOtp(mobile: string, signal?: AbortSignal): Promise<SendOtpResponseDto> {
  return apiRequest<SendOtpResponseDto>({
    method: "POST",
    path: "/auth/send-otp",
    body: { mobile },
    signal,
  });
}

export function verifyOtp(
  mobile: string,
  code: string,
  signal?: AbortSignal,
): Promise<VerifyOtpResponseDto> {
  return apiRequest<VerifyOtpResponseDto>({
    method: "POST",
    path: "/auth/verify-otp",
    body: { mobile, code },
    signal,
  });
}
