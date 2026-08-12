using HelpDev.Modules.Administration.Domain;

namespace HelpDev.Modules.Administration.Application;

public sealed class AdministrationException : Exception
{
    public AdministrationException(string message, string code)
        : base(message)
    {
        Code = code;
    }

    public AdministrationException(string message, string code, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}

public static class AdministrationApplicationErrorCodes
{
    public const string FeatureKeyRequired = AdministrationErrorCodes.FeatureKeyRequired;
    public const string FeatureKeyInvalid = AdministrationErrorCodes.FeatureKeyInvalid;
    public const string FeatureKeyDuplicate = AdministrationErrorCodes.FeatureKeyDuplicate;
    public const string FeatureNotFound = AdministrationErrorCodes.FeatureNotFound;
    public const string FeatureDescriptionInvalid = AdministrationErrorCodes.FeatureDescriptionInvalid;

    public const string SettingKeyRequired = AdministrationErrorCodes.SettingKeyRequired;
    public const string SettingKeyInvalid = AdministrationErrorCodes.SettingKeyInvalid;
    public const string SettingKeyDuplicate = AdministrationErrorCodes.SettingKeyDuplicate;
    public const string SettingNotFound = AdministrationErrorCodes.SettingNotFound;
    public const string SettingValueInvalid = AdministrationErrorCodes.SettingValueInvalid;
    public const string SettingValueTooLong = AdministrationErrorCodes.SettingValueTooLong;
    public const string SettingSensitiveKeyForbidden = AdministrationErrorCodes.SettingSensitiveKeyForbidden;

    public const string AnnouncementNotFound = AdministrationErrorCodes.AnnouncementNotFound;
    public const string AnnouncementTitleRequired = AdministrationErrorCodes.AnnouncementTitleRequired;
    public const string AnnouncementTitleInvalid = AdministrationErrorCodes.AnnouncementTitleInvalid;
    public const string AnnouncementBodyRequired = AdministrationErrorCodes.AnnouncementBodyRequired;
    public const string AnnouncementBodyInvalid = AdministrationErrorCodes.AnnouncementBodyInvalid;
    public const string AnnouncementScheduleInvalid = AdministrationErrorCodes.AnnouncementScheduleInvalid;
    public const string AnnouncementStatusInvalid = AdministrationErrorCodes.AnnouncementStatusInvalid;
    public const string AnnouncementCannotDeletePublished = AdministrationErrorCodes.AnnouncementCannotDeletePublished;

    public const string DashboardUnavailable = AdministrationErrorCodes.DashboardUnavailable;
    public const string PaginationInvalid = AdministrationErrorCodes.PaginationInvalid;
}
