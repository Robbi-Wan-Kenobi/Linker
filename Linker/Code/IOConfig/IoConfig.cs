using Linker.Channels;
using Linker.Code.Behaviours;
using Linker.Triggers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Linker.IOConfig
{
    public class IoConfig : INotifyPropertyChanged
    {        
        public event PropertyChangedEventHandler PropertyChanged;


        public bool IsHeadedApplication { get; set; } = false;
        public string HttpSendAdress { get; set; } = "https://reqbin.com/echo/post/json";

        public string HttpSendMetaAdress { get; set; } = "https://reqbin.com/echo/post/json";

        public string DeviceUuid { get; set; } = "5CySt3UR0u6spFDKHcA";  // TODO: Automate

        public string DeviceName { get; set; } = "Mill Afferden";  // TODO: Whatever
                
        [XmlArrayItem("ChannelZWave", typeof(ChannelZWave))]
        [XmlArrayItem("ChannelIO", typeof(ChannelIO))]
        public ObservableCollection<Channel> Channels { get; set; } = new ObservableCollection<Channel>();


        public Interval DatabaseRecordsSaveInterval { get; set; }
        
        public Interval SendRecordsToCloudInterval { get; set; }

        
    }
}
