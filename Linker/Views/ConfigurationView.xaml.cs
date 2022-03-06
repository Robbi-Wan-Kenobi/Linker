using Linker.Code;
using Linker.NetworkManager;
using Linker.Views;
using OpenZWave;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://github.com/OpenZWave/openzwave-dotnet-uwp/wiki

namespace Linker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConfigurationView : Page
    {

        public ConfigurationView()
        {
            this.InitializeComponent();

            DataContext = this;


            //ApplicationState.Instance.InitializeAsync().ContinueWith((t) =>
            //{
            //    GetSerialPorts();
            //}, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());

            //var menuItem = hamburgerMenu.MenuItems.FirstOrDefault(finder => "SettingsView".Equals(((NavigationViewItem)finder).Content.ToString(), StringComparison.Ordinal));
            //hamburgerMenu.SelectedItem
            //if (menuItem != null)
            hamburgerMenu.SelectedItem = SelectedNavigationViewItem;
            frame.Navigate(typeof(Views.SettingsView));
        }


        private void HamburgerMenu_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                frame.Navigate(typeof(Views.SettingsView));
                return;
            }

            var navItem = args.InvokedItemContainer as NavigationViewItem;
            string pageName = $"Linker.{navItem.Tag}";
            var pageType = Type.GetType(pageName);   //Link.Views.ChannelsView
            if (pageType != null)
                frame.Navigate(pageType);
        }


        //private void GetSerialPorts()
        //{
        //    if (ApplicationState.Instance.SerialPorts.Count == 1)
        //    {
        //        //hamburgerMenu.SelectedIndex = 0;
        //        ApplicationState.Instance.SerialPorts[0].IsActive = true; //Assume if there's only one port, that's the ZStick port
        //        (hamburgerMenu.Content as Frame).Navigate(typeof(Views.DevicesView));
        //    }
        //    else
        //    {
        //        if (!ApplicationState.Instance.SerialPorts.Any())
        //        {
        //            var _ = new Windows.UI.Popups.MessageDialog("No serial ports found").ShowAsync();
        //        }

        //        // hamburgerMenu.SelectedIndex = 1;
        //        hamburgerMenu.SelectedItem = hamburgerMenu.MenuItems[1];
        //        (hamburgerMenu.SelectedItem as NavigationViewItem).IsSelected = true;

        //        (hamburgerMenu.Content as Frame).Navigate(typeof(Views.SettingsView));
        //    }
        //}

       
       
    }
}
