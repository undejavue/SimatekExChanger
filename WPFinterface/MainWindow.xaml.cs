﻿using System;
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
using System.Windows.Threading;

namespace SimatekExCnahger
{
    public enum workerMode
    {
        ConnectingOPC,
        Connected,
        Disconnected,
        Worked
    }


    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string TAG = LogFilter.GUI.ToString();

        private ViewModel Model;
        private exOPCserver opcServer;
        private OraExchanger oraEx;
        private Timer oraTransmitFreq;

        private BackgroundWorker bgwStarter;
        private BackgroundWorker bgwOraTestConnection;
        private BackgroundWorker bgwOraSync;
        private BackgroundWorker bgwOPC;

        private dbLocalManager dbManager;

        private SimatekExChanger.wndLoading wndPrgLoading;


        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Model = new ViewModel();
            this.DataContext = Model;



            bgwStarter = new BackgroundWorker();
            bgwStarter.DoWork += BgWorker_DoWork;
            bgwStarter.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
            bgwStarter.ProgressChanged += BgWorker_ProgressChanged;
            bgwStarter.WorkerReportsProgress = true;
            bgwStarter.WorkerSupportsCancellation = true;

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
            opcServer.ReportMessage += opcServer_ReportMessage;
            opcServer.MarkedTagsChanged += OpcServer_MarkedTagsChanged;


            //configureOraTransmitRate();

            SearchServers(Model.selectedOPCserver.Host);
            bgwStarter.RunWorkerAsync();



            Model.isAutoRestart = SimatekExChanger.Properties.Settings.Default.isAutoRestart;
            Model.isAutoSynhronisation = SimatekExChanger.Properties.Settings.Default.isAutoSynhronisation;

