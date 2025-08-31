namespace XtFocusOnBaseIntegAPI.Model
{
    public class APIResponse
    {
        public class Login
        {
            public int iStatus { get; set; }
            public string Auth_Token { get; set; }
            public string sMessage { get; set; }
            public object obj { get; set; }
        }
    }
}
