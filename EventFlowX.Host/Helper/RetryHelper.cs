namespace EventFlowX.Host.Helper;

public static class RetryHelper
{
    public static bool CanRetry(int retryCount, int maxRetry)
        => retryCount < maxRetry;
}