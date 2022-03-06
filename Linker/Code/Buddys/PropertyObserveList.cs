using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linker.Code.Buddys
{
    public sealed class PropertyObserveCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public event EventHandler<ItemChangedEventArgs> ItemPropertyChanged;
        public delegate void StatusUpdateHandler(object sender, ItemChangedEventArgs e);


        public List<string> PropertyUpdateIgnoreList { get; } = new List<string>();

        public PropertyObserveCollection()
        {
            CollectionChanged += PropertyObserveCollectionChanged;
        }


        public PropertyObserveCollection(IEnumerable<T> items) : this()
        {
            if (items != null)
                foreach (var item in items)
                    this.Add(item);
        }

        private void PropertyObserveCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Object item in e.NewItems)
                    ((INotifyPropertyChanged)item).PropertyChanged += ItemPropChanged;
            if (e.OldItems != null)
                foreach (Object item in e.OldItems)
                    ((INotifyPropertyChanged)item).PropertyChanged -= ItemPropChanged;
        }


        /// <summary>
        /// Item Property changed check if this event needs an public update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemPropChanged(object sender, PropertyChangedEventArgs e)
        {
            bool ignorablePropertyFound = false;
            for (int i = 0; i < PropertyUpdateIgnoreList.Count; i++)
                if(PropertyUpdateIgnoreList[i].Equals(e.PropertyName, StringComparison.Ordinal))
                {
                    ignorablePropertyFound = true;
                    break;
                }
            if(!ignorablePropertyFound)
                ItemPropertyChanged?.Invoke(sender, new ItemChangedEventArgs(PropertyChangedReason.ItemChanged, e.PropertyName, sender));
        }
    }


    /// <summary>
    /// Updates if there was a database insert
    /// </summary>
    public class ItemChangedEventArgs : EventArgs
    {
        public PropertyChangedReason ChangeReason { get; private set; }

        public string PropertyName { get; private set; }

        public object ChangedItem { get; private set; }

        public ItemChangedEventArgs(PropertyChangedReason changedReason, string propertyName, object changedItem) : base()
        {
            ChangeReason = changedReason;
            PropertyName = propertyName;
            ChangedItem = changedItem;
        }
    }



    public enum PropertyChangedReason
    {
        // Summary:
        //     Much of the list has changed. Any listening controls should refresh all their
        //     data from the list.
        Reset,
        // Summary:
        //     An item added to the list. System.ComponentModel.ListChangedEventArgs.NewIndex
        //     contains the index of the item that was added.
        ItemAdded,
        // Summary:
        //     An item deleted from the list. System.ComponentModel.ListChangedEventArgs.NewIndex
        //     contains the index of the item that was deleted.
        ItemDeleted,
        // Summary:
        //     An item changed in the list. System.ComponentModel.ListChangedEventArgs.NewIndex
        //     contains the index of the item that was changed.
        ItemChanged
    }




    class ItemChangedList<T> : Collection<T> where T : INotifyPropertyChanged
    {
        
        public ItemChangedList()
        {
           
        }

        public ItemChangedList(IEnumerable<T> items) : this()
        {
            if (items != null)
                foreach (var item in items)
                    this.Add(item);
        }

     
        public void Add()
        {

        }
    }

}
