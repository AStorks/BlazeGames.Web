using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MySql.Data.MySqlClient;
using System.Web;
using System.Drawing;

namespace BlazeGames.Web.Core
{
    public class ErrorManager
    {
        public static bool DetailedError = true;
        public static MySqlConnection SqlConnection;
        public static Member LoggedInMember;
        public static Color BackgroundColor = Color.FromArgb(243, 224, 221);
        public static Color ForegroundColor = Color.FromArgb(81, 81, 81);
        public static Color BorderColor = Color.FromArgb(222, 183, 183);

        public static void Initialize(MySqlConnection SqlConn)
        {
            LoggedInMember = null;
            SqlConnection = SqlConn;
            BackgroundColor = Color.FromArgb(243, 224, 221);
            ForegroundColor = Color.FromArgb(81, 81, 81);
            BorderColor = Color.FromArgb(222, 183, 183);
        }

        public static void Initialize(Member LoggedInMem)
        {
            LoggedInMember = LoggedInMem;
            BackgroundColor = Color.FromArgb(243, 224, 221);
            ForegroundColor = Color.FromArgb(81, 81, 81);
            BorderColor = Color.FromArgb(222, 183, 183);
        }

        public static void Fatal(string msg)
        {
            Fatal(msg, "No detailed information was found.", "Fatal Error");
        }

        public static void Fatal(string msg, string detail)
        {
            Fatal(msg, detail, "Fatal Error");
        }

        public static void Fatal(Exception ex)
        {
            Fatal(ex, "No source avaliable.");
        }

        public static void Fatal(Exception ex, string Source)
        {
            HttpContext.Current.Response.StatusCode = 500;

            try
            {
                try
                {
                    if (ex.GetBaseException().GetType().ToString() == "MySql.Data.MySqlClient.MySqlException")
                        MySqlConnection.ClearAllPools();
                }
                catch { }

                Fatal("[" + ex.GetBaseException().GetType().ToString() + "]" + ex.GetBaseException().Message, ex.GetBaseException().ToString(), "Fatal " + ex.GetBaseException().GetType().ToString(), Source);
            }
            catch
            {
                try
                {
                    if (ex.GetType().ToString() == "MySql.Data.MySqlClient.MySqlException")
                        MySqlConnection.ClearAllPools();
                }
                catch { }

                Fatal("[" + ex.GetType().ToString() + "]" + ex.Message, ex.ToString(), "Fatal " + ex.GetType().ToString(), Source);
            }
        }

        public static void Fatal(string Message, string Detail, string Title)
        {
            Fatal(Message, Detail, Title, "No source avaliable.");
        }

        public static void Fatal(string Message, string Detail, string Title, string Source)
        {
            Message = Message.Replace("\r", "<br>");
            Detail = Detail.Replace("\r", "<br>");
            Source = Source.Replace("\r", "<br>").Replace("  ", "&nbsp;&nbsp;").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;").Replace("_", " ");

            string Buffer = File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Error.txt");
            Buffer = Buffer.Replace("<!--{Title}-->", Title);
            if (DetailedError)
                Buffer = Buffer.Replace("<!--{Message}-->", Message + "<br /><br />" + Detail);
            else
            {
                if(Utilities.isMobileBrowser())
                    Buffer = Buffer.Replace("<!--{Message}-->", "<div id='main' style='width:100%; margin-left:-11px; padding:10px; border: 1px solid #CCC;'>" + Message + "</div>");
                else
                    Buffer = Buffer.Replace("<!--{Message}-->", Message);
            }
            Buffer = Buffer.Replace("<!--{BackgroundColor}-->", System.Drawing.ColorTranslator.ToHtml(BackgroundColor));
            Buffer = Buffer.Replace("<!--{ForegroundColor}-->", System.Drawing.ColorTranslator.ToHtml(ForegroundColor));
            Buffer = Buffer.Replace("<!--{BorderColor}-->", System.Drawing.ColorTranslator.ToHtml(BorderColor));

            string Buffer2 = "";

            if (Utilities.isMobileBrowser())
                Buffer2 = File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Mobile/index.txt");
            else
                Buffer2 = File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Default/index.txt");

            Buffer2 = Buffer2.Replace("<!--{PageContents}-->", Buffer);
            Buffer2 = Buffer2.Replace("<!--{WIDGET_Clock}-->", File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Default/Widgets/Clock.txt"));
            try
            {
                if (LoggedInMember.IsValid)
                    Buffer2 = Buffer2.Replace("<!--{WIDGET_Login}-->", File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Default/Widgets/Login_Member.txt"));
                else
                    Buffer2 = Buffer2.Replace("<!--{WIDGET_Login}-->", File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Default/Widgets/Login_Guest.txt"));
            }
            catch
            {
                Buffer2 = Buffer2.Replace("<!--{WIDGET_Login}-->", File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Default/Widgets/Login_Guest.txt"));
            }
            Buffer2 = Buffer2.Replace("<!--{WIDGET_Social)-->", File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Default/Widgets/Social.txt"));
            Buffer2 = Buffer2.Replace("<!--{WIDGET_Posts}-->", File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Themes/Default/Widgets/Posts.txt"));

            Buffer2 = Buffer2.Replace("<!--{Time}-->", Utilities.GetDateTime());
            Buffer2 = Buffer2.Replace("<!--{Url_Secure)-->", Utilities.GetCurrentUrl(true));
            try
            {
                Buffer2 = Buffer2.Replace("<!--{Nickname}-->", LoggedInMember.Nickname);
                Buffer2 = Buffer2.Replace("<!--{ProfileImage}-->", LoggedInMember.GetProfileImage());
            }
            catch { }

            try
            {
                SqlConnection.Close();
            }
            catch { }

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Write(Buffer2);
            LoggedInMember = Member.Null();
            HttpContext.Current.Response.End();
        }

        public static void Message(string Message)
        {
            ErrorManager.BackgroundColor = System.Drawing.ColorTranslator.FromHtml("#DCECF2");
            ForegroundColor = Color.FromArgb(81, 81, 81);
            ErrorManager.BorderColor = System.Drawing.ColorTranslator.FromHtml("#B8C1DB");
            Fatal(Message, "No detailed information was found.", "Fatal Error", "No source avaliable.");
        }

        public static void Error(string Message)
        {
            BackgroundColor = Color.FromArgb(243, 224, 221);
            ForegroundColor = Color.FromArgb(81, 81, 81);
            BorderColor = Color.FromArgb(222, 183, 183);
            Fatal(Message, "No detailed information was found.", "Fatal Error", "No source avaliable.");
        }

        public static void Warn(string Message)
        {
            ErrorManager.BackgroundColor = System.Drawing.ColorTranslator.FromHtml("#EAE7D3");
            ForegroundColor = Color.FromArgb(81, 81, 81);
            ErrorManager.BorderColor = System.Drawing.ColorTranslator.FromHtml("#EADD79");
            Fatal(Message, "No detailed information was found.", "Fatal Error", "No source avaliable.");
        }
    }
}