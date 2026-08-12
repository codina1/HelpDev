namespace HelpDev.Modules.Content.Domain.Tools;

[Flags]
public enum PlatformSupport
{
    None = 0,
    Windows = 1,
    Linux = 2,
    MacOS = 4,
    Web = 8,
}
