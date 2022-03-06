using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Linker.Views
{
    /// <summary>
    /// Animated value when the value property goes up or down
    /// </summary>
    public sealed partial class NumbericChangeAnime : UserControl
    {
        public static double PositionPosValue { get; } = 25;
        public static double PositionNegValue { get; } = PositionPosValue * -1;


        public static Duration AnimeDuration { get; } = new Duration(TimeSpan.FromMilliseconds(150));
        public NumbericChangeAnime()
        {
            this.InitializeComponent();
            //DataContext = this;

            TranslationUpOldValue.To = PositionNegValue;
            TranslationUpOldValue.Duration = AnimeDuration;
            TranslationUpNewValue.From = PositionPosValue;
            TranslationUpNewValue.Duration = AnimeDuration;
            OpacityUpOldValue.Duration = AnimeDuration;
            OpacityUpNewValue.Duration = AnimeDuration;

            TranslationDownOldValue.To = PositionPosValue;
            TranslationDownOldValue.Duration = AnimeDuration;
            TranslationDownNewValue.From = PositionNegValue;
            TranslationDownNewValue.Duration = AnimeDuration;
            OpacityDownOldValue.Duration = AnimeDuration;
            OpacityDownNewValue.Duration = AnimeDuration;
        }


        public double OldValue { get; private set; }

        public double NumbericValue
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("NumbericValue", typeof(double), typeof(NumbericChangeAnime), new PropertyMetadata(0, new PropertyChangedCallback(OnValuePropertyChanged)));


        /// <summary>
        /// Value changed mate, time to animate
        /// </summary>
        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var me = sender as NumbericChangeAnime;
            me.newValue.Text = e.NewValue.ToString();
            me.oldValue.Text = e.OldValue.ToString();

            double newValue;
            double oldValue;

            if(double.TryParse(me.newValue.Text, out newValue) && double.TryParse(me.oldValue.Text, out oldValue))
            {
                if(newValue > oldValue)
                    me.StryBrd_Up.Begin();                   
                else if(newValue < oldValue)
                    me.StryBrd_Down.Begin();
            }                
        }
    }
}
