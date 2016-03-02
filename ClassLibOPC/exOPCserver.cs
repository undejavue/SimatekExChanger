using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Discovery;
using System.ComponentModel;
using System.Timers;


namespace ClassLibOPC
{
    public class exOPCserver
    {
        private Opc.URL url;
        private Opc.Da.Server server;
        private OpcCom.Factory factory;
        private Opc.Da.Subscription opcSubscription;
        private Opc.Da.ServerStatus serverStatus;
        private Timer reconnectTimer;
        
        public ObservableCollection<mServerItem> listServers;
        public string hostname;
        public bool isConnected;
        public ObservableCollection<mTag> monitoredTags;
        public List<mTag> restoredTagList;
        public mServerItem selectedServer;

        public List<string> messageLog;


        public exOPCserver ()
        {
            listServers = new ObservableCollection<mServerItem>();
            monitoredTags = new ObservableCollection<mTag>();
            restoredTagList = new List<mTag>();
            messageLog = new List<string>();
            logMessage("Message log started");

            isConnected = false;
            opcSubscription = null;

            selectedServer = new mServerItem(true);
            selectedServer.ReconnectInterval = 5000;

            configureWatchDog();

        }


        private void logMessage(string m)
        {
            messageLog.Add(DateTime.Now.ToString("hh:mm:ss") + ": " + m);
        }

        public ObservableCollection<mServerItem> GetServers(string hostname)
        {
            OpcCom.ServerEnumerator discovery = new OpcCom.ServerEnumerator();

            if (hostname == "") hostname = "localhost";

            selectedServer.Host = hostname;

            //Get all local OPC DA servers of version 3.0
            Opc.Server[] localservers = discovery.GetAvailableServers(Opc.Specification.COM_DA_30, hostname, null);

            listServers.Clear();

            foreach ( Opc.Server srv in localservers)
            {
                mServerItem si = new mServerItem(true);

                si.Name = srv.Name;
                si.Description = srv.Locale;
                si.UrlString = srv.Url.ToString();

                listServers.Add(si);
            }

            //Get all OPC DA servers of version 2.0 of machine "MyMachine"
            //Opc.Server[] hostservers = discovery.GetAvailableServers(Opc.Specification.COM_DA_20, "MNS1-179N", null);
 
            return listServers;
        }


        /// <summary>
        /// Refresh server status
        /// </summary>
        private void RefreshServerStatus()
        {

            if (server != null)
            {
                try
                {

                    selectedServer.isConnected = server.IsConnected;
                    selectedServer.Name = server.Name;
                    selectedServer.UrlString = server.Url.ToString();

                    serverStatus = server.GetStatus();
                    selectedServer.StatusInfo = serverStatus.StatusInfo;
                    selectedServer.ServerState = serverStatus.ServerState.ToString();
                    selectedServer.ProductVersion = serverStatus.ProductVersion;
                    selectedServer.VendorInfo = serverStatus.VendorInfo;

                }
                catch (Exception ex)
                {
                    OnReportMessage("Fail to get server status, " + ex.Message.ToString());

                    selectedServer.StatusInfo = "unknown";
                    selectedServer.ServerState = "unknown";
                    selectedServer.ProductVersion = "unknown";
                    selectedServer.VendorInfo = "unknown";

                }
            }
        }

        /// <summary>
        /// Server connection procedure
        /// </summary>
        /// <param name="urlString">OPC server URL in string format</param>
        /// <returns>isConnected</returns>
        public bool ConnectServer(string urlString)
        {
            isConnected = false;

            this.url = new Opc.URL(urlString);
            factory = new OpcCom.Factory();
            server = new Opc.Da.Server(factory, url);

            try
            {
                selectedServer = selectedServer ?? new mServerItem(true);
                server.Connect();
                if (isConnected = server.IsConnected)
                {
                    OnReportMessage("Server is connected");
                    server.ServerShutdown -= server_ServerShutdown;
                    server.ServerShutdown += server_ServerShutdown;

                    RefreshServerStatus();
                }
                else
                {
                    OnReportMessage("Server connection fail, unknown error");
                }
            }
            catch (Exception ex)
            {
                OnReportMessage(ex.Message.ToString());               
            }
            
            return isConnected;
        }



