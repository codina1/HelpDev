/**
 * Notification foundation types (Sprint 45).
 *
 * No backend notifications feed exists yet. UI must not invent items.
 * When an API lands, map responses into NotificationItem and drop the empty state.
 */

export type NotificationItem = {
  id: string;
  title: string;
  body: string;
  createdAtUtc: string;
  read: boolean;
  href?: string | null;
};

export type NotificationFeed = {
  items: NotificationItem[];
  unreadCount: number;
};

/** Placeholder until a real notifications endpoint is available. */
export function emptyNotificationFeed(): NotificationFeed {
  return { items: [], unreadCount: 0 };
}
