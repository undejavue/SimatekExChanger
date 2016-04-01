using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ClassLibOPC;
using ClassLibGlobal;
using ClassLibOracle;
using EFlocalDB;
using System.Windows.Media;

namespace SimatekExCnahger
{

    public enum ModelState
    {
        initialized,
        opcConneted,
        opcDisconneted,
        opcSubscribed,
        opcUnsubscribed,
        configLoaded
    }


    public class ViewModel : INotifyPropertyChanged
    {
        #region Public Properties

        public LogFilter logFilter;

        public static string TAG = LogFilter.GUI.ToString();

        /// <summary>
        /// Log of messages, buffer of strings
        /// </summary>
        private ObservableCollection<gLogEntity> messageLog { get; set; }
        public ObservableCollection<gLogEntity> messageLogFiltered { get; set; }
        /// <summary>
        /// List of OPC servers
        /// </summary>
        public ObservableCollection<mServerItem> opcListServers { get; set; }
        /// <summary>
        /// Tree of selected server nodes/branches
        /// </summary>
        public ObservableCollection<mTreeNode> opcTreeNodes { get; set; }
        /// <summary>
        /// List of opc tags in selected branch
        /// </summary>
        public ObservableCollection<mItem> opcListTagsInBranch { get; set; }
        /// <summary>
        /// List of monitored tags
        /// </summary>
        public ObservableCollection<mTag> opcMonitoredTags { get; set; }
        /// <summary>
        /// List of subscribed opc tags
        /// </summary>
        public ObservableCollection<mTag> opcSubscribedTags { get; set; }
        /// <summary>
        /// Selected OPC server
        /// </summary>
        private mServerItem _selectedOPCserver;
        public mServerItem selectedOPCserver
        {
            get { return _selectedOPCserver; }
            set
            {
                _selectedOPCserver = value;
                OnPropertyChanged(new PropertyChangedEventArgs("selectedOPCserver"));
            }
        }

        private vmError _opcError;
        public vmError opcError
        {
            get { return _opcError; }
            set
            {
                _opcError = value;
                OnPropertyChanged(new PropertyChangedEventArgs("opcError"));
            }
        }

        private gErrorEntity _gError;
        public gErrorEntity gError
        {
            get { return _gError; }
            set
            {
                _gError = value;
                OnPropertyChanged(new PropertyChangedEventArgs("gError"));
            }
        }

        /// <summary>
        /// Configured OPC server
        /// </summary>
        private dbServerItem _configuredServer;
        public dbServerItem configuredServer
        {
            get { return _configuredServer; }
            set
            {
                _configuredServer = value;
                OnPropertyChanged(new PropertyChangedEventArgs("configuredServer"));
            }
        }

        private ModelState _state;
        public ModelState state
        {
            get { return _state; }
            set
            {
                _state = value;
                OnPropertyChanged(new PropertyChangedEventArgs("state"));
                OnPropertyChanged(new PropertyChangedEventArgs("ModelStateString"));

            }
        }
        public string ModelStateString
        {
            get { return state.ToString(); }
        }

        private Brush _infoLineColor;
        public Brush infoLineColor
        {
            get
            {
                return _infoLineColor;
            }
            set
            {
                _infoLineColor = value;
                OnPropertyChanged(new PropertyChangedEventArgs("infoLineColor"));
            }
        }

        private bool _isRemoteDBConnected;
        public bool isRemoteDBConnected
        {
            get
            {
                return _isRemoteDBConnected;
            }
            set
            {
                _isRemoteDBConnected = value;
                OnPropertyChanged(new PropertyChangedEventArgs("isRemoteDBConnected"));
            }
        }

        private bool _isLocalDBConnected;
        public bool isLocalDBConnected
        {
            get
            {
                return _isLocalDBConnected;
            }
            set
            {
                _isLocalDBConnected = value;
                OnPropertyChanged(new PropertyChangedEventArgs("isLocalDBConnected"));
            }
        }

