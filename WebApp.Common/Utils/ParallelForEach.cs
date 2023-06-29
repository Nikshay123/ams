using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Common.Utils
{
    public static class ParallelForEach
    {
        /// <summary>
        /// Method to run asynchronous foreach function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="dop"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body, int dop = 100)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(dop)
                select Task.Run(async () =>
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            await body(partition.Current);
                        }
                    }
                }));
        }

        /// <summary>
        /// Method to run parallel foreach asynchronous function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="dop"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body, int dop = 100)
        {
            async Task AwaitPartition(IEnumerator<T> partition)
            {
                using (partition)
                {
                    while (partition.MoveNext())
                    {
                        await body(partition.Current);
                    }
                }
            }

            return Task.WhenAll(
                Partitioner
                    .Create(source)
                    .GetPartitions(dop)
                    .AsParallel()
                    .Select(p => AwaitPartition(p)));
        }
    }
}
