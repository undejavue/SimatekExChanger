using System.Collections.ObjectModel;
using System.Windows.Controls;
using ClassLibOracle;
using EFlocalDB;

namespace WPFinterface
{
    /// <summary>
    /// Interaction logic for ucDBtblRemote.xaml
    /// </summary>
    public partial class ucDBtblRemote : UserControl
    {
        private ObservableCollection<dbLocalRecord> observableCollection;

        public ucDBtblRemote(ObservableCollection<dbLocalRecord> collection)
        {
            InitializeComponent();

            if (collection != null)
                dgv_DBtable.ItemsSource = collection;
        }

        public ucDBtblRemote(ObservableCollection<FIX_STAN789_T> collection)
        {
            InitializeComponent();

            if (collection != null)
                dgv_DBtable.ItemsSource = collection;
        }

        public ucDBtblRemote(ObservableCollection<object> collection )
        {
            InitializeComponent();

            if (collection != null)
                dgv_DBtable.ItemsSource = collection;
        }


    }
}
