using Microsoft.VisualStudio.TestTools.UnitTesting;
using AircraftTransmissionSystem;
using System;
using System.Text.RegularExpressions;

namespace ATSTests.UnitTests
{
    [TestClass]
    public class PacketTests
    {
        // Packet Serialization Tests
        [TestMethod]
        public void Packet_Should_Serialize_Correctly()
        {
            // Arrange
            Packet packet = new Packet
            {
                header = new Header { TailNumber = "AC123", SequenceNumber = 1 },
                body = new Body { data = "Sample data" },
                trailer = new Trailer { Checksum = 123.45 }
            };

            // Act
            byte[] serializedData = packet.Serialize();

            // Assert
            Assert.IsNotNull(serializedData, "Serialized data should not be null.");
            Assert.IsTrue(serializedData.Length > 0, "Serialized data should not be empty.");
        }

        // Packet Deserialization Tests
        [TestMethod]
        public void Packet_Should_Deserialize_Correctly()
        {
            // Arrange
            Packet originalPacket = new Packet
            {
                header = new Header { TailNumber = "AC123", SequenceNumber = 1 },
                body = new Body { data = "Sample data" },
                trailer = new Trailer { Checksum = 123.45 }
            };
            byte[] serializedData = originalPacket.Serialize();

            // Act
            Packet deserializedPacket = Packet.Deserialize(serializedData);

            // Assert
            Assert.AreEqual(originalPacket.header.TailNumber, deserializedPacket.header.TailNumber, "TailNumber should match.");
            Assert.AreEqual(originalPacket.header.SequenceNumber, deserializedPacket.header.SequenceNumber, "SequenceNumber should match.");
            Assert.AreEqual(originalPacket.body.data, deserializedPacket.body.data, "Body data should match.");
            Assert.AreEqual(originalPacket.trailer.Checksum, deserializedPacket.trailer.Checksum, 0.00001, "Checksum should match.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Packet_Should_Throw_Exception_For_Null_Data()
        {
            // Arrange
            byte[] nullData = null;

            // Act
            Packet.Deserialize(nullData);
        }

        [TestMethod]
        public void Packet_Should_Handle_Empty_Data()
        {
            // Arrange
            Packet packet = new Packet
            {
                header = new Header { TailNumber = "", SequenceNumber = 0 },
                body = new Body { data = "" },
                trailer = new Trailer { Checksum = 0.0 }
            };

            // Act
            byte[] serializedData = packet.Serialize();
            Packet deserializedPacket = Packet.Deserialize(serializedData);

            // Assert
            Assert.AreEqual(packet.header.TailNumber, deserializedPacket.header.TailNumber, "Empty TailNumber should match.");
            Assert.AreEqual(packet.header.SequenceNumber, deserializedPacket.header.SequenceNumber, "Empty SequenceNumber should match.");
            Assert.AreEqual(packet.body.data, deserializedPacket.body.data, "Empty Body data should match.");
            Assert.AreEqual(packet.trailer.Checksum, deserializedPacket.trailer.Checksum, "Empty Checksum should match.");
        }

        [TestMethod]
        public void Packet_Should_Handle_Large_Data()
        {
            // Arrange
            string largeData = new string('A', 10000); // Create a string with 10,000 'A' characters
            Packet packet = new Packet
            {
                header = new Header { TailNumber = "AC123", SequenceNumber = 9999 },
                body = new Body { data = largeData },
                trailer = new Trailer { Checksum = 12345.6789 }
            };

            // Act
            byte[] serializedData = packet.Serialize();
            Packet deserializedPacket = Packet.Deserialize(serializedData);

            // Assert
            Assert.AreEqual(packet.header.TailNumber, deserializedPacket.header.TailNumber, "TailNumber should match for large data.");
            Assert.AreEqual(packet.header.SequenceNumber, deserializedPacket.header.SequenceNumber, "SequenceNumber should match for large data.");
            Assert.AreEqual(packet.body.data, deserializedPacket.body.data, "Body data should match for large data.");
            Assert.AreEqual(packet.trailer.Checksum, deserializedPacket.trailer.Checksum, 0.00001, "Checksum should match for large data.");
        }

        // Regex Validation Tests
        [TestMethod]
        public void Regex_Should_Match_Valid_Telemetry_Line()
        {
            // Arrange
            string validTelemetry = "7_8_2018 19:35:21,-0.799099,0.047375,0.028341,2154.000732,1116.693726,0.022695,0.001006,";
            string pattern = @"^\d{1,2}_\d{1,2}_\d{4} \d{1,2}:\d{1,2}:\d{1,2},-?\d{1,4}\.\d{6},-?\d{1,4}\.\d{6},-?\d{1,4}\.\d{6},-?\d{1,4}\.\d{6},-?\d{1,4}\.\d{6},-?\d{1,4}\.\d{6},-?\d{1,4}\.\d{6},$";
            Regex regex = new Regex(pattern);

            // Act
            bool isMatch = regex.IsMatch(validTelemetry);

            // Assert
            Assert.IsTrue(isMatch, "Regex did not match the valid telemetry line.");
        }

        [TestMethod]
        public void Regex_Should_Not_Match_Invalid_Telemetry_Line()
        {
            // Arrange
            string invalidTelemetry = "Invalid data format";
            Regex regex = new Regex(Program.format);

            // Act
            bool isMatch = regex.IsMatch(invalidTelemetry);

            // Assert
            Assert.IsFalse(isMatch);
        }
    }
}
