export type UserRole = "User" | "Writer" | "Admin";

export type AuthUser = {
  id: string;
  mobile: string;
  role: UserRole;
  firstName: string;
  lastName: string;
  displayName: string;
  email: string;
  profileImageUrl: string;
  expertise: string;
  interests: string;
  profileCompletionPercent?: number;
};

export type AuthSession = {
  accessToken: string;
  expiresIn: number;
  user: AuthUser;
};

export type SendOtpResponse = {
  message: string;
  expiresInSeconds: number;
  otp?: string | null;
};

export type UpdateProfileRequest = {
  firstName: string;
  lastName: string;
  email: string;
  profileImageUrl: string;
  expertise: string;
  interests: string;
};

export function getUserDisplayName(user: Pick<AuthUser, "firstName" | "lastName" | "displayName" | "mobile">): string {
  const fullName = `${user.firstName} ${user.lastName}`.trim();
  return fullName || user.displayName || user.mobile;
}

export function getUserInitials(user: Pick<AuthUser, "firstName" | "lastName" | "mobile">): string {
  const first = user.firstName?.trim()?.[0] ?? "";
  const last = user.lastName?.trim()?.[0] ?? "";
  if (first || last) return `${first}${last}`.toUpperCase();
  return user.mobile.slice(-2);
}
