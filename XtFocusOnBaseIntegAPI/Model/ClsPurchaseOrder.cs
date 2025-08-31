using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics;

namespace XtFocusOnBaseIntegAPI.Model
{
    public class PO
    {
        public class Request
        {

            public List<Data> data { get; set; }

        }
        public class Data
        {
           
            [JsonProperty("DocumentNo")]
            public string DocNo { get; set; }
            /// <summary>
            /// dd/MM/yyyy format
            /// </summary>

            [JsonProperty("DocumentDate")]
            public string Date__Date { get; set; }
            [JsonProperty("VendorAccount")]
            public string VendorAC__AcMstr { get; set; }
            /// <summary>
            /// currency code as per focus currency master
            /// </summary>

            [JsonProperty("CurrencyCode")]
            public string Currency__CurMstr { get; set; }

            public decimal ExchangeRate { get; set; }
            [JsonProperty("Entity")]
            public string Entity__Mstr { get; set; }

        
            [JsonProperty("Jurisdiction")]
            public string Jurisdiction__Mstr { get; set; }


            [JsonProperty("Place Of Supply")]
            public string Place_of_supply__Mstr { get; set; }
            [JsonProperty("Campaign")]
            public string Campaign__Mstr { get; set; } //in the json template it has a extra space Campaign __Id

            [JsonProperty("Requested By")]
            public string Requested_By__EmpMstr { get; set; } //emp master mapping
            [JsonProperty("Payment_Terms")]
            public string Payment_Terms { get; set; }
            [JsonProperty("Delivery_Terms")]
            public string Delivery_Terms { get; set; }
 
            public string Title { get; set; }
            [JsonProperty("Reason For Request")]
            public string Reason_for_Request { get; set; }
         
      

            [JsonProperty("Narration")]
            public string sNarration { get; set; }




            [JsonProperty("Department Cost Centre")]
            public string Department_Cost_Centre__Mstr { get; set; }
            /// <summary>
            /// includes value [ LPO,Agreement,Contract]
            /// </summary>
            [JsonProperty("PO Type")]
            public string PO_Type { get; set; }
            [JsonProperty("LineData")]
            public List<Body> Body { get; set; }

        }

        public class Body
        {
            
            [JsonProperty("Branch")]
            public string Branch__Mstr { get; set; }
            [JsonProperty("Investment Class")]
            public string Investment_Class__Mstr { get; set; }
            [JsonProperty("Item Category")]
            public string Item_Category__Mstr { get; set; }
            [JsonProperty("Request Type")]
            public string Request_Type__Mstr { get; set; } //Request Type __Id --extra space

            [JsonProperty("Activity Master")]        
            public string Activity_Master__Mstr { get; set; }

            [JsonProperty("Purchase Request No")]
            public string Purchase_Request_No__Mstr { get; set; }
            [JsonProperty("Item")]
            public string Item__Mstr { get; set; }
            [JsonProperty("Description")]
            public string Description { get; set; }
            [JsonProperty("TaxCode")]
            public string TaxCode__Mstr { get; set; }

            [JsonProperty("Purchase Account")]
            public string PurchaseAC__AcMstr { get; set; }
            [JsonProperty("Rate")]
            public double Rate { get; set; }
            [JsonProperty("Quantity")]
            public double Quantity { get; set; }
        

            //public List<Ref> Reference { get; set; }
            [JsonProperty("Remarks")]
            public string sRemarks { get; set; }

            [JsonProperty("Purpose Of Request")]
            public string Purpose_Of_Request { get; set; }

            /// <summary>
            /// Expected Date of Service is a number list [{0,urgent},{1,"3 days"},{2,"1 Week"},{3,"State exact Date"}] .Pass only number
            /// </summary>
            [JsonProperty("Expected Date of Service")]
            public int Expected_Date_of_Service_ { get; set; } = 0;
            /// <summary>
            /// dd/MM/yyyy format
            /// </summary>
            [JsonProperty("Exact Date")]
            public string Exact_Date__Date { get; set; }
            [JsonProperty("Project Master")]
            public string Project_Master__Mstr { get; set; }
            [JsonProperty("Discount_Value")]
            public double Val__obj__Discount { get; set; }
            //[JsonProperty("Discount%")]
           // public double Per__obj__Discount { get; set; } //add class for __obj
            [JsonProperty("VAT_Value")]
            public double Val__obj__VAT { get; set; } //add class for __obj
            [JsonProperty("VAT%")]
            public double Per__obj__VAT { get; set; } //add class for __obj
        }

     public  class POExtraClass
        {
            public string Input { get; set; }
            public string FieldName { get; set; }
            public int FieldId { get; set; }
            public string ColMap { get; set; }
            public string Value { get; set; }
        }

        public enum POExtraClass_Fieldids{
            Discount = 3,
            VAT=58

        }

        #region swaggerExample
        public class POExample : IExamplesProvider<PO.Request>
        {
            public PO.Request GetExamples()
            {
                PO.Request pos = new PO.Request();
                List<PO.Data> poList = new List<PO.Data>();
                List<PO.Body> bodies = new List<PO.Body>();
                //Body
                PO.Body body = new PO.Body()
                {
                    Branch__Mstr = "01",
                    Investment_Class__Mstr = "NA",
                    Item_Category__Mstr = "ADMIN",
                    Request_Type__Mstr = "",
                    Activity_Master__Mstr = "",
                    Purchase_Request_No__Mstr = "PRQ-NBC-2025-00001",
                    Item__Mstr = "P-03-03-001",
                    Description = "GMC - Car Insurance Renewal",
                    TaxCode__Mstr = "SR-REC",
                    PurchaseAC__AcMstr = "5-50-500302-00002",
                    Quantity = 1,
                    Rate = 4652.93,
                   
                    sRemarks = "",
                    Purpose_Of_Request = "",
                    Exact_Date__Date = "",
                    Expected_Date_of_Service_ = 0,
                    Project_Master__Mstr = "NA",
                   // Per__obj__Discount = 0,
                    Val__obj__Discount = 0,
                    Per__obj__VAT = 5,
                    Val__obj__VAT = 20


                };
                bodies.Add(body);
                Data data = new Data
                {
                    //Header
                    DocNo = "POO-NBC-2025-00596",
                    Date__Date = "28/08/2025",
                    VendorAC__AcMstr= "2-21-210400-00888",
                    Currency__CurMstr="AED",
                    ExchangeRate= 1,
                    Entity__Mstr="NBC",
                    Jurisdiction__Mstr="DXB",
                    Place_of_supply__Mstr="DXB",
                    Department_Cost_Centre__Mstr= "08-01",
                    Campaign__Mstr ="",
                    Requested_By__EmpMstr= "NBC016",
                    Payment_Terms= "30 Days",
                    Delivery_Terms= "2-3 Days",
                    Title= "Car Insurance Renewal",
                    Reason_for_Request= "Insurance Renewal for Company Car - GMC",
                    sNarration="",
                   PO_Type= "LPO",
                    Body =bodies
        

                };
                pos.data = new List<Data>();
                pos.data.Add(data);
                return pos;

            }
        }
        #endregion
    }

}
