using Microsoft.AspNetCore.Mvc;
using XtDevService.Model;
using XtDevService.Interface;
using XtDevService.Repository;

using XtFocusOnBaseIntegAPI.Model;
using static XtDevService.Repository.DBFunction;
using Microsoft.AspNetCore.Identity.Data;
using System.Collections;
using Newtonsoft.Json;
using System.Reflection;
using static XtFocusOnBaseIntegAPI.Model.PO;
using Swashbuckle.AspNetCore.Filters;

using static XtFocusOnBaseIntegAPI.Model.Account;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using log4net.Repository.Hierarchy;
using System.Reflection.Emit;

namespace XtFocusOnBaseIntegAPI.Controllers
{
    [ApiController]
    public class InboundController : ControllerBase
    {
        IDBMember Xlib = new DBFunction();
        Focus8API F8API = new Focus8API();
    
        private IConfiguration configuration { get; set; }

        public InboundController(IConfiguration iConfig)
        {

            this.configuration = iConfig;
        }
     
        #region Account master
        [HttpPost]
        [Route("api/Inbound/Account")]
        [SwaggerRequestExample(typeof(Account.Request),typeof(AccountExample))]
        public async Task<IActionResult> Account(
            [FromHeader(Name = "X-Api-UserName")] string username, [FromHeader(Name = "X-Api-Password")] string password,
            [FromHeader(Name ="CompCode")] string compCode,
            [FromBody] Account.Request request)
        {
            FocusApiResponse.RootMaster obj = new FocusApiResponse.RootMaster();
            List<FocusApiResponse.RootMaster> objList = new List<FocusApiResponse.RootMaster>();
            string filename = compCode + "_" + DateTime.Now.ToString("ddMMyyyy");
            string baseFocusAPIUrl = configuration.GetValue<string>("AppSettings:FocusAPI");
            Logger.Instance.LogInfo(filename, "calling Account API");
            //get token
            LogingResult objLogin =await BL.Login(new ClsLogin { username = username, password = password, companycode = compCode }, baseFocusAPIUrl);
            if (objLogin != null)
            {
                if(!string.IsNullOrEmpty( objLogin.Auth_Token))
                {
                    try
                    {
                        Logger.Instance.LogInfo(filename, "session Id created,Authorized");

                        if (!string.IsNullOrEmpty(compCode))
                        {
                            string connString = BL.GetConnectionString(compCode);

                         
                            Logger.Instance.LogInfo(filename, "Request payload : " + JsonConvert.SerializeObject(request));
                            Logger.Instance.LogInfo(filename, "url " + HttpContext.Request.Host.Value + " /api/Inbound/Account");
                            foreach (Account.RequestData item in request.data)
                            {
                                int iParentId = 0;
                                 obj = new FocusApiResponse.RootMaster();
                                try
                                {

                                    //check if account already exist
                                    int iAccountId = int.Parse(await Xlib.GetMasterData(1, "iMasterId", "scode='" + item.sCode + "' or sname='" + item.sCode + "'", "", connString));
                                    if (iAccountId > 0)
                                    {
                                        FocusApiResponse.DataMaster data = new FocusApiResponse.DataMaster();
                                        data.MasterId = iAccountId;

                                        obj.url = baseFocusAPIUrl + FocusApiUrlExtension.urlMasters + "Core__Account";
                                        obj.result = 1;
                                        obj.message = "Account already exist";
                                        obj.data = new List<FocusApiResponse.DataMaster> { data };
                                        objList.Add(obj);
                                       
                                        Logger.Instance.LogInfo(filename, "Response : " + JsonConvert.SerializeObject(obj));
                                    }

                                    else
                                    {
                                        Hashtable master = new Hashtable();

                                        foreach (PropertyInfo prop in item.GetType().GetProperties().Where(s => !s.Name.Contains("__Mstr")))
                                        {
                                            master.Add(prop.Name, item.GetType().GetProperty(prop.Name).GetValue(item, null));

                                        }
                                        foreach (PropertyInfo prop in item.GetType().GetProperties().Where(s => s.Name.Contains("__Mstr")))
                                        {
                                            string mstrName = prop.Name.Split('_')[0].Trim();
                                            string sCode = item.GetType().GetProperty(prop.Name).GetValue(item, null).ToString();
                                            int iMastertypid = int.Parse(Xlib.GetMasterTypeId(mstrName, "", connString));
                                            int iMasterId = int.Parse(await Xlib.GetMasterData(iMastertypid, "iMasterId", "sCode='" + sCode + "'", "", connString));
                                            //set master id for all master fields other than group
                                            if (mstrName != "AccountGrp")
                                            {
                                                //if (iMasterId == 0)
                                                //{
                                                //    iMasterId = BL.CreateMaster(sCode, sCode, mstrName, sessionId, baseFocusAPIUrl);
                                                //}
                                                if (mstrName == "City")
                                                {
                                                    master.Add("iCity__Code", item.GetType().GetProperty(prop.Name).GetValue(item, null));
                                                    master.Add("iCity__Id", iMasterId);
                                                }
                                                else
                                                {
                                                    master.Add(mstrName + "__Code", item.GetType().GetProperty(prop.Name).GetValue(item, null));
                                                    master.Add(mstrName + "__Id", iMasterId);
                                                }
                                            }

                                        }
                                        foreach (PropertyInfo prop in item.GetType().GetProperties().Where(s => s.Name.Contains("__Grp")))
                                        {
                                            string mstrName = prop.Name.Split('_')[0].Trim();
                                            string sCode = item.GetType().GetProperty(prop.Name).GetValue(item, null).ToString();
                                            int iMastertypid = int.Parse(Xlib.GetMasterTypeId(mstrName, "", connString));
                                            int iMasterId = int.Parse(await Xlib.GetMasterData(iMastertypid, "iMasterId", "sCode='" + sCode + "'", "", connString));
                                            iParentId = iMasterId;
                                            master.Add("ParentId", iMasterId);
                                        }
                                        string url = baseFocusAPIUrl + FocusApiUrlExtension.urlMasters + "Core__Account";

                                        PostingData postingData = new PostingData();
                                        postingData.data.Add(master);
                                        string sContent = JsonConvert.SerializeObject(postingData);
                                        Dictionary<string, string> headers = new Dictionary<string, string>();
                                        headers.Add("fSessionId", objLogin.Auth_Token);
                                        Logger.Instance.LogInfo(filename, "F8 Payload : " + sContent);
                                        Logger.Instance.LogInfo(filename, "fSessionId : " + objLogin.Auth_Token);
                                        Hashtable response = Xlib.PostToAPIAsync(sContent, headers, url, "application/json");
                                        obj = JsonConvert.DeserializeObject<FocusApiResponse.RootMaster>(response["Response"].ToString());
                                        objList.Add(obj);
                                      
                                        Logger.Instance.LogInfo(filename, "Response : " + JsonConvert.SerializeObject(obj));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    obj.message = ex.Message;
                                    obj.result = -1;
                                    objList.Add(obj);
                                    Logger.Instance.LogError(filename, ex.ToString(), ex);
                                    Logger.Instance.LogInfo(filename, "Response : " + JsonConvert.SerializeObject(obj));

                                    return Ok(obj);
                                }
                               

                            }
                        }
                    }
                    catch
                    {

                    }
                    finally
                    {
                       
                        await BL.LogOut(objLogin.Auth_Token,baseFocusAPIUrl);
                        Logger.Instance.LogInfo(filename, "LoggedOut from  " + objLogin.Auth_Token);
                    }

                 }
                else
                {
                    Logger.Instance.LogInfo(filename, "Invalid Username,Password,companyCode passed");
                    return StatusCode(401, "Unauthorized access.");



                }
            }
            else
            {

                Logger.Instance.LogInfo(filename, "Invalid Username,Password,companyCode passed");
                return Unauthorized();
            }
            return Ok(obj);
        }
        #endregion
        #region Item master
        [HttpPost]
        [Route("api/Inbound/Item")]
        [SwaggerRequestExample(typeof(Item.Request), typeof(ItemExample))]
        public async Task<IActionResult> Item([FromHeader(Name = "X-Api-UserName")] string username, [FromHeader(Name = "X-Api-Password")] string password,
            [FromHeader(Name = "CompCode")] string compCode, [FromBody] Item.Request request)
        {
            FocusApiResponse.RootMaster obj = new FocusApiResponse.RootMaster();
            List<FocusApiResponse.RootMaster> objList = new List<FocusApiResponse.RootMaster>();           
            string filename= compCode+"_"+DateTime.Now.ToString("ddMMyyyy");
            Logger.Instance.LogInfo(filename, "calling Item API");
            //get token
            string baseFocusAPIUrl = configuration.GetValue<string>("AppSettings:FocusAPI");
            LogingResult objLogin = await BL.Login(new ClsLogin { username = username, password = password, companycode = compCode }, baseFocusAPIUrl);
            if (objLogin != null)
            {
                if (!string.IsNullOrEmpty(objLogin.Auth_Token))
                {
                    try
                    {

                        if (!string.IsNullOrEmpty(compCode))
                        {
                            string connString = BL.GetConnectionString(compCode);
                           
                            Logger.Instance.LogInfo(filename, "url " + HttpContext.Request.Host.Value + " /api/Inbound/Item");
                            Logger.Instance.LogInfo(filename, "Request payload : " + JsonConvert.SerializeObject(request));
                            foreach (Item.RequestData item in request.data)
                            {
                                int iParentId = 0;
                                 obj = new FocusApiResponse.RootMaster();
                                try
                                {
                                    //check if account already exist
                                    int iMasterId = int.Parse(await Xlib.GetMasterData(2, "iMasterId", "scode='" + item.sCode + "' or sname='" + item.sCode + "'", "", connString));
                                    Xlib.EventLog("imasterid " + iMasterId);
                                    if (iMasterId > 0)
                                    {
                                        FocusApiResponse.DataMaster data = new FocusApiResponse.DataMaster();
                                        data.MasterId = iMasterId;
                                        obj.url = baseFocusAPIUrl + FocusApiUrlExtension.urlMasters + "Core__Product";
                                        obj.result = 1;
                                        obj.message = "Item already exist";
                                        obj.data = new List<FocusApiResponse.DataMaster> { data };
                                        objList.Add(obj);
                                        Logger.Instance.LogInfo(filename, "Response : " + JsonConvert.SerializeObject(obj));

                                    }

                                    else
                                    {
                                        Hashtable master = new Hashtable();

                                        foreach (PropertyInfo prop in item.GetType().GetProperties().Where(s => !s.Name.Contains("__Mstr")))
                                        {
                                            master.Add(prop.Name, item.GetType().GetProperty(prop.Name).GetValue(item, null));

                                        }


                                        string url = baseFocusAPIUrl + FocusApiUrlExtension.urlMasters + "Core__Product";
                                        Xlib.EventLog(url);
                                        PostingData postingData = new PostingData();
                                        postingData.data.Add(master);
                                        string sContent = JsonConvert.SerializeObject(postingData);
                                        Dictionary<string, string> headers = new Dictionary<string, string>();
                                        headers.Add("fSessionId", objLogin.Auth_Token);
                                        Logger.Instance.LogInfo(filename, "F8 Payload : " + sContent);
                                        Logger.Instance.LogInfo(filename, "fSessionId : " + objLogin.Auth_Token);

                                        Hashtable response = Xlib.PostToAPIAsync(sContent, headers, url, "application/json");
                                        obj = JsonConvert.DeserializeObject<FocusApiResponse.RootMaster>(response["Response"].ToString());
                                        objList.Add(obj);
                                        Logger.Instance.LogInfo(filename, "Response : " + JsonConvert.SerializeObject(obj));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    obj.result = -1;
                                    obj.message = ex.Message;
                                    objList.Add(obj);
                                    Logger.Instance.LogInfo(filename, "Response : " + JsonConvert.SerializeObject(obj));
                                    Logger.Instance.LogError(filename, ex.ToString(), ex);
                                    return Ok(obj);
                                }
                            }


                        }

                    }
                    catch(Exception ex)
                    {

                    }
                    finally
                    {

                        await BL.LogOut(objLogin.Auth_Token, baseFocusAPIUrl);
                        Logger.Instance.LogInfo(filename, "LoggedOut from  " + objLogin.Auth_Token);
                    }
                    }
                else
                {

                    Logger.Instance.LogInfo(filename, "Invalid Username,Password,companyCode passed");
                    return Unauthorized();
                }
            }
            else
            {

                Logger.Instance.LogInfo(filename, "Invalid Username,Password,companyCode passed");
                return Unauthorized();
            }


            return Ok(obj);
        }
        #endregion
        #region  PO
        [HttpPost]
        [Route("api/Inbound/PO")]
        [SwaggerRequestExample(typeof(PO.Request),typeof(POExample))]
        public async Task<IActionResult> PO([FromHeader(Name = "X-Api-UserName")] string username, [FromHeader(Name = "X-Api-Password")] string password,
            [FromHeader(Name = "CompCode")] string compCode, PO.Request request)
        {

            PO.Data objdata = request.data[0];

            string baseFocusAPIUrl = configuration.GetValue<string>("AppSettings:FocusAPI");
            FocusApiResponse.Root obj = new FocusApiResponse.Root();
            string filename = compCode + "_" + DateTime.Now.ToString("ddMMyyyy");
            Logger.Instance.LogInfo(filename, "calling PO API");
            Logger.Instance.LogInfo(filename, "url " + HttpContext.Request.Host.Value + " /api/Inbound/PO");
           
        
                Logger.Instance.LogInfo(filename, "compCode " + compCode);

                //get token
                LogingResult objLogin = await BL.Login(new ClsLogin { username = username, password = password, companycode = compCode }, baseFocusAPIUrl);
            if (objLogin != null)
            {
                if (!string.IsNullOrEmpty(objLogin.Auth_Token))
                {
                    try
                    {
                        string connString = BL.GetConnectionString(compCode);

                        //formatting header
                        Hashtable header = new Hashtable();
                        var names = new List<string> { "__Mstr", "obj__", "__AcMstr", "__EmpMstr", "__CurMstr","Body" };
                        foreach (PropertyInfo prop in objdata.GetType().GetProperties().Where(s => !names.Any(name=>s.Name.Contains(name)) ))
                        {
                            int iDate = 0;
                            if (prop.Name.Contains("__Date"))
                            {
                                var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                                string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;
                                try
                                {
                                    string _date = objdata.GetType().GetProperty(prop.Name).GetValue(objdata, null).ToString();
                                    if (!string.IsNullOrEmpty(_date))
                                    {
                                        Logger.Instance.LogInfo(filename, "Date " + _date);
                                        DateTime parsedDate = DateTime.ParseExact(_date, "dd/MM/yyyy", null);
                                        string formattedDate = parsedDate.ToString("yyyy-MM-dd");                             
                                        string sql = "select dbo.datetoint('" + formattedDate + "' )";                                                                          
                                        string sqlres = Xlib.GetScalarByQuery(sql, "", connString);                                      
                                        iDate = int.Parse(Xlib.GetScalarByQuery(sql, "", connString));

                                    }
                                    else
                                    {
                                        iDate = 0;
                                    }
                                    header.Add(prop.Name.Split(new string[] { "__Date" }, StringSplitOptions.None)[0], iDate);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Instance.LogError(filename, ex.Message, ex);
                                    obj.result = -1;
                                    obj.message = jsonPropertyName + " is invalid";
                                    Logger.Instance.LogInfo(filename, "Response " + JsonConvert.SerializeObject(obj));
                                    return Ok(obj);
                                }
                            }
                            else
                            {
                                header.Add(prop.Name, objdata.GetType().GetProperty(prop.Name).GetValue(objdata, null));
                            }
                        }
                        foreach (PropertyInfo prop in objdata.GetType().GetProperties().Where(s => s.Name.Contains("__Mstr")))
                        {
                            var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                            string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;
                            string mstrName = prop.Name.Split(new string[] { "__Mstr" }, StringSplitOptions.None)[0];
                            string sCode = objdata.GetType().GetProperty(prop.Name).GetValue(objdata, null).ToString();
                            if (!string.IsNullOrEmpty(sCode))
                            {

                                int iMasterId = 0;
                                //set master id for all master fields other than group

                                //get account id for accounts
                                int iMastertypid = int.Parse(Xlib.GetMasterTypeId(mstrName.Replace("_", ""), "", connString));
                                iMasterId = int.Parse(await Xlib.GetMasterData(iMastertypid, "iMasterId", "sCode='" + sCode + "' or sname='" + sCode + "'", "", connString));
                                if (iMasterId == 0)
                                {

                                    obj.result = -1;
                                    obj.message = jsonPropertyName + " is not defined ";
                                    Logger.Instance.LogInfo(filename, "Response " + JsonConvert.SerializeObject(obj));
                                    return Ok(obj);

                                }
                                string focusCode = (await Xlib.GetMasterData(1, "sCode", "imasterid=" + iMasterId + "", "", connString));

                                header.Add(mstrName.Replace("_", " ") + "__Code", sCode);
                                header.Add(mstrName.Replace("_", " ") + "__Id", iMasterId);

                            }
                        }
                        //account mapping
                        foreach (PropertyInfo prop in objdata.GetType().GetProperties().Where(s => s.Name.Contains("__AcMstr")))
                        {
                            var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                            string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;

                            string mstrName = prop.Name.Split(new string[] { "__AcMstr" }, StringSplitOptions.None)[0];
                            string sCode = objdata.GetType().GetProperty(prop.Name).GetValue(objdata, null).ToString();
                            int iMasterId = int.Parse(await Xlib.GetMasterData(1, "iMasterId", "sCode='" + sCode + "' or sname ='" + sCode + "'", "", connString));
                            //if (iMasterId == 0)
                            //{
                            //    iMasterId = BL.CreateMaster(sCode, sCode, mstrName, sessionId, baseFocusAPIUrl);
                            //}
                            if (iMasterId == 0)
                            {


                                obj.result = -1;
                                obj.message = jsonPropertyName + " is not defined ";
                                Logger.Instance.LogInfo(filename, "Response " + JsonConvert.SerializeObject(obj));
                                return Ok(obj);

                            }


                            header.Add(mstrName + "__Code", sCode);
                            header.Add(mstrName + "__Id", iMasterId);
                        }
                        //Employee mapping
                        foreach (PropertyInfo prop in objdata.GetType().GetProperties().Where(s => s.Name.Contains("__EmpMstr")))
                        {
                            var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                            string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;
                            string mstrName = prop.Name.Split(new string[] { "__EmpMstr" }, StringSplitOptions.None)[0];
                            string sCode = objdata.GetType().GetProperty(prop.Name).GetValue(objdata, null).ToString();
                            int iMasterId = int.Parse(await Xlib.GetMasterData(800, "iMasterId", "sCode='" + sCode + "' or sname ='" + sCode + "'", "", connString));
                            //if (iMasterId == 0)
                            //{
                            //    iMasterId = BL.CreateMaster(sCode, sCode, mstrName, sessionId, baseFocusAPIUrl);
                            //}
                            if (iMasterId == 0)
                            {


                                obj.result = -1;
                                obj.message = jsonPropertyName + " is not defined ";
                                Logger.Instance.LogInfo(filename, "Response " + JsonConvert.SerializeObject(obj));
                                return Ok(obj);

                            }


                            header.Add(mstrName + "__Code", sCode);
                            header.Add(mstrName + "__Id", iMasterId);
                        }
                        //currency master
                        foreach (PropertyInfo prop in objdata.GetType().GetProperties().Where(s => s.Name.Contains("__CurMstr")))
                        {

                            string sCode = objdata.GetType().GetProperty(prop.Name).GetValue(objdata, null).ToString();
                            string sql = "select iCurrencyId from mCore_Currency where sCode='" + sCode + "' ";
                            int iCurrencyId = int.Parse(Xlib.GetScalarByQuery(sql, "", connString));
                            header.Add("Currency__Code", objdata.GetType().GetProperty(prop.Name).GetValue(objdata, null));
                            header.Add("Currency__Id", iCurrencyId);
                        }
                       
                        //formatting body
                        List<Hashtable> body = new List<Hashtable>();
                        foreach (PO.Body lineItem in objdata.Body)
                        {
                            Hashtable item = new Hashtable();
                            //add all the field which is not a master
                            //exclude master , reference,Acmaster 
                            var names_body = new List<string> { "__Mstr", "obj__", "__AcMstr" };
                            foreach (PropertyInfo prop in lineItem.GetType().GetProperties().Where(s => !names.Any(name => s.Name.Contains(name))))
                            {
                                Logger.Instance.LogInfo(filename, "adding non master types "+ prop.Name);
                                item.Add(prop.Name, lineItem.GetType().GetProperty(prop.Name).GetValue(lineItem, null));

                            }
                            Logger.Instance.LogInfo(filename, "adding account");
                            foreach (PropertyInfo prop in lineItem.GetType().GetProperties().Where(s => s.Name.Contains("__AcMstr")))
                            {
                                var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                                string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;
                                string mstrName = prop.Name.Split('_')[0].Trim();
                                Logger.Instance.LogInfo(filename, "masterName " + mstrName);
                                string sCode = lineItem.GetType().GetProperty(prop.Name).GetValue(lineItem, null).ToString();
                                int iMasterId = int.Parse(await Xlib.GetMasterData(1, "iMasterId", "sCode='" + sCode + "' or sname='" + sCode + "'", "", connString));
                                //if (iMasterId == 0)
                                //{
                                //    iMasterId = BL.CreateMaster(sCode, sCode, mstrName, sessionId, baseFocusAPIUrl);
                                //}
                                if (iMasterId == 0)
                                {


                                    obj.result = -1;
                                    obj.message = jsonPropertyName + " is not defined ";
                                    return Ok(obj);

                                }


                                item.Add(mstrName + "__Code", sCode);
                                item.Add(mstrName + "__Id", iMasterId);
                            }
                            Logger.Instance.LogInfo(filename, "adding master");
                            //adding all masters
                            foreach (PropertyInfo prop in lineItem.GetType().GetProperties().Where(s => s.Name.Contains("__Mstr")))
                            {
                                var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                                string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;
                                string mstrName = prop.Name.Split(new string[] { "__Mstr" }, StringSplitOptions.None)[0];

                                Logger.Instance.LogInfo(filename, "masterName " + mstrName);
                                
                                string sCode = lineItem.GetType().GetProperty(prop.Name).GetValue(lineItem, null).ToString();
                                if (!string.IsNullOrEmpty(sCode))
                                {
                                    int iMasterId = 0;

                                    //// if the company master has space then it will passed with under score so remove underscore while passing to focus
                                    int iMastertypid = int.Parse(Xlib.GetMasterTypeId(mstrName.Replace("Item","Product").Replace("_", ""), "", connString));
                                    iMasterId = int.Parse(await Xlib.GetMasterData(iMastertypid, "iMasterId", "sCode='" + sCode + "' or sname ='" + sCode + "'", "", connString));
                                    if (iMasterId == 0)
                                    {


                                        obj.result = -1;
                                        obj.message = jsonPropertyName + " is not defined ";
                                        return Ok(obj);

                                    }
                                    // string focusCode = (await Xlib.GetMasterData(1, "sCode", "imasterid=" + iMasterId + "", "", connString));
                                    item.Add(mstrName.Replace("_", " ") + "__Code", sCode);
                                    item.Add(mstrName.Replace("_", " ") + "__Id", iMasterId);


                                }

                            }

                            Logger.Instance.LogInfo(filename, "adding extra classes");
                            //Adding Extra classes
                            foreach (PropertyInfo prop in lineItem.GetType().GetProperties().Where(s => s.Name.Contains("obj__")))
                            {
                                var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                                string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;
                                string mstrName = prop.Name.Split(new string[] { "obj__" }, StringSplitOptions.None)[1].Replace('_', ' ');
                                if (!item.ContainsKey(mstrName))
                                {
                                    Logger.Instance.LogInfo(filename, "masterName " + mstrName);
                                    string valuePer = mstrName == "Discount" ?"": lineItem.GetType().GetProperty("Per__obj__"+mstrName ).GetValue(lineItem, null).ToString();
                                    string value = lineItem.GetType().GetProperty("Val__obj__" + mstrName ).GetValue(lineItem, null).ToString();
                                    item.Add(mstrName, new PO.POExtraClass
                                    {
                                        Input = mstrName == "Discount" ? value : valuePer,
                                        FieldName = mstrName,
                                        FieldId = (int)Enum.Parse(typeof(PV.PVExtraClass_Fieldids), mstrName, true),
                                        Value = value

                                    }); 
                                }
                            }



                            body.Add(item);
                        }
                        string url = baseFocusAPIUrl + FocusApiUrlExtension.urlVouchers + "Purchases Orders";

                        PostingData postingData = new PostingData();
                        postingData.data.Add(new Hashtable { { "Header", header }, { "Body", body } });
                        string sContent = JsonConvert.SerializeObject(postingData);
                        Dictionary<string, string> headers = new Dictionary<string, string>();
                        headers.Add("fSessionId", objLogin.Auth_Token);
                        Logger.Instance.LogInfo(filename, "F8 Payload : " + sContent);
                        Logger.Instance.LogInfo(filename, "fSessionId : " + objLogin.Auth_Token);
                        Hashtable response = Xlib.PostToAPIAsync(sContent, headers, url, "application/json");
                        obj = JsonConvert.DeserializeObject<FocusApiResponse.Root>(response["Response"].ToString());
                        obj.url = url;
                        Logger.Instance.LogInfo(filename, "Response " + JsonConvert.SerializeObject(obj));

                    }

                    catch (Exception ex)
                    {
                        obj.message = ex.Message;
                        obj.result = -1;
                        Logger.Instance.LogInfo(filename, "Response " + JsonConvert.SerializeObject(obj));
                        Logger.Instance.LogError(filename, ex.ToString(), ex);
                        return Ok(obj);
                    }
                    finally
                    {
                        await BL.LogOut(objLogin.Auth_Token, baseFocusAPIUrl);
                    }
                }
                else
                {

                    Logger.Instance.LogInfo(filename, "Invalid Username,Password,companyCode passed");
                    return Unauthorized();
                }
            }
            else
            {

                Logger.Instance.LogInfo(filename, "Invalid Username,Password,companyCode passed");
                return Unauthorized();
            }
            return Ok(obj);
        }

        #endregion
        #region  PV
        [HttpPost]
        [Route("api/Inbound/PV")]
        [SwaggerRequestExample(typeof(PV.Request),typeof(PV.PVExample))]
        public async Task<IActionResult> PV([FromHeader(Name = "X-Api-UserName")] string username, [FromHeader(Name = "X-Api-Password")] string password,
            [FromHeader(Name = "CompCode")] string compCode, PV.Request request)
        {

            PV.Data objdata = request.data[0];

            string baseFocusAPIUrl = configuration.GetValue<string>("AppSettings:FocusAPI");
            FocusApiResponse.Root obj = new FocusApiResponse.Root();
            string filename = compCode + "_" + DateTime.Now.ToString("ddMMyyyy");
            Logger.Instance.LogInfo(filename, "calling PO API");
            Logger.Instance.LogInfo(filename, "url " + HttpContext.Request.Host.Value + " /api/Inbound/PO");


            Logger.Instance.LogInfo(filename, "compCode " + compCode);

            //get token
            LogingResult objLogin = await BL.Login(new ClsLogin { username = username, password = password, companycode = compCode }, baseFocusAPIUrl);
            if (objLogin != null)
            {
                if (!string.IsNullOrEmpty(objLogin.Auth_Token))
                {
                    try
                    {
                        string connString = BL.GetConnectionString(compCode);

                        //formatting header
                        Hashtable header = new Hashtable();
                        var names = new List<string> { "__Mstr", "obj__", "__AcMstr", "__EmpMstr", "__CurMstr", "Body" };
                        foreach (PropertyInfo prop in objdata.GetType().GetProperties().Where(s => !names.Any(name => s.Name.Contains(name))))
                        {
                            int iDate = 0;
                            if (prop.Name.Contains("__Date"))
                            {
                                var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                                string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;
                                try
                                {
                                    string _date = objdata.GetType().GetProperty(prop.Name).GetValue(objdata, null).ToString();
                                    if (!string.IsNullOrEmpty(_date))
                                    {
                                        Logger.Instance.LogInfo(filename, "Date " + _date);
                                        DateTime parsedDate = DateTime.ParseExact(_date, "dd/MM/yyyy", null);
                                        string formattedDate = parsedDate.ToString("yyyy-MM-dd");
                                        string sql = "select dbo.datetoint('" + formattedDate + "' )";
                                        string sqlres = Xlib.GetScalarByQuery(sql, "", connString);
                                        iDate = int.Parse(Xlib.GetScalarByQuery(sql, "", connString));

                                    }
                                    else
                                    {
                                        iDate = 0;
                                    }
                                    header.Add(prop.Name.Split(new string[] { "__Date" }, StringSplitOptions.None)[0], iDate);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Instance.LogError(filename, ex.Message, ex);
                                    obj.result = -1;
                                    obj.message = jsonPropertyName + " is invalid";
                                    Logger.Instance.LogInfo(filename, "Response " + JsonConvert.SerializeObject(obj));
                                    return Ok(obj);
                                }
                            }
                            else
                            {
                                header.Add(prop.Name, objdata.GetType().GetProperty(prop.Name).GetValue(objdata, null));
                            }
                        }
                        foreach (PropertyInfo prop in objdata.GetType().GetProperties().Where(s => s.Name.Contains("__Mstr")))
                        {
                            Logger.Instance.LogInfo(filename, "prop.Name " + prop.Name);
                            var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                            string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;
                            string mstrName = prop.Name.Split(new string[] { "__Mstr" }, StringSplitOptions.None)[0];
                            Logger.Instance.LogInfo(filename, "master name " + mstrName);
                            string sCode = objdata.GetType().GetProperty(prop.Name).GetValue(objdata, null).ToString();
                            if (!string.IsNullOrEmpty(sCode))
                            {
                                Logger.Instance.LogInfo(filename, "sCode " + sCode);

                                int iMasterId = 0;
                                //set master id for all master fields other than group

                                //get account id for accounts
                                int iMastertypid = int.Parse(Xlib.GetMasterTypeId(mstrName.Replace("_", ""), "", connString));
                           
                                iMasterId = int.Parse(await Xlib.GetMasterData(iMastertypid, "iMasterId", "sCode='" + sCode + "' or sname='" + sCode + "'", "", connString));
                                if (iMasterId == 0)
                                {

                                    obj.result = -1;
                                    obj.message = jsonPropertyName + " is not defined ";
                                    Logger.Instance.LogInfo(filename, "Response " + JsonConvert.SerializeObject(obj));
                                    return Ok(obj);

                                }
                              

                                header.Add(mstrName.Replace("_", " ") + "__Code", sCode);
                                header.Add(mstrName.Replace("_", " ") + "__Id", iMasterId);

                            }
                        }
                        //account mapping
                        foreach (PropertyInfo prop in objdata.GetType().GetProperties().Where(s => s.Name.Contains("__AcMstr")))
                        {
                            Logger.Instance.LogInfo(filename, "prop.Name " + prop.Name);
                            var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                            string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;

                            string mstrName = prop.Name.Split(new string[] { "__AcMstr" }, StringSplitOptions.None)[0];
                            Logger.Instance.LogInfo(filename, "master name " + mstrName);
                            string sCode = objdata.GetType().GetProperty(prop.Name).GetValue(objdata, null).ToString();
                            int iMasterId = int.Parse(await Xlib.GetMasterData(1, "iMasterId", "sCode='" + sCode + "' or sname ='" + sCode + "'", "", connString));
                            //if (iMasterId == 0)
                            //{
                            //    iMasterId = BL.CreateMaster(sCode, sCode, mstrName, sessionId, baseFocusAPIUrl);
                            //}
                            if (iMasterId == 0)
                            {


                                obj.result = -1;
                                obj.message = jsonPropertyName + " is not defined ";
                                Logger.Instance.LogInfo(filename, "Response " + JsonConvert.SerializeObject(obj));
                                return Ok(obj);

                            }


                            header.Add(mstrName + "__Code", sCode);
                            header.Add(mstrName + "__Id", iMasterId);
                        }
                        //Employee mapping
                        foreach (PropertyInfo prop in objdata.GetType().GetProperties().Where(s => s.Name.Contains("__EmpMstr")))
                        {
                            Logger.Instance.LogInfo(filename, "prop.Name " + prop.Name);
                            var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                            string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;
                            string mstrName = prop.Name.Split(new string[] { "__EmpMstr" }, StringSplitOptions.None)[0];
                            Logger.Instance.LogInfo(filename, "master name " + mstrName);
                            string sCode = objdata.GetType().GetProperty(prop.Name).GetValue(objdata, null).ToString();
                            int iMasterId = int.Parse(await Xlib.GetMasterData(800, "iMasterId", "sCode='" + sCode + "' or sname ='" + sCode + "'", "", connString));
                            //if (iMasterId == 0)
                            //{
                            //    iMasterId = BL.CreateMaster(sCode, sCode, mstrName, sessionId, baseFocusAPIUrl);
                            //}
                            if (iMasterId == 0)
                            {


                                obj.result = -1;
                                obj.message = jsonPropertyName + " is not defined ";
                                Logger.Instance.LogInfo(filename, "Response " + JsonConvert.SerializeObject(obj));
                                return Ok(obj);

                            }


                            header.Add(mstrName + "__Code", sCode);
                            header.Add(mstrName + "__Id", iMasterId);
                        }
                        //currency master
                        foreach (PropertyInfo prop in objdata.GetType().GetProperties().Where(s => s.Name.Contains("__CurMstr")))
                        {
                            Logger.Instance.LogInfo(filename, "prop.Name " + prop.Name);

                            string sCode = objdata.GetType().GetProperty(prop.Name).GetValue(objdata, null).ToString();
                            string sql = "select iCurrencyId from mCore_Currency where sCode='" + sCode + "' ";
                            int iCurrencyId = int.Parse(Xlib.GetScalarByQuery(sql, "", connString));
                            header.Add("Currency__Code", objdata.GetType().GetProperty(prop.Name).GetValue(objdata, null));
                            header.Add("Currency__Id", iCurrencyId);
                        }
                        header.Add("RefNo", objdata.DocNo);
                        //formatting body
                        List<Hashtable> body = new List<Hashtable>();
                        foreach (PV.Body lineItem in objdata.Body)
                        {
                            int iProduct = 0;
                            Hashtable item = new Hashtable();
                            //add all the field which is not a master
                            //exclude master , reference,Acmaster 
                            var names_body = new List<string> { "__Mstr", "obj__", "__AcMstr" , "__TrnMstr" };
                            foreach (PropertyInfo prop in lineItem.GetType().GetProperties().Where(s => !names.Any(name => s.Name.Contains(name))))
                            {
                                item.Add(prop.Name, lineItem.GetType().GetProperty(prop.Name).GetValue(lineItem, null));

                            }

                            //adding all masters
                            foreach (PropertyInfo prop in lineItem.GetType().GetProperties().Where(s => s.Name.Contains("__Mstr")))
                            {
                                var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                                string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;
                                string mstrName = prop.Name.Split(new string[] { "__Mstr" }, StringSplitOptions.None)[0];
                                Logger.Instance.LogInfo(filename, "master name " + mstrName);
                                string sCode = lineItem.GetType().GetProperty(prop.Name).GetValue(lineItem, null).ToString();
                                if (!string.IsNullOrEmpty(sCode))
                                {
                                    Logger.Instance.LogInfo(filename, "sCode " + sCode);
                                    int iMasterId = 0;

                                    //// if the company master has space then it will passed with under score so remove underscore while passing to focus
                                    int iMastertypid = int.Parse(Xlib.GetMasterTypeId(mstrName.Replace("Item", "Product").Replace("_", ""), "", connString));
                                    iMasterId = int.Parse(await Xlib.GetMasterData(iMastertypid, "iMasterId", "sCode='" + sCode + "' or sname ='" + sCode + "'", "", connString));
                                    if (iMasterId == 0)
                                    {


                                        obj.result = -1;
                                        obj.message = jsonPropertyName + " is not defined ";
                                        return Ok(obj);

                                    }
                                    if(prop.Name== "Item__Mstr")
                                    {
                                        iProduct = iMasterId;
                                    }
                                    // string focusCode = (await Xlib.GetMasterData(1, "sCode", "imasterid=" + iMasterId + "", "", connString));
                                    item.Add(mstrName.Replace("_", " ") + "__Code", sCode);
                                    item.Add(mstrName.Replace("_", " ") + "__Id", iMasterId);


                                }

                            }
                            //adding all masters
                            foreach (PropertyInfo prop in lineItem.GetType().GetProperties().Where(s => s.Name.Contains("__TrnMstr")))
                            {
                                var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                                string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;
                           
                                string mstrName = prop.Name.Split(new string[] { "__TrnMstr" }, StringSplitOptions.None)[0];
                                Logger.Instance.LogInfo(filename, "master name " + mstrName);
                              

                                int iVoucherType = (int)Enum.Parse(typeof(PV.PVTrn_iVtype), mstrName, true);


                                string sCode = lineItem.GetType().GetProperty(prop.Name).GetValue(lineItem, null).ToString();
                                if (!string.IsNullOrEmpty(sCode))
                                {
                                    int iMasterId = 0;

                                    string query = "select iHeaderId from tCore_Header_0 where sVoucherNo='" + sCode + "' and iVoucherType=" + iVoucherType + " and bSuspended=0";
                                    iMasterId = int.Parse( Xlib.GetScalarByQuery(query, "", connString));
                                    if (iMasterId == 0)
                                    {


                                        obj.result = -1;
                                        obj.message = jsonPropertyName + " is not defined ";
                                        return Ok(obj);

                                    }
                                    // string focusCode = (await Xlib.GetMasterData(1, "sCode", "imasterid=" + iMasterId + "", "", connString));
                                    item.Add(mstrName.Replace("_", " ") + "__Code", sCode);
                                    item.Add(mstrName.Replace("_", " ") + "__Id", iMasterId);


                                }

                            }
                            foreach (PropertyInfo prop in lineItem.GetType().GetProperties().Where(s => s.Name.Contains("__AcMstr")))
                            {
                                var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                                string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;
                                string mstrName = prop.Name.Split('_')[0].Trim();
                                Logger.Instance.LogInfo(filename, "master name " + mstrName);
                                string sCode = lineItem.GetType().GetProperty(prop.Name).GetValue(lineItem, null).ToString();
                                int iMasterId = int.Parse(await Xlib.GetMasterData(1, "iMasterId", "sCode='" + sCode + "' or sname='" + sCode + "'", "", connString));
                                //if (iMasterId == 0)
                                //{
                                //    iMasterId = BL.CreateMaster(sCode, sCode, mstrName, sessionId, baseFocusAPIUrl);
                                //}
                                if (iMasterId == 0)
                                {


                                    obj.result = -1;
                                    obj.message = jsonPropertyName + " is not defined ";
                                    return Ok(obj);

                                }


                                item.Add(mstrName + "__Code", sCode);
                                item.Add(mstrName + "__Id", iMasterId);
                            }

                            //Adding Extra classes
                            foreach (PropertyInfo prop in lineItem.GetType().GetProperties().Where(s => s.Name.Contains("__obj")))
                            {                                
                                    var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                                string jsonPropertyName = jsonPropertyAttribute?.PropertyName ?? prop.Name;
                                string mstrName = prop.Name.Split(new string[] { "__obj" }, StringSplitOptions.None)[0].Replace('_', ' ');
                                if (!item.ContainsKey(mstrName))
                                {
                                    Logger.Instance.LogInfo(filename, "master name " + mstrName);
                                string valuePer = mstrName == "Discount" ? "" : lineItem.GetType().GetProperty(mstrName + "__obj__Per").GetValue(lineItem, null).ToString();
                                string value = lineItem.GetType().GetProperty(mstrName + "__obj__Val").GetValue(lineItem, null).ToString();
                                
                                    item.Add(mstrName, new PV.PVExtraClass
                                    {
                                        Input = mstrName== "Discount" ? value:valuePer,
                                        FieldName = mstrName,
                                        FieldId = (int)Enum.Parse(typeof(PV.PVExtraClass_Fieldids), mstrName, true),
                                        Value = value

                                    });
                                }
                            }
                            //Adding reference
                            //List<PV.PvRef> references = new List<PV.PvRef>();

                            //PV.PvRef objRef = new PV.PvRef();
                            //objRef.Reference = "New Refrence "+ lineItem.BillNo;
                            //objRef.Amount = request.data[0].Net;
                            //objRef.CustomerId = (await Xlib.GetMasterData(1, "iMasterId", "sCode='" + request.data[0].VendorAC__AcMstr + "'", "", connString));
                            ////objRef.Reference = objRef.refAbbr + ":" + objRef.refVoucherNo + " :" + objRef.refDate;
                            ////string sql = "xsp_getrefence " + lineItem.Reference[0].refVoucherType + ",'" + lineItem.Reference[0].refVoucherNo + "'";
                            ////Xlib.EventLog(sql);
                            ////string iref = Xlib.GetScalarByQuery(sql, "", connString);
                            ////if (!string.IsNullOrEmpty(iref))
                            ////{
                            ////    objRef.@ref = int.Parse(iref);
                            ////}
                            //references.Add(objRef);

                            //// }
                            //lineItem.Reference = references;


                            //Adding PO

                            List<PV.PoRef> lpo = new List<PV.PoRef>();
                            string query_baseId = "select d.iBodyId from tCore_Data_0 d inner join tCore_Indta_0 i on d.iBodyId=i.iBodyId  inner join tCore_Header_0 h on h.iHeaderId=d.iHeaderId where " +
                                "h.sVoucherNo = '"+lineItem.PONo+"' and h.iVoucherType = 2560 and i.iProduct = "+ iProduct + "";
                            //Logger.Instance.LogInfo(filename,"iref_query "+ query_baseId);
                            string iBaseId = Xlib.GetScalarByQuery(query_baseId, "", connString);
                            PV.PoRef objPO = new PV.PoRef();
                            objPO.BaseTransactionId = iBaseId;
                            objPO.VoucherNo = "PurOrd:" + lineItem.PONo;
                            objPO.UsedValue = lineItem.Quantity;
                            objPO.RefId = iBaseId;

                            lpo.Add(objPO);

                            item.Add("L-Purchases Orders", lpo);

                            body.Add(item);
                        }
                        string url = baseFocusAPIUrl + FocusApiUrlExtension.urlVouchers + "Purchases Vouchers";

                        PostingData postingData = new PostingData();
                        postingData.data.Add(new Hashtable { { "Header", header }, { "Body", body } });
                        string sContent = JsonConvert.SerializeObject(postingData);
                        Dictionary<string, string> headers = new Dictionary<string, string>();
                        headers.Add("fSessionId", objLogin.Auth_Token);
                        Logger.Instance.LogInfo(filename, "F8 Payload : " + sContent);
                        Logger.Instance.LogInfo(filename, "fSessionId : " + objLogin.Auth_Token);
                        Hashtable response = Xlib.PostToAPIAsync(sContent, headers, url, "application/json");
                        obj = JsonConvert.DeserializeObject<FocusApiResponse.Root>(response["Response"].ToString());
                        obj.url = url;
                        Logger.Instance.LogInfo(filename, "Response " + JsonConvert.SerializeObject(obj));

                    }

                    catch (Exception ex)
                    {
                        obj.message = ex.Message;
                        obj.result = -1;
                        Logger.Instance.LogInfo(filename, "Response " + JsonConvert.SerializeObject(obj));
                        Logger.Instance.LogError(filename, ex.ToString(), ex);
                        return Ok(obj);
                    }
                    finally
                    {
                        await BL.LogOut(objLogin.Auth_Token, baseFocusAPIUrl);
                    }
                }
                else
                {

                    Logger.Instance.LogInfo(filename, "Invalid Username,Password,companyCode passed");
                    return Unauthorized();
                }
            }
            else
            {

                Logger.Instance.LogInfo(filename, "Invalid Username,Password,companyCode passed");
                return Unauthorized();
            }
            return Ok(obj);

        }

        #endregion
    }
}