        private bool _isOPCwaiting;
        public bool isOPCwaiting
        {
            get
            {
                return _isOPCwaiting;
            }
            set
            {
                _isOPCwaiting = value;
                OnPropertyChanged(new PropertyChangedEventArgs("isOPCwaiting"));
            }
        }

        private bool _isLocalDbLoading;
        public bool isLocalDbLoading
        {
            get
            {
                return _isLocalDbLoading;
            }
            set
            {
                _isLocalDbLoading = value;
                OnPropertyChanged(new PropertyChangedEventArgs("isLocalDbLoading"));
            }
        }

        private bool _isRemoteDbLoading;
        public bool isRemoteDbLoading
        {
            get
            {
                return _isRemoteDbLoading;
            }
            set
            {
                _isRemoteDbLoading = value;
                OnPropertyChanged(new PropertyChangedEventArgs("isRemoteDbLoading"));
            }
        }

        private bool _isSyncInProgress;
        public bool isSyncInProgress
        {
            get
            {
                return _isSyncInProgress;
            }
            set
            {
                _isSyncInProgress = value;
                OnPropertyChanged(new PropertyChangedEventArgs("isSyncInProgress"));
            }
        }

        private long _progressBarSync;
        public long progressBarSync
        {
            get
            {
                return _progressBarSync;
            }
            set
            {
                _progressBarSync = value;
                OnPropertyChanged(new PropertyChangedEventArgs("progressBarSync"));
            }
        }

        private long _progressBarOraTestConn;
        public long progressBarOraTestConn
        {
            get
            {
                return _progressBarOraTestConn;
            }
            set
            {
                _progressBarOraTestConn = value;
                OnPropertyChanged(new PropertyChangedEventArgs("progressBarOraTestConn"));
            }
        }

        public oraManualFields specialFields { get; set; }


        #endregion


        #region Interface Buttons


        private object _btn_LogFilterAll_Tag;
        public object btn_LogFilterAll_Tag
        {
            get { return _btn_LogFilterAll_Tag; }
            set
            {
                _btn_LogFilterAll_Tag = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_LogFilterAll_Tag"));
            }
        }

        private object _btn_LogFilterGUI_Tag;
        public object btn_LogFilterGUI_Tag
        {
            get { return _btn_LogFilterGUI_Tag; }
            set
            {
                _btn_LogFilterGUI_Tag = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_LogFilterGUI_Tag"));
            }
        }

        private object _btn_LogFilterOPC_Tag;
        public object btn_LogFilterOPC_Tag
        {
            get { return _btn_LogFilterOPC_Tag; }
            set
            {
                _btn_LogFilterOPC_Tag = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_LogFilterOPC_Tag"));
            }
        }

        private object _btn_LogFilterLocalDb_Tag;
        public object btn_LogFilterLocalDb_Tag
        {
            get { return _btn_LogFilterLocalDb_Tag; }
            set
            {
                _btn_LogFilterLocalDb_Tag = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_LogFilterLocalDb_Tag"));
            }
        }

        private object _btn_LogFilterRemoteDb_Tag;
        public object btn_LogFilterRemoteDb_Tag
        {
            get { return _btn_LogFilterRemoteDb_Tag; }
            set
            {
                _btn_LogFilterRemoteDb_Tag = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_LogFilterRemoteDb_Tag"));
            }
        }

        private Brush _btn_LogFilterAll_Brush;
        public Brush btn_LogFilterAll_Brush
        {
            get { return _btn_LogFilterAll_Brush; }
            set
            {
                _btn_LogFilterAll_Brush = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_LogFilterAll_Brush"));
            }
        }

        private Brush _btn_LogFilterGUI_Brush;
        public Brush btn_LogFilterGUI_Brush
        {
            get { return _btn_LogFilterGUI_Brush; }
            set
            {
                _btn_LogFilterGUI_Brush = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_LogFilterGUI_Brush"));
            }
        }

