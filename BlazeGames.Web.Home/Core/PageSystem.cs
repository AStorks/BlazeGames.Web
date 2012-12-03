using System.Web;
using System.Reflection;
using MySql.Data.MySqlClient;

namespace BlazeGames.Web.Core
{
    public class DynamicPages
    {
        public Page CurrentPage;
        public string PageURL;
        private object PageObject;
        private Member LoggedInMember;
        private Logging Log;
        private HttpHeader HttpHead;
        private Events Event;

        private MySqlConnection SqlConnection;

        public DynamicPages(string PageUrl, MySqlConnection SqlConnection, Member LoggedInMember, Logging Log, HttpHeader HttpHead, Events Event)
        {
            this.LoggedInMember = LoggedInMember;
            this.Log = Log;
            this.SqlConnection = SqlConnection;
            this.HttpHead = HttpHead;
            this.Event = Event;

            #region PageUrl
            if (PageUrl == "/")
                PageUrl = "/home/";
            else
                PageUrl = PageUrl.ToLower();
            if (!PageUrl.StartsWith("/"))
                PageUrl = "/" + PageUrl;
            if (!PageUrl.EndsWith("/"))
                PageUrl = PageUrl + "/";
            this.PageURL = PageUrl;
            #endregion

            string Domain = HttpContext.Current.Request.Url.Host.ToLower();
            CurrentPage = new Page(PageUrl, Domain, SqlConnection);

            if (CurrentPage.Exists)
            {
                if (CurrentPage.RequireSecure || LoggedInMember.RequestSecure)
                    Utilities.MoveToSecure();

                if (!CurrentPage.HtmlPage)
                {
                    if (CurrentPage.Compiled)
                    {
                        PageObject = Assembly.Load(CurrentPage.CompiledCode).CreateInstance("BlazeGames.Web." + "DynamicPage");
                        onPagePreInitialize();
                        if (!onPageAuthenticate())
                        {
                            Event.Call("AuthenticateFailure()");
                            ErrorManager.Error("Please login to view this page.");
                        }
                        else
                            Event.Call("AuthenticateSuccess()");
                    }
                    else
                    {
                        if (LoggedInMember.Authority >= 5)
                            ErrorManager.Message("<a href='/Admin/Pages/?Act=Compile&ID=" + CurrentPage.ID + "'>This page needs compiled first.</a>");
                        else
                            ErrorManager.Error("A developer needs to compile this page first.");
                    }
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                ErrorManager.Message("<img onclick='window.location=\"/Home/\";' style='margin-left:-10px;margin-top:-10px;margin-bottom:-14px;cursor:pointer;' src='https://c326078.ssl.cf1.rackcdn.com/404.png' alt='Error 404, The page you requested was not found on our server.' />");
            }
        }

        public bool onPageAuthenticate()
        {
            if (!CurrentPage.HtmlPage)
            {
                MethodInfo InitalizeMethod = PageObject.GetType().GetMethod("onPageAuthenticate");
                return (bool)InitalizeMethod.Invoke(PageObject, null);
            }
            return false;
        }

        public void onPagePreInitialize()
        {
            if (!CurrentPage.HtmlPage)
            {
                MethodInfo InitalizeMethod = PageObject.GetType().GetMethod("onPagePreInitialize");
                InitalizeMethod.Invoke(PageObject, new object[] { SqlConnection, HttpContext.Current, LoggedInMember, Log, CurrentPage, HttpHead, Event });
            }
        }

        public void onPageInitialize()
        {
            if (!CurrentPage.HtmlPage)
            {
                PageObject.GetType().GetMethod("onPageInitialize").Invoke(PageObject, null);
            }
        }

        public void onPageLoad()
        {
            if (!CurrentPage.HtmlPage)
                PageObject.GetType().GetMethod("onPageLoad").Invoke(PageObject, null);
        }

        public void onPageUnLoad()
        {
            if (!CurrentPage.HtmlPage)
                PageObject.GetType().GetMethod("onPageUnLoad").Invoke(PageObject, null);
        }

        public string onPageReturn()
        {
            if (!CurrentPage.HtmlPage)
            {
                Event.Call("PageLoadComplete()");
                return (string)PageObject.GetType().GetMethod("onPageReturn").Invoke(PageObject, null);
            }
            else
                return CurrentPage.Code;
        }
    }
}