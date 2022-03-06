using Linker.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace Linker.Code.Behaviours
{
    public abstract class Behaviour : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        public Behaviour()
        {
        }

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

        
        private double myvalue;
        [XmlIgnore]
        public double Value
        {
            get { return myvalue; }
            set 
            {
                myvalue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }


        private string dataBaseRecord;

        public string DataBaseRecord
        {
            get { return dataBaseRecord; }
            set 
            { 
                dataBaseRecord = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataBaseRecord)));
            }
        }



        public abstract void Count(object input);
        

        private Double multiplier = 1;

        public Double Multiplier
        {
            get { return multiplier; }
            set { multiplier = value; }
        }

        private static Type[] derivedTypes;

        public static Type[] DerivedTypes
        {
            get
            {
                if (derivedTypes == null)
                {
                    var derived_typeList = new List<Type>();
                    foreach (var domain_assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        var assembly_types = domain_assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Behaviour)) && !type.IsAbstract);
                        if (assembly_types.Any())
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

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
