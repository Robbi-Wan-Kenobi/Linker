using Linker.Code;
using Linker.Code.DataBase;
using Linker.Code.IOConfig;
using Linker.IOConfig;
using Linker.NetworkManager;
using Linker.Nodes;
using Linker.Views;
using OpenZWave;
using Serilog.Events;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://github.com/OpenZWave/openzwave-dotnet-uwp/wiki

namespace Linker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static FrameworkElement MainPageContentFrameworkElement = null;
        private static MainPage me;
        
        public bool MeasureTriggersRunning
        {
            get { return MeasureManager.Instance.MeasureTriggersRunning; }
            set { MeasureManager.Instance.MeasureTriggersRunning = value; }
        }

        public Watcher Watcher { get; }

        public MainPage()
        {
            Watcher = Watcher.Instance ?? new Watcher(this.Dispatcher);
            Watcher.Initialize();

            AppConfig.LoadIoConfigSynced();

            InitAll().ContinueWith((t) =>
            {
                if (true || AppConfig.IOConfiguration.IsHeadedApplication)
                {
                    this.InitializeComponent();
                    DataContext = this;
                    me = this;
                    MainPageContentFrameworkElement = (FrameworkElement)this.Content;
                    MeasureManager.Instance.PropertyChanged += Instance_PropertyChanged;

                    LogBuddy.Log(this,  LogEventLevel.Information, "Headed application started succesfull");
                }
                else
                {
                    AppConfig.SaveIoConfig();
                    MeasureManager.Instance.MeasureTriggersRunning = true;
                    LogBuddy.Log(this, LogEventLevel.Information, "Headless application started succesfull");
                    
                    var result = DatabaseManager.Instance.LoadLastNrOfRecordsAsync(2);

                    //DataSendManager.Instance.InitializeAsync();
                }
                DataSendManager.Instance.Initialize();

                GetSerialPorts();
            }, TaskScheduler.FromCurrentSynchronizationContext());
            AppConfig.Initilize();
            AppConfig.SaveIoChangesToFile = true;

            
            //ApplicationState.Instance.InitializeAsync().ContinueWith((t) =>
            //{
            //    GetSerialPorts();
            //}, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());

        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }


        private async Task InitAll()
        {
            Task[] tasks = new Task[]
            {                
                ApplicationState.Instance.InitializeAsync(),                
                MeasureManager.Instance.Initialize(),                
            };

            //foreach (Task item in tasks)
            //{
            //    System.Diagnostics.Debug.WriteLine( item.Status.ToString());
            //}

            await Task.WhenAll(tasks).ConfigureAwait(false);

            //foreach (Task item in tasks)
            //{
            //    System.Diagnostics.Debug.WriteLine(item.Status.ToString());
            //}

        }


        private void HamburgerMenu_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                me.frame.Navigate(typeof(ConfigurationView));
            }
            else
            {
                var navItem = args.InvokedItemContainer as NavigationViewItem;
                string pageName = navItem.Tag as string;

                if(pageName != null)
                {
                    var pageType = Type.GetType(pageName);   //Link.Views.ChannelsView
                    if (pageType != null)
                        NavigateToPage(pageType);
                }                
            }
        }


        /// <summary>
        /// Navigate the mainpage to the disered page
        /// </summary>
        public static void NavigateToPage(Type page, object parameters = null, bool navigateMenu = false)
        {
            if (navigateMenu)
            {
                var menuItem = me.MainNavigationMenu.MenuItems.FirstOrDefault(finder => page.FullName.Equals(((FrameworkElement)finder).Tag.ToString(), StringComparison.Ordinal));

                if (menuItem != null)
                    me.MainNavigationMenu.SelectedItem = menuItem;
            }
            me.frame.Navigate(page, parameters);
        }


        private async void GetSerialPorts()
        {
            Boolean? LastStartUpSuccesfull = ApplicationData.Current.LocalSettings.Values["LastStartUpSuccesfull"] as Boolean?;
            string LastSerialPortName = ApplicationData.Current.LocalSettings.Values["LastSerialPortName"] as string;
            string LastSerialPortID = ApplicationData.Current.LocalSettings.Values["LastSerialPortID"] as string;
            
            if (LastStartUpSuccesfull != null && LastSerialPortName != null && LastSerialPortID != null && LastStartUpSuccesfull == true)
            {
                var port = ApplicationState.Instance.SerialPorts.FirstOrDefault(finder => finder.Name.Equals(LastSerialPortName, StringComparison.Ordinal) && finder.PortID.Equals(LastSerialPortID, StringComparison.Ordinal));
                if (port != null)
                {
                    port.IsActive = true;

                    if (AppConfig.IOConfiguration.IsHeadedApplication)                                        
                        NavigateToPage(typeof(Views.FrontPage), null, true);

                    LogBuddy.Log(this, LogEventLevel.Information, $"previus selected serial port device {port.Name} selected at startup");
                    return;
                }
            }
            else
            {
                if (AppConfig.IOConfiguration.IsHeadedApplication)
                {
                    NavigateToPage(typeof(Views.SettingsView), null);
                    LogBuddy.Log(this, LogEventLevel.Information, "Could not find the Zwave stick from previus startup, redirect user to settings page");
                    _ = new Windows.UI.Popups.MessageDialog("Could not find the Zwave stick from previus startup. Select the Zwave stick device").ShowAsync();
                }
                else
                {
                    var port = await FindWorkingPorts().ConfigureAwait(false);

                    if (port != null)
                    {
                        LogBuddy.Log(this, LogEventLevel.Information, $"Serial port device {port.Name} found and selected");
                        ApplicationData.Current.LocalSettings.Values["LastStartUpSuccesfull"] = true;
                        ApplicationData.Current.LocalSettings.Values["LastSerialPortName"] = port.Name;
                        ApplicationData.Current.LocalSettings.Values["LastSerialPortID"] = port.PortID;
                    }
                    else
                    {
                        LogBuddy.Log(this, LogEventLevel.Information, $"Failed to find zwave serial port");
                    }
                }
            }            
        }




        private int startSuccesFull = 0; // 1 means true 0 means false. 


        private async Task<SerialPortInfo> FindWorkingPorts()
        {
            var itemSource = Watcher.Instance.Nodes as INotifyCollectionChanged;
            if (itemSource != null)
                itemSource.CollectionChanged += NodeList_CollectionChanged;

            return await Task.Run(() =>
            {
                foreach (SerialPortInfo portItem in ApplicationState.Instance.SerialPorts)
                {
                    portItem.IsActive = true;
                    Thread.Sleep(2000);

                    if(Interlocked.Exchange(ref startSuccesFull, 0) == 1)
                        return portItem; 
                    
                    portItem.IsActive = false;
                }
                return null;
            }).ConfigureAwait(false);
        }


        public bool StartUpSuccessfull { get; private set; }



        private void NodeList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Interlocked.Exchange(ref startSuccesFull, 1);

                ApplicationData.Current.LocalSettings.Values["LastStartUpSuccesfull"] = StartUpSuccessfull;

                var itemSource = sender as INotifyCollectionChanged;
                if (itemSource != null)
                    itemSource.CollectionChanged -= NodeList_CollectionChanged;

                LogBuddy.Log(this, LogEventLevel.Information, $"New node found after port selection, so succes");
            }
        }



    }
}
