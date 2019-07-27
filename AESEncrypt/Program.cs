using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using System.Data;
using System.Collections;

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
            tag:
            CookieCollection ccc = new CookieCollection();
            foreach (Cookie c in res.Cookies) ccc.Add(c);
            try
            {
                HttpWebResponse nres = Requests.Post("http://ids.chd.edu.cn/authserver/login?service=http%3A%2F%2Fportal.chd.edu.cn%2F", Data: data, Cookieslst: ccc);
                //Console.WriteLine($"状态码: {(int)nres.StatusCode} \n状态值: {nres.StatusCode}");
                int redir = 0;
                while ((int)nres.StatusCode == 302)
                {
                    foreach (Cookie c in nres.Cookies) ccc.Add(c);
                    nres = Requests.Post(nres.Headers["Location"], Data: data, Cookieslst: ccc);
                    redir++;
                    if (redir > 10) throw new InvalidOperationException("重定向次数太多了");
                }
                foreach (Cookie c in nres.Cookies) ccc.Add(c);
                HttpWebResponse grade = Requests.Get("http://bkjw.chd.edu.cn/eams/teach/grade/course/person!historyCourseGrade.action?projectType=MAJOR", Cookieslst: ccc, AllowRedirect: true);


                string result = Requests.GetResponseText(grade);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(result);
                var courselst = document.DocumentNode.SelectNodes("//tbody[@id]/tr");
                DataTable dt = new DataTable();
                Dictionary<string, string> tableHead1 = new Dictionary<string, string>
            {
                { "学年学期", "System.String" },
                { "课程代码", "System.String" },
                { "课程序号", "System.String" },
                { "课程名称", "System.String" },
                { "课程情况", "System.String" },
                { "课程类别", "System.String" },
                { "学分", "System.Int32" },
                { "期中成绩", "System.String" },
                { "期末成绩", "System.String" },
                { "平时成绩", "System.String" },
                { "总评成绩", "System.String" },
                { "实验成绩", "System.String" },
                { "最终成绩", "System.String" },
                { "绩点", "System.Double" }
            };
                foreach (var item in tableHead1) dt.Columns.Add(item.Key, Type.GetType(item.Value));


                foreach (HtmlNode node in courselst)
                {
                    var course = node.SelectNodes("./td");
                    ArrayList al = new ArrayList();
                    object[] oal = new object[14];
                    oal[0] = course[0].InnerText;
                    oal[1] = course[1].InnerText;
                    oal[2] = course[2].InnerText;
                    oal[3] = FormatCourse(course[3].InnerText);
                    if (course[3].SelectNodes("./span") != null)
                    {
                        oal[4] = course[3].SelectNodes("./span")[0].InnerText.Replace("(", "").Replace(")", "");
                    }
                    else oal[4] = "正常";
                    oal[5] = course[4].InnerText;
                    oal[6] = Convert.ToDouble(FormatCourse(course[5].InnerText));
                    for (int i = 7; i < 12; i++)
                    {
                        oal[i] = FormatCourse(course[i - 1].InnerText);
                    }
                    oal[13] = Convert.ToDouble(FormatCourse(course[12].InnerText));
                    foreach(var ttt in oal)
                    {
                        Console.WriteLine(ttt);
                    }
                    /*
                    for (int i = 0; i < course.Count; i++)
                    {
                        //Console.WriteLine($"course[{i}].innerText = { course[i].InnerText}");
                        //Console.WriteLine($"course[{i}].innerHtml = { course[i].InnerHtml}");

                        if (i == 3)
                        {
                            al.Add(FormatCourse(course[i].InnerText));
                            //Console.WriteLine($"course[{i}].innerText = { course[i].InnerText}");
                            if (course[i].ChildNodes.Count > 1) al.Add(course[i].SelectSingleNode("./span").InnerText.Replace("(", "").Replace(")", ""));
                            else al.Add("正常");
                        }
                        else if (i > 4)
                        {
                            if (FormatCourse(course[i].InnerText) == null || FormatCourse(course[i].InnerText) == "") al.Add("N/A");
                            if (i == 5 || i == 12)
                            {
                                if (i == 5) al.Add(Convert.ToDouble(FormatCourse(course[i].InnerText)));
                                var l = Convert.ToDouble(FormatCourse(course[i].InnerText));
                                Console.WriteLine($"course[{i}].D = { l }");
                            }
                            else al.Add(FormatCourse(course[i].InnerText));
                            string e = FormatCourse(course[i].InnerText);
                            Console.WriteLine($"C course[{i}].innerText = { e }");

                        }
                        //else if (i ==)
                        else {
                            al.Add(course[i].InnerHtml);
                            
                        }
                    }
                    for (int i = 0; i < 14; i++) Console.WriteLine($"al[{i}] = {al[i]}");
                    //object[] oal = new object[14];
                    for (int i = 0; i < 14; i++) oal[i] = al[i];*/

                    /*foreach (HtmlNode n2 in course)
                    {
                        Console.WriteLine(n2.InnerHtml);
                    }*/
                    dt.Rows.Add(oal);
                }
            }
            catch (InvalidOperationException er)
            {
                Console.WriteLine(er.ToString());
                Console.WriteLine("按任意键重启...");
                Console.ReadKey();
                goto tag;
            }
            //Console.WriteLine(result);
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
        public static string FormatCourse(string str)
        {
            return str.Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\t", "");
        }
    }




}
