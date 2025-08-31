
using Newtonsoft.Json;
using System.Net;
using System.Text;
using XtDevService.Interface;
using XtDevService.Repository;
using static XtDevService.Repository.DBFunction;
namespace XtFocusOnBaseIntegAPI
{
    #region F8API
    public class Focus8API
    {
        static IDBMember Xlib = new DBFunction();
        public static string Post(string url, string data, string sessionId, ref string err)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Encoding = Encoding.UTF8;
                    webClient.Headers.Add("fSessionId", sessionId);
                    webClient.Headers.Add("Content-Type", "application/json");
                    webClient.Timeout = 300000;
                    string text = webClient.UploadString(url, data);
                    //devLibWeb.EventLog("response:" + Convert.ToString(text), "xdevlibApi");
                    return text;
                };
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return null;
            }
        }

       

        public static string GetApi(string url, string sessionId, ref string err)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Encoding = Encoding.UTF8;
                    webClient.Headers.Add("fSessionId", sessionId);
                    webClient.Timeout = 300000;
                    return webClient.DownloadString(url);
                }

            }
            catch (Exception ex)
            {
                err = ex.Message;
                return null;
            }
        }

        public static loginRes Login(string sUrl, string sContent, ref string err)
        {
            bool flag = false;
            loginRes result = new loginRes();

            try
            {


                string text = sUrl + "/Login";


                using (WebClient webClient = new WebClient())
                {
                    webClient.Encoding = Encoding.UTF8;
                    webClient.Headers.Add("Content-Type", "application/json");
                    webClient.Timeout = 300000;
                    string text2 = webClient.UploadString(text, sContent);

                    result = JsonConvert.DeserializeObject<loginRes>(text2);
                    return result;
                }

            }
            catch (Exception ex)
            {
                result.message = ex.Message;

            }

            return result;
        }

        public static loginRes GetLogOut(string AccessToken, string sUrl)
        {
            loginRes result = new loginRes();

            try
            {
                string address = sUrl + "/Logout";
                using (WebClient webClient = new WebClient())
                {
                    webClient.Timeout = 300000;
                    webClient.Headers.Add("fSessionId", AccessToken);
                    webClient.Headers.Add("Content-Type", "application/json");
                    string value = webClient.DownloadString(address);
                    result = JsonConvert.DeserializeObject<loginRes>(value);

                    return result;
                }

            }
            catch (Exception ex)
            {
                result.message = ex.Message;

            }

            return result;
        }

        public class WebClientDel : System.Net.WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                return base.GetWebRequest(uri);
            }
        }
        public class WebClient : System.Net.WebClient
        {
            public int Timeout { get; set; }

            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest webRequest = base.GetWebRequest(uri);
                webRequest.Timeout = Timeout;
                ((HttpWebRequest)webRequest).ReadWriteTimeout = Timeout;
                return webRequest;
            }
        }
    }
#endregion
}
