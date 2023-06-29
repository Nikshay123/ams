using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace UnitTests.StandardSamples
{
    [TestClass]
    public class DbSamples : TestBase
    {
        [TestMethod]
        public void DbSample()
        {
            //using (var ccfmCtx = CCFMContext)
            //{
            //    ccfmCtx.Sex.AddRange(new Sex { SexId = 1 }, new Sex { SexId = 2 });
            //    ccfmCtx.SaveChanges();
            //}

            //var repo = new SexRepository(ITenantDbContextFactory);

            //var sex = Task.Run(async () => await repo.GetById(1)).Result;
            //Assert.AreEqual(sex.SexId, 1);

            //sex = Task.Run(async () => await repo.GetById(3)).Result;
            //Assert.IsNull(sex);
        }
    }
}