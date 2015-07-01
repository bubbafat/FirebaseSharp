using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Utilities
{
    internal static class TaskExtensions
    {
        public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, 
            TimeSpan timeout, 
            CancellationToken cancellationToken)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeout, cancellationToken)))
            {
                return await task;
            }

            throw new TimeoutException();
        }
    }
}