        private void server_ServerShutdown(string reason)
        {
            OnReportMessage("Server shutdown, reason: " + reason);
            RefreshServerStatus();

            restoredTagList = new List<mTag>(monitoredTags.ToList());

            DisconnectServer();
            reconnectTimer.Enabled = true;
        }


        private void configureWatchDog()
        {
            reconnectTimer = new System.Timers.Timer();
            reconnectTimer.Elapsed += reconnectTimer_Elapsed;
            reconnectTimer.Interval = selectedServer.ReconnectInterval;
            reconnectTimer.AutoReset = false;

        }

        private void startStopWatchDog(bool startstop)
        {
            reconnectTimer.Interval = selectedServer.ReconnectInterval;
            reconnectTimer.Enabled = startstop;
        }

        private void reconnectTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)      
        {
            OnReportMessage("Server is down, trying to reconnect.."); 
        
            if ( ConnectServer(url.ToString()) )
            {
                startStopWatchDog(false);

                if (restoredTagList.Count() > 0)
                    SubscribeTags(restoredTagList);
            }
            else
            {
                startStopWatchDog(true);
            }

        }


        /// <summary>
        /// Disconnect server and clear all data
        /// </summary>
        /// <returns>IsConnected</returns>
        public bool DisconnectServer()
        {
            if (server != null)
            {
                if (server.IsConnected)
                {
                    if (opcSubscription != null)
                        UnSubcribe();

                    monitoredTags.Clear();
                    server.ServerShutdown -= server_ServerShutdown;

                    server.Disconnect();
                    isConnected = server.IsConnected;

                    RefreshServerStatus();

                    OnReportMessage("Server is disconnected");
                    server.Dispose();
                }
                else
                {
                    OnReportMessage("Server is already disconnected");
                }

            }
            return isConnected;
        }



        /// <summary>
        /// Get OPC server tree by default, from root branch
        /// </summary>
        /// <returns>Collection of all server branches</returns>
        public ObservableCollection<mTreeNode> GetTree()
        {
            Opc.ItemIdentifier itemId = null;

            return GetTree(itemId);
        }


        /// <summary>
        /// Get OPC tree for selected branch
        /// </summary>
        /// <param name="itemId">Parent level element</param>
        /// <returns>Collection of OPC children</returns>
        public ObservableCollection<mTreeNode> GetTree(Opc.ItemIdentifier itemId)
        {
            ObservableCollection<mTreeNode> tree = new ObservableCollection<mTreeNode>();

            if (isConnected)
            {
                Opc.Da.BrowsePosition position;
                Opc.Da.BrowseFilters filters = new Opc.Da.BrowseFilters() { BrowseFilter = Opc.Da.browseFilter.branch };
                Opc.Da.BrowseElement[] elements = server.Browse(itemId, filters, out position);

                if (elements != null)
                {

                    foreach (Opc.Da.BrowseElement el in elements)
                    {
                        mTreeNode node = new mTreeNode();
                        node.Description = el.Name;
                        node.Name = el.ItemName;
                        node.Path = el.ItemPath;                     

                        if (el.HasChildren )
                        {
                            var childItemId = new Opc.ItemIdentifier(el.ItemPath, el.ItemName);
                            node.Nodes = new ObservableCollection<mTreeNode>(GetTree(childItemId));
                        }

                        tree.Add(node);
                    }
                }
            }

            RefreshServerStatus();

            return tree;
        }


        /// <summary>
        /// Get list of OPC tags for selected group/branch
        /// </summary>
        /// <param name="path">Root item/leaf path</param>
        /// <param name="name">Root item/leaf name</param>
        /// <returns>Collection of tags, names and path only, not values</returns>
        public ObservableCollection<mItem> GetTags(string path, string name)
        {
            ObservableCollection<mItem> tags = new ObservableCollection<mItem>();
            
            var itemId = new Opc.ItemIdentifier(path, name);
            Opc.Da.BrowsePosition position;
            Opc.Da.BrowseFilters filters = new Opc.Da.BrowseFilters() { BrowseFilter = Opc.Da.browseFilter.item };
            Opc.Da.BrowseElement[] elements = server.Browse(itemId, filters, out position);

            if (elements != null)
            {
                foreach (Opc.Da.BrowseElement el in elements)
                {
                    mItem tag = new mItem(el.ItemName, el.ItemPath);
                    tags.Add(tag);

                    
                }
            }

            return tags;
        }


