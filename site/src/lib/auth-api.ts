import { sendOtp as sendOtpV1, verifyOtp as verifyOtpV1, type AuthUserDto } from "@/lib/api/auth";
import type { AuthSession, AuthUser, SendOtpResponse } from "@/types/auth";

export function getAuthHeaders(token: string): HeadersInit {
  return {
    Authorization: `Bearer ${token}`,
  };
}

function mapAuthUser(data: AuthUserDto | Record<string, unknown>): AuthUser {
  const record = data as Record<string, unknown>;
  return {
    id: String(record.id),
    mobile: String(record.mobile),
    role: record.role as AuthUser["role"],
    firstName: String(record.firstName ?? ""),
    lastName: String(record.lastName ?? ""),
    displayName: String(record.displayName ?? record.mobile ?? ""),
    email: String(record.email ?? ""),
    profileImageUrl: String(record.profileImageUrl ?? ""),
    expertise: String(record.expertise ?? ""),
    interests: String(record.interests ?? ""),
    profileCompletionPercent: Number(record.profileCompletionPercent ?? 0),
  };
}

export async function sendOtp(mobile: string, signal?: AbortSignal): Promise<SendOtpResponse> {
  const response = await sendOtpV1(mobile, signal);
  return {
    message: response.message,
    expiresInSeconds: response.expiresInSeconds,
    otp: response.otp ?? null,
  };
}

export async function verifyOtp(
  mobile: string,
  code: string,
  signal?: AbortSignal,
): Promise<AuthSession> {
  const data = await verifyOtpV1(mobile, code, signal);
  return {
    accessToken: data.accessToken,
    expiresIn: data.expiresIn,
    user: mapAuthUser(data.user),
  };
}

export { mapAuthUser };
