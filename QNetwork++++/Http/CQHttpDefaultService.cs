﻿using QNetwork.Http.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace QNetwork.Http.Server
{
    public class CQHttpDefaultService : CQHttpService
    {
        int m_IconIndex = 0;
        List<string> m_Icons = new List<string>();
        public CQHttpDefaultService()
        {
            string str = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string[] files = Directory.GetFiles(str, "*.ico");
            this.m_Icons.AddRange(files);
        }

        void CoppyStream(Stream src, Stream dst)
        {
            byte[] buf = new byte[8192];
            while(src.Position < src.Length)
            {
                int read_len = src.Read(buf, 0, buf.Length);
                dst.Write(buf, 0, read_len);
            }
        }
        object m_IconIndexLock = new object();
        override public bool Process(CQHttpRequest req, out CQHttpResponse resp, out int process_result_code)
        {

            process_result_code = 1;
            resp = new CQHttpResponse(req.HandlerID);
            switch (req.ResourcePath)
            {
                case "/PostTest":
                    {
                        resp.Content = new MemoryStream();
                        req.Content.Position = 0;
                        this.CoppyStream(req.Content, resp.Content);
                        resp.Content.Position = 0;
                        resp.ContentLength = req.Content.Length;
                        resp.ContentType = req.Headers["CONTENT-TYPE"];
                        resp.Connection = Connections.Close;
                    }
                    break;
                case "/favicon.ico":
                    {
                        if (this.m_Icons.Count() > 0)
                        {
                            resp.Content = new FileStream(this.m_Icons[this.m_IconIndex++], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            Monitor.Enter(this.m_IconIndexLock);
                            this.m_IconIndex = this.m_IconIndex + 1;
                            if (this.m_IconIndex >= this.m_Icons.Count)
                            {
                                this.m_IconIndex = 0;
                            }
                            Monitor.Exit(this.m_IconIndexLock);

                            resp.Content.Position = 0;
                            resp.ContentLength = resp.Content.Length;
                            resp.ContentType = "image/x-icon";
                            resp.Connection = Connections.Close;
                        }
                        else
                        {
                            resp.Content = null;
                            resp.ContentLength = 0;
                            resp.ContentType = "";
                            resp.Connection = Connections.Close;
                        }

                    }
                    break;
                case "/Test":
                    {
                        resp.Content = new FileStream("test.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        resp.Content.Position = 0;
                        resp.ContentLength = resp.Content.Length;
                        resp.ContentType = "text/plain";
                        resp.Connection = Connections.Close;
                    }
                    break;
                default:
                    {
                        string content_str = string.Format("Hello world\r\n{0}\r\nQEmbedded Http server\r\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo));
                        resp.Content = new MemoryStream(Encoding.UTF8.GetBytes(content_str));
                        resp.ContentLength = resp.Content.Length;
                        resp.ContentType = "text/plain";
                        resp.Connection = Connections.KeepAlive;
                    }
                    break;
            }
            resp.Set200();

            return true;
        }

        public override bool TimeOut_Cache()
        {
            return base.TimeOut_Cache();
            //bool result = true;
            //Monitor.Enter(this.m_CachesLock);
            //try
            //{
            //    Monitor.Enter(this.m_CachesLock);

            //}
            //catch (Exception ee)
            //{
            //    System.Diagnostics.Trace.WriteLine(ee.Message);
            //    System.Diagnostics.Trace.WriteLine(ee.StackTrace);
            //    result = false;
            //}
            //finally
            //{
            //    Monitor.Exit(this.m_CachesLock);
            //}
            //Monitor.Exit(this.m_CachesLock);
            //return result;
        }

        public override bool CloseHandler(List<string> handlers)
        {
            return true;
        }
    }
}
