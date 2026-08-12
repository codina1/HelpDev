using System.Diagnostics;

namespace HelpDev.Integration.Tests.Helpers;

public static class EventuallyAsyncHelper
{
    public static async Task EventuallyAsync(
        Func<Task> assertion,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        pollInterval ??= TimeSpan.FromMilliseconds(200);

        var stopwatch = Stopwatch.StartNew();
        Exception? lastException = null;

        while (stopwatch.Elapsed < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await assertion();
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
            }

            await Task.Delay(pollInterval.Value, cancellationToken);
        }

        throw new TimeoutException(
            $"Assertion did not succeed within {timeout.Value.TotalSeconds:0.#}s.",
            lastException);
    }

    public static Task EventuallyAsync(
        Action assertion,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default) =>
        EventuallyAsync(
            () =>
            {
                assertion();
                return Task.CompletedTask;
            },
            timeout,
            pollInterval,
            cancellationToken);
}
