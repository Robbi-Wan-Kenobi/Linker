using Linker.Channels;
using Linker.Code;
using Linker.Code.Behaviours;
using Linker.Code.IOConfig;
using Linker.Code.Nodes;
using OpenZWave;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Linker.ViewDialogs
{
    public sealed partial class NewNodeDialog : ContentDialog, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MeasureZWaveNode meaureNode;

        public bool NewCreatedMeaureNode { get; private set; }



        private bool isNumbericItem => ZWValueId.Type != ZWValueType.Bool && ZWValueId.Type != ZWValueType.List;

        public Visibility NumbericPartVisible => isNumbericItem ? Visibility.Visible : Visibility.Collapsed;

        public Visibility ListSelectionPartVisible => isNumbericItem ? Visibility.Collapsed : Visibility.Visible;
 


        private Visibility multiplierComboBoxErrorVisible = Visibility.Collapsed;

        public Visibility MultiplierComboBoxErrorVisible
        {
            get { return multiplierComboBoxErrorVisible; }
            set 
            { 
                multiplierComboBoxErrorVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MultiplierComboBoxErrorVisible)));
            }
        }


        public MeasureZWaveNode MeaureNode
        {
            set { meaureNode = value; }
            get
            {
                if (meaureNode == null)
                {
                    meaureNode = new MeasureZWaveNode();
                    NewCreatedMeaureNode = true;
                }
                return meaureNode;
            }
        }


        

        public double Multiplier
        {
            get { return MeaureNode.Behaviour != null ? MeaureNode.Behaviour.Multiplier : 1; }
            set
            {
                if (MeaureNode != null && MeaureNode.Behaviour != null && MeaureNode.Behaviour.Multiplier != value)
                {
                    MeaureNode.Behaviour.Multiplier = value;         
                }
            }
        }


        public ZWValueId ZWValueId { get; set; }


        public NewNodeDialog(ZWValueId zWValueId)
        {
            if (zWValueId == null)
                return;

            this.InitializeComponent();

            ZWValueId = zWValueId;
            Loaded += NewNodeDialog_Loaded;
            DataContext = this;
        }

        private void NewNodeDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (NewCreatedMeaureNode)
            {
                string nodeName = ZWManager.Instance.GetNodeProductName(ZWValueId.HomeId, ZWValueId.NodeId);
                MeaureNode.ZWValueId = ZWValueId;
                MeaureNode.Name = ZWManager.Instance.GetValueLabel(ZWValueId);
                MeaureNode.Unit = ZWManager.Instance.GetValueUnits(ZWValueId);

                //MeaureNode.Path = Channel.stringPathBuilder(ZWaveChannel, nodeName, MeaureNode.Name, ZWValueId.Id);
                //MeaureNode.Trigger = AppConfig.IOConfiguration.Triggers.FirstOrDefault();

                if (isNumbericItem)
                {
                    MeaureNode.Behaviour = new BehaviourMultiplier() { Multiplier = 1 };                    
                    IsPrimaryButtonEnabled = true;
                }
                else
                {
                    MeaureNode.Behaviour = new BehaviourBoolCount() { Multiplier = 1 };
                    IsPrimaryButtonEnabled = false;  // only when user selects behaviour item from combobox
                }
                    
                MeaureNode.Behaviour.DataBaseRecord = MeasureManager.GetNewRecordName();


                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
            }

            MultiplierActiveSwitch.IsOn = MeaureNode.Behaviour != null && MeaureNode.Behaviour.Multiplier != 1;
            //ComboBoxMultiplier.Text = MeaureNode.Behaviour.Multiplier.ToString();
        }


      



        public string[] TriggerValueListItems
        {
            get
            {
                if (isNumbericItem) return null;

                if (ZWValueId.Type == ZWValueType.Bool) return new string[] {"True", "False" };

                string[] values;
                ZWManager.Instance.GetValueListItems(ZWValueId, out values);
                return values;
            }
        }


        

        public string TriggerValueSelectedItem
        {
            get 
            { 
                return (MeaureNode.Behaviour as BehaviourBoolCount)?.TriggerValue; 
            }
            set
            {
                if (MeaureNode.Behaviour == null)
                    MeaureNode.Behaviour = new BehaviourBoolCount() { TriggerValue = value };
                else if(MeaureNode.Behaviour is BehaviourBoolCount)
                    ((BehaviourBoolCount) MeaureNode.Behaviour).TriggerValue = value;

                IsPrimaryButtonEnabled = true;
            }
        }



        public List<double> MultiplierList { get; } = new List<double> { 0.25, 0.5, 1, 2, 10, 20, 100 };


        private ChannelZWave zWaveChannel;

        public ChannelZWave ZWaveChannel
        {            
            get
            {
                if (zWaveChannel == null)
                    zWaveChannel = (ChannelZWave)AppConfig.IOConfiguration.Channels.FirstOrDefault(finder => finder is ChannelZWave);
                return zWaveChannel;
            }
        }

        



        private void MultiplierComboBox_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            args.Handled = !args.Text.All(c => Char.IsDigit(c)|| c.Equals('.'));
            MultiplierComboBoxErrorVisible = args.Handled ? Visibility.Visible : Visibility.Collapsed;
        }


        private void ContentDialog_OkButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if(NewCreatedMeaureNode)
            {
                //if (!isNumbericItem && MeaureNode.Behaviour.GetType() == typeof(BehaviourBoolCount))
                //{
                //    ((BehaviourBoolCount)MeaureNode.Behaviour).TriggerValue = ComboBoxTriggerValue.SelectedItem.ToString();
                //}
                object localvalue = ChannelZWave.GetObjectValue(ZWValueId);
                MeaureNode.RawValue = localvalue.ToString();

                if (ZWaveChannel != null)
                {
                    ZWaveChannel.Nodes.Add(MeaureNode);
                }
                else
                    LogBuddy.Log(this, LogEventLevel.Error, "Where the fuck did the ZWaveChannel go?");
            }            
        }



        private void ContentDialog_CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ZWValueId = null;

        }

        private void ComboBoxTriggerValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (!isNumbericItem && e.AddedItems.Count > 0)
            //{
            //    IsPrimaryButtonEnabled = true;
            //    ((BehaviourBoolCount)MeaureNode.Behaviour).TriggerValue = e.AddedItems.First().ToString();
            //}
                
        }
    }
}
