using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ClassLibOPC;
using EFconfigDB;
using ClassLibOracle;


namespace WPFinterface
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<mServerItem> list_servers;
        private ObservableCollection<mTreeNode> treeNodes;
        private ObservableCollection<mItem> list_tags;
        private exOPCserver opcServer;
        private dbServerItem serverConfig;

        private List<string> log;

        private bool oraConnectionMonitor;


        public ObservableCollection<mTag> subscribedTags;

        public ObservableCollection<mTag> monitoredTags;

        public mItem host;


        /// <summary>
        /// Main window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }



        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            list_servers = new ObservableCollection<mServerItem>();
            lst_Servers.ItemsSource = list_servers;

            treeNodes = new ObservableCollection<mTreeNode>();

            opcServer = new exOPCserver();
            opcServer.ReportMessage += opcServer_ReportMessage;

            subscribedTags = new ObservableCollection<mTag>();
            
            dgrid_Values.ItemsSource = subscribedTags;


            host = new mItem("LOCALHOST", "localhost");
            txt_hostname.DataContext = host;
        }

        /// <summary>
        /// Print log
        /// </summary>
        /// <param name="s">Log string</param>
        public void print2result(string s)
        {
            txt_result.AppendText("\r\n" + DateTime.Now.ToString("h:mm:ss") + ": " + s);
            txt_result.ScrollToEnd();
        }


        /// <summary>
        /// Event handle for OPC server class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void opcServer_ReportMessage(object sender, exEventArgs args)
        {
            print2result(args.message);
        }

        //---- Main options --------------------------------------------

        private void SearchServers(string hostName)
        {
            ObservableCollection<mServerItem> srvs = opcServer.GetServers(hostName);

            foreach (mServerItem si in srvs)
            {
                list_servers.Add(si);
            }
        }

        private void Connect()
        {
            mServerItem selected_Server = (mServerItem)lst_Servers.SelectedItem;
            if (selected_Server != null)
            {
                opcServer.ConnectServer(selected_Server.UrlString);
                grid_ServerStatus.DataContext = opcServer.SelectedServer;
            }
        }

        private void Disconnect()
        {
            opcServer.DisconnectServer();

            if (treeNodes != null)
                treeNodes.Clear();
            if (list_tags != null)
                list_tags.Clear();
            if (subscribedTags != null)
                subscribedTags.Clear();
        }
        private void BrowseServer()
        {
            if (opcServer.IsConnected)
            {
                treeNodes = opcServer.GetTree();
            }
            else
                treeNodes.Clear();

            treeControl.ItemsSource = treeNodes;
        }

        private void BrowseTags()
        {
            mTreeNode node = (mTreeNode)treeControl.SelectedItem;
            if (node != null)
            {
                list_tags = new ObservableCollection<mItem>(opcServer.GetTags(node.Path, node.Name));
                dgrid_Tags.ItemsSource = list_tags;
            }
        }


        private void SelectTagsForMonitor()
        {
            mItem selectedTag = (mItem)dgrid_Tags.SelectedItem;
            mTag tag = new mTag(selectedTag.Name, selectedTag.Path);

            subscribedTags.Add(tag);
        }

        private void Subscribe()
        {
            opcServer.SubscribeTags(subscribedTags.ToList());

            monitoredTags = new ObservableCollection<mTag>(opcServer.MonitoredTags);

            dgrid_Values.ItemsSource = monitoredTags;

            monitoredTags[0].PropertyChanged += MainWindow_PropertyChanged;
            
        }

        void MainWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (oraConnectionMonitor)
            {
                oraConnectionMonitor = TransmitToOracle(monitoredTags);

            }
        }



        private void UnSubscribe()
        {
            opcServer.UnSubcribe();
            dgrid_Values.ItemsSource = subscribedTags;
        }




        private void SaveConfiguration()
        {
            serverConfig = new dbServerItem();

            if (opcServer != null)
            {
                serverConfig.Name = opcServer.hostname;
                serverConfig.urlString = opcServer.SelectedServer.UrlString;

            }

            if (subscribedTags != null)
            {
                foreach (mTag tag in subscribedTags)
                {
                    dbTagItem t = new dbTagItem(tag.Name, tag.Path);
                    serverConfig.monitoredTags.Add(t);
                }
            }
            
            dbConfig config = new dbConfig();

            FileWorks fw = new FileWorks();

            string path = fw.GetSaveFilePath();

            if (path != "")
            {

                config.Save(serverConfig, path);
            }
         
        }


        private void LoadConfiguration()
        {


            FileWorks fw = new FileWorks();

            string path = fw.GetLoadFilePath();

            if (path != "")
            {
                serverConfig = new dbServerItem();

                dbConfig config = new dbConfig();

                serverConfig = config.Load(path);
                config.Save(serverConfig, path);
            }

        }


        private bool TransmitToOracle(ObservableCollection<mTag> tags)
        {
            if (tags.Count() > 1)
            {
                OraExchanger oraEx = new OraExchanger();
                oraEx.ReportMessage += oraEx_ReportMessage;

                if (oraEx.isConnectionOK)
                {

                    FIX_STAN789_T ent = new FIX_STAN789_T();

                    try
                    {
                        ent.N_STAN = Decimal.Parse(tags[0].Value);
                        ent.COUNTER = Int32.Parse(tags[1].Value);
                    }
                    catch
                    {
                        ent.N_STAN = 1;
                        ent.COUNTER = 999;
                    }

                    ent.INCOMIN_DATE = DateTime.Now;
                    ent.WHEN = DateTime.Now;

                    return oraEx.insertData(ent);
                }
                else return false;
            }
            else
                return false;

        }

        private void oraEx_ReportMessage(object sender, oraEventArgs args)
        {
            LogMessages(args.message);
        }


        private void LogMessages(string m)
        {
            if (log == null) log = new List<string>();

            log.Add(m);

        }

        private void PrintFromLog()
        {
            if (log != null)
            {
                foreach (string s in log)
                {
                    print2result(s);
                }

                log.Clear();
            }
        }


        //--------- Buttons
        private void btn_GetServers_Click(object sender, RoutedEventArgs e)
        {          
            SearchServers(host.Name);
        }


        private void btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void btn_Disconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect(); 
        }



        private void btn_GetTree_Click(object sender, RoutedEventArgs e)
        {
            BrowseServer();     
        }


        private void btn_GetTags_Click(object sender, RoutedEventArgs e)
        {
            BrowseTags();
        }

        private void btn_SelectTag_Click(object sender, RoutedEventArgs e)
        {
            SelectTagsForMonitor();
        }

        private void btn_Subscribe_Click(object sender, RoutedEventArgs e)
        {
            Subscribe();
        }


        private void btn_UnSubscribe_Click(object sender, RoutedEventArgs e)
        {
            UnSubscribe();
        }

        private void treeControl_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BrowseTags();
        }

        private void lst_Servers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Connect();
            BrowseServer();
        }

        private void dgrid_Tags_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectTagsForMonitor();
        }

        private void btn_SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            SaveConfiguration();
        }

        private void btn_LoadConfig_Click(object sender, RoutedEventArgs e)
        {
            LoadConfiguration();
        }

        private void btn_insertToDatabase_Click(object sender, RoutedEventArgs e)
        {
            TransmitToOracle(monitoredTags);
        }

        //---------------------------------------------------------------------



    }
}
