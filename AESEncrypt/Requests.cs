using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;


namespace AESEncrypt
{
    class Requests
    {
        /// <summary>
        /// Get函数
        /// </summary>
        /// <param name="Url">地址</param>
        /// <param name="Headers">请求头(可选)</param>
        /// <param name="Proxy">代理(可选) 格式"http://proxyserver:80/"</param>
        /// <param name="Cookieslst">Cookies列表(可选)</param>
        /// <param name="AllowRedirect">是否允许重定向(默认不允许)</param>
        /// <returns>服务器响应: 返回HttpWebResponse对象</returns>
        public static HttpWebResponse Get(string Url, WebHeaderCollection Headers = null, string Proxy = null, CookieCollection Cookieslst = null, bool AllowRedirect = false)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            req.Method = "GET";
            req.CookieContainer = new CookieContainer();
            req.ContentType = "application/x-www-form-urlencoded";
            req.Headers.Add(HttpRequestHeader.AcceptEncoding, "identity");

            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.157 Safari/537.36";
            if (Cookieslst != null) foreach(Cookie co in Cookieslst) req.CookieContainer.Add(co);
            if (Headers != null) req.Headers = Headers;
            if (Proxy != null)
            {
                //格式"http://proxyserver:80/"
                req.Proxy = new WebProxy(Proxy, true);
            }
            
            req.AllowAutoRedirect = AllowRedirect;
            
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            return resp;
        }

        //string html = new StreamReader(resp.GetResponseStream()).ReadToEnd();

        /// <summary>
        /// SetHeaderValue设置请求头
        /// </summary>
        /// <param name="header">要添加请求头的HeaderCollection对象</param>
        /// <param name="name">要添加的一个key</param>
        /// <param name="value">要添加的一个值</param>
        public static void SetHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection", BindingFlags.Instance | BindingFlags.NonPublic);
            if (property != null)
            {
                var collection = property.GetValue(header, null) as NameValueCollection;
                collection[name] = value;
            }
        }

        /// <summary>
        /// Post函数
        /// </summary>
        /// <param name="Url">地址</param>
        /// <param name="Data">数据 </param>
        /// <param name="Headers">请求头</param>
        /// <param name="Cookieslst">Cookies列表(可选)</param>
        /// <param name="Proxy">代理</param>
        /// <param name="AllowRedirect">是否允许重定向</param>
        /// <returns>响应 HttpWebResponse对象</returns>
        public static HttpWebResponse Post(string Url, string Data = null,WebHeaderCollection Headers = null, CookieCollection Cookieslst = null, string Proxy = null, bool AllowRedirect = false)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            req.Method = "POST";
            req.Accept = "*/*";
            //req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            req.ContentType = "application/x-www-form-urlencoded";
            //req.ServicePoint.Expect100Continue = false;
            //req.ServicePoint.UseNagleAlgorithm = false;//禁止Nagle算法加快载入速度
            //if (!string.IsNullOrEmpty(options.XHRParams)) { req.AllowWriteStreamBuffering = true; } else { request.AllowWriteStreamBuffering = false; }; //禁止缓冲加快载入速度
            req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");//定义gzip压缩页面支持
            //req.Connection = "keep-alive";
            req.KeepAlive = true;
            req.CookieContainer = new CookieContainer();
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.157 Safari/537.36";
            if (Cookieslst != null) foreach (Cookie co in Cookieslst) req.CookieContainer.Add(co);
            if (Data != null)
            {
                //req.ContentType = "application/json";
                //string datastr = JsonConvert.SerializeObject(Data);
                
                byte[] postBytes = Encoding.UTF8.GetBytes(Data);
                req.ContentLength = postBytes.Length;
                Stream postDataStream = req.GetRequestStream();
                postDataStream.Write(postBytes, 0, postBytes.Length);
                postDataStream.Close();
            }
            if (Headers != null) req.Headers = Headers;
            if (Proxy != null)
            {
                //"http://proxyserver:80/"
                req.Proxy = new WebProxy(Proxy, true);
            }
            req.AllowAutoRedirect = AllowRedirect;
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            return resp;
        }

        /// <summary>
        /// 获取响应文本
        /// </summary>
        /// <param name="response">响应</param>
        /// <returns>响应文本</returns>
        public static string GetResponseText(HttpWebResponse response)
        {
            string result = null;
            if (response.ContentEncoding.ToLower().Contains("gzip"))//解压
            {
                using (GZipStream stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            else if (response.ContentEncoding.ToLower().Contains("deflate"))//解压
            {
                using (DeflateStream stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            else
            {
                using (Stream stream = response.GetResponseStream())//原始
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            return result;
        }
    }
    
}
