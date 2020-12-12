using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Transfer.Core
{
    public class TransferManager
    {
        private readonly SemaphoreSlim _mutex;

        public TransferManager(int maxConcurrentTransfers)
        {
            _mutex = maxConcurrentTransfers > 0
                ? new SemaphoreSlim(maxConcurrentTransfers)
                : throw new ArgumentOutOfRangeException(nameof(maxConcurrentTransfers),
                    maxConcurrentTransfers, "Value cannot be less than 1.");
        }

        public Task TransferDataAsync(TransferInfo transfer, Func<Task, TransferInfo, Task> handler = null) =>
            TransferDataAsync(new[] { transfer }, handler);

        public Task TransferDataAsync(IEnumerable<TransferInfo> transfers, Func<Task, TransferInfo, Task> handler = null)
        {
            if (transfers == null)
                throw new ArgumentNullException(nameof(transfers));

            return Task.WhenAll(transfers.Select(async ti =>
            {
                await _mutex.WaitAsync().ConfigureAwait(false);

                var transferTask = ti.Transfer.TransferDataAsync(ti.Progress, ti.Token);
                var handlerTask = handler == null
                    ? Task.CompletedTask
                    : handler(transferTask, ti);

                var task = Task.WhenAll(transferTask, handlerTask);
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch
                {
                    if (handler == null || handlerTask.IsFaulted)
                        throw task.Exception;

                    return;
                }
                finally
                {
                    _mutex.Release();
                }
            }));
        }
    }
}
