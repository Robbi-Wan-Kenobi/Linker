using Linker.Code;
using Linker.Code.DataBase;
using Linker.Code.IOConfig;
using Linker.NetworkManager;
using Linker.Triggers;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Linker.Views
{
    public sealed partial class SettingsView : Page, INotifyPropertyChanged
    {
        public ApplicationState VM => ApplicationState.Instance;
        public event PropertyChangedEventHandler PropertyChanged;


        //public int BrightnessValue
        //{
        //    get { return ScrBrighnessBuddy.Brightness; }
        //    set  { ScrBrighnessBuddy.Brightness = value; }
        //}


        public bool MeasureTriggersRunning
        {
            get { return MeasureManager.Instance.MeasureTriggersRunning; }
            set { MeasureManager.Instance.MeasureTriggersRunning = value; }
        }

        public SettingsView()
        {
            this.InitializeComponent();
            if (!VM.SerialPorts.Any())
                NoDevicesPanel.Visibility = Visibility.Visible;

            DataContext = this;

            MeasureManager.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await VM.RefreshSerialPortsAsync();
            }
            catch (Exception ex)
            {
                LogBuddy.Log(this, LogEventLevel.Error, $"Serialport list refresh failed, reason: {ex.Message}");
            }
            NoDevicesPanel.Visibility = VM.SerialPorts.Any() ? Visibility.Collapsed : Visibility.Visible;
        }




        public int DatabaseRecordsSaveInterval => AppConfig.IOConfiguration.DatabaseRecordsSaveInterval.IntervalTime;

        public int SendRecordsToCloudInterval => AppConfig.IOConfiguration.SendRecordsToCloudInterval.IntervalTime;

        private void IntervalTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.NewText))                
                ((TextBox)sender).Text = ((TextBox)sender).Tag.ToString();
            else
                args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }


        private void ButtonRestart_Click(object sender, RoutedEventArgs e)
        {
            var result = CoreApplication.RequestRestartAsync("");
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
        }


        private void SaveDbFile_Click(object sender, RoutedEventArgs e)
        {
            StoreFile();
        }


        private void LoadDbFile_Click(object sender, RoutedEventArgs e)
        {
            LoadFile();
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            byte value = Convert.ToByte(btn.Tag.ToString());
           // ScrBrighnessBuddy.SetNewBrightness(value);
        }




        private void ResetDb_Click(object sender, RoutedEventArgs e)
        {
            if (MeasureTriggersRunning)
            {
                var messageDialog = new MessageDialog("Not possible to clear database measurements are running");
                var result2 = messageDialog.ShowAsync();                
                return;
            }
            else
            {
                var messageDialog2 = new MessageDialog("Are you sure? this will destory all data recorded.");
                
                messageDialog2.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(CommandRemoveDBInvokedHandler)));
                messageDialog2.Commands.Add(new UICommand("No"));

                var result3 = messageDialog2.ShowAsync();
            }
        }

        private void CommandRemoveDBInvokedHandler(IUICommand command)
        {
            LogBuddy.Log(this, LogEventLevel.Information, "User pressed database reset");

            var result = DatabaseManager.Instance.GiveSqlCommand("DROP TABLE [BuffelData]");
            _ = MeasureManager.Instance.Initialize();
            //TODO:reinit measuremanager
        }


        /// <summary>
        /// Store  database file with a file picker
        /// </summary>
        /// <returns></returns>
        private async void StoreFile()
        {
            try
            {
                
                StorageFile localDbFile = await ApplicationData.Current.LocalFolder.GetFileAsync(DatabaseManager.DatabaseName).AsTask();
                if (localDbFile != null)
                {
                    FileSavePicker savePicker = new FileSavePicker();
                    savePicker.SuggestedSaveFile = localDbFile;
                    savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                    savePicker.FileTypeChoices.Add("sqliteDB", new List<string> { ".db" });

                    StorageFile pickerfile = await savePicker.PickSaveFileAsync();
                    if (pickerfile != null)
                    {
                        await localDbFile.CopyAndReplaceAsync(pickerfile).AsTask();
                        LogBuddy.Log(this, LogEventLevel.Information, $"User stored the database file {pickerfile.Name} to {pickerfile.Path}");
                    }
                }   
                else
                {
                    LogBuddy.Log(this, LogEventLevel.Information, "Failed to store the database");
                }
            }
            catch(Exception ex)
            {
                LogBuddy.Log(this, LogEventLevel.Information, $"User action to manualy save the database file failed, reason: {ex.Message}");
            }
        }

        /// <summary>
        /// Load databse file with a filePicker
        /// </summary>
        /// <returns></returns>
        private async void LoadFile()
        {
            FileOpenPicker loadPicker = new FileOpenPicker();
            loadPicker.FileTypeFilter.Add(".db");
            loadPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            StorageFile pickerFileTask = await loadPicker.PickSingleFileAsync();
            if (pickerFileTask != null)
            {
                var originalFileTask = await ApplicationData.Current.LocalFolder.GetFileAsync(DatabaseManager.DatabaseName);
                if (originalFileTask != null)
                {
                    await pickerFileTask.CopyAndReplaceAsync(originalFileTask);
                    LogBuddy.Log(this, LogEventLevel.Information, $"User replaced the database file with the file {pickerFileTask.Name} that came from {pickerFileTask.Path}");
                }
            }            
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string value = (string) e.AddedItems.First();
            object selectedTheme;
            Enum.TryParse(typeof(ElementTheme), value, out selectedTheme);
            MainPage.MainPageContentFrameworkElement.RequestedTheme = (ElementTheme)selectedTheme;
        }



        public string ThemeName
        {
            get { return MainPage.MainPageContentFrameworkElement.RequestedTheme.ToString(); }            
        }


        private string[] themeNames;

        

        public string[] ThemeNames
        {
            get
            {
                if (themeNames == null)
                    themeNames = Enum.GetNames(typeof(ElementTheme));
                return themeNames;
            }
        }


        private void serialPortsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var SelectedcheckBox = sender as CheckBox;
            
            var serialPortInfo = SelectedcheckBox.Tag as SerialPortInfo;

            if (serialPortInfo != null)
            {
                ApplicationData.Current.LocalSettings.Values["LastStartUpSuccesfull"] = false;
                ApplicationData.Current.LocalSettings.Values["LastSerialPortName"] = serialPortInfo.Name;
                ApplicationData.Current.LocalSettings.Values["LastSerialPortID"] = serialPortInfo.PortID;

                LogBuddy.Log(this, LogEventLevel.Information, $"New serial port device {serialPortInfo.Name} selected");

                foreach (SerialPortInfo port in ApplicationState.Instance.SerialPorts)
                    if (port != serialPortInfo)
                        port.IsActive = false;

                var itemSource = Watcher.Instance.Nodes as INotifyCollectionChanged;
                if (itemSource != null)
                    itemSource.CollectionChanged += NodeList_CollectionChangedTest;
            }
        }


        private void NodeList_CollectionChangedTest(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ApplicationData.Current.LocalSettings.Values["LastStartUpSuccesfull"] = true;

                var itemSource = sender as INotifyCollectionChanged;
                if (itemSource != null)
                    itemSource.CollectionChanged -= NodeList_CollectionChangedTest;

                LogBuddy.Log(this, LogEventLevel.Information, $"New node found after new port selection, so succes");
            }
        }

        private void ButtonZwaveCache_Click(object sender, RoutedEventArgs e)
        {
            _ = RemoveZwaveCacheFile();
        }

        private async Task RemoveZwaveCacheFile()
        {
            StorageFolder appInstalledFolder = ApplicationData.Current.LocalFolder;
            var files = appInstalledFolder.GetFilesAsync();

            foreach (StorageFile fileItem in await files)
                if(fileItem.Name.StartsWith("ozwcache_", StringComparison.OrdinalIgnoreCase))
                    _ = fileItem.DeleteAsync(StorageDeleteOption.PermanentDelete);

            LogBuddy.Log(this, LogEventLevel.Information, "user removed the zwave log cache");
        }


        //private ElementTheme myVar;

        //public IEnumerable<ElementTheme> ThemeNames
        //{
        //    get { return Enum.GetValues(typeof(ElementTheme)).Cast<ElementTheme>();  ; }
        //}
    }
}
