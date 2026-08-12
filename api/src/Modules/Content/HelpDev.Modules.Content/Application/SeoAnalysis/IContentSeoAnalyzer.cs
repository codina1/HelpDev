namespace HelpDev.Modules.Content.Application.SeoAnalysis;

public interface IContentSeoAnalyzer
{
    SeoAnalysisReportDto Analyze(SeoAnalysisInput input, DateTime analyzedAtUtc);
}
