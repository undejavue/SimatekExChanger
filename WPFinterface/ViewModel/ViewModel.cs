using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibOPC;
using ClassLibGlobal;
using System.Windows.Threading;
using EFconfigDB;
using System.Windows.Media;

namespace WPFinterface
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

        /// <summary>
        /// Log of messages, buffer of strings
        /// </summary>
        public ObservableCollection<gLogEntity> messageLog { get; set; }
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


        private bool _isDbServerConnected;
        public bool isDbServerConnected
        {
            get
            {
                return _isDbServerConnected;
            }
            set
            {
                _isDbServerConnected = value;
                OnPropertyChanged(new PropertyChangedEventArgs("isDbServerConnected"));
            }
        }


        private long _progressBar;
        public long progressBar
        {
            get
            {
                return _progressBar;
            }
            set
            {
                _progressBar = value;
                OnPropertyChanged(new PropertyChangedEventArgs("progressBar"));
            }
        }

        #endregion


        #region Interface Buttons


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


        public ViewModel()
        {
            changeState(ModelState.initialized);

        }

        public void addLogRecord(string record)
        {
            messageLog.Add(new gLogEntity(record));
        }

        public List<string> unloadMessageLog()
        {
            return messageLog.Select(k => k.Entry).ToList();
        }

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
            messageLog.Add(new gLogEntity("Start log"));

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

            
            gError = new gErrorEntity(1, "Created in model");
            opcError = new vmError(gError);
        }

        public void Clear()
        {
            opcListServers.Clear();
            opcTreeNodes.Clear();
            opcListTagsInBranch.Clear();
            opcMonitoredTags.Clear();
            opcSubscribedTags.Clear();

            infoLineColor = new SolidColorBrush(Colors.WhiteSmoke);

            addLogRecord("ViewModel is cleared");
        }

        public void SaveServer()
        {
            configuredServer = new dbServerItem();

            configuredServer.opcHost = selectedOPCserver.Host;
            configuredServer.opcURL = selectedOPCserver.UrlString;
            configuredServer.opcRecconect = selectedOPCserver.isReconnect;

            foreach (mTag tag in opcMonitoredTags)
            {
                dbTagItem t = new dbTagItem(tag.Name, tag.Path, tag.Description);
                configuredServer.opcMonitoredTags.Add(t);
            }
            addLogRecord("Server configuration copied");
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
                    mTag t = new mTag(tag.Name, tag.Path);
                    opcSubscribedTags.Add(t);
                }
                addLogRecord("Server configuration loaded successfully");
                addLogRecord("Server url is: " + selectedOPCserver.UrlString);
                addLogRecord("Tags count for subscribtion: " + opcSubscribedTags.Count.ToString());
                return true;
            }
            catch (Exception ex)
            {
                addLogRecord("Fail to load server configuration...");
                addLogRecord(ex.Message);
                return false;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

    }

}