        private Brush _btn_LogFilterOPC_Brush;
        public Brush btn_LogFilterOPC_Brush
        {
            get { return _btn_LogFilterOPC_Brush; }
            set
            {
                _btn_LogFilterOPC_Brush = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_LogFilterOPC_Brush"));
            }
        }

        private Brush _btn_LogFilterLocalDb_Brush;
        public Brush btn_LogFilterLocalDb_Brush
        {
            get { return _btn_LogFilterLocalDb_Brush; }
            set
            {
                _btn_LogFilterLocalDb_Brush = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_LogFilterLocalDb_Brush"));
            }
        }

        private Brush _btn_LogFilterRemoteDb_Brush;
        public Brush btn_LogFilterRemoteDb_Brush
        {
            get { return _btn_LogFilterRemoteDb_Brush; }
            set
            {
                _btn_LogFilterRemoteDb_Brush = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_LogFilterRemoteDb_Brush"));
            }
        }


        private bool _btn_Connect_isEnable;
        public bool btn_Connect_isEnable
        {
            get { return _btn_Connect_isEnable; }
            set
            {
                _btn_Connect_isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_Connect_isEnable"));
            }
        }


        private bool _btn_Search_isEnable;
        public bool btn_Search_isEnable
        {
            get { return _btn_Search_isEnable; }
            set
            {
                _btn_Search_isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_Search_isEnable"));
            }
        }


        private bool _btn_TryConfig_isEnable;
        public bool btn_TryConfig_isEnable
        {
            get { return _btn_TryConfig_isEnable; }
            set
            {
                _btn_TryConfig_isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_TryConfig_isEnable"));
            }
        }


        private bool _btn_Subscribe_isEnable;
        public bool btn_Subscribe_isEnable
        {
            get { return _btn_Subscribe_isEnable; }
            set
            {
                _btn_Subscribe_isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_Subscribe_isEnable"));
            }
        }


        private bool _btn_Unsubscribe_isEnable;
        public bool btn_Unsubscribe_isEnable
        {
            get { return _btn_Unsubscribe_isEnable; }
            set
            {
                _btn_Unsubscribe_isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_Unsubscribe_isEnable"));
            }
        }


        private bool _btn_SyncOn_isEnable;
        public bool btn_SyncOn_isEnable
        {
            get { return _btn_SyncOn_isEnable; }
            set
            {
                _btn_SyncOn_isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_SyncOn_isEnable"));
            }
        }


        private bool _btn_SyncOff_isEnable;
        public bool btn_SyncOff_isEnable
        {
            get { return _btn_SyncOff_isEnable; }
            set
            {
                _btn_SyncOff_isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_SyncOff_isEnable"));
            }
        }


        private bool _btn_ClearTags_isEnable;
        public bool btn_ClearTags_isEnable
        {
            get { return _btn_ClearTags_isEnable; }
            set
            {
                _btn_ClearTags_isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_ClearTags_isEnable"));
            }
        }


        private bool _btn_LoadConfig_isEnable;
        public bool btn_LoadConfig_isEnable
        {
            get { return _btn_LoadConfig_isEnable; }
            set
            {
                _btn_LoadConfig_isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_LoadConfig_isEnable"));
            }
        }


        private bool _btn_SaveConfig_isEnable;
        public bool btn_SaveConfig_isEnable
        {
            get { return _btn_SaveConfig_isEnable; }
            set
            {
                _btn_SaveConfig_isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("btn_SaveConfig_isEnable"));
            }
        }

        #endregion


        #region Interface views

        private bool _lbl_InitConnection_isVisible;
        public bool lbl_InitConnection_isVisible
        {
            get { return _lbl_InitConnection_isVisible; }
            set
            {
                _lbl_InitConnection_isVisible = value;
                OnPropertyChanged(new PropertyChangedEventArgs("lbl_InitConnection_isVisible"));
            }
        }

