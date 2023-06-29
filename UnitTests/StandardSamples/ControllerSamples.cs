using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace UnitTests.StandardSamples
{
    [TestClass]
    public class ControllerSamples : TestBase
    {
        [TestMethod]
        public void ControllerSample()
        {
            //const string SexName = "Test Sex";
            //var sexServiceMock = new Mock<ISexService>();
            //sexServiceMock.Setup(x => x.GetSexById(1)).ReturnsAsync(new SexModel { SexId = 1, Name = SexName });

            //var sexController = new SexController(sexServiceMock.Object);
            //var actionResult = Task.Run(async () => await sexController.GetSexById(1)).Result;

            //var model = (actionResult?.Result as OkObjectResult)?.Value as SexModel;
            //Assert.IsNotNull(model);
            //Assert.AreEqual(model.Name, SexName);
        }
    }
}