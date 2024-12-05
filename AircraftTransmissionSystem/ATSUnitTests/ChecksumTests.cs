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
        public void Checksum_Should_Return_Zero_For_Zero_Inputs()
        {
            // Arrange
            double altitude = 0.0;
            double pitch = 0.0;
            double bank = 0.0;

            // Act
            double actualChecksum = Program.checksum(altitude, pitch, bank);

            // Assert
            Assert.AreEqual(0.0, actualChecksum);
        }


        [TestMethod]
        public void Checksum_Should_Handle_Negative_Values()
        {
            // Arrange
            double altitude = -1116.693726;
            double pitch = -0.022695;
            double bank = -0.001006;
            double expectedChecksum = (altitude + pitch + bank) / 3;

            // Act
            double actualChecksum = Program.checksum(altitude, pitch, bank);

            // Assert
            Assert.AreEqual(expectedChecksum, actualChecksum, 0.00001); // Allowable precision tolerance
        }
    }
}
