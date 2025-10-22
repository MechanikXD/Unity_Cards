using System.Threading;

namespace Other
{
    public static class ThreadingExtensions
    {
        public static CancellationTokenSource Reset(this CancellationTokenSource cts)
        {
            cts.Cancel();
            cts.Dispose();
            return new CancellationTokenSource();
        }
    }
}