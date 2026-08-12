namespace HelpDev.Modules.Media.Application.Validation;

public interface IImageFileInspector
{
    Task<ImageInspectionResult> InspectAsync(
        Stream content,
        string? declaredContentType,
        CancellationToken cancellationToken = default);
}

public sealed record ImageInspectionResult(
    string DetectedContentType,
    int Width,
    int Height,
    string SafeExtension);
