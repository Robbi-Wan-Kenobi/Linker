using OpenZWave;
using Linker.Nodes;
using System.Xml.Serialization;

namespace Linker.Code.Nodes
{
    // https://openzwave.github.io/openzwave-dotnet-uwp/doc/html/c2cfd9ac-b827-4020-b867-470ba91e24c1.htm
    public class MeasureZWaveNode : MeasureNode
    {

        // Summary:
        //     Get the Z-Wave command class that created and manages this value. Knowledge of
        //     command classes is not required to use OpenZWave, but this information is exposed
        //     in case it is of interest.
        public byte CommandClassId { get; set; }

        //
        // Summary:
        //     Get a 64Bit Integer that represents this ValueID. This Integer is not guaranteed
        //     to be valid across restarts of OpenZWave.
        public ulong Id { get; set; }


        private ZWValueId zwValueID;

        [XmlIgnore]
        public ZWValueId ZWValueId
        {
            get { return zwValueID; }
            set
            {
                zwValueID = value;
                Id = zwValueID.Id;
                CommandClassId = zwValueID.CommandClassId;
            }
        }



        /// <summary>
        /// Compairs the zwave ID and zwave commandclass
        /// </summary>
        public bool EqualZWaveValue(ZWValueId zWValueId)
        {
            if (zWValueId == null)
                return false;

            return this.Id == zWValueId.Id && this.CommandClassId == zWValueId.CommandClassId;
        }


        /// <summary>
        /// Compairs the zwave ID and zwave commandclass
        /// </summary>
        public bool EqualZWaveValue(ulong id, byte commandClassId)
        {
            return this.Id == id && this.CommandClassId == commandClassId;
        }

        public override bool Equals(object obj)
        {
            ZWValueId zWValueId = obj as ZWValueId;
            if (zWValueId != null)
                return EqualZWaveValue(zWValueId.Id, zWValueId.CommandClassId);
            else
                return ReferenceEquals(obj, this);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
