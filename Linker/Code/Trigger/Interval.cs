using Linker.Code;
using Linker.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Xml.Serialization;
using Windows.UI.Xaml;

namespace Linker.Triggers
{
    public class Interval : INotifyPropertyChanged
    {
        public event ElapsedEventHandler Elapsed;        
        public event PropertyChangedEventHandler PropertyChanged;

        private static Timer globalTrigger = new Timer(1000);

        [XmlIgnore]
        public bool EnableInterval { get; set; }



        static Interval()
        {
            globalTrigger.Enabled = true;
        }


        public Interval()
        {
            globalTrigger.Elapsed += GlobalTrigger_Elapsed;
        }

        private int tickCount;


        private int intervalTime;


        /// <summary>
        /// In seconds
        /// </summary>
        public int IntervalTime
        {
            get { return intervalTime; }
            set
            {
                intervalTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IntervalTime)));
            }
        }



        private void GlobalTrigger_Elapsed(object sender, ElapsedEventArgs e)
        {
            var timer = sender as Timer;
            timer.Stop();
            if (EnableInterval)
            {
                tickCount += 1;

                if (tickCount >= IntervalTime)
                {
                    tickCount = 0;
                    Elapsed?.Invoke(this, e);
                }
            }
            else
            {
                tickCount = 0;
            }
            timer.Start();
        }


        private static Type[] derivedTypes;

        public static Type[] DerivedTypes
        {
            get
            {
                if (derivedTypes == null)
                    derivedTypes = new Type[] { typeof(Interval) };

                return derivedTypes;
            }
        }
    }
}
