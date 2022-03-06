using Linker.Channels;
using Linker.Code.Buddys;
using Linker.Code.Nodes;
using Linker.IOConfig;
using Linker.Nodes;
using Linker.Triggers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Linker.Code.IOConfig
{
    class AppConfig
    {
        private static string ioConfigFileName = "IoConfig.xml";
        private static int saveIOFileSaveDelay = 8;
        private static IoConfig ioConfiguration;
        private static event EventHandler ioConfigLoadCompleted;
        static volatile object locker = new Object();


        public static PropertyObserveCollection<MeasureNode> CombinedChannelsList { get; private set; }

        public static IoConfig IOConfiguration
        {
            get
            {
                if (ioConfiguration == null)
                    lock (locker)
                        LoadIoConfigSynced();
                
                return ioConfiguration;
            }
        }


        static AppConfig()
        {
            SaveFileDelayTimer = new DispatcherTimer();
            SaveFileDelayTimer.Interval = TimeSpan.FromSeconds(saveIOFileSaveDelay);
            SaveFileDelayTimer.Tick += SaveFileDelayTimer_Tick;
        }

        

        private AppConfig()
        { }


        public static void SaveIoConfig()
        {
            try
            {
                var task = XmlBuddy.SaveObjectToXml(ioConfiguration, ioConfigFileName);
            }
            catch (Exception ex)
            {
                LogBuddy.Log("Failed to save IO Config to" + ioConfigFileName + " reason:" + ex.Message);
            }
        }




        public static void LoadIoConfigSynced()
        {
            lock (locker)
            {
                var task = Task.Run(async () => await LoadIoConfigASync().ConfigureAwait(false));
                task.Wait();
            }
        }


        public static async Task LoadIoConfigASync()
        {   
            try
            {
                ioConfiguration = await XmlBuddy.LoadObjectFromXml<IoConfig>(ioConfigFileName).ConfigureAwait(true);                
            }
            catch (Exception ex)
            {
                LogBuddy.Log("Failed to load IO config async to" + ioConfigFileName + " reason:" + ex.Message);
            }

            if (ioConfiguration == null)
            {
                ioConfiguration = new IoConfig();
                LogBuddy.Log($"Failed to load IO config { ioConfigFileName }, created new initial configuration");
            }

            ioConfigLoadCompleted?.Invoke(null, new EventArgs());
        }

        

        public static bool SaveIoChangesToFile { get; set; }

        public static void Initilize()
        {
            AddObligatedItemsifNotPresent();

            InitObserveIoConfigChanges();
            CombinedChannelsList.PropertyUpdateIgnoreList.Add("Value");
            CombinedChannelsList.PropertyUpdateIgnoreList.Add("RawValue");
            CombinedChannelsList.ItemPropertyChanged += CombinedChannelsList_ItemPropertyChanged;
            CombinedChannelsList.CollectionChanged += CombinedChannelsList_CollectionChanged;
            ioConfiguration.PropertyChanged += IoConfiguration_PropertyChanged; ;
        }

        private static void IoConfiguration_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (SaveIoChangesToFile)
            {
                saveFileNextRound = true;
                AutoSaveIoConfig();
            }
        }

        private static void CombinedChannelsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SaveIoChangesToFile)
            {
                saveFileNextRound = true;
                AutoSaveIoConfig();
            }
        }

        private static void CombinedChannelsList_ItemPropertyChanged(object sender, ItemChangedEventArgs e)
        {
            if (SaveIoChangesToFile)
            {
                saveFileNextRound = true;
                AutoSaveIoConfig();
            }
        }



        private static DispatcherTimer SaveFileDelayTimer;

        private static void AutoSaveIoConfig()
        {
            if (SaveFileDelayTimer.IsEnabled)
            {
                SaveFileDelayTimer.Stop();                
                SaveFileDelayTimer.Start();
            }
            else
            {
                if(saveFileNextRound)
                {
                    SaveIoConfig();
                    saveFileNextRound = false;
                    SaveFileDelayTimer.Start();
                }                               
            }
        }

        private static bool saveFileNextRound;

        private static void SaveFileDelayTimer_Tick(object sender, object e)
        {
            SaveFileDelayTimer.Stop();
            AutoSaveIoConfig();
        }

        /// <summary>
        /// Adds ons zwave channel, one no conversion and one interval if it does not excist
        /// </summary>
        private static void AddObligatedItemsifNotPresent()
        {
            if (!IOConfiguration.Channels.Any(finder => finder is ChannelZWave))
                IOConfiguration.Channels.Add(new ChannelZWave() { ChannelNumber = 1, Name = "Zwave channel" });

            if (IOConfiguration.DatabaseRecordsSaveInterval == null)
                IOConfiguration.DatabaseRecordsSaveInterval = new Interval() { IntervalTime = 10 };

            if (IOConfiguration.SendRecordsToCloudInterval == null)
                IOConfiguration.SendRecordsToCloudInterval = new Interval() { IntervalTime = 600 };
        }


        /// <summary>
        /// Observes all channel collection for channel removal, so that the trigger timer and database
        /// </summary>
        private static void InitObserveIoConfigChanges()
        {
            bool initialInitObserveIoConfig = CombinedChannelsList == null;
            if (initialInitObserveIoConfig)
            {
                CombinedChannelsList = new PropertyObserveCollection<MeasureNode>();
                AppConfig.IOConfiguration.Channels.CollectionChanged += (s, e) => InitObserveIoConfigChanges();
            }

            foreach (Channel channelItem in IOConfiguration.Channels)
            {
                var zwaveChannel = channelItem as ChannelZWave;
                if (zwaveChannel == null) continue;

                foreach (MeasureZWaveNode measureItem in zwaveChannel.Nodes)
                    if (initialInitObserveIoConfig || !CombinedChannelsList.Contains(measureItem))
                        CombinedChannelsList.Add(measureItem);

                WatchChannel(zwaveChannel.Nodes);  // TODO: OR THIS ON OR THE OTHER IS WRONG
            }
        }


        private static List<INotifyCollectionChanged> connectedLists = new List<INotifyCollectionChanged>();


        private static void WatchChannel(IList itemsList)
        {
            if (itemsList == null)
                return;

            var observatableList = itemsList as INotifyCollectionChanged;
            if (observatableList != null && !connectedLists.Contains(observatableList))
            {
                observatableList.CollectionChanged += ChannelList_CollectionChanged;
                connectedLists.Add(observatableList);
            }
        }

        private static void ChannelList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (MeasureNode measureItem in e.NewItems)
                        CombinedChannelsList.Add(measureItem);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (MeasureNode measureItem in e.OldItems)
                        CombinedChannelsList.Remove(measureItem);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    InitObserveIoConfigChanges();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (MeasureNode measureItem in e.OldItems)
                        CombinedChannelsList.Remove(measureItem);
                    foreach (MeasureNode measureItem in e.NewItems)
                        CombinedChannelsList.Add(measureItem);
                    break;
                default:
                    return;
            }
        }
    }
}
