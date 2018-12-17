﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;
using QNetwork.Http.Server;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using WPF_Server_Http.UIData;
using System.Web.Script.Serialization;
using QNetwork.Http.Server.Accept;
using QNetwork.Http.Server.Service;
using WPF_Server_Http.Define;
using static QNetwork.Http.Server.CQHttpServer;
using QNetwork.Http.Server.Cache;
using QNetwork;
using QNetwork.Http.Server.Log;

namespace WPF_Server_Http
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window, IQHttpServer_Log
    {
        CQHttpServer m_TestServer = new CQHttpServer();
        CQMainUI m_MainUI;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            


            //JavaScriptSerializer js = new JavaScriptSerializer();
            //string str = js.Serialize(new CQAA());
            if (this.m_MainUI == null)
            {
                this.DataContext = this.m_MainUI = new CQMainUI();
                // 取得本機名稱
                string strHostName = Dns.GetHostName();
                // 取得本機的IpHostEntry類別實體，用這個會提示已過時
                //IPHostEntry iphostentry = Dns.GetHostByName(strHostName);

                // 取得本機的IpHostEntry類別實體，MSDN建議新的用法
                IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);

                // 取得所有 IP 位址
                foreach (IPAddress ipaddress in iphostentry.AddressList)
                {
                    // 只取得IP V4的Address
                    if (ipaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        CQSocketListen_Address net_address = new CQSocketListen_Address() { IP = ipaddress.ToString(), Port = 3333 };
                        this.m_MainUI.AddressList.Add(new CQListenAddress() { Address = net_address });
                    }
                }
                if (this.m_MainUI.AddressList.Any(x => x.Address.ToEndPint().ToString() == "127.0.0.1") == false)
                {
                    CQSocketListen_Address net_address = new CQSocketListen_Address() { IP = "127.0.0.1", Port = 3333 };
                    this.m_MainUI.AddressList.Add(new CQListenAddress() { Address = net_address });
                }
                //this.m_TestServer.OnServiceChange += M_TestServer_OnServiceChange;
                //this.m_TestServer.OnHttpHandlerChange += M_TestServer_OnHttpHandlerChange;

                List<IQHttpService> services = new List<IQHttpService>();
                services.Add(new CQHttpService_Test());
                services.Add(new CQHttpService_Playback());
                services.Add(new CQHttpService_WebSocket());
                services.Add(new CQHttpService_ServerOperate());
                services.Add(new CQHttpService_WebMediaPlayer());
                services.Add(new CQHttpService_Test());
                this.m_TestServer.Logger = this;
                this.m_TestServer.Open(this.m_MainUI.AddressList.Select(x => x.Address).ToList(), services, true);
            }
        }

        private void button_add_listen_Click(object sender, RoutedEventArgs e)
        {
            CQSocketListen_Address ssd = new CQSocketListen_Address() { IP = this.m_MainUI.Listen_IP, Port = this.m_MainUI.Listen_Port };
            CQListenAddress address = new CQListenAddress() { Address = ssd };
            this.m_TestServer.OpenListen(ssd);
        }

        private void checkbox_listen_control_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            CQListenAddress address = checkbox.DataContext as CQListenAddress;
            if (checkbox != null)
            {
                if (address.IsOpen == true)
                {
                    this.m_TestServer.OpenListen(address.Address);
                }
                else
                {
                    this.m_TestServer.CloseListen(address.Address);

                }
            }
        }

        public bool LogProcess(LogStates_Process state, string handler_id, string process_id, DateTime time, CQHttpRequest request, CQHttpResponse response)
        {
            switch(state)
            {
                case LogStates_Process.CreateHandler:
                    {

                    }
                    break;
                case LogStates_Process.CreateRequest:
                    {

                    }
                    break;
                case LogStates_Process.ProcessRequest:
                    {

                    }
                    break;
                case LogStates_Process.CreateResponse:
                    {

                    }
                    break;
            }
            return true;
        }

        public bool LogAccept(LogStates_Accept state, string ip, int port, CQSocketListen obj)
        {
            if(state == LogStates_Accept.Create)
            {

            }
            else
            {

            }
            var vv = this.m_MainUI.AddressList.Where(x => x.Address == obj.Addrss);
            foreach (var oo in vv)
            {
                oo.ListenState = state;
            }
            return true;
        }

        public bool LogCache(LogStates_Cache state, DateTime time, string manager_id, string cache_id, string name)
        {
            switch(state)
            {
                case LogStates_Cache.CreateManager:
                    {
                        CQCache cache = new CQCache();
                        cache.ManagerID = manager_id;
                        cache.Name = name;
                        cache.Begin = time;
                        cache.End = DateTime.MaxValue;
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.m_MainUI.Caches.Add(cache);
                        }));
                    }
                    break;
                case LogStates_Cache.CreateCahce:
                    {
                        CQCache cache = new CQCache();
                        cache.ManagerID = manager_id;
                        cache.CacheID = cache_id;
                        cache.Name = name;
                        cache.Begin = time;
                        cache.End = DateTime.MaxValue;
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.m_MainUI.Caches.Add(cache);
                        }));
                    }
                    break;
                case LogStates_Cache.DestoryCache:
                    {
                        var vv = this.m_MainUI.Caches.Where(x => x.CacheID == cache_id);
                        foreach (var oo in vv)
                        {
                            oo.End = time;
                        }
                    }
                    break;
                case LogStates_Cache.DestoryManager:
                    {
                        var vv = this.m_MainUI.Caches.Where(x => x.ManagerID == manager_id);
                        foreach(var oo in vv)
                        {
                            oo.End = time;
                        }
                    }
                    break;
            }
            return true;
        }
    }

    
}
