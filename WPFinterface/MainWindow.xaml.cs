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
using System.Collections;
using Microsoft.Win32;

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

        private BackgroundWorker bgWorker;
        private BackgroundWorker bgwOraTestConnection;
        private BackgroundWorker bgwOraSync;

        private BackgroundWorker bgwOPC;

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
            bgWorker.WorkerSupportsCancellation = true;

            bgwOraTestConnection = new BackgroundWorker();
            bgwOraTestConnection.DoWork += BgwOraTestConnection_DoWork;
            bgwOraTestConnection.RunWorkerCompleted += BgwOraTestConnection_RunWorkerCompleted;
            bgwOraTestConnection.WorkerSupportsCancellation = true;

            bgwOraSync = new BackgroundWorker();
            bgwOraSync.DoWork += BgwOraSync_DoWork;
            bgwOraSync.RunWorkerCompleted += BgwOraSync_RunWorkerCompleted;
            bgwOraSync.ProgressChanged += BgwOraSync_ProgressChanged;
            bgwOraSync.WorkerSupportsCancellation = true;
            bgwOraSync.WorkerReportsProgress = true;

            bgwOPC = new BackgroundWorker();
            bgwOPC.DoWork += BgwOPC_DoWork;
            bgwOPC.RunWorkerCompleted += BgwOPC_RunWorkerCompleted;

            opcServer = new exOPCserver();
            
            Model.opcError = new vmError(opcServer.error);
            //Model.gError = opcServer.error;

            opcServer.ReportMessage += opcServer_ReportMessage;
            opcServer.MarkedTagsChanged += OpcServer_MarkedTagsChanged;


            configureOraTransmitRate();

            SearchServers("localhost");
            bgWorker.RunWorkerAsync();
          
        }

        private void BgwOPC_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                bool isOk = (bool)e.Result;
                if (isOk)
                {
                    Model.changeState(ModelState.opcConneted);
                    Model.selectedOPCserver = opcServer.selectedServer;
                }
                else
                {
                    Model.addLogRecord("OPC connection timed out");
                }
            }

            Model.isOPCwaiting = false;
        }

        private void BgwOPC_DoWork(object sender, DoWorkEventArgs e)
        {
            string url = (string)e.Argument;
            e.Result = opcServer.ConnectServer(url);

        }

        private void BgwOraSync_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int p = e.ProgressPercentage;
        }

        private void BgwOraSync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Model.addLogRecord("Synchronization cancelled");
                return;
            }

            if (e.Result != null)
            {
                List<int> ids = (List<int>)e.Result; 
                Model.addLogRecord(ids.Count().ToString() + " local records pushed to remote db");


                Dispatcher.BeginInvoke(new Action(() =>
                {
                    dbManager.updateFlags(ids.ToList());

                }));
                

                Model.isSyncInProgress = false;
            }
        }

        private void BgwOraSync_DoWork(object sender, DoWorkEventArgs e)
        {
            
            if ( (dbManager != null) & (oraEx != null) & (oraEx.isConnectionOK) )
            {
                Model.isSyncInProgress = true;
                //Get local records
                List<dbLocalRecord> records = dbManager.getNotSyncRecords();
                bgwOraSync.ReportProgress(30);

                //Cancellation check
                if (bgWorker.CancellationPending) { e.Cancel = true; return;}

                //Save id's of not sync records
                List<int> ids = records.Select(r => r.pid).ToList();
                bgwOraSync.ReportProgress(40);

                List<FIX_STAN789_T> oraRecords = new List<FIX_STAN789_T>();
                float p = 40;
                float max = records.Count();
                float step = (70f - p) / max;
                RecordLocalToRemoteConverter converter = new RecordLocalToRemoteConverter();  
                foreach (dbLocalRecord r in records)
                {
                    oraRecords.Add(converter.return_oraRecord(r));
                    p = p + step;
                    bgwOraSync.ReportProgress((int)p);
                }

                //Cancellation check
                if (bgWorker.CancellationPending) { e.Cancel = true; return; }

                if (oraEx.insert(oraRecords))
                {
                    bgwOraSync.ReportProgress(100);
                    e.Result = ids;
                }
            }
        }

        private void BgwOraTestConnection_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool isOk = (bool)e.Result;
            Model.isRemoteDBConnected = isOk;
            Model.addLogRecord("Ora connectin test finished...result = " + isOk.ToString());

            //if ( (!bgwOraSync.IsBusy) & (isOk) )
            //bgwOraSync.RunWorkerAsync();
        }

        private void BgwOraTestConnection_DoWork(object sender, DoWorkEventArgs e)
        {

            e.Result = oraEx.TestConnection();
        }

        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Model.progressBarOraTestConn = e.ProgressPercentage;
            
            progress.Value = e.ProgressPercentage;
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            oraEx.ReportMessage -= oraEx_ReportMessage;
            oraEx.ReportMessage += oraEx_ReportMessage;
            OraTableInit();
            Model.addLogRecord("Finish test oracle connection");
            Model.isRemoteDBConnected = oraEx.isConnectionOK;
            Model.lbl_InitConnection_isVisible = false;
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (bgWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            Model.lbl_InitConnection_isVisible = true;

            oraEx = new OraExchanger();
            bgWorker.ReportProgress(100);

        }





        #region Local Database operations

        private void LocalDBCreate()
        {

            FileWorks fw = new FileWorks();

            string path = fw.GetSaveFilePath();
            if (path != "")
            {
                dbManager = new dbLocalManager(path);
                dbManager.ReportMessage -= DbManager_ReportMessage;
                dbManager.ReportMessage += DbManager_ReportMessage;
            }

            Model.isLocalDBConnected = true;
        }



        private void LocalDBInsert(bool flag)
        {

            if ((dbManager != null) & (Model.opcMonitoredTags.Count > 0)) 
            {
                dbManager.insert(Model.opcMonitoredTags, flag);
            }
        }


        private void LocalDBGetRecords()
        {

            if (dbManager != null)
            {
                ucDBtblLocal ucLocalTable = new ucDBtblLocal(dbManager.getAllRecords());
                wndDBbrowser dbWindow = new wndDBbrowser();

                dbWindow.Content = ucLocalTable;
                dbWindow.Title = "Local database ";
                dbWindow.Show();
            }
        }

        
        private void DataInsertProcedure()
        {
            bool flag = false;

            if (Model.isRemoteDBConnected)
            {
                flag = OraInsert();
                Model.isRemoteDBConnected = flag;

                if ((Model.isSyncInProgress) & (!bgwOraSync.IsBusy))
                    bgwOraSync.RunWorkerAsync();
            }
            else
            {
                if (!bgwOraTestConnection.IsBusy)
                bgwOraTestConnection.RunWorkerAsync();
            }
             
            LocalDBInsert(flag);
        }

        #endregion

        #region Message Handlers


        private void OpcServer_MarkedTagsChanged(object sender)
        {
           
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DataInsertProcedure();

            }));
        }

        private void DbManager_ReportMessage(object sender, gEventArgs args)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Model.addLogRecord(args.message);

            }));

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
                Model.addLogRecord("Trying to connect to selected server");
                Model.isOPCwaiting = true;


               if (opcServer.ConnectServer(selected_Server.UrlString))
               {
                        Model.changeState(ModelState.opcConneted);
                        
               }
               Model.selectedOPCserver = opcServer.selectedServer;
               Model.isOPCwaiting = false;

            }



            //mServerItem selected_Server = (mServerItem)lst_Servers.SelectedItem;
            //if (selected_Server != null)
            //{
            //    Model.addLogRecord("Trying to connect to selected server");
            //    Model.isOPCwaiting = true;

            //    bgwOPC.RunWorkerAsync(selected_Server.UrlString);
            //}
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

                var mt = Model.opcSubscribedTags.FirstOrDefault(t => t.NameInDb == newTag.NameInDb);
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
                tag.NameInDb = s;
                Model.opcSubscribedTags.Add(tag);
            }
        }

        private void OraShowTable()
        {

            ucDBtblRemote ucOraTable = new ucDBtblRemote(oraEx.GetRecords());
            wndDBbrowser dbWindow = new wndDBbrowser();

            dbWindow.Content = ucOraTable;
            dbWindow.Title = "Remote database";
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





        private bool OraInsert()
        {
            if (oraEx != null)
                if (oraEx.isConnectionOK)
                {
                    return oraEx.insert(Model.opcMonitoredTags.ToList());
                }

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
            Model.addLogRecord("Ora connectin test started...");
            bgwOraTestConnection.RunWorkerAsync();
        }

        private void btn_ConfigConnect_Click(object sender, RoutedEventArgs e)
        {
            ConnectFromConfig();
            
        }

        private void btn_OraTableInit_Click(object sender, RoutedEventArgs e)
        {
            //OraTableInit();
            string sOut = string.Join("\r\n", GetVersionFromRegistry().ToArray());
            MessageBox.Show(sOut,".NET Framework version");

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
            DataInsertProcedure();
        }

        private void btn_LocalTableView_Click(object sender, RoutedEventArgs e)
        {
            LocalDBGetRecords();
        }

        private void btn_WaitingCancel_Click(object sender, RoutedEventArgs e)
        {
            bgWorker.CancelAsync();
        }

        // Checking the version using >= will enable forward compatibility, 
        // however you should always compile your code on newer versions of
        // the framework to ensure your app works the same.
        private static string CheckFor45DotVersion(int releaseKey)
        {
            if (releaseKey >= 393295)
            {
                return "4.6 or later";
            }
            if ((releaseKey >= 379893))
            {
                return "4.5.2 or later";
            }
            if ((releaseKey >= 378675))
            {
                return "4.5.1 or later";
            }
            if ((releaseKey >= 378389))
            {
                return "4.5 or later";
            }
            // This line should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }

        private static List<string> GetVersionFromRegistry()
        {
            List<string> outlist = new List<string>();

            // Opens the registry key for the .NET Framework entry.
            using (RegistryKey ndpKey =
                RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").
                OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                // As an alternative, if you know the computers you will query are running .NET Framework 4.5 
                // or later, you can use:
                // using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, 
                // RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    if (versionKeyName.StartsWith("v"))
                    {

                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        string name = (string)versionKey.GetValue("Version", "");
                        string sp = versionKey.GetValue("SP", "").ToString();
                        string install = versionKey.GetValue("Install", "").ToString();
                        if (install == "") //no install info, must be later.
                            outlist.Add(versionKeyName + "  " + name);
                        else
                        {
                            if (sp != "" && install == "1")
                            {
                                outlist.Add(versionKeyName + "  " + name + "  SP" + sp);
                            }

                        }
                        if (name != "")
                        {
                            continue;
                        }
                        foreach (string subKeyName in versionKey.GetSubKeyNames())
                        {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string)subKey.GetValue("Version", "");
                            if (name != "")
                                sp = subKey.GetValue("SP", "").ToString();
                            install = subKey.GetValue("Install", "").ToString();
                            if (install == "") //no install info, must be later.
                                outlist.Add(versionKeyName + "  " + name);
                            else
                            {
                                if (sp != "" && install == "1")
                                {
                                    outlist.Add("  " + subKeyName + "  " + name + "  SP" + sp);
                                }
                                else if (install == "1")
                                {
                                    outlist.Add("  " + subKeyName + "  " + name);
                                }
                            }
                        }
                    }
                }
            }

            return outlist;
        }
    }
}
