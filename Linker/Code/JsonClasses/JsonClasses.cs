using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linker.Code.JsonClasses
{


    //public class SendDataMaster
    //{
    //    public string DeviceUuid { get; set; }

    //    public string DeviceName { get; set; }

    //    public DataRow[] Data { get; set; } = new DataRow[2];
    //}

    //public class DataRow
    //{
    //    public int RowNr { get; set; }

    //    public string TimeStamp { get; set; }

    //    public DataItem[] Data { get; set; } = new DataItem[2];
    //}


    //public class DataItem
    //{
    //    public string ItemId { get; set; }

    //    public string Value { get; set; }
    //}


    public class SendDataMaster
    {
        public string DeviceUuid { get; set; }

        public string DeviceName { get; set; }

        public DataItem[] Data { get; set; } = new DataItem[2];
    }

   


    public class DataItem
    {
        public string ItemId { get; set; }

        public string Value { get; set; }

        public string TimeStamp { get; set; }
    }


    public class DataConfigMaster
    {
        public string DeviceUuid { get; set; }

        public string DeviceName { get; set; }


        public ConfigItem[] DataConfig;


    }


    public class ConfigItem
    {
        public string Name { get; set; }

        public string Unit { get; set; }

        public string ItemId { get; set; }

        public int MeasureIntervalSec { get; set; }

        public int ZwCommandClassId { get; set; }

        public Int64 ZwId { get; set; }
    }
}
