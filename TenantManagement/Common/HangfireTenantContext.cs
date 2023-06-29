using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading;
using TenantManagement.Common.Interfaces;

namespace TenantManagement.Common
{
    public class BackgroundJobContext : IDisposable
    {
        private static ConcurrentDictionary<int, BackgroundJobContext> JobContext = new();

        public static BackgroundJobContext GetContext()
        {
            BackgroundJobContext jc = null;
            JobContext.TryGetValue(Thread.CurrentThread.ManagedThreadId, out jc);
            return jc;
        }

        public static BackgroundJobContext SetContext(BackgroundJobContext jcp)
        {
            BackgroundJobContext jc = null;
            JobContext.TryAdd(Thread.CurrentThread.ManagedThreadId, jcp);
            return jc;
        }

        public static void ClearContext()
        {
            BackgroundJobContext jc = null;
            JobContext.TryRemove(Thread.CurrentThread.ManagedThreadId, out jc);
        }

        public Guid TenantId { get; set; }

        public BackgroundJobContext()
        { }

        public BackgroundJobContext(Guid tenantId)
        {
            TenantId = tenantId;
            SetContext(this);
        }

        public BackgroundJobContext(IRequestContext rc)
        {
            TenantId = rc.TenantId.Value;
            SetContext(this);
        }

        public void Dispose()
        {
            ClearContext();
        }
    }

    public class BackgroundJobFilter : JobFilterAttribute, IClientFilter
    {
        public void OnCreated(CreatedContext filterContext)
        { }

        public void OnCreating(CreatingContext filterContext)
        {
            var jc = BackgroundJobContext.GetContext();
            filterContext.SetJobParameter(nameof(BackgroundJobContext), jc);
        }
    }

    public class ServiceJobActivatorScope : JobActivatorScope
    {
        private readonly IServiceScope _serviceScope;

        public ServiceJobActivatorScope(IServiceScope serviceScope)
        {
            if (serviceScope == null)
                throw new ArgumentNullException(nameof(serviceScope));

            _serviceScope = serviceScope;
        }

        public override object Resolve(Type type)
        {
            return ActivatorUtilities.GetServiceOrCreateInstance(_serviceScope.ServiceProvider, type);
        }

        public override void DisposeScope()
        {
            _serviceScope.Dispose();
        }
    }

    public class CustomHangfireJobActivator : JobActivator
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CustomHangfireJobActivator(IServiceScopeFactory serviceScopeFactory)
        {
            if (serviceScopeFactory == null)
                throw new ArgumentNullException(nameof(serviceScopeFactory));

            _serviceScopeFactory = serviceScopeFactory;
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            var serviceScope = _serviceScopeFactory.CreateScope();

            //retrieve tenant context from job and apply it to new ioc container
            var jc = context.GetJobParameter<BackgroundJobContext>(nameof(BackgroundJobContext));
            if (jc != null)
            {
                var rc = serviceScope.ServiceProvider.GetRequiredService<IRequestContext>();
                rc.SetBackgroundContext(jc.TenantId);
            }

            return new ServiceJobActivatorScope(serviceScope);
        }
    }
}