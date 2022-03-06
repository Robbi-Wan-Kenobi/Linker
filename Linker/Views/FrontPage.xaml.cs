using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Linker.Code;
using Linker.Code.Buddys;
using Linker.Code.IOConfig;
using Linker.Nodes;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Linker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FrontPage : Page
    {
        public PropertyObserveCollection<MeasureNode> NodeCollection   { get { return AppConfig.CombinedChannelsList; } }



        public FrontPage()
        {
            this.InitializeComponent();

            DataContext = this;

            //$"ms-appx:///DeviceIcons/{GenericType}.png
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var clickedItem = sender as FrameworkElement;

            MainPage.NavigateToPage(typeof(DatabaseTrendView), clickedItem.Tag, true);
        }


        private async Task GetAllImages()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var assets = await appInstalledFolder.GetFolderAsync("DeviceIcons");
                       
            foreach (StorageFile fileItem in await assets.GetFilesAsync())
                ImagesDeviceIcons.Add(fileItem);
        }


        public ObservableCollection<StorageFile> ImagesDeviceIcons { get; private set; } = new ObservableCollection<StorageFile>();
        



        private void ImagesButton_Click(object sender, RoutedEventArgs e)
        {
            var reslt = GetAllImages();
        }
    }
}
