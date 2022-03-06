using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace Linker.Channels
{

    public abstract class Channel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string  name;

        [XmlAttribute]
        public string  Name
        {
            get { return name; }
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }


        private int channelNumber;

        public int ChannelNumber
        {
            get { return channelNumber; }
            set
            {
                channelNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChannelNumber)));
            }
        }




        public abstract void Initialize();

        


        private static Type[] derivedTypes;
        
        public static Type[] DerivedTypes
        {
            get
            {
                if(derivedTypes == null)
                {
                    var derived_typeList = new List<Type>();
                    foreach (var domain_assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        var assembly_types = domain_assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Channel)) && !type.IsAbstract);
                        if(assembly_types.Any())
                        {
                            derived_typeList.AddRange(assembly_types);
                            derivedTypes = derived_typeList.ToArray();
                            return derivedTypes;
                        }
                    }
                }
                return derivedTypes;
            }
        }



    }

    

   
}
