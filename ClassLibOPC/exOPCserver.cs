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
        private Opc.Da.Subscription opcSubscription;
        private Opc.Da.ServerStatus serverStatus;
              
        
        public ObservableCollection<mServerItem> servers_list;
        public string hostname;
        public bool IsConnected;
        public ObservableCollection<mTag> MonitoredTags;

        public mServerItem SelectedServer;

        private Timer reconnectTimer;


        public exOPCserver ()
        {
            servers_list = new ObservableCollection<mServerItem>();
            IsConnected = false;
            MonitoredTags = new ObservableCollection<mTag>();
            opcSubscription = null;

            configureWatchDog();
        }


        public ObservableCollection<mServerItem> GetServers(string hostname)
        {
            OpcCom.ServerEnumerator discovery = new OpcCom.ServerEnumerator();
            
            //Get all local OPC DA servers of version 2.0
            Opc.Server[] localservers = discovery.GetAvailableServers(Opc.Specification.COM_DA_20, hostname, null);


            foreach ( Opc.Server srv in localservers)
            {
                mServerItem si = new mServerItem();

                si.Name = srv.Name;
                si.Description = srv.Locale;
                si.UrlString = srv.Url.ToString();

                servers_list.Add(si);
            }

            //Get all OPC DA servers of version 2.0 of machine "MyMachine"
            //Opc.Server[] hostservers = discovery.GetAvailableServers(Opc.Specification.COM_DA_20, "MNS1-179N", null);
 
            return servers_list;
        }


        /// <summary>
        /// Refresh server status
        /// </summary>
        private void RefreshServerStatus()
        {
            //if (SelectedServer==null)
            //{
            //    SelectedServer = new mServerItem();
            //}

            SelectedServer = SelectedServer ?? new mServerItem();

            serverStatus = server.GetStatus();
            SelectedServer.IsConnected = server.IsConnected;
            SelectedServer.StatusInfo = serverStatus.StatusInfo;
            SelectedServer.ServerState = serverStatus.ServerState.ToString();
            SelectedServer.ProductVersion = serverStatus.ProductVersion;
            SelectedServer.VendorInfo = serverStatus.VendorInfo;
        }

        /// <summary>
        /// Server connection procedure
        /// </summary>
        /// <param name="urlString">OPC server URL in string format</param>
        /// <returns>IsConnected</returns>
        public bool ConnectServer(string urlString)
        {
            IsConnected = false;

            this.url = new Opc.URL(urlString);
            

            if (server == null)
            {               
                server = new Opc.Da.Server(new OpcCom.Factory(), url);
                
            }

            if (!server.IsConnected)
            {
                try
                {

                    server.Connect();
                    RefreshServerStatus();

                    if (IsConnected = server.IsConnected)
                    {
                        OnReportMessage("Server is connected");
                        server.ServerShutdown -= server_ServerShutdown;
                        server.ServerShutdown += server_ServerShutdown;
                    }
                    else
                    {
                        OnReportMessage("Server is not connected, unknown error");
                    }
                }
                catch (Exception ex)
                {
                    OnReportMessage(ex.Message.ToString());
                }
            }

            return IsConnected;
        }

        private void server_ServerShutdown(string reason)
        {
            //OnReportMessage("Server shutdown, reason: " + reason);
            //RefreshServerStatus();
            reconnectTimer.Enabled = true;

        }


        private void configureWatchDog()
        {
            reconnectTimer = new System.Timers.Timer();
            reconnectTimer.Elapsed += reconnectTimer_Elapsed;
            reconnectTimer.Interval = 5000;
            reconnectTimer.AutoReset = false;


        }

        private void startStopWatchDog(bool startstop)
        {
            reconnectTimer.Enabled = startstop;
        }

        private void reconnectTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //OnReportMessage("Server down, trying to reconnect..");
            e.SignalTime.ToString();
            ConnectServer(server.Url.ToString());
            

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

                    server.Disconnect();
                    IsConnected = server.IsConnected;
                    SelectedServer.IsConnected = server.IsConnected;

                    OnReportMessage("Server is disconnected");

                    server.ServerShutdown -= server_ServerShutdown;
                    server.Dispose();
                }
                else
                {
                    OnReportMessage("Server is already disconnected");
                }

                OnReportMessage("No connected server available");
            }
            return IsConnected;
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

            if (IsConnected)
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
                    var contain = MonitoredTags.Any(t => t.Name == tag.Name);

                    if (!contain)
                    {

                        // Repeat this next part for all the items you need to subscribe
                        Opc.Da.Item item = new Opc.Da.Item();
                        item.ItemName = tag.Name;
                        //item.ClientHandle = "handle"; // handle is up to you, but i use a logical name for it
                        item.Active = true;
                        item.ActiveSpecified = true;
                        opcItems.Add(item);

                        MonitoredTags.Add(tag);
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
                
                MonitoredTags.Last(t => t.Name.Equals(item.ItemName)).Value = item.Value.ToString();
                MonitoredTags.Last(t => t.Name.Equals(item.ItemName)).Quality = item.Quality.ToString();
                
                
            }

            RefreshServerStatus();
        }


        public void UnSubcribe()
        {
            opcSubscription.DataChanged -= opcSubscription_DataChanged;
            try
            {
                server.CancelSubscription(opcSubscription);
                opcSubscription.Dispose();

                MonitoredTags.Clear();

                OnReportMessage("OPC Subscription cleared");
            }
            catch (Exception ex)
            {
                OnReportMessage(ex.Message.ToString());
            }

            RefreshServerStatus();
        }

        //--- Message handler
        public delegate void OPCserverEventHandler(object sender, exEventArgs args);

        public event OPCserverEventHandler ReportMessage;

        protected virtual void OnReportMessage(string message)
        {
            if (ReportMessage != null)
            {
                ReportMessage(this, new exEventArgs(message));
            }
        }



    }
}
