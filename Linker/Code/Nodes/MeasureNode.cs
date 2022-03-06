using Linker.Code.Behaviours;
using Linker.Triggers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace Linker.Nodes
{
    public class MeasureNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        
        private string name;

        [XmlAttribute]
        public string Name
        {
            get { return name; }
            set 
            { 
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        private string unit;

        
        public string Unit
        {
            get { return unit; }
            set 
            { 
                unit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Unit)));
            }
        }


        // public String Path { get; set; }



        // public string ImageName { get; set; } = "1.png";
        private static List<int> tempUriList { get; } = new List<int> { 1, 10, 16, 2, 33, 7 };

        private static int tempUriCounter;

        [XmlIgnore]
        public Uri NodeIcon
        {
            get
            {
                if (tempUriCounter < tempUriList.Count - 1)
                    tempUriCounter += 1;
                else
                    tempUriCounter = 0;
                //string wtf = 
                return new Uri($"ms-appx:///DeviceIcons/{tempUriList[tempUriCounter].ToString()}.png");
            }
        }





        private Behaviour behaviour;


        [XmlElement("Behaviours")]
        [XmlElement("BehaviourBoolCount", typeof(BehaviourBoolCount))]
        [XmlElement("BehaviourMultiplier", typeof(BehaviourMultiplier))]
        public Behaviour Behaviour
        {
            get { return behaviour; }
            set
            {
                if (value != null && behaviour != value)
                {
                    if(behaviour != null)
                        Behaviour.PropertyChanged -= Behaviour_PropertyChanged;
                        
                    behaviour = value;
                    behaviour.PropertyChanged += Behaviour_PropertyChanged;
                    
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Behaviour)));
                }
            }
        }

        /// <summary>
        /// A property in the Behavior changed, trigger trough this node
        /// </summary>
        private void Behaviour_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        private string rawValue;

        [XmlIgnore]
        public string RawValue
        {
            get { return rawValue; }
            set
            {
                rawValue = value;
                if (Behaviour != null)
                    Behaviour.Count(rawValue);
                
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RawValue)));
            }
        }
    }

    
   
}
