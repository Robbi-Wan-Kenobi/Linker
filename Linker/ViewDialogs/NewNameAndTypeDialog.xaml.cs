using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Linker.Views
{
    public sealed partial class NewNameAndTypeDialog : ContentDialog
    {
        //public event PropertyChangedEventHandler PropertyChanged;
        private string[] typeNames;

        public string[] TypeNames
        {
            get { return typeNames; }
            set { typeNames = value; }
        }

        public bool ForceTypeSelection { get; set; }



        public string ItemName { get; set; }

        public string SelectedType { get; set; }

        //public string SelectedType
        //{
        //    get { return string.Empty; }
        //    set
        //    {
        //        if (PropertyChanged != null)
        //            PropertyChanged.Invoke(this, new PropertyChangedEventArgs("SelectedType"));
        //    }
        //}

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ForceTypeSelection)
            {
                IsPrimaryButtonEnabled = e.AddedItems.Count > 0;
            }
        }


        public NewNameAndTypeDialog()
        {
            this.InitializeComponent();
            DataContext = this;
        }

        protected override void OnApplyTemplate()
        {
            if (ForceTypeSelection)
                IsPrimaryButtonEnabled = false;

            base.OnApplyTemplate(); 
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }


       
    }
}
