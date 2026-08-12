namespace HelpDev.Modules.Search.Application.Indexing;

public enum SearchProjectionOutcome
{
    NoOp = 0,
    Created = 1,
    Updated = 2,
    Skipped = 3,
    Removed = 4,
}
