using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;
using OpenZWave;
using Linker.Code.Nodes;
using Linker.NetworkManager;
using System;
using Linker.Nodes;

namespace Linker.Channels
{
    public class ChannelZWave : Channel
    {

        [XmlIgnore]
        public IEnumerable<Node> BrowseList
        {
            get { return Watcher.Instance.Nodes; }
        }

        [XmlArrayItem("MeasureZWaveNode", typeof(MeasureZWaveNode))]
        public ObservableCollection<MeasureZWaveNode> Nodes { get; set; } = new ObservableCollection<MeasureZWaveNode>();



        public override void Initialize()
        {


            ZWManager.Instance.NotificationReceived += Instance_NotificationReceived;
            var itemSource = Watcher.Instance.Nodes as INotifyCollectionChanged;
            if (itemSource != null)
                itemSource.CollectionChanged += NodeList_CollectionChanged;

        }




        private void Instance_NotificationReceived(ZWManager sender, NotificationReceivedEventArgs e)
        {
            var temp = e;
            var tampnotification = temp.Notification;
            if (Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher == null)
            {
                NotificationHandler(e.Notification);
            }
            else
            {
                // Handle the notification on a thread that can safely
                // modify the controls without throwing an exception.
#if NETFX_CORE
                var _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
#else
            Dispatcher.BeginInvoke(new Action(() =>
#endif
                {
                    NotificationHandler(e.Notification);
                }
#if !NETFX_CORE
            )
#endif
            );
            }
        }

        /// <summary>
        /// The notification handler. This is called by the ZWave library every time
        /// an event occurs on the ZWave network, a device is found/lost, etc
        /// </summary>
        private void NotificationHandler(ZWNotification notification)
        {
            if (notification.Type == ZWNotificationType.ValueChanged)
            {
                var debugvalue = notification.ValueId;
                var MeasurevalueId = Nodes.FirstOrDefault(finder => finder.EqualZWaveValue(debugvalue));
                if (MeasurevalueId != null)
                {
                    //string localvalue;

                    //ZWManager.Instance.GetValueAsString(notification.ValueId, out localvalue);
                    object localvalue = GetObjectValue(notification.ValueId);


                    MeasurevalueId.RawValue = localvalue.ToString();



                    //TODO: make this more efficent, every value change causes a lookup among all zwavevalue items

                }
            }
        }


        public static object GetObjectValue(ZWValueId v)
        {
            var manager = ZWManager.Instance;
            switch (v.Type)
            {
                case ZWValueType.Bool:
                    bool r1;
                    manager.GetValueAsBool(v, out r1);
                    return r1;
                case ZWValueType.Byte:
                    byte r2;
                    manager.GetValueAsByte(v, out r2);
                    return r2;
                case ZWValueType.Decimal:
                    decimal r3;
                    string r3s;
                    manager.GetValueAsString(v, out r3s);
                    return r3s;
                case ZWValueType.Int:
                    Int32 r4;
                    manager.GetValueAsInt(v, out r4);
                    return r4;
                case ZWValueType.List:
                    string value;
                    manager.GetValueListSelection(v, out value);
                    return value;
                case ZWValueType.Schedule:
                    return "Schedule";
                case ZWValueType.Short:
                    short r7;
                    manager.GetValueAsShort(v, out r7);
                    return r7;
                case ZWValueType.String:
                    string r8;
                    manager.GetValueAsString(v, out r8);
                    return r8;
                default:
                    return "";
            }
        }


        /// <summary>
        /// Node added to list
        /// </summary>
        private void NodeList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Node nodeItem in e.NewItems)
                {
                    CheckForInitReadValues((IList)nodeItem.UserValues);

                    var list = nodeItem.UserValues as ObservableCollection<ZWValueId>;
                    if (list != null)
                    {
                        list.CollectionChanged += ZwaveUserValueList_CollectionChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Zwave value added to node list
        /// </summary>
        private void ZwaveUserValueList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                CheckForInitReadValues(e.NewItems);
            }
        }


        /// <summary>
        /// Initilizes MeasureZWaveNode by adding the right zwValueID to it
        /// </summary>
        /// <param name="zwValueList"></param>
        private void CheckForInitReadValues(IList zwValueList)
        {
            MeasureZWaveNode measureZWaveNode;

            foreach (ZWValueId valueItem in zwValueList)
            {
                measureZWaveNode = Nodes.FirstOrDefault(finder => finder.EqualZWaveValue(valueItem));
                if (measureZWaveNode != null)
                    measureZWaveNode.ZWValueId = valueItem;
            }
        }
    }
}
