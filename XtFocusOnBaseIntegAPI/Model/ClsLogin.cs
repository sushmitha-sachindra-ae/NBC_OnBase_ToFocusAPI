using Swashbuckle.AspNetCore.Filters;

namespace XtFocusOnBaseIntegAPI.Model
{
    public class ClsLogin
    {
        public string username { get; set; }
        public string password { get; set; }
        public string companycode { get; set; }
    }
    public class ClsLoginExample : IExamplesProvider<ClsLogin>
    {
        public ClsLogin GetExamples()
        {
            return new ClsLogin
            {
                username = "apiOnbase",
                password = "focus@123",
                companycode = "020"
            };
        }
    }

}