        /// <summary>
        /// OPC subsription procedure
        /// </summary>
        /// <param name="Tags">List of manitored tags</param>
        public void SubscribeTags(List<mTag> Tags)
        {
            if (server.IsConnected)
            {
                List<Opc.Da.Item> opcItems = new List<Opc.Da.Item>();
                bool needToAddItems = false;

                foreach (mTag tag in Tags)
                {
                    var contain = monitoredTags.Any(t => t.Name == tag.Name);

                    if (!contain)
                    {

                        // Repeat this next part for all the items you need to subscribe
                        Opc.Da.Item item = new Opc.Da.Item();
                        item.ItemName = tag.Name;
                        //item.ClientHandle = "handle"; // handle is up to you, but i use a logical name for it
                        item.Active = true;
                        item.ActiveSpecified = true;
                        opcItems.Add(item);

                        monitoredTags.Add(tag);
                        needToAddItems = true;
                    }
                }

                if (needToAddItems)
                try
                {

                    Opc.Da.SubscriptionState subscriptionState = new Opc.Da.SubscriptionState();
                    subscriptionState.Active = true;
                    subscriptionState.UpdateRate = 40;
                    subscriptionState.Deadband = 0;

                    if (opcSubscription != null) { }

                    opcSubscription =  (Opc.Da.Subscription)this.server.CreateSubscription(subscriptionState);

                    Opc.Da.ItemResult[] result = opcSubscription.AddItems(opcItems.ToArray());
                    
                    for (int i = 0; i < result.Length; i++)
                        opcItems[i].ServerHandle = result[i].ServerHandle;

                    opcSubscription.DataChanged += opcSubscription_DataChanged;

                    OnReportMessage("OPC tags subscription created successfully");
                }
                catch (Exception ex)
                {
                    OnReportMessage("OPC tags subscription failed");
                    OnReportMessage(ex.Message.ToString());
                }
            }
            else
            {
                OnReportMessage("Connect server first");
            }

            RefreshServerStatus();
        }

        public void opcSubscription_DataChanged(object subscriptionHandle, object requestHandle, Opc.Da.ItemValueResult[] values)
        {
            foreach (Opc.Da.ItemValueResult item in values)
            {
                
                monitoredTags.Last(t => t.Name.Equals(item.ItemName)).Value = item.Value.ToString();
                monitoredTags.Last(t => t.Name.Equals(item.ItemName)).Quality = item.Quality.ToString();
                
                
            }

            RefreshServerStatus();
        }


        public void UnSubcribe()
        {
               
            try
            {
                opcSubscription.DataChanged -= opcSubscription_DataChanged;
                server.CancelSubscription(opcSubscription);
                opcSubscription.Dispose();

                monitoredTags.Clear();
                OnReportMessage("OPC Subscription cleared");
            }
            catch (Exception ex)
            {
                OnReportMessage(ex.Message.ToString());
            }

            RefreshServerStatus();
        }


        /// <summary>
        /// Message handles
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public delegate void OPCserverEventHandler(object sender, exEventArgs args);

        public event OPCserverEventHandler ReportMessage;

        protected virtual void OnReportMessage(string message)
        {
            OPCserverEventHandler ReportMessageCopy = ReportMessage;

            if (ReportMessageCopy != null)
            {
                ReportMessageCopy(this, new exEventArgs(message));
                logMessage(message);
            }
        }

        protected virtual void OnReportError(int error, string message)
        {
            OPCserverEventHandler ReportMessageCopy = ReportMessage;

            if (ReportMessageCopy != null)
            {
                ReportMessageCopy(this, new exEventArgs(error, message));
                logMessage(message);
                logMessage("Error code = " + error.ToString());
            }
        }



    }
}
