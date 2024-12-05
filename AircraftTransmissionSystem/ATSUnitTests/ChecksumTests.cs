using Microsoft.VisualStudio.TestTools.UnitTesting;
using AircraftTransmissionSystem;

namespace ATSUnitTests
{
    [TestClass]
    public class ChecksumTests
    {
        [TestMethod]
        public void Checksum_Should_Calculate_Correctly()
        {
            // Arrange
            double altitude = 1116.693726;
            double pitch = 0.022695;
            double bank = 0.001006;
            double expectedChecksum = (altitude + pitch + bank) / 3;

            // Act
            double actualChecksum = Program.checksum(altitude, pitch, bank);

            // Assert
            Assert.AreEqual(expectedChecksum, actualChecksum, 0.00001); // Allowable precision tolerance
        }

        [TestMethod]
        public void Checksum_Should_Handle_Large_Values()
        {
            // Arrange
            double altitude = 1e12;
            double pitch = 1e8;
            double bank = 1e6;
            double expectedChecksum = (altitude + pitch + bank) / 3;

            // Act
            double actualChecksum = Program.checksum(altitude, pitch, bank);

            // Assert
            Assert.AreEqual(expectedChecksum, actualChecksum, 0.00001);
        }

        [TestMethod]
        public void Checksum_Should_Handle_Small_Values()
        {
            // Arrange
            double altitude = 1e-12;
            double pitch = 1e-8;
            double bank = 1e-6;
            double expectedChecksum = (altitude + pitch + bank) / 3;

            // Act
            double actualChecksum = Program.checksum(altitude, pitch, bank);

            // Assert
            Assert.AreEqual(expectedChecksum, actualChecksum, 0.00001);
        }

        [TestMethod]
        public void Checksum_Should_Handle_NonFinite_Values()
        {
            // Arrange
            double altitude = double.PositiveInfinity;
            double pitch = double.NegativeInfinity;
            double bank = double.NaN;

            // Act
            double actualChecksum = Program.checksum(altitude, pitch, bank);

            // Assert
            Assert.AreEqual(double.NaN, actualChecksum, "Checksum should return NaN for non-finite inputs.");
        }
    }
}
