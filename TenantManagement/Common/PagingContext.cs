using System.Collections.Generic;

namespace TenantManagement.Common
{
    public class PagingContext
    {
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
    }
}