            if (Model.isAutoRestart)
            {
                wndPrgLoading = new SimatekExChanger.wndLoading();
                wndPrgLoading.Show();
                AutoRestart();
                wndPrgLoading.Close();
            }

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
                string TAG = dbLocalManager.TAG;
                Model.addLogRecord(TAG, args.message);
            }));

        }

        private void opcServer_ReportMessage(object sender, exEventArgs args)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                string TAG = exOPCserver.TAG;
                Model.addLogRecord(TAG, args.message);

                if (args.error != 0)
                {
                }
            }));
        }

        private void oraEx_ReportMessage(object sender, oraEventArgs args)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                string TAG = OraExchanger.TAG;
                Model.addLogRecord(TAG,args.message);
            }));
        }

        #endregion



        #region Background works

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
                    Model.addLogRecord(exOPCserver.TAG, "OPC connection timed out");
                }
            }  
        }

        private void BgwOPC_DoWork(object sender, DoWorkEventArgs e)
        {

            Model.isOPCwaiting = true;
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
                Model.addLogRecord(OraExchanger.TAG,"Synchronization cancelled");
                return;
            }

            if (e.Result != null)
            {
                List<int> ids = (List<int>)e.Result;
                if (ids.Count > 0)
                {
                    Model.addLogRecord(OraExchanger.TAG, ids.Count().ToString() + " local records pushed to remote db");

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        dbManager.updateFlags(ids.ToList());
                    }));
                }
                else
                {
                    Model.addLogRecord(TAG, "Nothing to sync");
                    if (Model.cmdToSyncNow) MessageBox.Show("Nothing to syncronize", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                 
                Model.isSyncInProgress = false;
                Model.isSyncPending = false; // 
                Model.cmdToSyncNow = false;
            }
        }

        private void SyncNow()
        {
            Model.cmdToSyncNow = true;
            if ( (!bgwOraSync.IsBusy) )
                bgwOraSync.RunWorkerAsync();
        }

        private void BgwOraSync_DoWork(object sender, DoWorkEventArgs e)
        {
            Model.isSyncPending = true;

            if ( (dbManager != null) && (!Model.isSyncDenied) && (oraEx != null) && (oraEx.isDbConnectionOK) && ((oraEx.isInsertOk) || (Model.cmdToSyncNow)) )
            {

                //Get local records
                List<dbLocalRecord> records = dbManager.getNotSyncRecords();

                if (records.Count > 0)
                {
                    Model.isSyncInProgress = true;
                }
                else
                {
                    Model.isSyncPending = false;
                    Model.cmdToSyncNow = false;
                    e.Result = new List<int>();
                    return;
                }

                bgwOraSync.ReportProgress(30);

                //Cancellation check
                if (bgwStarter.CancellationPending) { e.Cancel = true; return;}

                //Save id's of not sync records
                List<int> ids = records.Select(r => r.pid).ToList();
                bgwOraSync.ReportProgress(40);

                List<oraEntity> oraRecords = new List<oraEntity>();
                float p = 40;
                float max = records.Count();
                float step = (70f - p) / max;
                EntLocalToRemoteConverter converter = new EntLocalToRemoteConverter();
                foreach (dbLocalRecord r in records)
                {
                    oraRecords.Add(converter.return_oraRecord(r));
                    p = p + step;
                    bgwOraSync.ReportProgress((int)p);
                }

                //Cancellation check
                if (bgwStarter.CancellationPending) { e.Cancel = true; return; }

                if (oraRecords.Count > 0)
                {
                    if (oraEx.insert(oraRecords))
                    {
                        bgwOraSync.ReportProgress(100);
                        e.Result = ids;
                        Model.remoteDbNumberOfRecords += oraRecords.Count;
                    }
                    else
                    {
                        Model.isSyncInProgress = false;
                        Model.isSyncPending = false; // 
                        Model.cmdToSyncNow = false;
                    }
                }
            }


        }

        private void BgwOraTestConnection_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                bool isOk = (bool)e.Result;
                Model.isRemoteDBConnected = isOk;
                Model.addLogRecord(OraExchanger.TAG, "Ora connectin test finished...result = " + isOk.ToString());

                if ((!bgwOraSync.IsBusy) & (isOk))
                    bgwOraSync.RunWorkerAsync();
            }
        }

        private void BgwOraTestConnection_DoWork(object sender, DoWorkEventArgs e)
        {
            if (oraEx != null)
                e.Result = oraEx.TestConnection();
            else
                e.Cancel = true;
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
            Model.addLogRecord(OraExchanger.TAG, "Finish test oracle connection");
            Model.isRemoteDBConnected = oraEx.isDbConnectionOK;
            Model.lbl_InitConnection_isVisible = false;

            Model.isSyncPending = Model.isAutoSynhronisation;
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (bgwStarter.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            Model.lbl_InitConnection_isVisible = true;
            oraEx = new OraExchanger();
            bgwStarter.ReportProgress(100);
        }

        #endregion



        #region Local Database operations

        private void LocalDBCreate()
        {
            FileWorks fw = new FileWorks();
            string path = fw.GetSaveFilePath();
            if (path != "")
            {
                dbManager = new dbLocalManager(path, true);
                dbManager.ReportMessage -= DbManager_ReportMessage;
                dbManager.ReportMessage += DbManager_ReportMessage;
                Model.isLocalDBConnected = true;
                Model.localDbPath = path;
            }
        }


        private void LocalDBLoad()
        {
            FileWorks fw = new FileWorks();
            string path = fw.GetLoadFilePath();
            if (path != "")
            {
                dbManager = new dbLocalManager(path, false);
                dbManager.ReportMessage -= DbManager_ReportMessage;
                dbManager.ReportMessage += DbManager_ReportMessage;
                Model.isLocalDBConnected = true;
                Model.localDbPath = path;
            }           
        }


        private bool LocalDBLoadAuto(string path)
        {
            if (path != "")
            {
                dbManager = new dbLocalManager(path, false);
                dbManager.ReportMessage -= DbManager_ReportMessage;
                dbManager.ReportMessage += DbManager_ReportMessage;
                Model.isLocalDBConnected = true;
                Model.localDbPath = path;

                Model.addLogRecord(TAG, "LocalDB loaded in auto mode");
                return true;
            }
            Model.addLogRecord(TAG, "Fail to load LocalDB");
            return false;

        }


        private void LocalDBInsert(bool flag)
        {
            if ((dbManager != null) & (Model.opcMonitoredTags.Count > 0)) 
            {
                bool result = dbManager.insert(Model.opcMonitoredTags, flag, Model.specialFields.G_UCHASTOK, Model.specialFields.N_STAN);
                if (result) Model.localDbNumberOfRecords++;
            }
        }


        private void LocalDBGetRecords()
        {
            Model.isLocalDbLoading = true;
            if (dbManager != null)
            {
                Dispatcher.Invoke(DispatcherPriority.Background,
                              new Action(delegate {
                                  ucDBtblLocal ucLocalTable = new ucDBtblLocal(dbManager.getAllRecords());
                                  wndDBbrowser dbWindow = new wndDBbrowser();
                                  dbWindow.Content = ucLocalTable;
                                  dbWindow.Title = "Local database ";
                                  dbWindow.Show();
                              }));
            }
            Model.isLocalDbLoading = false;
        }

        
        private void DataInsertProcedure()
        {
            bool flag = false;


            if (Model.isRemoteDBConnected)
            {
                flag = OraInsert();
                Model.isRemoteInsertOk = flag;
                Model.isRemoteDBConnected = flag;

                if (flag) Model.remoteDbNumberOfRecords++;

                if (flag && (Model.isSyncPending) && (!bgwOraSync.IsBusy))
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

      

        #region OPC Server operations

        private void SearchServers(string hostName)
        {
            Model.opcListServers.Clear();
            foreach (mServerItem i in opcServer.GetServers(hostName)) { Model.opcListServers.Add(i); }
            Model.addLogRecord(exOPCserver.TAG, "Server search complited...");
        }

        private void Connect()
        {
            mServerItem selected_Server = (mServerItem)lst_Servers.SelectedItem;
            if (selected_Server != null)
            {
                Model.addLogRecord(exOPCserver.TAG, "Trying to connect to selected server");
                if (opcServer.ConnectServer(selected_Server.UrlString))
                {
                    Model.changeState(ModelState.opcConneted);
                }
                Model.selectedOPCserver = opcServer.selectedServer;
            }


            //mServerItem selected_Server = (mServerItem)lst_Servers.SelectedItem;
            //if (selected_Server != null)
            //{
            //    Model.addLogRecord("Trying to connect to selected server");
            //    //Model.isOPCwaiting = true;

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
            
            if ( (dgrid_Tags.SelectedItem != null) & (dgrid_Subscribed.SelectedItem != null) )
            {
                mItem selectedTag = (mItem)dgrid_Tags.SelectedItem;

                bool contain = Model.opcSubscribedTags.Any(t => t.Name == selectedTag.Name);
                if (!contain)
                {
                    mTag subscrTag = (mTag)dgrid_Subscribed.SelectedItem as mTag;
                    mTag sourceTag = Model.opcSubscribedTags.FirstOrDefault(t => t.NameInDb == subscrTag.NameInDb);
                    sourceTag.Name = selectedTag.Name;
                    sourceTag.Path = selectedTag.Path;
                }
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

            Model.opcSubscribedTags.Clear();

            foreach (mTag t in Model.opcMonitoredTags)
            {
                t.Value = "x";
                t.Quality = "unsubsribed";
                Model.opcSubscribedTags.Add(t);
            }

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
                if (config.Save(Model.configuredServer, path))
                Model.addLogRecord(TAG, "Config saved");
            }

            SimatekExChanger.Properties.Settings.Default.configPath = path;
            SimatekExChanger.Properties.Settings.Default.Save();
        }


        private void LoadConfiguration(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                FileWorks fw = new FileWorks();
                path = fw.GetLoadFilePath();
            }

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
            Model.isSyncDenied = true;
            if (opcServer.ConnectServer(Model.selectedOPCserver.UrlString))
            {
                Model.changeState(ModelState.opcConneted);
                Model.selectedOPCserver = opcServer.selectedServer;

                LocalDBLoadAuto(Model.localDbPath);
                Subscribe();
                
            }

            Model.isSyncDenied = false;
        }

        private void AutoRestart()
        {
            LoadConfiguration(SimatekExChanger.Properties.Settings.Default.configPath);
            ConnectFromConfig();
            // DataInsertProcedure();
        }


        #endregion



        #region Remote Database operations


        private void OraTableInit()
        {
            Model.opcSubscribedTags.Clear();


            foreach (string s in OraExchanger.GetFields())
            {
                mTag tag = new mTag();
                tag.NameInDb = s;
                Model.opcSubscribedTags.Add(tag);
            }
        }

        private void OraShowTable()
        {
            if (oraEx != null)
            {
                Model.isRemoteDbLoading = true;
                Dispatcher.Invoke(DispatcherPriority.Background,
                  new Action(delegate
                  {
                      ucDBtblRemote ucOraTable = new ucDBtblRemote(oraEx.GetRecords());
                      wndDBbrowser dbWindow = new wndDBbrowser();

                      dbWindow.Content = ucOraTable;
                      dbWindow.Title = "Remote database";
                      dbWindow.Show();
                  }));
                Model.isRemoteDbLoading = false;
            }
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
                if (oraEx.isDbConnectionOK)
                {
                    return oraEx.insert(Model.opcMonitoredTags.ToList(),Model.specialFields);
                }
            return false;
        }

        
        #endregion



        #region Interface

        //--------- Buttons
        private void btn_GetServers_Click(object sender, RoutedEventArgs e)
        {          
            SearchServers(Model.selectedOPCserver.Host);
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
            Model.isOPCwaiting = true;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                          new Action(delegate {
                                              Connect();
                                              BrowseServer();
                                          }));

            Model.isOPCwaiting = false;
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
            LoadConfiguration(string.Empty);
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

        

        private void btn_oraTestConnection_Click(object sender, RoutedEventArgs e)
        {
            Model.addLogRecord(OraExchanger.TAG, "Ora connectin test started...");
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
            LocalDBLoad();
        }

        private void btn_LocalTableInsert_Click(object sender, RoutedEventArgs e)
        {
            Model.addLogRecord(dbLocalManager.TAG, "Local Db test record to insert...");
            LocalDBInsert(true);
            
        }

        private void btn_LocalTableView_Click(object sender, RoutedEventArgs e)
        {
            LocalDBGetRecords();
        }

        private void btn_WaitingCancel_Click(object sender, RoutedEventArgs e)
        {
            bgwStarter.CancelAsync();
        }

        private void btn_RemoteTableInsert_Click(object sender, RoutedEventArgs e)
        {
            if (oraEx != null)
            {
                //Dispatcher.Invoke(DispatcherPriority.Background,
                //    new Action(delegate
                //    {
                //        //oraEx.AddTestRecord();
                //    }));
                oraManualFields man = new oraManualFields();
                man.G_UCHASTOK =  Model.specialFields.G_UCHASTOK.ToString().ToLower();
                man.N_STAN = Model.specialFields.N_STAN;
                bool b = oraEx.insert(Model.opcMonitoredTags.ToList(), man );
                if (b) Model.remoteDbNumberOfRecords++;
            }

        }

        private void btn_CreateLocalDb_Click(object sender, RoutedEventArgs e)
        {
            LocalDBCreate();
        }

        #endregion



        #region Additional functions

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



        #endregion

        private void btn_LogFilter_Click(object sender, RoutedEventArgs e)
        {
            LogFilter TAG = (LogFilter)((Button)sender).Tag;

            Model.SetLogFilter(TAG);
        }

        private void btn_RemoteInitFields_Click(object sender, RoutedEventArgs e)
        {

            OraTableInit();

        }

        private void btn_Sync_Click(object sender, RoutedEventArgs e)
        {
            SyncNow();
        }
    }
}
