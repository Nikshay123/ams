using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.StandardSamples
{
    [TestClass]
    public class ServiceSamples : TestBase
    {
        [TestMethod]
        public void ServiceSample()
        {
            //const string LocationName = "Test Location";
            //var locRepoMock = new Mock<IPOLocationRepo>();
            //locRepoMock.Setup(x => x.Location(1)).ReturnsAsync(new Locations { Name = LocationName });

            //var locService = new LocationService(locRepoMock.Object, CreateMapper(new LocationProfile()));
            //var loc = Task.Run(async () => await locService.GetLocationById(1)).Result;

            //Assert.AreEqual(loc.Name, LocationName);
        }
    }
}