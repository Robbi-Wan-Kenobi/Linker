using Linker.Code.DataBase;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml.Data;
using System.Threading.Tasks;
using System;
using Windows.UI.Xaml.Controls.Primitives;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Linker.Code;
using Linker.Nodes;
using Serilog.Events;
using Linker.Code.IOConfig;

namespace Linker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DatabaseDataView : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Dataholder> TableRows { get; }

        private DataGrid dataGrid;

        private bool autoUpdateTableActive;

        

        public bool AutoUpdateTableActive
        {
            get { return autoUpdateTableActive; }
            set
            {
                if (autoUpdateTableActive != value)
                {
                    autoUpdateTableActive = value;

                    if (autoUpdateTableActive)
                        DatabaseManager.Instance.OnDatabaseValuesUpdated += Instance_OnDatabaseValuesUpdated;
                    else
                        DatabaseManager.Instance.OnDatabaseValuesUpdated -= Instance_OnDatabaseValuesUpdated;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoUpdateTableActive)));
            }
        }


        public DatabaseDataView()
        {
            this.InitializeComponent();
            TableRows = new ObservableCollection<Dataholder>();
                       
            DataContext = this;

            dataGrid = GetDataGrid(this.Content);

            Loaded += DatabaseDataView_Loaded;
        }

        

        private void DatabaseDataView_Loaded(object sender, RoutedEventArgs e)
        {


            SetDataBaseColumns(dataGrid.Columns);


            AutoUpdateTableActive = true;
        }



       

        private void GetLatestValues_Click(object sender, RoutedEventArgs e)
        {
            int recordsAmount;
            if(int.TryParse(amountInputField.Text, out recordsAmount))
            {
                AutoUpdateTableActive = false;

                var UpdateDataBaseButtonClick = NewTableRowsAsync(recordsAmount, TableRows);
                UpdateDataBaseButtonClick.Wait();

                AutoUpdateTableActive = true;
            }
        }



        private async Task NewTableRowsAsync(int amount, ObservableCollection<Dataholder> tableRows)
        {
            tableRows.Clear();

            var result = await DatabaseManager.Instance.LoadLastNrOfRecordsAsync(amount);
            foreach (var item in result)
                tableRows.Add(new Dataholder(item));

            if (tableRows.Count > 100)
                tableRows.RemoveAt(0);
        }


        private void AutoUpdateSwitch_Click(object sender, RoutedEventArgs e)
        {
            var toggle = (ToggleButton)sender;
            AutoUpdateTableActive = toggle.IsChecked == true;
        }


        

        private void Instance_OnDatabaseValuesUpdated(object sender, ValuesUpdateEventArgs e)
        {
            var result = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => {
                
                var databaseFetch = DatabaseManager.Instance.LoadLastRecord(null).ConfigureAwait(true);
                var dataholder = new Dataholder();
                dataholder.SetValues(await databaseFetch);
                TableRows.Add(dataholder);
            });            
        }


        /// <summary>
        /// Returns a datagridColumn that is bound to one of the DataHolder objects
        /// </summary>
        private DataGridColumn GetBindedColumn(string columnName, int bindColumnNr)
        {
            var column = new DataGridTextColumn();
            column.Header = columnName;
            column.Binding = new Binding { Path = new PropertyPath($"data{bindColumnNr}") };
            return column;
        }


        private void TextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }



        /// <summary>
        /// returns the datagrid on page so that the name property does not have to be used (can cause build errors)
        /// </summary>
        private DataGrid GetDataGrid(UIElement content)
        {
            Panel panel = content as Panel;
            if (panel != null)
                return panel.Children.FirstOrDefault(finder => finder is DataGrid) as DataGrid;

            LogBuddy.Log(this, LogEventLevel.Error, "Cannot find datagrid on page");
            return null;
        }




        


        /// <summary>
        /// Sets all the database columns
        /// </summary>
        /// <param name="databaseColumns">the datagrid collection where to set the columns</param>
        private void SetDataBaseColumns(ObservableCollection<DataGridColumn> databaseColumns)
        {
            int counter = 0;
            List<string> columnsNames = new List<string>();
            foreach (string columnName in DatabaseManager.Instance.GetColumnNames())
            {
                counter += 1;

                if (columnName.StartsWith("Ch", StringComparison.OrdinalIgnoreCase) && char.IsNumber(columnName[2]))
                {
                    MeasureNode node = AppConfig.CombinedChannelsList.FirstOrDefault(finder => finder.Behaviour != null && finder.Behaviour.DataBaseRecord.Equals(columnName, StringComparison.Ordinal));
                    if (node != null)
                    {
                        databaseColumns.Add(GetBindedColumn(node.Name, counter));
                        continue;
                    }
                }
                databaseColumns.Add(GetBindedColumn(columnName, counter));
            }
        }




    }




    public class Dataholder
    {
        public string data1 { get; private set; } = string.Empty;
        public string data2 { get; private set; } = string.Empty;
        public string data3 { get; private set; } = string.Empty;
        public string data4 { get; private set; } = string.Empty;
        public string data5 { get; private set; } = string.Empty;
        public string data6 { get; private set; } = string.Empty;
        public string data7 { get; private set; } = string.Empty;
        public string data8 { get; private set; } = string.Empty;
        public string data9 { get; private set; } = string.Empty;
        public string data10 { get; private set; } = string.Empty;

        private string[] dataItems;

        public Dataholder()
        {
            dataItems = new string[] { data1, data2, data3, data4, data5, data6, data7, data8, data9, data10 };
        }

        public Dataholder(object[] input) 
        {
            dataItems = new string[] { data1, data2, data3, data4, data5, data6, data7, data8, data9, data10 };
            SetValues(input);
        }

        public void SetValues(object[] input)
        {
            if (input == null)
                return;

            int lowestLength = (input.Length < dataItems.Length) ? input.Length : dataItems.Length;

            if (lowestLength >= 10)
                lowestLength = 10;
            
            switch (input.Length)
            {
                case 1:
                    if (input[0] != null) data1 = input[0].ToString();
                    break;
                case 2:
                    if (input[0] != null) data1 = input[0].ToString();
                    if (input[1] != null) data2 = input[1].ToString();
                    break;
                case 3:
                    if (input[0] != null) data1 = input[0].ToString();
                    if (input[1] != null) data2 = input[1].ToString();
                    if (input[2] != null) data3 = input[2].ToString();
                    break;
                case 4:
                    if (input[0] != null) data1 = input[0].ToString();
                    if (input[1] != null) data2 = input[1].ToString();
                    if (input[2] != null) data3 = input[2].ToString();
                    if (input[3] != null) data4 = input[3].ToString();
                    break;
                case 5:
                    if (input[0] != null) data1 = input[0].ToString();
                    if (input[1] != null) data2 = input[1].ToString();
                    if (input[2] != null) data3 = input[2].ToString();
                    if (input[3] != null) data4 = input[3].ToString();
                    if (input[4] != null) data5 = input[4].ToString();
                    break;
                case 6:
                    if (input[0] != null) data1 = input[0].ToString();
                    if (input[1] != null) data2 = input[1].ToString();
                    if (input[2] != null) data3 = input[2].ToString();
                    if (input[3] != null) data4 = input[3].ToString();
                    if (input[4] != null) data5 = input[4].ToString();
                    if (input[5] != null) data6 = input[5].ToString();
                    break;
                case 7:
                    if (input[0] != null) data1 = input[0].ToString();
                    if (input[1] != null) data2 = input[1].ToString();
                    if (input[2] != null) data3 = input[2].ToString();
                    if (input[3] != null) data4 = input[3].ToString();
                    if (input[4] != null) data5 = input[4].ToString();
                    if (input[5] != null) data6 = input[5].ToString();
                    if (input[6] != null) data7 = input[6].ToString();
                    break;
                case 8:
                    if (input[0] != null) data1 = input[0].ToString();
                    if (input[1] != null) data2 = input[1].ToString();
                    if (input[2] != null) data3 = input[2].ToString();
                    if (input[3] != null) data4 = input[3].ToString();
                    if (input[4] != null) data5 = input[4].ToString();
                    if (input[5] != null) data6 = input[5].ToString();
                    if (input[6] != null) data7 = input[6].ToString();
                    if (input[7] != null) data8 = input[7].ToString();
                    break;
                case 9:
                    if (input[0] != null) data1 = input[0].ToString();
                    if (input[1] != null) data2 = input[1].ToString();
                    if (input[2] != null) data3 = input[2].ToString();
                    if (input[3] != null) data4 = input[3].ToString();
                    if (input[4] != null) data5 = input[4].ToString();
                    if (input[5] != null) data6 = input[5].ToString();
                    if (input[6] != null) data7 = input[6].ToString();
                    if (input[7] != null) data8 = input[7].ToString();
                    if (input[8] != null) data9 = input[8].ToString();
                    break;
                case 10:
                    if (input[0] != null) data1 = input[0].ToString();
                    if (input[1] != null) data2 = input[1].ToString();
                    if (input[2] != null) data3 = input[2].ToString();
                    if (input[3] != null) data4 = input[3].ToString();
                    if (input[4] != null) data5 = input[4].ToString();
                    if (input[5] != null) data6 = input[5].ToString();
                    if (input[6] != null) data7 = input[6].ToString();
                    if (input[7] != null) data8 = input[7].ToString();
                    if (input[8] != null) data9 = input[8].ToString();
                    if (input[9] != null) data10 = input[9].ToString();
                    break;
                default:
                    break;
            }
        }
    }
}
