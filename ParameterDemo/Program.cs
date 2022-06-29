using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Web;
using Authing.ApiClient.Domain.Client.Impl.AuthenticationClient;
using Authing.ApiClient.Types;
using Newtonsoft.Json;

namespace ParameterDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var s in args)
            {
                Console.WriteLine(s);
            }

            AuthenticationClient client = new AuthenticationClient((o) =>
            {
                o.AppId = "61c2d04b36324259776af784";
                o.Secret = "879fdbe5408cfe09d6f69755b77df2fd";
            });
            var url = client.BuildAuthorizeUrl(new OidcOption()
            {
                RedirectUri = @"http://localhost:54321/?start=D:\Repo\ServiceDemo\ParameterDemo\bin\Debug\ParameterDemo.exe",
                Scope = "openid username profile email phone address offline_access"
            });
            //var user = client.GetUserInfoByAccessToken("ztBT7m3B5OS-7ZFB3-JReGPrHoxe6aiP2ph8eNclM7d").Result;
            if (args.Length == 0)
            {
                Console.WriteLine("参数未传递");
                Console.ReadLine();
                return;
            }
            var res = GetParam(args?[0]);
            Console.WriteLine(res["code"]);
            var tokenRes = client.GetAccessTokenByCode(res["code"]).Result;
            var user = client.GetUserInfoByAccessToken(tokenRes.AccessToken).Result;
            client.AccessToken = tokenRes.AccessToken;
            var res1 = client.CheckLoggedIn();
            Console.WriteLine(res1);
            Console.WriteLine(JsonConvert.SerializeObject(user));
            Console.WriteLine(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName);
            Console.WriteLine(System.Environment.CurrentDirectory);
            Console.WriteLine(System.AppDomain.CurrentDomain.BaseDirectory);
            Console.WriteLine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
            Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
            //Console.WriteLine(user?.Email);
            Console.ReadLine();
        }

        public static Dictionary<string, string> GetParam(string url)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            int start = 0, end = 0;
            var resstring = HttpUtility.UrlDecode(url);
            start = resstring.IndexOf("?");
            resstring = resstring.Substring(start + 1);
            start = 0;
            while (end != -1)
            {
                end = resstring.IndexOf("&", start, StringComparison.Ordinal);
                if (end != -1)
                {
                    var temp = resstring.Substring(start, end - start).Split('=');
                    res.Add(temp?[0], temp?[1]);
                }
                else
                {
                    var temp = resstring.Substring(start).Split('=');
                    if (temp.Length == 1) break;
                    res.Add(temp?[0], temp?[1]);
                }
                start = end + 1;
            }

            return res;
        }


    }
}
