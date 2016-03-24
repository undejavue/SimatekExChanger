using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Data.Entity;

using ClassLibOracle;
using EFlocalDB;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.Specialized;
using System.Windows.Media;

namespace SimatekExCnahger
{
    /// <summary>
    /// Interaction logic for ucDBtblRemote.xaml
    /// </summary>
    public partial class ucDBtblLocal : UserControl
    {
        //private BindingList<dbLocalRecord> observableCollection;

        dbLocalContext context;

        public ucDBtblLocal(BindingList<dbLocalRecord> collection)
        {
            InitializeComponent();

            if (collection != null)
                dgv_DBtable.ItemsSource = collection;

            dgv_DBtable.DataContextChanged += Dgv_DBtable_DataContextChanged;

            collection.ListChanged += Collection_ListChanged;

            

        }

        private void Collection_ListChanged(object sender, ListChangedEventArgs e)
        {
            //dgv_DBtable.ScrollIntoView(CollectionView.NewItemPlaceholder);

            if (dgv_DBtable.Items.Count > 0)
            {
                var border = VisualTreeHelper.GetChild(dgv_DBtable, 0) as Decorator;
                if (border != null)
                {
                    var scroll = border.Child as ScrollViewer;
                    if (scroll != null) scroll.ScrollToEnd();
                }
            }
        }

        private void Dgv_DBtable_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            //dgv_DBtable.ScrollIntoView(CollectionView.NewItemPlaceholder);
        }

        public ucDBtblLocal(ObservableCollection<FIX_STAN789_T> collection)
        {
            InitializeComponent();

            if (collection != null)
                dgv_DBtable.ItemsSource = collection;
        }

        public ucDBtblLocal(ObservableCollection<object> collection )
        {
            InitializeComponent();

            if (collection != null)
                dgv_DBtable.ItemsSource = collection;
        }

        public ucDBtblLocal(string dbFileName)
        {
            InitializeComponent();

            context = new dbLocalContext(dbFileName, false);
            context.dbRecords.Load();

            dgv_DBtable.ItemsSource = context.dbRecords.Local.ToBindingList();
            //ObservableCollection<dbLocalRecord>  records = new ObservableCollection<dbLocalRecord>(context.dbRecords.Local);
            //dgv_DBtable.ItemsSource = records;
        }

        public ucDBtblLocal(dbLocalContext cont)
        {
            InitializeComponent();


            cont.dbRecords.Load();

            dgv_DBtable.ItemsSource = cont.dbRecords.Local.ToBindingList();
            //ObservableCollection<dbLocalRecord>  records = new ObservableCollection<dbLocalRecord>(context.dbRecords.Local);
            //dgv_DBtable.ItemsSource = records;
        }

    }
}
