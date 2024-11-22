using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace AircraftTransmissionSystem
{

    struct Header
    {
        public string TailNumber;
        public uint SequenceNumber;
    }

    struct Body
    {
        public string data;
    }

    struct Trailer
    {
        public double Checksum;
    }

    internal class Packet
    {


        public Header header;
        public Body body;
        public Trailer trailer;

        public Packet (string line)
        {
            string[] dataArray = line.Split ('|');

            header.TailNumber = dataArray[0];
            header.SequenceNumber = Convert.ToUInt32(dataArray[1]);
            body.data = dataArray[2];
            trailer.Checksum = Convert.ToDouble(dataArray[3]);
        }
        

        public Packet()
        {
            this.header = new Header();
            this.body = new Body();
            this.trailer = new Trailer();
        }

        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream ())
            {
                using (BinaryWriter bw = new BinaryWriter(m))
                {
                    bw.Write(this.header.TailNumber);
                    bw.Write(this.header.SequenceNumber);
                    bw.Write(this.body.data);
                    bw.Write(this.trailer.Checksum);
                }
                return m.ToArray ();
            }
        }

        public static Packet Deserialize(Byte[] data)
        {
            Packet result = new Packet();
            using (MemoryStream ms = new MemoryStream(data)) // Initialize MemoryStream with data
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    result.header.TailNumber = br.ReadString();
                    result.header.SequenceNumber = br.ReadUInt32();
                    result.body.data = br.ReadString();
                    result.trailer.Checksum = br.ReadDouble();
                }
            }
            return result;
        }

        public static string PacketToString(Packet packet)
        {
            string result = "";
            result += packet.header.TailNumber + " ";
            result += packet.header.SequenceNumber.ToString() + " ";
            result += packet.body.data + " ";
            result += Convert.ToString(packet.trailer.Checksum) + ";";
            return result;
        }
    }
}
