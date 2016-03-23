using System.Collections.ObjectModel;
using System.Windows.Controls;
using ClassLibOracle;
using EFlocalDB;
using System.ComponentModel;
using System.Windows.Media;

namespace SimatekExCnahger
{
    /// <summary>
    /// Interaction logic for ucDBtblRemote.xaml
    /// </summary>
    public partial class ucDBtblRemote : UserControl
    {

        private Decorator border;
        private ScrollViewer scroll;


        public ucDBtblRemote(BindingList<FIX_STAN789_T> collection)
        {
            InitializeComponent();

            if (collection != null)
                dgv_DBtable.ItemsSource = collection;

            collection.ListChanged += Collection_ListChanged;

  
        }

        private void Collection_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (dgv_DBtable.Items.Count > 0)
            {
                border = VisualTreeHelper.GetChild(dgv_DBtable, 0) as Decorator;
                if (border != null)
                {
                    scroll = border.Child as ScrollViewer;
                    if (scroll != null) scroll.ScrollToEnd();
                }
            }
        }
    }
}
