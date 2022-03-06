using Linker.Channels;
using Linker.Code.Behaviours;
using Linker.Code.DataBase;
using Linker.Code.IOConfig;
using Linker.Code.Nodes;
using Linker.Nodes;
using Linker.Triggers;
using Serilog.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Linker.Code
{

    class MeasureManager : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        private Interval recordsSaveInterval => AppConfig.IOConfiguration.DatabaseRecordsSaveInterval;
        private bool initilized;


        private static readonly MeasureManager uniqueInstanceEager = new MeasureManager();

        private MeasureManager()
        { }

        
        public static MeasureManager Instance
        {
            get { return uniqueInstanceEager; }
        }




        public Task Initialize()
        {
            //loadIoConfigTask = LoadIoConfigASync();

            return RebuildNodesAndDataBase();
        }


        


        private async Task RebuildNodesAndDataBase()
        {
            await DatabaseManager.Instance.InitializeDatabaseAsync().ConfigureAwait(false);
            
            try
            {
                //DatabaseManager.Instance.CreateNonExsistingColumns(CombinedChannelsList.ToArray());
                GetPreviusValuesFromDatabase();
            }
            catch (Exception ex)
            {
                LogBuddy.Log(this, LogEventLevel.Error, $"RebuildNodesAndDataBase, Database error: {ex.Message}");
            }

            if(!initilized)
                recordsSaveInterval.Elapsed += RecordsSaveInterval_IntervalElapsed;

            foreach (Channel channelItem in AppConfig.IOConfiguration.Channels)
                channelItem.Initialize();

            initilized = true;
        }

        private void RecordsSaveInterval_IntervalElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DatabaseManager.Instance.InsertData(AppConfig.CombinedChannelsList);
        }






        private void GetPreviusValuesFromDatabase()
        {
            var dbColumnNames = DatabaseManager.Instance.GetColumnNames();
            var counldNotFindPaths = new List<string>();

            //TODO:  directly cast behaviors and fill there values

            foreach (Channel channelItem in AppConfig.IOConfiguration.Channels)
            {
                var channelZWave = channelItem as ChannelZWave;
                if (channelZWave != null)
                {
                    List<MeasureNode> nodes = new List<MeasureNode>();
                    foreach (MeasureNode measureZWaveNode in channelZWave.Nodes)
                        if (measureZWaveNode.Behaviour != null && measureZWaveNode.Behaviour is BehaviourBoolCount) // only these have previus values
                            if (dbColumnNames.Contains(measureZWaveNode.Behaviour.DataBaseRecord))
                                nodes.Add(measureZWaveNode);
                            else
                                counldNotFindPaths.Add(measureZWaveNode.Behaviour.DataBaseRecord);

                    if(nodes.Count > 0)
                    {
                        var recordsToLOad = Array.ConvertAll(nodes.ToArray(), conv => conv.Behaviour.DataBaseRecord);
                        var taskResult = DatabaseManager.Instance.LoadLastRecord(recordsToLOad);
                        double value;
                        object[] valueResult;

                        taskResult.Wait();
                        valueResult = taskResult.Result;

                        if (valueResult != null)
                            for (int i = 0; i < nodes.Count; i++)
                                if (valueResult[i] != null && double.TryParse(valueResult[i].ToString(), out value))
                                    nodes[i].Behaviour.Value = value;
                    }
                }
            }
        }




        

       public static string GetNewRecordName()
        {
            var recordNames = AppConfig.CombinedChannelsList.ToList();

            if (recordNames.Count > 0)
            {
                recordNames.OrderBy(orderer => orderer.Behaviour.DataBaseRecord);
                MeasureNode latestRecord = recordNames.Last();
                string recordName = latestRecord.Behaviour.DataBaseRecord;
                string number = string.Empty;

                foreach (char character in recordName.Reverse())
                    if (char.IsNumber(character))
                        number = number.Insert(0, character.ToString(NumberFormatInfo.InvariantInfo));
                    else
                        break;
                
                int numberOutcome;
                if (int.TryParse(number, out numberOutcome))
                    return string.Concat("Record_", (numberOutcome + 1));
            }
            return "Record_1";
        }





        public bool ConfigurationChanged { get; set; }

        private bool measureTriggersRunning;

        public bool MeasureTriggersRunning
        {
            get { return measureTriggersRunning; }
            set
            {
                measureTriggersRunning = value;
                recordsSaveInterval.EnableInterval = measureTriggersRunning;

                if (measureTriggersRunning && ConfigurationChanged)
                {
                    //RebuildNodesAndDataBase();
                  

                    ConfigurationChanged = false;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MeasureTriggersRunning)));
            }
        }


       


        public string GetShortUUID()
        {
            return Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
        }




      


        /// <summary>
        /// Get the real name based on the database name
        /// </summary>
        /// <param name="databaseName">the full path database name, example Ch1.Z-Uno.Temperature14411</param>
        /// <returns>string empty if nothing is found, else the name as it is called by the user</returns>
        public string GetNodeNameFromDataBaseName(string databaseName)
        {
            foreach (Channel channelItem in AppConfig.IOConfiguration.Channels)
            {
                var zChannel = channelItem as ChannelZWave;
                if(zChannel != null)
                {
                    var node = zChannel.Nodes.FirstOrDefault(finder => finder.Behaviour != null && finder.Behaviour.DataBaseRecord.Equals(databaseName, StringComparison.Ordinal));
                    if (node != null)
                        return node.Name;
                }
            }
            return string.Empty;
        }


      

        public void Dispose()
        {
           // triggerTimer.Dispose();
        }
    }



   
}
