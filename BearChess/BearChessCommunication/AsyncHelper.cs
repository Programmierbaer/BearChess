﻿using System;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    using System.Threading;
    using System.Threading.Tasks;

    public static class AsyncHelper
    {
        private static readonly TaskFactory _taskFactory = new
            TaskFactory(CancellationToken.None,
                        TaskCreationOptions.None,
                        TaskContinuationOptions.None,
                        TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
            => _taskFactory
               .StartNew(func)
               .Unwrap()
               .GetAwaiter()
               .GetResult();

        public static void RunSync(Func<Task> func)
            => _taskFactory
               .StartNew(func)
               .Unwrap()
               .GetAwaiter()
               .GetResult();
    }
}
