using Linker.Code;
using Linker.Code.Buddys;
using Linker.Code.IOConfig;
using Linker.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Linker.Views
{
    public sealed partial class NodesControl : UserControl
    {
        public PropertyObserveCollection<MeasureNode> NodeCollection { get { return AppConfig.CombinedChannelsList; } }

        public NodesControl()
        {
            this.InitializeComponent();
            DataContext = this;
        }
    }
}
