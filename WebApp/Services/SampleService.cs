using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using WebApp.Data.Entities;
using WebApp.Data.Repositories.Interfaces;
using WebApp.Services.Interfaces;

namespace WebApp.Services
{
    public class SampleService : CrudBaseService<Sample>, ISampleService
    {
        public SampleService(ISampleRepository StoragePlanRepository, IMapper mapper, ILogger<SampleService> logger) :
            base(StoragePlanRepository, mapper, logger)
        { }

        protected override List<string> ModifiableProperties => new()
        {
            nameof(Sample.StringValue),
            nameof(Sample.IntValue),
            nameof(Sample.DecimalValue),
            nameof(Sample.EnumStoredAsString)
        };
    }
}