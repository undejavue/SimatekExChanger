using System.Collections.ObjectModel;
using System.Windows.Controls;
using ClassLibOracle;

namespace WPFinterface
{
    /// <summary>
    /// Interaction logic for ucDBtblRemote.xaml
    /// </summary>
    public partial class ucDBtblRemote : UserControl
    { 
        

        public ucDBtblRemote(ObservableCollection<FIX_STAN789_T> Oraitems)
        {
            InitializeComponent();
            if (Oraitems != null)
                dgv_DBtable.ItemsSource = Oraitems;
        }

        public ucDBtblRemote(ObservableCollection<object> collection )
        {
            InitializeComponent();

            if (collection != null)
                dgv_DBtable.ItemsSource = collection;
        }


    }
}
