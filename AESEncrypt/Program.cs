using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace AESEncrypt
{
    class Program
    {
        static void Main(string[] args)
        {

            HttpWebResponse res = Requests.Get("http://ids.chd.edu.cn/authserver/login?service=http%3A%2F%2Fportal.chd.edu.cn%2F");
            string content = Requests.GetResponseText(res);
            Regex regkey = new Regex("var\\s*?pwdDefaultEncryptSalt\\s*?=\\s*?\"(.*?)\"");
            Regex reglt = new Regex("name\\s*?=\\s*?\"lt\"\\s*?value\\s*?=\\s*?\"(.*?)\"");
            Regex regdllt = new Regex("name\\s*?=\\s*?\"dllt\"\\s*?value\\s*?=\\s*?\"(.*?)\"");
            Regex regexecution = new Regex("name\\s*?=\\s*?\"execution\"\\s*?value\\s*?=\\s*?\"(.*?)\"");
            Regex reg_eventId = new Regex("name\\s*?=\\s*?\"_eventId\"\\s*?value\\s*?=\\s*?\"(.*?)\"");
            Regex regrmShown = new Regex("name\\s*?=\\s*?\"rmShown\"\\s*?value\\s*?=\\s*?\"(.*?)\"");
            Match m1 = regkey.Match(content);
            Match m2 = reglt.Match(content);
            Match m3 = regdllt.Match(content);
            Match m4 = regexecution.Match(content);
            Match m5 = reg_eventId.Match(content);
            Match m6 = regrmShown.Match(content);

            string key = m1.Groups[0].Value.Split('"')[1];
            string lt = m2.Groups[0].Value.Split('"')[3];
            string dllt = m3.Groups[0].Value.Split('"')[3];
            string execution = m4.Groups[0].Value.Split('"')[3];
            string _eventId = m5.Groups[0].Value.Split('"')[3];
            string rmShown = m6.Groups[0].Value.Split('"')[3];

            Console.Write("输入用户名: ");
            string uname = Console.ReadLine();
            Console.Write("输入密码: ");
            string code = Console.ReadLine();
            string encrypted = Convert.ToBase64String(AESEncrypt.Encrypt(GetRandomString(64) + code, AESEncrypt.StringToByteArray(key, 16), AESEncrypt.StringToByteArray(GetRandomString(16), 16)));

            string data = "username=" + uname;
            data += "&password=" + HttpUtility.UrlEncode(encrypted);
            data += "&lt=" + lt;
            data += "&dllt=" + dllt;
            data += "&execution=" + execution;
            data += "&_eventId=" + _eventId;
            data += "&rmShown=" + rmShown;
            CookieCollection ccc = new CookieCollection();
            foreach (Cookie c in res.Cookies)
            {
                ccc.Add(c);
            }
            HttpWebResponse nres = Requests.Post("http://ids.chd.edu.cn/authserver/login?service=http%3A%2F%2Fportal.chd.edu.cn%2F", Data: data, Cookieslst: ccc);
            //Console.WriteLine($"状态码: {(int)nres.StatusCode} \n状态值: {nres.StatusCode}");
            while ((int)nres.StatusCode == 302)
            {
                foreach(Cookie c in nres.Cookies)
                {
                    ccc.Add(c);
                }
                nres = Requests.Post(nres.Headers["Location"], Data: data, Cookieslst: ccc);
            }
            foreach (Cookie c in nres.Cookies)
            {
                ccc.Add(c);
            }
            HttpWebResponse grade = Requests.Get("http://bkjw.chd.edu.cn/eams/teach/grade/course/person!historyCourseGrade.action?projectType=MAJOR", Cookieslst: ccc,AllowRedirect: true);

            //string fff = new StreamReader(grade.GetResponseStream()).ReadToEnd();
            //Console.WriteLine(fff);
            Console.WriteLine(Requests.GetResponseText(grade));
            Console.ReadKey();
            
        }
        public static string GetRandomString(int Len)
        {
            string Dict = "ABCDEFGHJKMNPQRSTWXYZabcdefhijkmnprstwxyz2345678";
            double DictNum = Dict.Length;
            string Result = "";
            Random random = new Random(Guid.NewGuid().GetHashCode());
            //Console.WriteLine(random.Next());
            for (int i = 0; i < Len; i++)
            {
                Result += Dict.ElementAt((int)Math.Floor(random.NextDouble() * DictNum));
                //retStr += $aes_chars.charAt(Math.floor(Math.random() * aes_chars_len));
            }
            return Result;

        }
    }

    
    

}
