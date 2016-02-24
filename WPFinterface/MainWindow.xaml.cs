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
using System.Timers;

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
        private exOPCserver opcServer;
        private ObservableCollection<mServerItem> opcListServers;
        private ObservableCollection<mTreeNode> opcTreeNodes;
        private ObservableCollection<mItem> opcListTagsInBranch;
        public ObservableCollection<mTag> opcMonitoredTags;


        private dbServerItem configuredServer;
        private mServerItem serverStatusUI;

        private bool oraConnectionMonitor;
        private Timer oraTransmitFreq;
        private OraExchanger oraEx;

        public ObservableCollection<mTag> subscribedTags;

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
            opcListServers = new ObservableCollection<mServerItem>();
            lst_Servers.ItemsSource = opcListServers;

            opcTreeNodes = new ObservableCollection<mTreeNode>();

            opcServer = new exOPCserver();
            opcServer.ReportMessage += opcServer_ReportMessage;

            subscribedTags = new ObservableCollection<mTag>();          
            dgrid_Values.ItemsSource = subscribedTags;

            host = new mItem("LOCALHOST", "localhost");
            txt_hostname.DataContext = host;

            serverStatusUI = new mServerItem(true);
            grid_ServerStatus.DataContext = serverStatusUI;

            configuredServer = new dbServerItem();


            configureOraTransmitRate();
        }

        /// <summary>
        /// Print log
        /// </summary>
        /// <param name="s">Log string</param>
        public void print2result(string s)
        {
            txt_result.AppendText("\r\n" + DateTime.Now.ToString("hh:mm:ss") + ": " + s);
            txt_result.ScrollToEnd();
        }


        /// <summary>
        /// Event handle for OPC server class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void opcServer_ReportMessage(object sender, exEventArgs args)
        {

            Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (args.error.Equals(999))
                //{
                //    print2result("ERROR CODE 999");
                //}

                print2result(args.message);

            }));
        }

        

        //---- Main options --------------------------------------------

        private void SearchServers(string hostName)
        {
            ObservableCollection<mServerItem> srvs = opcServer.GetServers(hostName);
            opcListServers.Clear();
            foreach (mServerItem si in srvs)
            {
                opcListServers.Add(si);
            }
        }

        private void Connect()
        {
            mServerItem selected_Server = (mServerItem)lst_Servers.SelectedItem;
            if (selected_Server != null)
            {
                opcServer.ConnectServer(selected_Server.UrlString);
                grid_ServerStatus.DataContext = opcServer.selectedServer;
                serverStatusUI = opcServer.selectedServer;

                lst_Servers.IsEnabled = false;

            }
        }

        private void Disconnect()
        {
            opcServer.DisconnectServer();

            if (opcTreeNodes != null)
                opcTreeNodes.Clear();
            if (opcListTagsInBranch != null)
                opcListTagsInBranch.Clear();
            if (subscribedTags != null)
                subscribedTags.Clear();

            grid_ServerStatus.DataContext = new mServerItem(true);
            lst_Servers.IsEnabled = true;

            SubscriptionClear();

            treeControl.IsEnabled = true;
            dgrid_Tags.IsEnabled = true;
            btn_ClearSubscription.IsEnabled = true;
        }
        private void BrowseServer()
        {
            if (opcServer.isConnected)
            {
                opcTreeNodes = opcServer.GetTree();
            }
            else
                opcTreeNodes.Clear();

            treeControl.ItemsSource = opcTreeNodes;
        }

        private void BrowseTags()
        {
            mTreeNode node = (mTreeNode)treeControl.SelectedItem;
            if (node != null)
            {
                opcListTagsInBranch = new ObservableCollection<mItem>(opcServer.GetTags(node.Path, node.Name));
                dgrid_Tags.ItemsSource = opcListTagsInBranch;
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
            opcMonitoredTags = new ObservableCollection<mTag>(opcServer.monitoredTags);
            dgrid_Values.ItemsSource = opcMonitoredTags;

            treeControl.IsEnabled = false;
            dgrid_Tags.IsEnabled = false;
            btn_ClearSubscription.IsEnabled = false;
          
        }



        private void UnSubscribe()
        {
            opcServer.UnSubcribe();
            dgrid_Values.ItemsSource = subscribedTags;

            foreach (mTag t in subscribedTags)
            {
                t.Value = "x";
                t.Quality = "unsubsribed";
            }

            treeControl.IsEnabled = true;
            dgrid_Tags.IsEnabled = true;
            btn_ClearSubscription.IsEnabled = true;
        }


        private void SubscriptionClear()
        {
            subscribedTags.Clear();
        }




        private void SaveConfiguration()
        {
            configuredServer = new dbServerItem();
            if (opcServer != null)
            {
                configuredServer.Name = opcServer.hostname;
                configuredServer.urlString = opcServer.selectedServer.UrlString;
            }

            if (subscribedTags != null)
            {
                foreach (mTag tag in subscribedTags)
                {
                    dbTagItem t = new dbTagItem(tag.Name, tag.Path);
                    configuredServer.monitoredTags.Add(t);
                }
            }
            
            dbConfig config = new dbConfig();
            FileWorks fw = new FileWorks();

            string path = fw.GetSaveFilePath();
            if (path != "")
            {
                config.Save(configuredServer, path);
            }       
        }


        private void LoadConfiguration()
        {
            FileWorks fw = new FileWorks();
            string path = fw.GetLoadFilePath();

            if (path != "")
            {
                configuredServer = new dbServerItem();
                dbConfig config = new dbConfig();
                configuredServer = config.Load(path);
                //config.Save(serverConfig, path);
            }



        }
        private void OracleSyncStartStop(bool startStop)
        {
            oraTransmitFreq.Enabled = startStop;
        }


        private void configureOraTransmitRate()
        {

            oraTransmitFreq = new System.Timers.Timer();
            
            oraTransmitFreq.Interval = 5000;
            oraTransmitFreq.AutoReset = false;

            oraTransmitFreq.Elapsed += oraTransmitFreq_Elapsed;


        }

        private void oraTransmitFreq_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool isDone = TransmitToOracle(opcMonitoredTags);
            OracleSyncStartStop(true);

            if (!isDone)
            {
                print2result("Oracle connection fail, try again...");
            }
        }


        private bool TransmitToOracle(ObservableCollection<mTag> tags)
        {
            if (tags.Count() > 1)
            {
                
                oraEx = new OraExchanger();
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

        void oraEx_ReportMessage(object sender, oraEventArgs args)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                print2result(args.message);
            }));
        }



        //--------- Buttons
        private void btn_GetServers_Click(object sender, RoutedEventArgs e)
        {          
            SearchServers(host.Name);
        }


        private void btn_Disconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect(); 
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


        private void lst_Servers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mServerItem selectedServer = (mServerItem)lst_Servers.SelectedItem;
            txt_path.DataContext = selectedServer;
            configuredServer.urlString = selectedServer.UrlString;
        }

        private void btn_ClearSubscription_Click(object sender, RoutedEventArgs e)
        {
            SubscriptionClear();
        }

        private void btn_oraStartSync_Click(object sender, RoutedEventArgs e)
        {
            OracleSyncStartStop(true);
        }

        private void btn_oraStopSync_Click(object sender, RoutedEventArgs e)
        {
            OracleSyncStartStop(false);
        }

        //---------------------------------------------------------------------



    }
}
