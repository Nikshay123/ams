using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Common.Interfaces;
using TenantManagement.Models;
using TenantManagement.Services.Interfaces;

namespace TenantManagement.Services
{
    public class BackgroundJobService : IBackgroundJobService, IDisposable
    {
        protected readonly IRequestContext _requestContext;
        private readonly ILogger<BackgroundJobService> _logger;
        protected ConcurrentBag<BackgroundJobSpecModel> _jobs = new();
        protected ConcurrentBag<string> _completedJobNames = new();

        public BackgroundJobService(IRequestContext requestContext, ILogger<BackgroundJobService> logger)
        {
            _requestContext = requestContext;
            _logger = logger;
        }

        public bool RegisterJob<T>(Expression<Func<T, Task>> methodCall, TimeSpan? delay = null, string name = null)
        {
            if (!string.IsNullOrEmpty(name) && _completedJobNames.Contains(name))
            {
                return true;
            }

            if (System.Transactions.Transaction.Current == null)
            {
                using var jc = new BackgroundJobContext(_requestContext.TenantId.Value);
                if (delay == null)
                {
                    Hangfire.BackgroundJob.Enqueue<T>(methodCall);
                }
                else
                {
                    Hangfire.BackgroundJob.Schedule<T>(methodCall, delay.Value);
                }

                if (!string.IsNullOrEmpty(name))
                {
                    _completedJobNames.Add(name);
                }

                return true;
            }
            else
            {
                var job = _jobs.FirstOrDefault(x => !string.IsNullOrEmpty(x.Name) && x.Name == name);
                if (job == null)
                {
                    job = new BackgroundJobSpecModel
                    {
                        Name = name,
                        MethodCall = methodCall,
                        GenericMethodType = typeof(T),
                        Delay = delay
                    };

                    _jobs.Add(job);
                }
                else
                {
                    job.MethodCall = methodCall;
                    job.GenericMethodType = typeof(T);
                    job.Delay = delay;
                }
            }

            return false;
        }

        public bool Dispatch()
        {
            if (System.Transactions.Transaction.Current == null)
            {
                foreach (var job in _jobs)
                {
                    using var jc = new BackgroundJobContext(_requestContext.TenantId.Value);

                    if (job.Delay == null)
                    {
                        var enqueueRef = typeof(Hangfire.BackgroundJob).GetMethods()
                            .Where(x => x.Name == nameof(Hangfire.BackgroundJob.Enqueue) && x.IsGenericMethod && x.GetParameters().FirstOrDefault(x => x.ToString().Contains("System.Func")) != null)
                            .FirstOrDefault();

                        if (enqueueRef != null)
                        {
                            var genericRef = enqueueRef.MakeGenericMethod(job.GenericMethodType);
                            genericRef.Invoke(null, new[] { job.MethodCall });
                        }
                    }
                    else
                    {
                        var scheduleRef = typeof(Hangfire.BackgroundJob).GetMethods()
                            .Where(x => x.Name == nameof(Hangfire.BackgroundJob.Schedule) && x.IsGenericMethod && x.GetParameters().FirstOrDefault(x => x.ToString().Contains("System.Func")) != null)
                            .FirstOrDefault();

                        if (scheduleRef != null)
                        {
                            var genericRef = scheduleRef.MakeGenericMethod(job.GenericMethodType);
                            genericRef.Invoke(null, new[] { job.MethodCall, job.Delay.Value });
                        }
                    }

                    if (!string.IsNullOrEmpty(job.Name))
                    {
                        _completedJobNames.Add(job.Name);
                    }
                }

                _jobs.Clear();
                return true;
            }

            return false;
        }

        private bool Disposed = false;

        public void Dispose()
        {
            if (!Disposed)
            {
                _logger.LogDebug("Disposing: Dispatching Queued Background Jobs");
                try
                {
                    Dispatch();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DispatchJobs threw an exception!!!");
                }
                finally
                {
                    Disposed = true;
                }
            }
        }
    }
}