using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Linker.NetworkManager;
using OpenZWave;
using Linker.Code.Nodes;
using System;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Linker.Views
{
    public sealed partial class SelectionDialogZNode : ContentDialog
    {
        public ObservableCollection<Tuple<string, ZWValueId>> SelectedItems { get; set; }


        public SelectionDialogZNode()
        {
            this.InitializeComponent();
            SelectedItems = new ObservableCollection<Tuple<string, ZWValueId>>();

            DataContext = this;

            Loaded += SelectionDialog_Loaded;
        }

        private void SelectionDialog_Loaded(object sender, RoutedEventArgs e)
        {
            //every item needs to be selected
            //foreach (ZWValueId item in SelectedItems)
            //{
            //    CheckSelectiom(item);
            //}

            //foreach (Node nodeItem in ItemsSource)
            //{
            //    var list = (ObservableCollection<ZWValueId>) nodeItem.UserValues;
            //    list.CollectionChanged += ZwaveValueList_CollectionChanged;
            //}

            //var itemSource = ItemsSource as INotifyCollectionChanged;

            //if (itemSource != null)
            //    itemSource.CollectionChanged += NodeList_CollectionChanged;
        }

          

        /// <summary>
        /// Select the node if it is presend
        /// </summary>
        /// <param name="valueId"></param>
        private void CheckSelectiom(MeasureZWaveNode valueId)
        {
            
           
        }


        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList<TreeViewNode>), typeof(SelectionDialogZNode), new PropertyMetadata(null));



        /// <summary>
        /// Should be the ok button, when the button is pressed the SelectwedItems list is updated with missing and district items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            foreach (TreeViewNode nodeItem in listView.SelectedNodes)
            {
                string parentName = string.Empty;
                if (nodeItem.Parent != null && nodeItem.Parent.Content != null)
                {
                    var node = nodeItem.Parent.Content as Node;
                    if(node != null)
                        parentName = node.Product;
                }

                if(nodeItem.Content != null && nodeItem.Content.GetType() == typeof(ZWValueId))
                {
                    var pathAndValue = new Tuple<string, ZWValueId>(parentName, (ZWValueId)nodeItem.Content);
                    SelectedItems.Add(pathAndValue);
                }
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // DO nothing
        }





        public IEnumerable<Node> ItemsSource
        {
            get { return (IEnumerable<Node>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<Node>), typeof(SelectionDialogZNode), new PropertyMetadata(null));

    }




    public class NodeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NodeTemplate { get; set; }
        public DataTemplate ValueTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is Node)
                return NodeTemplate;
            if (item is ZWValueId)
                return ValueTemplate;
            return base.SelectTemplateCore(item);       
        }
    }
}
