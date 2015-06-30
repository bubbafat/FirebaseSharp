using System;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable
{
    internal static class TaskExtensions
    {
        public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeout)))
            {
                return await task;
            }

            throw new TimeoutException();
        }
    }
}
