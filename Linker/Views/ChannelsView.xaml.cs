using Linker.Channels;
using Linker.Code;
using Linker.Code.IOConfig;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Linker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated l. ,ithin a Frame.
    /// </summary>
    public sealed partial class ChannelsView : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    
        public ObservableCollection<Channel> ChannelList => AppConfig.IOConfiguration.Channels;


        private bool deleteItemEnabled;
        public bool DeleteItemEnabled
        {
            get { return deleteItemEnabled; }
            set
            {
                deleteItemEnabled = value;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(DeleteItemEnabled)));
            }
        }

        public ChannelsView()
        {
            this.InitializeComponent();
            
            DataContext = this;
        }

        private async void Channel_Add(object sender, RoutedEventArgs e)
        {
            var channelDialog = new NewNameAndTypeDialog();
            channelDialog.ItemName = "New Channel name";
            channelDialog.TypeNames = Array.ConvertAll(Channel.DerivedTypes, conv => conv.Name);
            channelDialog.Title = "Create new channel";
            channelDialog.ForceTypeSelection = true;

            var result = await channelDialog.ShowAsync();

            if(result == ContentDialogResult.Primary && !string.IsNullOrEmpty(channelDialog.SelectedType))
            {
                Type t = Type.GetType("Linker.Channels." + channelDialog.SelectedType);
                var tempChannel = (Channel)Activator.CreateInstance(t);                
                tempChannel.Name = channelDialog.ItemName;
                if (ChannelList.Count > 0)
                    tempChannel.ChannelNumber = ChannelList.Max(item => item.ChannelNumber) +1;
                else
                    tempChannel.ChannelNumber = 1;
                ChannelList.Add(tempChannel);
            }
        }




        private void Channel_Delete(object sender, RoutedEventArgs e)
        {
            if (MasterDetailsViewer.SelectedItem != null)
                ChannelList.Remove((Channel)MasterDetailsViewer.SelectedItem);
        }

        private void Channel_IOConfigSave(object sender, RoutedEventArgs e)
        {
            AppConfig.SaveIoConfig();
        }

        private void Channel_IOConfigLoad(object sender, RoutedEventArgs e)
        {
            AppConfig.LoadIoConfigSynced();
        }

        private void MasterDetailsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteItemEnabled = e.AddedItems.Count > 0;
            
        }
    }
}
