using System;

namespace TenantManagement.Models
{
    public class BackgroundJobSpecModel
    {
        public string Name { get; set; }
        public object MethodCall { get; set; }
        public Type GenericMethodType { get; set; }
        public TimeSpan? Delay { get; set; }
    }
}