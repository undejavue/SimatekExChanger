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
using System.Globalization;


namespace WPFinterface
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel Model;       
        private exOPCserver opcServer;
        private OraExchanger oraEx;
        private Timer oraTransmitFreq;


        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Model = new ViewModel();
            this.DataContext = Model;

            opcServer = new exOPCserver();
            opcServer.ReportMessage += opcServer_ReportMessage;

            configureOraTransmitRate();
        }



        #region Message Handlers

        /// <summary>
        /// Event handle for OPC server class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void opcServer_ReportMessage(object sender, exEventArgs args)
        {
            
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Model.addLogRecord(args.message);
            }));
        }

        private void oraEx_ReportMessage(object sender, oraEventArgs args)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Model.addLogRecord(args.message);
            }));
        }

        #endregion


        #region OPC Server operations

        private void SearchServers(string hostName)
        {
            Model.opcListServers.Clear();
            foreach (mServerItem i in opcServer.GetServers(hostName)) { Model.opcListServers.Add(i); }
            Model.addLogRecord("Server search complited...");         
        }

        private void Connect()
        {
            mServerItem selected_Server = (mServerItem)lst_Servers.SelectedItem;
            if (selected_Server != null)
            {
                if ( opcServer.ConnectServer(selected_Server.UrlString) )
                {
                    Model.changeState(ModelState.opcConneted);
                }
                Model.selectedOPCserver = opcServer.selectedServer;
            }
        }

        private void Disconnect()
        {
            opcServer.DisconnectServer();
            Model.changeState(ModelState.opcDisconneted);
            SubscriptionClear();

        }

        private void BrowseServer()
        {
            if (opcServer.isConnected)
            {
                Model.opcTreeNodes.Clear();
                foreach (mTreeNode node in opcServer.GetTree()) { Model.opcTreeNodes.Add(node); }
            }
            else
                Model.opcTreeNodes.Clear();
        }


        private void BrowseTags()
        {
            mTreeNode node = (mTreeNode)treeControl.SelectedItem;
            if (node != null)
            {
                Model.opcListTagsInBranch.Clear();
                foreach (mItem tag in opcServer.GetTags(node.Path, node.Name))
                {
                    Model.opcListTagsInBranch.Add(tag);
                }
            }
        }


        private void SelectTagsForMonitor()
        {
            mItem selectedTag = (mItem)dgrid_Tags.SelectedItem;
            mTag tag = new mTag(selectedTag.Name, selectedTag.Path);

            var contain = Model.opcSubscribedTags.Any(t => t.Name == tag.Name);

            if (!contain) Model.opcSubscribedTags.Add(tag);

            //сделать подсветку в таблице тех, которые выбраны
        }


        private void Subscribe()
        {
            opcServer.SubscribeTags(Model.opcSubscribedTags.ToList());
            Model.opcMonitoredTags.Clear();

            Model.changeState(ModelState.opcSubscribed);

            foreach (mTag tag in opcServer.monitoredTags)
            { Model.opcMonitoredTags.Add(tag); }
      
        }



        private void UnSubscribe()
        {
            opcServer.UnSubcribe();
            Model.changeState(ModelState.opcUnsubscribed);

            //foreach (mTag t in Model.opcSubscribedTags)
            //{
            //    t.Value = "x";
            //    t.Quality = "unsubsribed";
            //}

        }


        private void SubscriptionClear()
        {
            Model.opcSubscribedTags.Clear();
        }


        #endregion


        #region Configuration Save-Load operations
        private void SaveConfiguration()
        {
            //configuredServer = new dbServerItem();

            //if (opcServer != null)
            //{
            //    configuredServer.Name = opcServer.hostname;
            //    configuredServer.urlString = opcServer.selectedServer.UrlString;
            //}

            //if (subscribedTags != null)
            //{
            //    foreach (mTag tag in subscribedTags)
            //    {
            //        dbTagItem t = new dbTagItem(tag.Name, tag.Path);
            //        configuredServer.monitoredTags.Add(t);
            //    }
            //}
            
            //dbConfig config = new dbConfig();
            //FileWorks fw = new FileWorks();

            //string path = fw.GetSaveFilePath();
            //if (path != "")
            //{
            //    config.Save(configuredServer, path);
            //}       
        }


        private void LoadConfiguration()
        {
            //FileWorks fw = new FileWorks();
            //string path = fw.GetLoadFilePath();

            //if (path != "")
            //{
            //    configuredServer = new dbServerItem();
            //    dbConfig config = new dbConfig();
            //    configuredServer = config.Load(path);
            //    //config.Save(serverConfig, path);
            //}

        }

        #endregion


        #region Database operations

        private void OracleSyncStartStop(bool startStop)
        {
            oraTransmitFreq.Enabled = startStop;
        }


        private void configureOraTransmitRate()
        {

            oraTransmitFreq = new System.Timers.Timer();
            
            oraTransmitFreq.Interval = 8000;
            oraTransmitFreq.AutoReset = false;

            oraTransmitFreq.Elapsed += oraTransmitFreq_Elapsed;

        }

        private void oraTransmitFreq_Elapsed(object sender, ElapsedEventArgs e)
        {


            if (Model.opcMonitoredTags != null)
            {
                if (TransmitToOracle(Model.opcMonitoredTags))
                {
                    
                }
                else
                {
                    Model.addLogRecord("Transmit to database fail, try again...");
                }

                OracleSyncStartStop(true);
            }
            else
            {
                Model.addLogRecord("Nothing to transmit");
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
        #endregion


        #region Interface

        //--------- Buttons
        private void btn_GetServers_Click(object sender, RoutedEventArgs e)
        {          
            SearchServers("LOCALHOST");
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
            Model.selectedOPCserver.UrlString = selectedServer.UrlString;
            
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

        #endregion

        private void btn_oraTestConnection_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
