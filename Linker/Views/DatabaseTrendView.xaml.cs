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
using LiveCharts;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Navigation;
using System.Collections;
using LiveCharts.Uwp;
using System.Runtime.CompilerServices;
using Serilog.Events;
using Linker.Code.Behaviours;
using Linker.Code.IOConfig;

namespace Linker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DatabaseTrendView : Page, INotifyPropertyChanged
    {
        private ZoomingOptions zoomingMode;


        public event PropertyChangedEventHandler PropertyChanged;
        public List<string> Paths { get; private set; } = new List<string>();

        private bool autoUpdateTableActive;

        private CartesianChart ChartControl { get; set; }

        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();
        public ObservableCollection<string> Labels { get; set; }
        public Func<double, string> XFormatter { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public ZoomingOptions ZoomingMode
        {
            get { return zoomingMode; }
            set
            {
                zoomingMode = value;
                ChartControl.Zoom = zoomingMode;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomingMode)));
            }
        }




        public DatabaseTrendView()
        {
            this.InitializeComponent();

            ChartControl = GetChartControl(this.Content);

            Labels = new ObservableCollection<string>();
            SeriesCollection = new SeriesCollection();

            ZoomingMode = ZoomingOptions.Xy;

            XFormatter = val => new DateTime((long)val).ToString("dd MMM");
            //YFormatter = val => val.ToString("C");


            ChartControl.DisableAnimations = true;
            DataContext = this;
            Loaded += DatabaseDataView_Loaded;

            Unloaded += DatabaseTrendView_Unloaded;
        }

        private void DatabaseTrendView_Unloaded(object sender, RoutedEventArgs e)
        {
            //AutoUpdateTableActive = false;
            //ChartControl.Series = null;

            DatabaseManager.Instance.OnDatabaseValuesUpdated -= Instance_OnDatabaseValuesUpdated;
        }

        
        /// <summary>
        /// returns the datagrid on page so that the name property does not have to be used (can cause build errors)
        /// </summary>
        private CartesianChart GetChartControl(UIElement content)
        {
            Panel panel = content as Panel;
            if (panel != null)
                return panel.Children.FirstOrDefault(finder => finder is CartesianChart) as CartesianChart;

            LogBuddy.Log(this, LogEventLevel.Error, "Cannot find CartesianChart on page");
            return null;
        }


        private Axis XAxis { get; set; }

        private Axis YAxis { get; set; }


        private void DatabaseDataView_Loaded(object sender, RoutedEventArgs e)
        {
            //YFormatter = value => value.ToString("t");
            ChartControl.AxisX = new AxesCollection();
            XAxis = new Axis()
            {
                //Title = "Time",
                Labels = Labels,
                LabelFormatter = XFormatter,
                Separator = new Separator()
                {
                    StrokeThickness = 0.5,
                    Stroke = new SolidColorBrush(Colors.Green),
                    StrokeDashArray = new DoubleCollection() { 4, 1 }
                }
            };
            ChartControl.AxisX.Add(XAxis);

            ChartControl.AxisY = new AxesCollection();
            YAxis = new Axis()
            {
                Title = "Values",
                LabelFormatter = YFormatter,
                Separator = new Separator()
                {
                    StrokeThickness = 0.5,
                    Stroke = new SolidColorBrush(Colors.Green),
                    StrokeDashArray = new DoubleCollection() { 1, 4 }
                }
            };
            ChartControl.AxisY.Add(YAxis);

            ChartControl.Series = SeriesCollection;


            var result = DisplayTrendValues(40);
            result.Wait();

            //AutoUpdateTableActive = true;
            DatabaseManager.Instance.OnDatabaseValuesUpdated += Instance_OnDatabaseValuesUpdated;
        }


        


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var Parameter = e.Parameter as string;
            if (Parameter != null)
                Paths.Add(Parameter);
            else
            {
                var pathParameters = e.Parameter as IEnumerable<string>;
                if (pathParameters != null)
                    Paths.AddRange(pathParameters);
            }

        }


        private void TogleZoomingMode(object sender, RoutedEventArgs e)
        {
            switch (ZoomingMode)
            {
                case ZoomingOptions.None:
                    ZoomingMode = ZoomingOptions.X;
                    break;
                case ZoomingOptions.X:
                    ZoomingMode = ZoomingOptions.Y;
                    break;
                case ZoomingOptions.Y:
                    ZoomingMode = ZoomingOptions.Xy;
                    break;
                case ZoomingOptions.Xy:
                    ZoomingMode = ZoomingOptions.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("togleZoomingMode");
            }
        }


        private void ResetZoomOnClick(object sender, RoutedEventArgs e)
        {
            //Use the axis MinValue/MaxValue properties to specify the values to display.
            //use double.Nan to clear it.

            XAxis.MinValue = double.NaN;
            XAxis.MaxValue = double.NaN;
            YAxis.MinValue = double.NaN;
            YAxis.MaxValue = double.NaN;
        }


        private void Instance_OnDatabaseValuesUpdated(object sender, ValuesUpdateEventArgs e)
        {
            var result = UpdateLatestValue();
        }


        private async Task UpdateLatestValue()
        {
            if (Paths == null || Paths.Count == 0)
                return;

            var datTimePaths = new List<string>(Paths);
            datTimePaths.Insert(0, "DateTime");

            try
            {
                var databaseFetch = DatabaseManager.Instance.LoadLastRecord(datTimePaths.ToArray()).ConfigureAwait(true);
                var dispatchTask = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    double parseResult;
                    int iRowCounter = 0;
                    var result = await databaseFetch;
                    if (result.Length > 0)
                    {

                        Labels.Add(result[0].ToString());

                        foreach (object dbResult in result.Skip(1))
                        {
                            if (Double.TryParse(dbResult.ToString(), out parseResult))
                            {
                                if (SeriesCollection.Count > 0)
                                    if (SeriesCollection[0].Values != null && SeriesCollection[0].Values.Count > 0)
                                        SeriesCollection[iRowCounter].Values.Add(parseResult);

                                if (SeriesCollection[iRowCounter].Values.Count > 100)
                                    SeriesCollection[iRowCounter].Values.RemoveAt(0);
                            }

                            iRowCounter += 1;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LogBuddy.Log(this, LogEventLevel.Error, $"Failed to fetch database values while showwing trend, reason: { ex.Message }");
            }
        }


        private async Task DisplayTrendValues(int amount)
        {
            if (Paths == null || Paths.Count == 0)
                return;

            var datTimePaths = new List<string>(Paths);
            datTimePaths.Insert(0, "DateTime");

            foreach (string path in datTimePaths.Skip(1))
            {
                MeasureNode node = AppConfig.CombinedChannelsList.FirstOrDefault(finder => finder.Behaviour != null && finder.Behaviour.DataBaseRecord.Equals(path, StringComparison.Ordinal));
                if (node != null)
                    SeriesCollection.Add(new LineSeries() { Title = node.Name, Tag = node.Behaviour.DataBaseRecord });
            }
            
            var databaseFetch = DatabaseManager.Instance.LoadLastRecords(datTimePaths.ToArray(), amount);

            if (databaseFetch != null)
            {
                try
                {
                    var dispatchTask = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        var columnsList = new List<object[]>();
                        List<object[]> dbResult = await databaseFetch.ConfigureAwait(true);

                        if (dbResult.Count > 0)
                        {
                            for (int icolumn = 0; icolumn < dbResult[0].Length; icolumn++)
                                columnsList.Add(new object[dbResult.Count]);

                            for (int iRows = 0; iRows < dbResult.Count; iRows++)
                                for (int iColumn = 0; iColumn < dbResult[iRows].Length; iColumn++)
                                    columnsList[iColumn][iRows] = dbResult[iRows][iColumn];

                            if (columnsList.Count > 0)
                            {
                                for (int iRows = 0; iRows < columnsList[0].Length; iRows++)
                                    Labels.Add(columnsList[0][iRows].ToString());

                                for (int icolumn = 0; icolumn < SeriesCollection.Count; icolumn++)
                                {
                                    var doublearray = Array.ConvertAll<object, double>(columnsList[icolumn + 1], conv => conv == DBNull.Value ? 0 : double.Parse(conv.ToString()));

                                    SeriesCollection[icolumn].Values = new ChartValues<double>(doublearray);
                                }
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    LogBuddy.Log(this, LogEventLevel.Error, $"Failed to fetch database values while showwing trend, reason: { ex.Message }");
                }
            }
        }


        public bool AutoUpdateTableActive
        {
            get { return autoUpdateTableActive; }
            set
            {
                //if (autoUpdateTableActive != value)
                //{
                //    autoUpdateTableActive = value;

                //    if (autoUpdateTableActive)
                //        DatabaseManager.Instance.OnDatabaseValuesUpdated += Instance_OnDatabaseValuesUpdated;
                //    else
                //        DatabaseManager.Instance.OnDatabaseValuesUpdated -= Instance_OnDatabaseValuesUpdated;
                //}
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoUpdateTableActive)));
            }
        }

        
    }


    public class ZoomingModeCoverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((ZoomingOptions)value)
            {
                case ZoomingOptions.None:
                    return "None";
                case ZoomingOptions.X:
                    return "Zoom X";
                case ZoomingOptions.Y:
                    return "Zoom Y";
                case ZoomingOptions.Xy:
                    return "Zoom XY";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
