using OpenZWave;
using Linker.NetworkManager;
using Linker.Channels;
using Linker.Code;
using Linker.Code.Behaviours;
using Linker.Code.Nodes;
using Linker.Triggers;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Linker.Code.IOConfig;
using Windows.UI.Xaml.Data;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Linker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChannelView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //public static ObservableCollection<Interval> TriggerList => AppConfig.IOConfiguration.Triggers;
       // public static ObservableCollection<Behaviour> BehavioursList => AppConfig.IOConfiguration.Behaviours;


        public ChannelView()
        {
            this.InitializeComponent();
            DataContext = this;
        }

        


        public Channel MyChannel
        {
            get { return (Channel)GetValue(MyChannelProperty); }
            set { SetValue(MyChannelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyChannel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyChannelProperty =
            DependencyProperty.Register("MyChannel", typeof(Channel), typeof(ChannelView), new PropertyMetadata(null, OnChannelPropertyChanged));


        /// <summary>
        /// opens the nodeSelection dialog
        /// </summary>
        private async void AddMeasureNode_Click(object sender, RoutedEventArgs e)
        {
            if(MyChannel is ChannelZWave)
            {
                ChannelZWave channel = MyChannel as ChannelZWave;

                var nodeSelection = new SelectionDialogZNode();
                nodeSelection.ItemsSource = channel.BrowseList;

                var result = await nodeSelection.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    Tuple<string, ZWValueId>[] selectedItemsCopy = nodeSelection.SelectedItems.ToArray();

                    foreach (Tuple<string, ZWValueId> item in selectedItemsCopy)
                    {
                        string parentPath = item.Item1;
                        ZWValueId zWValueId = item.Item2;

                        if (!channel.Nodes.Any(finder => finder.EqualZWaveValue(zWValueId)))
                        {
                            MeasureZWaveNode node = new MeasureZWaveNode();
                            node.ZWValueId = zWValueId;
                            node.Name = ZWManager.Instance.GetValueLabel(zWValueId);
                            node.Unit = ZWManager.Instance.GetValueUnits(zWValueId);
                            //node.Path = Channel.stringPathBuilder(channel, parentPath, node.Name, zWValueId.Id);
                            //node.Trigger = TriggerList.FirstOrDefault();
                            bool isNumbericItem = zWValueId.Type != ZWValueType.Bool && zWValueId.Type != ZWValueType.List; ;
                            if (isNumbericItem)
                                node.Behaviour = new BehaviourMultiplier() { Multiplier = 1 };
                            else
                                node.Behaviour = new BehaviourBoolCount();

                            node.Behaviour.DataBaseRecord = MeasureManager.GetNewRecordName();


                            channel.Nodes.Add(node);

                            string value = string.Empty;                            
                            if(ZWManager.Instance.GetValueAsString(zWValueId, out value))
                            {
                                node.RawValue = value;
                            }
                        }                        
                    }                    
                }
            }          
        }



        private ZWValueId GetZwValueFromSource(IEnumerable nodes, uint homeID, uint nodeId, uint commandClassId)
        {
            foreach (var nodeItem in nodes)
            {
                Node node = nodeItem as Node;
                if (node != null)
                {
                    foreach (var valueItem in node.UserValues)
                    {
                        if (valueItem.HomeId == homeID && valueItem.NodeId == nodeId && valueItem.CommandClassId == commandClassId)
                        {
                            return valueItem;
                        }
                    }
                }
            }
            return null;
        }




        private void DeleteMeasureNode_Click(object sender, RoutedEventArgs e)
        {
            ChannelZWave channel = MyChannel as ChannelZWave;
            if(channel != null)
            {
                var selectedValue = NodeView.SelectedItem as MeasureZWaveNode;
                if(selectedValue != null)
                {
                    channel.Nodes.Remove(selectedValue);
                }
            }
            
        }


        private static void OnChannelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as ChannelView;
            me.ChannelSelected = e.NewValue != null;

            if (me.PropertyChanged != null)
                me.PropertyChanged.Invoke(me, new PropertyChangedEventArgs(nameof(ChannelSelected)));
        }

        public bool ChannelSelected { get; set; }



        private void NodeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NodeSelected = NodeView.SelectedItem != null;

            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(NodeSelected)));
        }

        public bool NodeSelected { get; set; }

        private void TextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
    }





    public class BehaviourTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, string l) =>
            (v != null && v.GetType() == typeof(BehaviourBoolCount)) ? Visibility.Visible : Visibility.Collapsed; 
        public object ConvertBack(object v, Type t, object p, string l) => null;
    }

}
