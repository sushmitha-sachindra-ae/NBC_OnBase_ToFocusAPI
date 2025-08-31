using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;

namespace XtFocusOnBaseIntegAPI.Model
{
    public class Account
    {
        #region request 
        public class Request
        {

            public List<RequestData> data { get; set; }

        }
        public class RequestData
        {
            [JsonProperty("Code")]
            public string sCode { get; set; }
            [JsonProperty("Name")]
            public string sName { get; set; }
            [JsonProperty("Address")]
            public string sAddres { get; set; }
            [JsonProperty("City")]
            public string City__Mstr { get; set; }
            [JsonProperty("TelNo")]
            public string sTelNo { get; set; }
            [JsonProperty("Fax")]
            public string sFaxNo { get; set; }
            public string TRN { get; set; }
            [JsonProperty("Country")]
            public string Country__Mstr { get; set; }
            /// <summary>
            /// PurchaseType is a string list-->{"Local","Import"}
            /// </summary>

            public string PurchaseType { get; set; }

            /// <summary>
            /// PaymentType is a number list-->[{1,OWN},{2,TPA},{3,NEFT},{4,IMPS},{5,RTGS}}      
            /// </summary>
             [JsonProperty("PaymentType")]
            public int iPaymentType { get; set; } //[{1,OWN},{2,TPA},{3,NEFT},{4,IMPS},{5,RTGS}} .Pass only the number
            [JsonProperty("PlaceOfSupply")]
            public string PlaceOfSupply__Mstr { get; set; }
            [JsonProperty("BankName")]
            public string BankName { get; set; }
            [JsonProperty("BankAccountName")]
            public string sBankAccountName { get; set; }
            [JsonProperty("BankAccountNumber")]
            public string sBankAccountNumber { get; set; }            
      
            [JsonIgnore]
            public int iAccountType { get; set; } = 6; //vendor account
            [JsonProperty("Group")]
            public string Account__Grp { get; set; }
        }

        #endregion
        public class AccountExample : IExamplesProvider<Account.Request>
        {
            public Account.Request GetExamples()
            {
                Account.Request obj = new Account.Request();
                obj.data = new List<Account.RequestData>();
                obj.data.Add(new Account.RequestData
                {
                    sCode = "2-21-210400-00151",
                    sName = "Owner Association’s VENDOR/SUPPLIER",
                    sAddres = "",
                    City__Mstr="Dubai",
                    sTelNo="",
                    sFaxNo="",
                    TRN="",
                    Country__Mstr="UAE",
                    PurchaseType="Local",
                    iPaymentType=1,
                    PlaceOfSupply__Mstr="DXB",
                    BankName="",
                    sBankAccountName="",
                    sBankAccountNumber="",
                    Account__Grp="vendor"

                });

                return obj;

            }
        }
    }
}
