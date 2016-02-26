using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibOPC;

namespace WPFinterface
{

    public enum ModelState
    {
        initialized,
        opcConneted,
        opcDisconneted
    }

    public class ViewModel : Entity
    {

        /// <summary>
        /// Log of messages, buffer of strings
        /// </summary>
        public ObservableCollection<string> messageLog { get; set; }


        public ObservableCollection<mServerItem> opcListServers;
        public ObservableCollection<mTreeNode> opcTreeNodes;
        public ObservableCollection<mItem> opcListTagsInBranch;
        public ObservableCollection<mTag> opcMonitoredTags;
        public ObservableCollection<mTag> opcSubscribedTags;
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

        #region Interface model


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
            Initialize();
            
        }

        public void changeState(ModelState s)
        {
            state = s;
        }

        private void changeModel()
        {
            switch (state)
            {
                case ModelState.initialized:

                    Initialize();

                    break;
                case ModelState.opcConneted:

                    btn_ClearTags_isEnable = true;
                    btn_Connect_isEnable = false;
                    btn_LoadConfig_isEnable = false;
                    btn_Subscribe_isEnable = true;
                    btn_Unsubscribe_isEnable = true;

                    btn_SyncOff_isEnable = true;
                    btn_SyncOn_isEnable = true;

                    list_Servers_isEnable = false;
                    list_Branches_isEnable = true;
                    list_Tags_isEnable = true;
                    list_SubscribedTags_isEnable = true;

                    break;
                case ModelState.opcDisconneted:

                    btn_ClearTags_isEnable = true;
                    btn_Connect_isEnable = true;
                    btn_LoadConfig_isEnable = true;
                    btn_Subscribe_isEnable = true;
                    btn_Unsubscribe_isEnable = true;

                    btn_SyncOff_isEnable = true;
                    btn_SyncOn_isEnable = true;

                    list_Servers_isEnable = true;
                    list_Branches_isEnable = true;
                    list_Tags_isEnable = true;
                    list_SubscribedTags_isEnable = true;

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

            btn_ClearTags_isEnable = true;
            btn_Connect_isEnable = true;
            btn_LoadConfig_isEnable = true;
            btn_Subscribe_isEnable = true;
            btn_SyncOff_isEnable = true;
            btn_SyncOn_isEnable = true;
            btn_Unsubscribe_isEnable = true;

            list_Branches_isEnable = true;
            list_Servers_isEnable = true;
            list_SubscribedTags_isEnable = true;
            list_Tags_isEnable = true;

            state = ModelState.initialized;

        }

    }

}