namespace GroundTerminalSoftware.Models
{
    public class FlightDataEntry
    {
        public string TailNum { get; set; }
        public string Timestamp { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Weight { get; set; }
        public float Altitude { get; set; }
        public float Pitch { get; set; }
        public float Bank { get; set; }
    }
}
