using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;

namespace XtFocusOnBaseIntegAPI.Model
{
    public class Item
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
            [JsonProperty("Itemtype")]
            public int iProductType { get; set; } //[{1,"Service"},{2,"Raw Material"},
                                                  //{3,"Intermediate Item"},{4,"Finished goods"}
                                                  //,{5,"Non Stock item"},{6,"Modifier"},{7,"PRT Consumable"}
                                                  //,{8,"PRT Tools"},{9,"Ticket"}]


        }

        #endregion
    }
    public class ItemExample : IExamplesProvider<Item.Request>
    {
        public Item.Request GetExamples()
        {
            Item.Request obj = new Item.Request();
            obj.data = new List<Item.RequestData>(); 
            obj.data.Add(new Item.RequestData
            {
                sCode = "P-04",
                sName = "Vehicles",
                iProductType = 4
            });

            return obj;
            
        }
    }
}
