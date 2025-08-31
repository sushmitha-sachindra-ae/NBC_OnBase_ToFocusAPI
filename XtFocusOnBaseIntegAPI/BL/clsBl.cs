using Newtonsoft.Json;
using System.Collections;
using System.Configuration;
using XtDevService.Interface;
using XtDevService.Model;
using XtDevService.Repository;
using static XtDevService.Repository.DBFunction;
using XtFocusOnBaseIntegAPI.Model;

namespace XtFocusOnBaseIntegAPI
{
    public class BL
    {
        static IDBMember Xlib = new DBFunction();
        #region
        public static int CreateMaster(string sName, string sCode, string masterName, string sessionId, string baseUrl)
        {
            int iMasterId = 0;
            try
            {
                Hashtable master = new Hashtable();
                master.Add("sname", sName);
                master.Add("sCode", sCode);
                PostingData postingData = new PostingData();
                postingData.data.Add(master);
                string sContent = JsonConvert.SerializeObject(postingData);
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("fSessionId", sessionId);
                string url = baseUrl + XtDevService.Model.FocusApiUrlExtension.urlMasters + "Core__" + masterName;
                Hashtable response = Xlib.PostToAPIAsync(sContent, headers, url, "application/json");
                FocusApiResponse.RootMaster obj = JsonConvert.DeserializeObject<FocusApiResponse.RootMaster>(response["Response"].ToString());
                if (obj.result == 1)
                {
                    iMasterId = obj.data[0].MasterId;
                }
            }
            catch (Exception e)
            {
                Xlib.ErrLog(e.Message + " Create Master " + "-->" + masterName);
            }
            return iMasterId;
        }
        #endregion
        public static string GetConnectionString(string compCode)
        {
            string constring = "";
            try
            {
                ServerDetail objSer = Xlib.GetServerDetailsOnly(compCode);
                constring = "server=" + objSer.ServerName + ";Database=" + objSer.DatabaseName + " ;Integrated Security=false;User ID=" + objSer.UserID + ";password=" + objSer.password;
            }
            catch (Exception ex)
            {

            }
            return constring;
        }
        #region


        public static async Task<LogingResult> Login(ClsLogin request,string baseFocusAPIUrl)
        {
            string filename = DateTime.Now.ToString("ddMMyyyy");
            //Logger.Instance.LogInfo(filename, "url " + HttpContext.Request.Host.Value + " /api/Inbound/Login");
           // string baseFocusAPIUrl = configuration.GetValue<string>("AppSettings:FocusAPI");
            LogingResult res = new LogingResult();
            try
            {
                List<Hashtable> datas = new List<Hashtable>();
                Hashtable data = new Hashtable { { "Username", request.username }, { "password", request.password }, { "CompanyCode", request.companycode } };
                datas.Add(data);
                Hashtable login = new Hashtable { { "data", datas }, { "result", "1" }, { "message", "" } };

                string sContent = JsonConvert.SerializeObject(login);
                Logger.Instance.LogInfo(filename, "Request Payload " + JsonConvert.SerializeObject(request));
                res = Xlib.GetSessionId(sContent, baseFocusAPIUrl);
                Logger.Instance.LogInfo(filename, "Response  " + JsonConvert.SerializeObject(res));

            }
            catch (Exception ex)
            {
                res.iStatus = 0;
                res.sMessage = ex.Message;
                Logger.Instance.LogInfo(filename, "Response  " + JsonConvert.SerializeObject(res));
                Logger.Instance.LogError(filename, ex.Message, ex);

            }
            return res;
        }
        #endregion

        #region LogOut

        public static async Task<loginRes> LogOut(string acesstoken,string baseFocusAPIUrl)
        {
            loginRes result = new loginRes();
            string filename = DateTime.Now.ToString("ddMMyyyy");
            try
            {
              
                Logger.Instance.LogInfo(filename, "calling Login API");
           
                Logger.Instance.LogInfo(filename, "session id " + acesstoken);
                result = Focus8API.GetLogOut(acesstoken, baseFocusAPIUrl);
            }
            catch (Exception ex)
            {
                result.message = ex.Message;
                result.result = 0;
                Logger.Instance.LogInfo(filename, "Response " + result);
                Logger.Instance.LogError(filename, ex.Message, ex);
            }
            return result;
        }

        #endregion
    }
}
