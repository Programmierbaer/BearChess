using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChess.HidDriver;

namespace UnitTestsBearChessBase
{
    [TestClass]
    public class UnitTestHidDevice
    {
        private static ushort vendorId = 0x2D80;
        private static ushort usagePage = 0xFF00;
        private readonly byte[] _startReading = { 0x21, 0x01, 0x00 };

        [TestMethod]
        public void FindDevices()
        {
            var deviceHandling = new DeviceHandling();
            var numberOfDevices = deviceHandling.GetNumberOfDevices();
            Assert.IsNotNull(numberOfDevices);
        }


        [TestMethod]
        public void FindChessnutDevice()
        {
            var deviceHandling = new DeviceHandling();
            var findDeviceNumber = deviceHandling.FindReadDevice(vendorId, usagePage);
            Assert.IsTrue(findDeviceNumber);
            var manufacturer = deviceHandling.GetManufacturer();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(manufacturer));
            var product = deviceHandling.GetProduct();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(product));
            deviceHandling.Write(_startReading);
            var read = deviceHandling.Read();
            Assert.IsTrue(read.Length>0);
        }
    }
}