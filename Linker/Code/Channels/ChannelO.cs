using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Windows.Devices.Gpio;

namespace Linker.Channels
{
    public class ChannelIO : Channel
    {
        private const string browseInputName = "Gpio input  {0}";
        private const string browseOutputName = "Gpio output {0}";

        //public string ImportantSetting { get; set; } = "I got nothing MF";

        //private List<String> browseList;

        //public new List<String> BrowseList
        //{
        //    get
        //    {
        //        if (browseList == null)
        //        {
        //            browseList = new List<string>();
        //            if (gpio != null)
        //                for (int i = 1; i < gpio.PinCount + 1; i++)
        //                    browseList.Add(string.Format(browseInputName, i));
        //            else
        //                for (int i = 1; i < 11; i++)
        //                    browseList.Add(string.Format(browseInputName, i));
        //        }
        //        return browseList;
        //    }
        //}

       // public ObservableCollection<Node> Nodes { get; set; }
       // ObservableCollection<Node> Nodes { get; set; }

        private GpioController gpio;

        public ChannelIO()
        {
            // Get the default GPIO controller on the system
            gpio = GpioController.GetDefault();
            if (gpio == null)
                return; // GPIO not available on this system

            var test = gpio.OpenPin(4, GpioSharingMode.SharedReadOnly);

            test.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            //test.ValueChanged += Test_ValueChanged;
            // Open GPIO 5
            //using (GpioPin pin = gpio.OpenPin(5))
            //{
            //    // Latch HIGH value first. This ensures a default value when the pin is set as output
            //    pin.Write(GpioPinValue.High);


            //    // Set the IO direction as output
            //    pin.SetDriveMode(GpioPinDriveMode.Output);

            //} // Close pin - will revert to its power-on state
        }

        [XmlIgnore]
        public List<string> BrowseList
        {
            get
            {
               
                return null;
            }
        }


        public override void Initialize()
        {
            
        }
    }
}
