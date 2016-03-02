using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibOPC;
using ClassLibGlobal;

namespace WPFinterface
{

    public enum ModelState
    {
        initialized,
        opcConneted,
        opcDisconneted,
        opcSubscribed,
        opcUnsubscribed
    }

    public class ViewModel : INotifyPropertyChanged
    {

        /// <summary>
        /// Log of messages, buffer of strings
        /// </summary>
        public ObservableCollection<LogItem> messageLog { get; set; }
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
        /// <summary>
        /// Configured OPC server
        /// </summary>
        private mServerItem _configuredOPCserver;
        public mServerItem configuredOPCserver
        {
            get { return _configuredOPCserver; }
            set
            {
                _configuredOPCserver = value;
                OnPropertyChanged(new PropertyChangedEventArgs("configuredOPCserver"));
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
            }
        }



        #region Interface buttons and views


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
            messageLog.Add(new LogItem(record));
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
                    btn_Subscribe_isEnable = true;
                    btn_Unsubscribe_isEnable = false;
                    btn_Search_isEnable = false;

                    btn_SyncOff_isEnable = true;
                    btn_SyncOn_isEnable = true;

                    list_Servers_isEnable = false;
                    list_Branches_isEnable = true;
                    list_Tags_isEnable = true;
                    list_SubscribedTags_isEnable = true;
                    break;

                case ModelState.opcDisconneted:

                    btn_ClearTags_isEnable = false;
                    btn_Connect_isEnable = true;
                    btn_LoadConfig_isEnable = true;
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
                    break;

                case ModelState.opcSubscribed:

                    btn_Subscribe_isEnable = false;
                    btn_Unsubscribe_isEnable = true;
                    btn_ClearTags_isEnable = false;
                    btn_Search_isEnable = false;

                    dgrid_Monitored_isVisible = true;
                    dgrid_Subscribed_isVisible = false;

                  break;

                case ModelState.opcUnsubscribed:

                    btn_Subscribe_isEnable = true;
                    btn_Unsubscribe_isEnable = false;
                    btn_ClearTags_isEnable = true;

                    dgrid_Monitored_isVisible = false;
                    dgrid_Subscribed_isVisible = true;

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
            configuredOPCserver = new mServerItem(true);

            messageLog = new ObservableCollection<LogItem>();
            messageLog.Add(new LogItem("Start log"));

            btn_ClearTags_isEnable = false;
            btn_Connect_isEnable = true;
            btn_LoadConfig_isEnable = true;
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

            //state = ModelState.initialized;

        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

    }

}