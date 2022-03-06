using Linker.Code;
using Linker.Code.Behaviours;
using Linker.Code.IOConfig;
using Linker.Code.Nodes;
using OpenZWave;
using System;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Linker
{
    public class NegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
            {
                value = !(bool)value;
                if (targetType == typeof(Windows.UI.Xaml.Visibility))
                    value = ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }



    public class BehaviourValueListItems : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            MeasureZWaveNode zwValItem = value as MeasureZWaveNode;
            ZWValueId zwValueId = null;

            if (zwValItem != null && zwValItem.ZWValueId != null)
                zwValueId = zwValItem.ZWValueId;

            if (zwValueId == null)
                zwValueId = value as ZWValueId;
            
            if (zwValueId != null)
            {
                if (zwValueId.Type == ZWValueType.Bool) 
                    return new string[] { "True", "False" };

                string[] values;
                ZWManager.Instance.GetValueListItems(zwValueId, out values);
                return values;
            }
            return null;                       
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }



    public class BehaviourTriggerValue : IValueConverter
    {
        public object Convert(object v, Type t, object p, string l) =>
            (v as BehaviourBoolCount)?.TriggerValue;

        public object ConvertBack(object v, Type t, object p, string l)
        {
            return null;
        }
    }


    //public class BehaviourTriggerValue : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        BehaviourBoolCount behavior = value as BehaviourBoolCount;
            
    //        if (behavior != null)
    //        {
                
    //            return behavior.TriggerValue;
    //        }
    //        return null;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}


    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
            {
                if ((bool)value) return 1d;
                return 0.5;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public  class VisibleWhenZeroConverter : IValueConverter
    {
        public  object Convert(object v, Type t, object p, string l) =>
            Equals(0d, (double)v) ? Visibility.Visible : Visibility.Collapsed;

        public  object ConvertBack(object v, Type t, object p, string l) => null;
    }


    public class BoolTrueToVisibileConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, string l) =>
            (bool)v ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object v, Type t, object p, string l) => null;
    }




    public class AddOrEditNodeConverter : DependencyObject, IValueConverter
    {
        public Symbol IconOne
        {
            get { return (Symbol)GetValue(IconOneProperty); }
            set  { SetValue(IconOneProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconOneProperty =
            DependencyProperty.Register("IconOne", typeof(Symbol), typeof(AddOrEditNodeConverter), new PropertyMetadata(null));


        public Symbol IconTwo
        {
            get { return (Symbol)GetValue(IconTwoProperty); }
            set   { SetValue(IconTwoProperty, value);  }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconTwoProperty =
            DependencyProperty.Register("IconTwo", typeof(Symbol), typeof(AddOrEditNodeConverter), new PropertyMetadata(null));

        

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ZWValueId zwValue = value as ZWValueId;
            if(zwValue != null)
            {
                if (AppConfig.CombinedChannelsList.Any(finder => finder.Equals(zwValue)))
                    return new SymbolIcon(IconOne);
                else
                    return new SymbolIcon(IconTwo); ;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    public class IconConverter : DependencyObject, IValueConverter
    {        
        public SymbolIcon IconOne
        {
            get { return (SymbolIcon)GetValue(IconOneProperty); }
            set { SetValue(IconOneProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconOneProperty =
            DependencyProperty.Register("IconOne", typeof(SymbolIcon), typeof(IconConverter), new PropertyMetadata(null));


        public SymbolIcon IconTwo
        {
            get { return (SymbolIcon)GetValue(IconTwoProperty); }
            set { SetValue(IconTwoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconTwoProperty =
            DependencyProperty.Register("IconTwo", typeof(SymbolIcon), typeof(IconConverter), new PropertyMetadata(null));


       

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? valueBool = value as bool?;
            if (valueBool != null)
            {
                if (valueBool == true)
                    return IconOne;
                else
                    return IconTwo;
            }

            int? valueint = value as int?;
            if (valueint != null)
            {
                if (valueint == 1)
                    return IconOne;
                else
                    return IconTwo;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    public class TypeToNameCoverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, string l) =>
            v.GetType().Name;

        public object ConvertBack(object v, Type t, object p, string l) => null;
    }
}
