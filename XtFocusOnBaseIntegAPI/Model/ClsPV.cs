using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;

namespace XtFocusOnBaseIntegAPI.Model
{
    public class PV
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
            /// <summary>
            /// dd/MM/yyyy format
            /// </summary>
            [JsonProperty("Due Date")]
            public string DueDate__Date { get; set; }
        
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

            /// <summary>
            /// includes value [ LPO,Agreement,Contract]
            /// </summary>
            [JsonProperty("PO Type")]
            public string PO_Type { get; set; }

            [JsonProperty("Narration")]
            public string sNarration { get; set; }

            [JsonProperty("Supplier Invoice No")]
            public string Supplier_Invoice_No { get; set; }
            /// <summary>
            /// dd/MM/yyyy format
            /// </summary>
            [JsonProperty("Supplier Invoice Date")]
            public string Supplier_Invoice_Date__Date { get; set; }



            public decimal Net { get; set; }
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
            public string Purchase_Request_No__TrnMstr { get; set; }//transaction master,ivouchertype is conactenated after the string "Trmstr"
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
            [JsonProperty("Discount_Value")]
            public double Discount__obj__Val { get; set; }
            //[JsonProperty("Discount%")]
            //public double Discount__obj__Per { get; set; } //add class for __obj
            [JsonProperty("VAT_Value")]
            public double VAT__obj__Val { get; set; } //add class for __obj
            [JsonProperty("VAT%")]
            public double VAT__obj__Per { get; set; } //add class for __obj


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
            /// <summary>
            ///Purchase order doc no
            /// </summary>
            public string PONo { get; set; }


            [JsonProperty("Department Cost Centre")]
            public string Department_Cost_Centre__Mstr { get; set; }
        }

        public class PVExtraClass
        {
            public string Input { get; set; }
            public string FieldName { get; set; }
            public int FieldId { get; set; }
            public string ColMap { get; set; }
            public string Value { get; set; }
        }

        public enum PVExtraClass_Fieldids
        {
            Discount = 3,
            VAT = 58

        }
        public enum PVTrn_iVtype
        {
            Purchase_Request_No = 7939

        }
        public class PoRef
        {
            public string BaseTransactionId { get; set; }
            public int VoucherType { get; set; } = 2560;
            public string VoucherNo { get; set; }
            public double UsedValue { get; set; }
            public int LinkId { get; set; } = 167772928;
            public string RefId { get; set; }
        
        }
        public class PvRef
        {
            public string CustomerId { get; set; }
            public decimal Amount { get; set; }
            public string BillNo { get; set; }
            public int reftype { get; set; }
            public int mastertypeid { get; set; }
            public string Reference { get; set; }
            public int artag { get; set; }
            public int ref_ { get; set; }
            public int tag { get; set; }
        }

        #region swaggerExample
        public class PVExample : IExamplesProvider<PV.Request>
        {
            public PV.Request GetExamples()
            {
                PV.Request pos = new PV.Request();
                List<PV.Data> poList = new List<PV.Data>();
                List<PV.Body> bodies = new List<PV.Body>();
                //Body
                PV.Body body = new PV.Body()
                {
                    Branch__Mstr = "01",
                    Investment_Class__Mstr = "04",
                    Item_Category__Mstr = "ADMIN",
                    Request_Type__Mstr = "",
                    Activity_Master__Mstr = "",
                    Purchase_Request_No__TrnMstr = "PRQ-NBC-2025-00001",
                    Item__Mstr = "P-03-03-001",
                    Description = "SAX HEAVY DUTY STAPLER FOR 200SHT",
                    TaxCode__Mstr = "SR-REC",
                    PurchaseAC__AcMstr = "5-50-500807-00003",
                    Quantity = 1,
                    Rate = 400,
                 //   Discount__obj__Per = 0,
                    Discount__obj__Val = 0,
                    VAT__obj__Per = 5,
                    VAT__obj__Val = 20,
                    sRemarks = "",
                    Purpose_Of_Request = "",
                    Exact_Date__Date = "",
                    Expected_Date_of_Service_ = 0,
                    Project_Master__Mstr = "",
                    Department_Cost_Centre__Mstr = "07-01",
                    PONo = "POO-NBC-2025-00598"



                };
                bodies.Add(body);
                Data data = new Data
                {
                    //Header
                    DocNo = "PVV-NBC-2025-00698",
                    Date__Date = "03/07/2025",
                    VendorAC__AcMstr = "2-21-210400-00004",
                    Currency__CurMstr = "AED",
                    ExchangeRate = 1,
                    Entity__Mstr = "NBC",
                    Jurisdiction__Mstr = "DXB",
                    Place_of_supply__Mstr = "DXB",
                   
                    Campaign__Mstr = "",
                    Requested_By__EmpMstr = "NBC003",
                    Payment_Terms = "30 Days",
                    Delivery_Terms = "2-3 Days",
                    Title = "Stationery Items Request - Investment Department",
                    Reason_for_Request = "Stationery Items Request - Investment Department",
                    sNarration = "",
                    PO_Type = "LPO",
                    Supplier_Invoice_Date__Date="03/07/2025",
                    Supplier_Invoice_No="00000125",
                    DueDate__Date="25/07/2025",
                    Body = bodies


                };
                pos.data = new List<Data>();
                pos.data.Add(data);
                return pos;

            }
        }
        #endregion
    }
}
