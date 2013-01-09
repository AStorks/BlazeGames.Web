using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using BlazeGames.Web.Core;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace BlazeGames.Web
{
    public partial class _default : System.Web.UI.Page
    {
        MySqlConnection SqlConnection;
        System.Web.HttpCookie BGxCookie;

        bool FatalOnce = false;

        string GetFromResources(string resourceName)
        {
            Assembly assem = this.GetType().Assembly;
            using (Stream stream = assem.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        DynamicPages PageSys;

        protected void Page_Load(object sender, EventArgs e)
        {
            int i = 0;

            while(true)
            {
                i++;

                try
                {
                    Response.Clear();

                    ErrorManager.Initialize(Member.Null());
                    if (Utilities.isMobileBrowser())
                        Utilities.MoveToSecure();

                    DateTime startTime = DateTime.Now;

                    string HTTP_Protocol = (Request.IsSecureConnection) ? "https://" : "http://";
                    Uri PageUri = new Uri(HTTP_Protocol + Request.Url.Host + Request.RawUrl);

                    try
                    {
                        SqlConnection = new MySqlConnection("Server=localhost;Uid=root;Pwd=hl1vlAbR9a3Riu;database=blazegameshome5;Pooling=true;Min Pool Size=5;Max Pool Size=60;");
                        SqlConnection.Open();
                    }
                    catch (MySql.Data.MySqlClient.MySqlException)
                    {
                        MySqlConnection.ClearAllPools();

                        SqlConnection = new MySqlConnection("Server=localhost;Uid=root;Pwd=hl1vlAbR9a3Riu;database=blazegameshome5");
                        SqlConnection.Open();
                    }

                    #region Build PageEditor If Needed
                    BlazeGames.Web.Core.Page PageEditor = new BlazeGames.Web.Core.Page(1, SqlConnection);
                    if (PageEditor.Code != File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/bin/PageEditor.cs"))
                    {
                        PageEditor.Code = File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/bin/PageEditor.cs");
                        System.CodeDom.Compiler.CompilerErrorCollection errors = PageEditor.Compile();

                        if (errors.Count >= 1)
                            ErrorManager.Error(errors[0].ErrorText);
                    }
                    #endregion

                    ErrorManager.Initialize(SqlConnection);

                    string NewSessionKey = "";

                    while (true)
                    {
                        NewSessionKey = Guid.NewGuid().ToString();
                        MySqlCommand SessionVerifyQuery = new MySqlCommand("SELECT ID FROM members WHERE WebSessionKey=@WebSessionKey", SqlConnection);
                        SessionVerifyQuery.Parameters.AddWithValue("@WebSessionKey", NewSessionKey);
                        MySqlDataReader SessionVerifyReader = SessionVerifyQuery.ExecuteReader();

                        if (!SessionVerifyReader.Read())
                        {
                            SessionVerifyReader.Close();
                            break;
                        }
                        else
                        {
                            SessionVerifyReader.Close();
                            continue;
                        }
                    }

                    if (Request.Cookies["BlazeGames"] == null)
                    {
                        BGxCookie = new System.Web.HttpCookie("BlazeGames");
                        BGxCookie.Values.Add("WebSession", NewSessionKey);
                        BGxCookie.Expires = DateTime.Now.AddDays(7);
                        BGxCookie.Domain = ".blaze-games.com";
                        Response.Cookies.Add(BGxCookie);
                    }
                    else
                    {
                        BGxCookie = Request.Cookies["BlazeGames"];

                        System.Web.HttpCookie BGxCookieNew = new System.Web.HttpCookie("BlazeGames");
                        BGxCookieNew.Values.Add("WebSession", BGxCookie.Values["WebSession"]);
                        BGxCookieNew.Expires = DateTime.Now.AddDays(7);
                        BGxCookieNew.Domain = ".blaze-games.com";
                        Response.Cookies.Add(BGxCookieNew);

                        BGxCookie = BGxCookieNew;
                    }

                    //Response.Write(BGxCookie.Values["WebSession"] + "<br />" + Request.UserHostAddress);
                    //Response.End();

                    Member LoggedInMember;

                    if (Utilities.GET("Account") != "" && Utilities.GET("Password") != "")
                    {
                        string Account = Utilities.GET("Account"),
                            Password = Utilities.GET("Password");

                        if (Member.TryLoginWithPassword(Account, Password, SqlConnection))
                            LoggedInMember = new Member(Account, SqlConnection);
                        else
                            LoggedInMember = new Member(BGxCookie.Values["WebSession"], Request.UserHostAddress, SqlConnection);
                    }
                    else
                        LoggedInMember = new Member(BGxCookie.Values["WebSession"], Request.UserHostAddress, SqlConnection);

                    if (Utilities.POST("Account") != "" && Utilities.POST("Password") != "" && Utilities.POST("Act") == "Login" && !LoggedInMember.IsValid)
                    {
                        if (!Member.Login(Utilities.POST("Account"), Utilities.POST("Password"), BGxCookie.Values.Get("WebSession"), Request.UserHostAddress, SqlConnection))
                            ErrorManager.Fatal("Login Failed!\r\n<a href='" + PageUri.AbsolutePath + "'>Go Back</a><script>setTimeout('location.href = \"" + PageUri.AbsolutePath + "\";', 2500);</script>", "Account: " + Utilities.POST("Account") + "<br />Hash: " + Member.HashPassword(Utilities.POST("Password")));
                        else
                        {
                            LoggedInMember.Load();
                            ErrorManager.Initialize(LoggedInMember);

                            ErrorManager.Message("Login Complete!\r\n<a href='" + PageUri.AbsolutePath + "'>Continue</a><script>setTimeout('location.href = \"" + PageUri.AbsolutePath + "\";', 2500);</script>");
                        }
                    }
                    else
                        if (Utilities.GET("Act") == "Logout")
                        {
                            BGxCookie = new System.Web.HttpCookie("BlazeGames");
                            BGxCookie.Values.Add("WebSession", Guid.NewGuid().ToString());
                            BGxCookie.Domain = ".blaze-games.com";
                            BGxCookie.Expires = DateTime.Now.AddMonths(1);
                            Response.Cookies.Add(BGxCookie);

                            ErrorManager.Initialize(new Member("", "", SqlConnection));
                            ErrorManager.Message("Logout Complete!\r\n<a href='" + PageUri.AbsolutePath + "'>Go Back</a><script>setTimeout('location.href = \"" + PageUri.AbsolutePath + "\";', 2500);</script>");
                        }
                    LoggedInMember.Load();
                    ErrorManager.Initialize(LoggedInMember);

                    Logging Log = new Logging(SqlConnection, LoggedInMember);
                    HttpHeader HttpHead = new HttpHeader();
                    Core.Events Event = new Core.Events();

                    PageSys = new DynamicPages(PageUri.AbsolutePath, SqlConnection, LoggedInMember, Log, HttpHead, Event);
                    PageSys.onPageInitialize();
                    PageSys.onPageLoad();

                    string Buffer = "";

                    if (Utilities.isMobileApps())
                        Buffer = File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/MobileApps/index.txt");
                    else if (Utilities.isMobileBrowser())
                        Buffer = File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Mobile/index.txt");
                    else
                        Buffer = File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/" + HttpHead.Theme + "/index.txt");

                    Buffer = Buffer.Replace("<!--{PageHeader}-->", HttpHead.ToString() + "\r\n<style type='text/css'>\r\n" + PageSys.CurrentPage.PageCSS + "\r\n</style>\r\n" + "<script type='text/javascript'>\r\n" + PageSys.CurrentPage.PageJS + "\r\n</script>");
                    Buffer = Buffer.Replace("<!--{PageContents}-->", PageSys.CurrentPage.PageHTML);
                    Buffer = Buffer.Replace("<!--{PageCode}-->", PageSys.onPageReturn());
                    Buffer = Buffer.Replace("<!--{WIDGET_Clock}-->", File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Default/Widgets/Clock.txt"));
                    if (LoggedInMember.IsValid)
                        Buffer = Buffer.Replace("<!--{WIDGET_Login}-->", File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Default/Widgets/Login_Member.txt"));
                    else
                        Buffer = Buffer.Replace("<!--{WIDGET_Login}-->", File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Default/Widgets/Login_Guest.txt"));
                    Buffer = Buffer.Replace("<!--{WIDGET_Social)-->", File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Default/Widgets/Social.txt"));
                    Buffer = Buffer.Replace("<!--{WIDGET_Posts}-->", File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Default/Widgets/Posts.txt"));

                    Buffer = Buffer.Replace("<!--{Time}-->", Utilities.GetDateTime());
                    Buffer = Buffer.Replace("<!--{Nickname}-->", LoggedInMember.Nickname);
                    Buffer = Buffer.Replace("<!--{ProfileImage}-->", LoggedInMember.GetProfileImage());
                    Buffer = Buffer.Replace("<!--{Url_Secure)-->", Utilities.GetCurrentUrl(true));
                    Buffer = Buffer.Replace("<!--{PageName}-->", PageSys.PageURL);

                    foreach (string ParamKey in HttpHead.ThemeParameters.Keys)
                        Buffer = Buffer.Replace("{" + ParamKey + "}", HttpHead.ThemeParameters[ParamKey]);
                    Buffer = Buffer.Replace("{ThemePath}", "/Themes/" + HttpHead.Theme + "/");

                    DateTime stopTime = DateTime.Now;
                    TimeSpan duration = stopTime - startTime;

                    SqlConnection.CancelQuery(100);
                    SqlConnection.Close();
                    //SqlConnection.Dispose();

                    Response.Write(@"<!--

------------------------------------------
---- Blaze Games Web v5 Debug Console ----
------------------------------------------
Script Execution Time: " + duration.Milliseconds + @"MS
sqlConnection State: " + SqlConnection.State + @"

-->
");

                    Response.Write(Buffer);
                    Response.Flush();
                    Response.End();

                    PageSys.onPageUnLoad();

                    break;
                }
                catch (Exception ex) { if (i == 5) { throw ex; } continue; }
            }
        }

        private void Page_Unload(object sender, EventArgs e)
        {
            ErrorManager.Initialize(Member.Null());
        }

        private void Page_Error(object sender, EventArgs e)
        {
            Exception Error = Server.GetLastError();
            string HTTP_Protocol = (Request.IsSecureConnection) ? "https://" : "http://";

            Utilities.SendEmail("ashton@blzgms.co", "Site Exception", "URL: " + HTTP_Protocol + Request.Url.Host + Request.RawUrl + "\r\n\r\n" + Error.GetBaseException().ToString());

            if (Error.GetType().ToString() == "System.NullReferenceException" && !FatalOnce)
            {
                FatalOnce = true;
                Response.Clear();
                Response.ClearHeaders();

                MySqlConnection.ClearAllPools();

                
                Response.Redirect(HTTP_Protocol + Request.Url.Host + Request.RawUrl);
            }
            else
                ErrorManager.Fatal(Error.GetBaseException());
        }
    }
}