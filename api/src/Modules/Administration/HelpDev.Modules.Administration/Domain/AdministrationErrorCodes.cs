namespace HelpDev.Modules.Administration.Domain;

public static class AdministrationErrorCodes
{
    public const string FeatureKeyRequired = "administration_feature_key_required";
    public const string FeatureKeyInvalid = "administration_feature_key_invalid";
    public const string FeatureKeyDuplicate = "administration_feature_key_duplicate";
    public const string FeatureNotFound = "administration_feature_not_found";
    public const string FeatureDescriptionInvalid = "administration_feature_description_invalid";

    public const string SettingKeyRequired = "administration_setting_key_required";
    public const string SettingKeyInvalid = "administration_setting_key_invalid";
    public const string SettingKeyDuplicate = "administration_setting_key_duplicate";
    public const string SettingNotFound = "administration_setting_not_found";
    public const string SettingValueInvalid = "administration_setting_value_invalid";
    public const string SettingValueTooLong = "administration_setting_value_too_long";
    public const string SettingSensitiveKeyForbidden = "administration_setting_sensitive_key_forbidden";

    public const string AnnouncementNotFound = "administration_announcement_not_found";
    public const string AnnouncementTitleRequired = "administration_announcement_title_required";
    public const string AnnouncementTitleInvalid = "administration_announcement_title_invalid";
    public const string AnnouncementBodyRequired = "administration_announcement_body_required";
    public const string AnnouncementBodyInvalid = "administration_announcement_body_invalid";
    public const string AnnouncementScheduleInvalid = "administration_announcement_schedule_invalid";
    public const string AnnouncementStatusInvalid = "administration_announcement_status_invalid";
    public const string AnnouncementCannotDeletePublished = "administration_announcement_cannot_delete_published";

    public const string DashboardUnavailable = "administration_dashboard_unavailable";
    public const string PaginationInvalid = "administration_pagination_invalid";
}
