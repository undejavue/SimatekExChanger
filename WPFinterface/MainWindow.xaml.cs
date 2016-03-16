using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Timers;
using ClassLibOPC;
using EFlocalDB;
using ClassLibOracle;
using ClassLibGlobal;
using System.Collections.Generic;
using System.ComponentModel;

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

        private Timer progressTimer;

        private BackgroundWorker bgWorker;

        private dbLocalManager dbManager;




        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Model = new ViewModel();
            this.DataContext = Model;

            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            

            opcServer = new exOPCserver();
            
            Model.opcError = new vmError(opcServer.error);
            //Model.gError = opcServer.error;

            opcServer.ReportMessage += opcServer_ReportMessage;
            opcServer.MarkedTagsChanged += OpcServer_MarkedTagsChanged;


            configureOraTransmitRate();

            SearchServers("localhost");
            bgWorker.RunWorkerAsync();
          
        }

        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Model.progressBar = e.ProgressPercentage;
            
            progress.Value = e.ProgressPercentage;
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            oraEx.ReportMessage -= oraEx_ReportMessage;
            oraEx.ReportMessage += oraEx_ReportMessage;
            OraTableInit();
            Model.addLogRecord("Finish test oracle connection");
            Model.isDbServerConnected = oraEx.isConnectionOK;
            Model.lbl_InitConnection_isVisible = false;
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            

            OraGetFirstConnection();
        }

        private void OraGetFirstConnection()
        {
            Model.lbl_InitConnection_isVisible = true;

            oraEx = new OraExchanger();
            bgWorker.ReportProgress(100);
        }


        private void OpcServer_MarkedTagsChanged(object sender)
        {
            OraInsert();
        }



        #region Local Database operations

        private void LocalDBCreate()
        {

            FileWorks fw = new FileWorks();

            string path = fw.GetSaveFilePath();
            if (path != "")
            {
                dbManager = new dbLocalManager(path);
            }
        }

        private void LocalDBInsert()
        {

            if (dbManager != null)
            {
                dbManager.insert(Model.opcMonitoredTags);
            }
        }


        private void LocalDBGetRecords()
        {

            if (dbManager != null)
            {
                ucDBtblRemote ucLocalTable = new ucDBtblRemote(dbManager.getRecords());
                wndDBbrowser dbWindow = new wndDBbrowser();

                dbWindow.Content = ucLocalTable;
                dbWindow.Show();
            }
        }

        #endregion




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

                if (args.error != 0)
                {
                    //Model.opcError.code = args.error;
                    // Model.opcError.message = args.message;
                }

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
                if (opcServer.ConnectServer(selected_Server.UrlString))
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
            if (!contain)
            {
                mTag newTag = (mTag)dgrid_Subscribed.SelectedItem as mTag;

                var mt = Model.opcSubscribedTags.FirstOrDefault(t => t.Description == newTag.Description);
                mt.Name = tag.Name;
                mt.Path = tag.Path;
            }
        }


        private void Subscribe()
        {
            opcServer.SubscribeTags(Model.opcSubscribedTags.Where(t => t.Name != null).ToList());
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
            if (Model.selectedOPCserver != null)
            {
                Model.SaveServer();
            }

            dbConfManager config = new dbConfManager();
            FileWorks fw = new FileWorks();

            string path = fw.GetSaveFilePath();
            if (path != "")
            {
                config.Save(Model.configuredServer, path);
            }
        }


        private void LoadConfiguration()
        {
            FileWorks fw = new FileWorks();
            string path = fw.GetLoadFilePath();

            if (path != "")
            {
                dbConfManager config = new dbConfManager();
                if (Model.LoadServer(config.Load(path)))
                {
                    Model.changeState(ModelState.configLoaded);
                }
            }
        }

        private void ConnectFromConfig()
        {
            if (opcServer.ConnectServer(Model.selectedOPCserver.UrlString))
            {
                Model.changeState(ModelState.opcConneted);
                Model.selectedOPCserver = opcServer.selectedServer;
                Subscribe();
            }
        }

        #endregion


        #region Database operations


        private void OraTableInit()
        {
            Model.opcSubscribedTags.Clear();

            if (oraEx != null)

            foreach (string s in oraEx.GetFields())
            {
                mTag tag = new mTag();
                tag.Description = s;
                Model.opcSubscribedTags.Add(tag);
            }
        }

        private void OraShowTable()
        {

            ucDBtblRemote ucOraTable = new ucDBtblRemote(oraEx.bindContext());
            wndDBbrowser dbWindow = new wndDBbrowser();

            dbWindow.Content = ucOraTable;
            dbWindow.Show();
            
            
        }


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
            //if (Model.opcMonitoredTags != null)
            //{
            //    if (OraInsert(Model.opcMonitoredTags))
            //    {

            //    }
            //    else
            //    {
            //        Model.addLogRecord("Transmit to database fail, try again...");
            //    }

            //    OracleSyncStartStop(true);
            //}
            //else
            //{
            //    Model.addLogRecord("Nothing to transmit");
            //}
        }





        private void OraInsert()
        {
            //oraEx = oraEx ?? new OraExchanger();

            List<oraEntity> list = new List<oraEntity>();

            foreach (mTag t in Model.opcMonitoredTags)
            {
                oraEntity ora = new oraEntity(t.Description, t.Value);
                list.Add(ora);
            }

            oraEx.items = new ObservableCollection<oraEntity>(list);
            oraEx.bindData();

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
            if (lst_Servers.SelectedItem != null)
            {
                mServerItem selectedServer = (mServerItem)lst_Servers.SelectedItem;
                Model.selectedOPCserver.UrlString = selectedServer.UrlString;
            }
            
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
            //OraInsert(Model.opcMonitoredTags);
            OraInsert();
        }

        private void btn_ConfigConnect_Click(object sender, RoutedEventArgs e)
        {
            ConnectFromConfig();
            
        }

        private void btn_OraTableInit_Click(object sender, RoutedEventArgs e)
        {
            OraTableInit();
        }

        private void btn_OraShowTable_Click(object sender, RoutedEventArgs e)
        {
            OraShowTable();
        }

        private void dgrid_Subscribed_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            mTag tag = (mTag)dgrid_Subscribed.SelectedItem;

            if (tag != null)
            {
                tag.onChange = !tag.onChange;
            }
        }

        private void btn_ShowLocalTable_Click(object sender, RoutedEventArgs e)
        {
            LocalDBCreate();
        }

        private void btn_LocalTableInsert_Click(object sender, RoutedEventArgs e)
        {
            LocalDBInsert();
        }

        private void btn_LocalTableView_Click(object sender, RoutedEventArgs e)
        {
            LocalDBGetRecords();
        }
    }
}
