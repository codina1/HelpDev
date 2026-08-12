namespace HelpDev.Modules.Content.Domain.AiWorkflow;

public enum ContentIdeaStatus
{
    Draft = 0,
    Researching = 1,
    Writing = 2,
    Review = 3,
    Completed = 4,
    Cancelled = 5,
}

public enum AiContentWorkflowStep
{
    Research = 0,
    Outline = 1,
    Draft = 2,
    Seo = 3,
    Review = 4,
}
