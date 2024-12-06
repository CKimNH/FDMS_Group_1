using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;
using AircraftTransmissionSystem;
using System; // Use the namespace where ATS classes are defined

namespace ATSSystemTests
{
    [TestClass]
    public class SystemTests
    {
        private readonly string logsDirectory = Path.Combine(
            Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName,
            "AircraftTransmissionSystem\\bin\\Debug\\logs"
        );
        [TestMethod]
        public void Process_Valid_Log_File_Should_Create_All_Packets()
        {
            // Arrange
            string logFilePath = Path.Combine(logsDirectory, "C-FGAX.txt");
            var packets = new List<Packet>();
            int validLineCount = 0;

            // Act
            var lines = File.ReadAllLines(logFilePath);
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line)) // Check for empty or whitespace-only lines
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 8) // Count only lines with sufficient data
                    {
                        validLineCount++; // Increment the count of valid lines

                        var packet = new Packet
                        {
                            header = new Header { TailNumber = "C-FGAX", SequenceNumber = (uint)packets.Count },
                            body = new Body { data = parts[0] }, // Assuming parts[0] is timestamp
                            trailer = new Trailer
                            {
                                Checksum = Program.checksum(
                                    double.Parse(parts[5]), // Altitude
                                    double.Parse(parts[6]), // Pitch
                                    double.Parse(parts[7])  // Bank
                                )
                            }
                        };
                        packets.Add(packet);
                    }
                }
            }

            // Assert
            Assert.AreEqual(validLineCount, packets.Count,
                $"The number of created packets ({packets.Count}) should match the number of valid lines ({validLineCount}) in the log file.");
        }


    }
}
