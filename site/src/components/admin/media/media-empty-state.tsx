import { AdminEmptyState } from "@/components/admin/feedback/admin-empty-state";

type MediaEmptyStateProps = {
  filtered: boolean;
  onClearFilters?: () => void;
  onUpload?: () => void;
};

/** Distinguishes the global-empty and filtered-empty states. */
export function MediaEmptyState({ filtered, onClearFilters, onUpload }: MediaEmptyStateProps) {
  if (filtered) {
    return (
      <AdminEmptyState
        icon="media"
        title="رسانه‌ای با عبارت جستجوی فعلی پیدا نشد"
        description="عبارت جستجو را تغییر دهید یا فیلتر را پاک کنید."
        primaryAction={
          onClearFilters ? (
            <button
              type="button"
              onClick={onClearFilters}
              className="adm-btn adm-btn-outline adm-focus"
            >
              پاک کردن جستجو
            </button>
          ) : undefined
        }
      />
    );
  }

  return (
    <AdminEmptyState
      icon="media"
      title="هنوز رسانه‌ای بارگذاری نشده است"
      description="اولین تصویر خود را بارگذاری کنید."
      primaryAction={
        onUpload ? (
          <button type="button" onClick={onUpload} className="adm-btn adm-btn-primary adm-focus">
            بارگذاری رسانه
          </button>
        ) : undefined
      }
    />
  );
}
