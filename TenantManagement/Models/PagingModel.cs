using System.Collections.Generic;
using TenantManagement.Common;

namespace TenantManagement.Models
{
    public class PagingModel<T> : PagingContext
    {
        public PagingModel(PagingContext pagingCtx, List<T> result)
        {
            Total = pagingCtx.Total;
            Limit = pagingCtx.Limit;
            Offset = pagingCtx.Offset;
            Result = result;
        }

        public List<T> Result { get; set; }
    }
}