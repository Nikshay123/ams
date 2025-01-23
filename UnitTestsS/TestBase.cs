using AutoMapper;
using WebApp.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Interfaces;

namespace UnitTests
{
    public class TestBase
    {
        private DbContextOptions<WebAppContext> dbOptions = new DbContextOptionsBuilder<WebAppContext>()
         .UseInMemoryDatabase(databaseName: "WebAppDB")
         .Options;

        protected WebAppContext WebAppContext
        {
            get
            {
                return new WebAppContext(dbOptions);
            }
        }

        protected ITenantDbContextFactory ITenantDbContextFactory
        {
            get
            {
                var tdbf = new Mock<ITenantDbContextFactory>();
                tdbf.Setup(x => x.DbContext<WebAppContext>()).Returns(WebAppContext);
                return tdbf.Object;
            }
        }

        protected IRequestContext IRequestContext
        {
            get
            {
                var tdbf = new Mock<IRequestContext>();
                return tdbf.Object;
            }
        }

        protected IMapper IMapper
        {
            get
            {
                var tdbf = new Mock<IMapper>();
                return tdbf.Object;
            }
        }

        protected IMapper CreateMapper(params Profile[] profiles)
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                foreach (Profile profile in profiles)
                {
                    mc.AddProfile(profile);
                }
            });

            return mappingConfig.CreateMapper();
        }
    }
}