        private bool _dgrid_Subscribed_isVisible;
        public bool dgrid_Subscribed_isVisible
        {
            get { return _dgrid_Subscribed_isVisible; }
            set
            {
                _dgrid_Subscribed_isVisible = value;
                OnPropertyChanged(new PropertyChangedEventArgs("dgrid_Subscribed_isVisible"));
            }
        }

        private bool _dgrid_Monitored_isVisible;
        public bool dgrid_Monitored_isVisible
        {
            get { return _dgrid_Monitored_isVisible; }
            set
            {
                _dgrid_Monitored_isVisible = value;
                OnPropertyChanged(new PropertyChangedEventArgs("dgrid_Monitored_isVisible"));
            }
        }


        private bool _list_Servers_isEnable;
        public bool list_Servers_isEnable
        {
            get { return _list_Servers_isEnable; }
            set
            {
                _list_Servers_isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("list_Servers_isEnable"));
            }
        }

        private bool _list_Branches_isEnable;
        public bool list_Branches_isEnable
        {
            get { return _list_Branches_isEnable; }
            set
            {
                _list_Branches_isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("list_Branches_isEnable"));
            }
        }

        private bool _list_Tags_isEnable;
        public bool list_Tags_isEnable
        {
            get { return _list_Tags_isEnable; }
            set
            {
                _list_Tags_isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("list_Tags_isEnable"));
            }
        }


        private bool _list_SubscribedTags_isEnable;
        public bool list_SubscribedTags_isEnable
        {
            get { return _list_SubscribedTags_isEnable; }
            set
            {
                _list_SubscribedTags_isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("list_SubscribedTags_isEnable"));
            }
        }

        #endregion


        #region Constructors
        public ViewModel()
        {
            changeState(ModelState.initialized);

        }
        #endregion


        #region Public methods
      
        public void changeState(ModelState s)
        {
            state = s;
            changeModel();
        }

        private void changeModel()
        {
            switch (state)
            {
                case ModelState.initialized:

                    Initialize();
                    break;

                case ModelState.opcConneted:

                    btn_ClearTags_isEnable = false;
                    btn_Connect_isEnable = false;
                    btn_LoadConfig_isEnable = false;
                    btn_SaveConfig_isEnable = true;
                    btn_TryConfig_isEnable = false;
                    btn_Subscribe_isEnable = true;
                    btn_Unsubscribe_isEnable = false;
                    btn_Search_isEnable = false;

                    btn_SyncOff_isEnable = true;
                    btn_SyncOn_isEnable = true;

                    list_Servers_isEnable = false;
                    list_Branches_isEnable = true;
                    list_Tags_isEnable = true;
                    list_SubscribedTags_isEnable = true;

                    infoLineColor = new SolidColorBrush(Colors.LightGreen);

                    break;

                case ModelState.opcDisconneted:

                    btn_ClearTags_isEnable = false;
                    btn_Connect_isEnable = true;
                    btn_LoadConfig_isEnable = true;
                    btn_SaveConfig_isEnable = false;
                    btn_Subscribe_isEnable = false;
                    btn_Unsubscribe_isEnable = false;
                    btn_Search_isEnable = true;

                    btn_SyncOff_isEnable = true;
                    btn_SyncOn_isEnable = true;

                    list_Servers_isEnable = true;
                    list_Branches_isEnable = true;
                    list_Tags_isEnable = true;
                    list_SubscribedTags_isEnable = true;

                    dgrid_Monitored_isVisible = false;
                    dgrid_Subscribed_isVisible = true;

                    Clear();

                    break;

                case ModelState.opcSubscribed:

                    btn_Subscribe_isEnable = false;
                    btn_Unsubscribe_isEnable = true;
                    btn_ClearTags_isEnable = false;
                    btn_Search_isEnable = false;
                    btn_TryConfig_isEnable = false;

                    list_Branches_isEnable = false;
                    list_Tags_isEnable = false;

                    dgrid_Monitored_isVisible = true;
                    dgrid_Subscribed_isVisible = false;

                    break;

                case ModelState.opcUnsubscribed:

                    btn_Subscribe_isEnable = true;
                    btn_Unsubscribe_isEnable = false;
                    btn_ClearTags_isEnable = true;

                    list_Branches_isEnable = true;
                    list_Tags_isEnable = true;

                    dgrid_Monitored_isVisible = false;
                    dgrid_Subscribed_isVisible = true;

                    break;
                case ModelState.configLoaded:

                    btn_TryConfig_isEnable = true;

                    list_Branches_isEnable = false;
                    list_Tags_isEnable = false;
                    infoLineColor = new SolidColorBrush(Colors.DarkSlateBlue);

                    break;

            }
        }

        public void Initialize()
        {
            opcListServers = new ObservableCollection<mServerItem>();
            opcTreeNodes = new ObservableCollection<mTreeNode>();
            opcListTagsInBranch = new ObservableCollection<mItem>();
            opcMonitoredTags = new ObservableCollection<mTag>();
            opcSubscribedTags = new ObservableCollection<mTag>();
            selectedOPCserver = new mServerItem(true);

            messageLog = new ObservableCollection<gLogEntity>();
            messageLog.Add(new gLogEntity(TAG, "Start log"));
            messageLogFiltered = new ObservableCollection<gLogEntity>();

            btn_ClearTags_isEnable = false;
            btn_Connect_isEnable = true;
            btn_LoadConfig_isEnable = true;
            btn_SaveConfig_isEnable = false;
            btn_TryConfig_isEnable = false;
            btn_Subscribe_isEnable = false;
            btn_SyncOff_isEnable = true;
            btn_SyncOn_isEnable = true;
            btn_Unsubscribe_isEnable = false;
            btn_Search_isEnable = true;

            list_Branches_isEnable = true;
            list_Servers_isEnable = true;
            list_SubscribedTags_isEnable = true;
            list_Tags_isEnable = true;

            dgrid_Monitored_isVisible = false;
            dgrid_Subscribed_isVisible = true;

            lbl_InitConnection_isVisible = true;

            infoLineColor = new SolidColorBrush(Colors.White);

            isLocalDBConnected = false;
            isRemoteDBConnected = false;
            isSyncInProgress = false;
            
            gError = new gErrorEntity(1, "Created in model");
            opcError = new vmError(gError);

            specialFields = new oraManualFields();
            specialFields.N_STAN = 3;
            specialFields.G_UCHASTOK = "D";

            FilterButtonsSetTags();
            SetLogFilter(LogFilter.All);

        }

        public void Clear()
        {
            opcListServers.Clear();
            opcTreeNodes.Clear();
            opcListTagsInBranch.Clear();
            opcMonitoredTags.Clear();
            opcSubscribedTags.Clear();

            infoLineColor = new SolidColorBrush(Colors.WhiteSmoke);

            addLogRecord(TAG, "ViewModel is cleared");
        }

        public void SaveServer()
        {
            configuredServer = new dbServerItem();

            configuredServer.opcHost = selectedOPCserver.Host;
            configuredServer.opcURL = selectedOPCserver.UrlString;
            configuredServer.opcRecconect = selectedOPCserver.isReconnect;

            foreach (mTag tag in opcMonitoredTags)
            {
                configuredServer.opcMonitoredTags.Add(new dbTagItem(tag));
            }
            addLogRecord(TAG, "Server configuration copied");
        }


        public bool LoadServer(dbServerItem srv)
        {
            try
            {
                configuredServer = new dbServerItem();
                configuredServer = srv;

                selectedOPCserver.Host = srv.opcHost;
                selectedOPCserver.UrlString = srv.opcURL;
                selectedOPCserver.isReconnect = srv.opcRecconect;

                foreach (dbTagItem tag in srv.opcMonitoredTags)
                {
                    mTag t = tag;
                    opcSubscribedTags.Add(t);
                }
                addLogRecord(TAG, "Server configuration loaded successfully");
                addLogRecord(TAG, "Server url is: " + selectedOPCserver.UrlString);
                addLogRecord(TAG, "Tags count for subscribtion: " + opcSubscribedTags.Count.ToString());
                return true;
            }
            catch (Exception ex)
            {
                addLogRecord(TAG, "Fail to load server configuration...");
                addLogRecord(TAG, ex.Message);
                return false;
            }
        }

        #endregion


        #region Message Log filter

        public void addLogRecord(string TAG, string record)
        {
            messageLog.Add(new gLogEntity(TAG, record));
            if (TAG.Equals(logFilter.ToString()) | logFilter.Equals(LogFilter.All))
            {
                messageLogFiltered.Add(new gLogEntity(TAG, record));
            }

            if (messageLog.Count > 1000)
            {
                //using (dbConfManager conf = new dbConfManager())
                //{
                //    conf.SaveLogs(messageLog.ToList());
                //}
                // MAKE DROP FOR BIG COUNT
            }
        }

        public List<string> unloadMessageLog()
        {
            return messageLog.Select(k => k.Entry).ToList();
        }

        public void SetLogFilter(LogFilter TAG)
        {
            FilterLog(TAG);

            Brush active = new SolidColorBrush(Colors.LightGreen);
            Brush inactive = new SolidColorBrush(Colors.WhiteSmoke);

            switch (TAG)
            {
                case LogFilter.All:
                    btn_LogFilterAll_Brush = active;
                    btn_LogFilterGUI_Brush = active;
                    btn_LogFilterOPC_Brush = active;
                    btn_LogFilterLocalDb_Brush = active;
                    btn_LogFilterRemoteDb_Brush = active;
                    break;
                case LogFilter.GUI:
                    btn_LogFilterAll_Brush = inactive;
                    btn_LogFilterGUI_Brush = active;
                    btn_LogFilterOPC_Brush = inactive;
                    btn_LogFilterLocalDb_Brush = inactive;
                    btn_LogFilterRemoteDb_Brush = inactive;
                    break;
                case LogFilter.LocalDB:
                    btn_LogFilterAll_Brush = inactive;
                    btn_LogFilterGUI_Brush = inactive;
                    btn_LogFilterOPC_Brush = inactive;
                    btn_LogFilterLocalDb_Brush = active;
                    btn_LogFilterRemoteDb_Brush = inactive;
                    break;
                case LogFilter.RemoteDB:
                    btn_LogFilterAll_Brush = inactive;
                    btn_LogFilterGUI_Brush = inactive;
                    btn_LogFilterOPC_Brush = inactive;
                    btn_LogFilterLocalDb_Brush = inactive;
                    btn_LogFilterRemoteDb_Brush = active;
                    break;
                case LogFilter.OPC:
                    btn_LogFilterAll_Brush = inactive;
                    btn_LogFilterGUI_Brush = inactive;
                    btn_LogFilterOPC_Brush = active;
                    btn_LogFilterLocalDb_Brush = inactive;
                    btn_LogFilterRemoteDb_Brush = inactive;
                    break;
            }

            
        }

        private void  FilterButtonsSetTags()
        {
            btn_LogFilterAll_Tag = LogFilter.All;
            btn_LogFilterGUI_Tag = LogFilter.GUI;
            btn_LogFilterOPC_Tag = LogFilter.OPC;
            btn_LogFilterLocalDb_Tag = LogFilter.LocalDB;
            btn_LogFilterRemoteDb_Tag = LogFilter.RemoteDB;
        }

        private void FilterLog(LogFilter TAG)
        {
            logFilter = TAG;

            if (TAG == LogFilter.All)
            {
                messageLogFiltered.Clear();
                foreach (gLogEntity entry in messageLog)
                {
                    messageLogFiltered.Add(entry);
                }
            }
            else
            {
                messageLogFiltered.Clear();
                foreach (gLogEntity entry in messageLog.Where(m => m.Tag == TAG.ToString()))
                {
                    messageLogFiltered.Add(entry);
                }
            }
        }
        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

    }

}