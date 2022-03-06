using Linker.Code;
using Linker.Code.IOConfig;
using Linker.Code.Nodes;
using Linker.NetworkManager;
using Linker.Nodes;
using Linker.ViewDialogs;
using OpenZWave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Linker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ZWOptions : Page
    {
        public Watcher Watcher => Watcher.Instance;

        public ZWOptions()
        {
            this.InitializeComponent();

            SplitView.IsPaneOpen = false;

            SplitView.OpenPaneLength = RightPane.ActualWidth;
        }


        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SplitView.OpenPaneLength = RightPane.ActualWidth;
        }

        private void NodesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SplitView.IsPaneOpen = NodesListView.SelectedItem != null;

            Node = NodesListView.SelectedItem as Node;
        }


        private void AppBarButtonValuesList_Click(object sender, RoutedEventArgs e)
        {
            _ = AppBarButtonValuesList_ClickAsync(sender);
        }

        private async Task AppBarButtonValuesList_ClickAsync(object sender)
        {
            var button = sender as AppBarButton;
            var zwaveValue = button.Tag as ZWValueId;

            if (zwaveValue != null)
            {
                MeasureNode measureNode = AppConfig.CombinedChannelsList.FirstOrDefault(finder => finder.Equals(zwaveValue));

                var newNodeDialog = new NewNodeDialog(zwaveValue);
                newNodeDialog.Title = measureNode == null ? "Create new measure item" : "Add excisting measure item";
                newNodeDialog.MeaureNode = measureNode as MeasureZWaveNode;

                await newNodeDialog.ShowAsync();
                

                if(newNodeDialog.ZWValueId != null)
                {
                    var iconConverter = this.Resources["AddOrEditNodeConverter"] as AddOrEditNodeConverter;
                    if (iconConverter != null)
                        button.Icon = new SymbolIcon(iconConverter.IconOne);
                }
            }            
        }




        public Node Node
        {
            get { return (Node)GetValue(NodeProperty); }
            set { SetValue(NodeProperty, value); }
        }

        public static readonly DependencyProperty NodeProperty =
            DependencyProperty.Register("Node", typeof(Node), typeof(ZWOptions), new PropertyMetadata(null, OnNodePropertyChanged));

        private static void OnNodePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ZWOptions).VM.UpdateNode(e.NewValue as Node);
        }

        public DeviceViewVM VM { get; private set; } = new DeviceViewVM(null);


        private void PowerOn_ContextMenuClick(object sender, RoutedEventArgs e)
        {
            //var value = new ZWValueID(Node.HomeID, Node.Id, ZWValueGenre.Basic, 0x20,  )
            //m_manager.SetValue(value, 0x255);
        }

        private void PowerOff_ContextMenuClick(object sender, RoutedEventArgs e)
        {
            //m_manager.SetNodeOff(Node.HomeId, Node.Id);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new MessageDialog("Remove device from controller?", "Confirm");
            dlg.Commands.Add(new UICommand("OK", (s) =>
            {
                VM.Remove();
            }));
            dlg.Commands.Add(new UICommand("Cancel"));
            var _ = dlg.ShowAsync();
        }

        private void Refresh_ContextMenuClick(object sender, RoutedEventArgs e)
        {
            Node.RefreshNodeInfo();
        }

        
    }
}
