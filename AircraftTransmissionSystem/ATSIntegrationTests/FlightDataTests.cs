using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using AircraftTransmissionSystem;
using System;

namespace ATSIntegrationTests
{
    [TestClass]
    public class ATSIntegrationTests
    {
        // Path to the logs in the ATS project
        private readonly string logsDirectory = Path.Combine(
            Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName,
            "AircraftTransmissionSystem\\bin\\Debug\\logs"
        );

        // Unit Test: Checksum Validation
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
            Assert.AreEqual(expectedChecksum, actualChecksum, 0.00001, "Checksum calculation is incorrect.");
        }

        // Unit Test: Regex Validation
        [TestMethod]
        public void Regex_Should_Match_Valid_Telemetry_Line()
        {
            // Arrange
            string validTelemetry = "7_8_2018 19:34:3,-0.319754,-0.716176,1.797150,2154.670410,1643.844116,0.022278,0.033622,";
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
            string pattern = @"^\d{1,2}_\d{1,2}_\d{4} \d{1,2}:\d{1,2}:\d{1,2},-?\d{1,4}\.\d{6},-?\d{1,4}\.\d{6},-?\d{1,4}\.\d{6},-?\d{1,4}\.\d{6},-?\d{1,4}\.\d{6},-?\d{1,4}\.\d{6},-?\d{1,4}\.\d{6},$";
            Regex regex = new Regex(pattern);

            // Act
            bool isMatch = regex.IsMatch(invalidTelemetry);

            // Assert
            Assert.IsFalse(isMatch, "Regex matched an invalid telemetry line.");
        }

        // Integration Test: Parse Valid Log File
        [TestMethod]
        public void Parse_Valid_Log_File_Should_Return_All_FlightDataEntries()
        {
            // Arrange
            string logFilePath = Path.Combine(logsDirectory, "C-FGAX.txt");

            // Act
            var entries = FlightDataParser.ParseFile(logFilePath);

            // Assert
            Assert.IsNotNull(entries, "Entries should not be null.");
            Assert.IsTrue(entries.Count > 0, "The log file should contain at least one entry.");
            Assert.AreEqual("7_8_2018 19:34:3", entries[0].Timestamp, "First entry timestamp should match.");
        }

        // Integration Test: Handle Invalid Log File
        [TestMethod]
        public void Parse_Log_File_With_Invalid_Lines_Should_Skip_Invalid_Entries()
        {
            // Arrange
            string logFilePath = Path.Combine(logsDirectory, "C-GEFC.txt");

            // Act
            var entries = FlightDataParser.ParseFile(logFilePath);

            // Assert
            Assert.IsNotNull(entries, "Entries should not be null.");
            Assert.IsTrue(entries.Count > 0, "The log file should contain valid entries.");
        }

        // Integration Test: Process Large Log File
        [TestMethod]
        public void Process_Large_Log_File_Should_Handle_All_Lines()
        {
            // Arrange
            string logFilePath = Path.Combine(logsDirectory, "C-QWWT.txt");

            // Act
            var entries = FlightDataParser.ParseFile(logFilePath);

            // Debugging: Log the actual count
            Console.WriteLine($"Actual Entry Count: {entries.Count}");

            // Assert
            Assert.IsNotNull(entries, "Entries should not be null.");
            Assert.IsTrue(entries.Count > 500, $"The log file should contain more than 500 entries. Actual count: {entries.Count}");
        }


        // Integration Test: Parse Log File and Create Packets
        [TestMethod]
        public void Parse_Log_File_Should_Create_Valid_Packets()
        {
            // Arrange
            string logFilePath = Path.Combine(logsDirectory, "C-FGAX.txt");
            var entries = FlightDataParser.ParseFile(logFilePath);
            var packets = new List<Packet>();

            // Act
            foreach (var entry in entries)
            {
                var packet = new Packet
                {
                    Header = new Header { TailNumber = "C-FGAX", SequenceNumber = (uint)packets.Count },
                    Body = new Body { Data = entry },
                    Trailer = new Trailer { Checksum = Program.checksum(entry.Altitude, entry.Pitch, entry.Bank) }
                };
                packets.Add(packet);
            }

            // Assert
            Assert.IsTrue(packets.Count > 0, "Packets should be created for valid log entries.");
            Assert.AreEqual("C-FGAX", packets[0].Header.TailNumber, "First packet TailNumber should match.");
        }
    }

    // Helper classes and models
    public static class FlightDataParser
    {
        public static List<FlightDataEntry> ParseFile(string filePath)
        {
            var entries = new List<FlightDataEntry>();

            using (var reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var entry = ParseLine(line);
                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }
            }

            return entries;
        }

        public static FlightDataEntry ParseLine(string line)
        {
            try
            {
                var parts = line.Split(',');

                return new FlightDataEntry
                {
                    Timestamp = parts[0].Trim(),
                    X = float.Parse(parts[1]),
                    Y = float.Parse(parts[2]),
                    Z = float.Parse(parts[3]),
                    Weight = float.Parse(parts[4]),
                    Altitude = float.Parse(parts[5]),
                    Pitch = float.Parse(parts[6]),
                    Bank = float.Parse(parts[7])
                };
            }
            catch
            {
                // Return null for invalid lines
                return null;
            }
        }
    }

    public class FlightDataEntry
    {
        public string Timestamp { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Weight { get; set; }
        public float Altitude { get; set; }
        public float Pitch { get; set; }
        public float Bank { get; set; }
    }

    public class Packet
    {
        public Header Header { get; set; }
        public Body Body { get; set; }
        public Trailer Trailer { get; set; }
    }

    public class Header
    {
        public string TailNumber { get; set; }
        public uint SequenceNumber { get; set; }
    }

    public class Body
    {
        public FlightDataEntry Data { get; set; }
    }

    public class Trailer
    {
        public double Checksum { get; set; }
    }
}
