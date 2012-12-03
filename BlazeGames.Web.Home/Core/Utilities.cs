using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.Net.Mail;
using JdSoft.Apple.Apns.Notifications;
//using MoonAPNS;

namespace BlazeGames.Web.Core
{
    public class Utilities
    {
        public static string[] Countries = new string[] { "United States of America", "Canada", "United Kingdom", " ", "Afghanistan", "Albania", "Algeria", "American Samoa", "Andorra", "Angola", "Anguilla", "Antigua & Barbuda", "Argentina", "Armenia", "Aruba", "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bermuda", "Bhutan", "Bolivia", "Bonaire", "Bosnia & Herzegovina", "Botswana", "Brazil", "British Indian Ocean Ter", "Brunei", "Bulgaria", "Burkina Faso", "Burundi", "Cambodia", "Cameroon", "Canary Islands", "Cape Verde", "Cayman Islands", "Central African Republic", "Chad", "Channel Islands", "Chile", "China", "Christmas Island", "Cocos Island", "Colombia", "Comoros", "Congo", "Cook Islands", "Costa Rica", "Cote D'Ivoire", "Croatia", "Cuba", "Curacao", "Cyprus", "Czech Republic", "Denmark", "Djibouti", "Dominica", "Dominican Republic", "East Timor", "Ecuador", "Egypt", "El Salvador", "Equatorial Guinea", "Eritrea", "Estonia", "Ethiopia", "Falkland Islands", "Faroe Islands", "Fiji", "Finland", "France", "French Guiana", "French Polynesia", "French Southern Ter", "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Gibraltar", "Great Britain", "Greece", "Greenland", "Grenada", "Guadeloupe", "Guam", "Guatemala", "Guinea", "Guyana", "Haiti", "Honduras", "Hong Kong", "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq", "Ireland", "Isle of Man", "Israel", "Italy", "Jamaica", "Japan", "Jordan", "Kazakhstan", "Kenya", "Kiribati", "Korea North", "Korea South", "Kuwait", "Kyrgyzstan", "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania", "Luxembourg", "Macau", "Macedonia", "Madagascar", "Malaysia", "Malawi", "Maldives", "Mali", "Malta", "Marshall Islands", "Martinique", "Mauritania", "Mauritius", "Mayotte", "Mexico", "Midway Islands", "Moldova", "Monaco", "Mongolia", "Montserrat", "Morocco", "Mozambique", "Myanmar", "Nambia", "Nauru", "Nepal", "Netherland Antilles", "Netherlands (Holland, Europe)", "Nevis", "New Caledonia", "New Zealand", "Nicaragua", "Niger", "Nigeria", "Niue", "Norfolk Island", "Norway", "Oman", "Pakistan", "Palau Island", "Palestine", "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Pitcairn Island", "Poland", "Portugal", "Puerto Rico", "Qatar", "Republic of Montenegro", "Republic of Serbia", "Reunion", "Romania", "Russia", "Rwanda", "St Barthelemy", "St Eustatius", "St Helena", "St Kitts-Nevis", "St Lucia", "St Maarten", "St Pierre & Miquelon", "St Vincent & Grenadines", "Saipan", "Samoa", "Samoa American", "San Marino", "Sao Tome & Principe", "Saudi Arabia", "Senegal", "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands", "Somalia", "South Africa", "Spain", "Sri Lanka", "Sudan", "Suriname", "Swaziland", "Sweden", "Switzerland", "Syria", "Tahiti", "Taiwan", "Tajikistan", "Tanzania", "Thailand", "Togo", "Tokelau", "Tonga", "Trinidad & Tobago", "Tunisia", "Turkey", "Turkmenistan", "Turks & Caicos Is", "Tuvalu", "Uganda", "Ukraine", "United Arab Emirates", "Uruguay", "Uzbekistan", "Vanuatu", "Vatican City State", "Venezuela", "Vietnam", "Virgin Islands (Brit)", "Virgin Islands (USA)", "Wake Island", "Wallis & Futana Is", "Yemen", "Zaire", "Zambia", "Zimbabwe" };

        public static string MD5(string input)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string GetForumDate(DateTime Time)
        {
            if (Time.Date == DateTime.Now.Date)
                return "Today, " + Time.ToString("h:mm tt");
            else
                return Time.ToString("d MMM yyyy");
        }

        public static string GetPostDate(DateTime Time)
        {
            if (Time.Date == DateTime.Now.Date)
                return "Today, " + Time.ToString("h:mm tt");
            if(Time.Year == DateTime.Now.Year)
                return Time.ToString("dd MMMM, h:mm tt");
            else
                return Time.ToString("dd MMMM yyyy, h:mm tt");
        }

        public static string GetDateTime()
        {
            return DateTime.Now.ToString("dddd MMMM d, yyyy h") + "<span id='ts'>:</span>" + DateTime.Now.ToString("mmtt");
        }

        public static string GetSortableDate(DateTime Time)
        {
            return Time.ToString("yyyyMMddHHmmss");
        }

        public static string GetTimeSpan(TimeSpan ts)
        {
            if (ts.TotalSeconds <= 10)
                return "a few seconds ago";
            else if (ts.TotalSeconds < 60)
                return ts.Seconds + " seconds ago";
            else if (ts.TotalMinutes < 60)
                if (ts.Minutes == 1)
                    return ts.Minutes + " minute ago";
                else
                    return ts.Minutes + " minutes ago";
            else if (ts.TotalHours < 24)
                if(ts.Hours == 1)
                    return ts.Hours + " hour ago";
                else
                    return ts.Hours + " hours ago";
            else if (ts.TotalDays < 365)
            {
                if(ts.Days == 1)
                    return ts.Days + " day ago";
                else
                    return ts.Days + " days ago";
            }
            else
                return "over a year ago";
        }

        public static string GET(string key)
        {
            HttpContext Http = HttpContext.Current;

            foreach (string GetKey in Http.Request.QueryString.AllKeys)
            {
                try
                {
                    if (GetKey.Split('?')[1] == key)
                        return Http.Request.QueryString[GetKey];
                }
                catch { }
            }

            if (Http.Request.QueryString[key] != null)
                return Http.Request.QueryString[key];
            else
            {
                string[] urlparams = Http.Request.RawUrl.Split('/');
                foreach (string urlparam in urlparams)
                {
                    string[] urlparamarr = urlparam.Split('-');
                    if (urlparamarr.Length >= 2 && urlparamarr[0] == key)
                        return String.Join("-", urlparamarr, 1, (urlparamarr.Length - 1));
                    else if (urlparamarr.Length == 1 && urlparamarr[0] == key)
                        return "true";
                }

                return "";
            }
        }

        public static string POST(string key)
        {
            HttpContext Http = HttpContext.Current;

            if (Http.Request.Form[key] != null)
                return Http.Request.Form[key];
            else
                return "";
        }

        public static string Encode(string Input)
        {
            return HttpContext.Current.Server.HtmlEncode(Input);
        }

        public static string Decode(string Input)
        {
            return HttpContext.Current.Server.HtmlDecode(Input);
        }

        public static string CodeMirror(string Name, string Code)
        {
            return CodeMirror(Name, Code, "text/x-csharp");
        }

        private static HttpContext HttpC = null;

        public static HttpContext Http()
        {
            if (HttpC == null)
                HttpC = HttpContext.Current;

            return HttpC;
        }

        public static string CodeMirror(string Name, string Code, string Mode)
        {
            return "<style>#contents{ width:900px; } .container{ width:900px; } #sidebar{ display:none; } .Err_Highlight{ color: red; text-decoration:underline; }</style>" + @"<textarea id='" + Name + @"' cols=""120"" rows=""50"">" + Utilities.Encode(Code) + @"</textarea>
<script>
    var editor" + @" = CodeMirror.fromTextArea(document.getElementById('" + Name + @"'), {
    indentUnit: 4,
    indentWithTabs: true,
    lineNumbers: true,
    matchBrackets: true,
    mode: '" + Mode + @"',
    theme: ""lesser-dark"",
    fixedGutter: true,
    onChange: function(cm, ln) {
       Edited();
       CheckCodeThread('" + Mode + @"');
    },
    onGutterClick: function(cm, n) {
        scrollTo(n, 0);
    },
    extraKeys: {
            ""F11"": function(cm) {
              setFullScreen(cm, !isFullScreen(cm));
            },
            ""F5"": function()
            {
                CompilePage(false);
            },
            ""F6"": function()
            {
                CompilePage(false);
            },
            ""Ctrl-Q"": function()
            {
                window.location = '/Admin/Pages/';
            },
            ""Ctrl-D"": function()
            {
                alert('Debug unavaliable.');
            },
            ""Esc"": function(cm) {
                setFullScreen(cm, false);
            }
        }
    });

    CodeMirror.connect(window, ""resize"", function()
    {
          var showing = document.body.getElementsByClassName(""CodeMirror-fullscreen"")[0];
          if (!showing) return;
          showing.CodeMirror.getScrollerElement().style.height = winHeight() + ""px"";
    });

    init(editor);
</script>";
        }

        public static string CountryList(string Name)
        {
            return CountryList(Name, "");
        }

        public static string CountryList(string Name, string Selected)
        {
            string ret = "<select name='" + Name + "' class='dropdown' style=\"width: 200px;\">";

            foreach (string Country in Countries)
            {
                if(Country == " ")
                    ret += "<option value=''>-------------------------------</option>";
                else if (Country == Selected)
                    ret += "<option selected value='" + Country + "'>" + Country + "</option>";
                else
                    ret += "<option value='" + Country + "'>" + Country + "</option>";
            }

            ret += "</select>";

            return ret;
        }

        public static void MoveToSecure()
        {
            HttpContext Http = HttpContext.Current;
            if(!Http.Request.IsSecureConnection)
                Http.Response.Redirect(GetCurrentUrl(true));
        }

        public static string GetCurrentUrl(bool Secure=false)
        {
            HttpContext Http = HttpContext.Current;
            string UrlBuffer;

            if (Secure)
                UrlBuffer = "https://";
            else
                UrlBuffer = "http://";

            UrlBuffer += Http.Request.Url.Host;
            UrlBuffer += Http.Request.RawUrl;

            return UrlBuffer;
        }

        public static bool isMobileApps()
        {
            HttpContext context = HttpContext.Current;

            if (context.Request.RawUrl.ToLower().Contains("mobile/apps"))
                return true;

            return false;
        }

        public static bool isMobileBrowser()
        {
            //GETS THE CURRENT USER CONTEXT
            HttpContext context = HttpContext.Current;

            if (context.Request.Url.Host == "m.blaze-games.com")
                return true;
            //THEN TRY BUILT IN ASP.NT CHECK
            if (context.Request.Browser.IsMobileDevice)
                return true;
            //THEN TRY CHECKING FOR THE HTTP_X_WAP_PROFILE HEADER
            if (context.Request.ServerVariables["HTTP_X_WAP_PROFILE"] != null)
                return true;
            //THEN TRY CHECKING THAT HTTP_ACCEPT EXISTS AND CONTAINS WAP
            if (context.Request.ServerVariables["HTTP_ACCEPT"] != null && context.Request.ServerVariables["HTTP_ACCEPT"].ToLower().Contains("wap"))
                return true;
            //AND FINALLY CHECK THE HTTP_USER_AGENT 
            //HEADER VARIABLE FOR ANY ONE OF THE FOLLOWING
            if (context.Request.ServerVariables["HTTP_USER_AGENT"] != null)
            {
                //Create a list of all mobile types
                string[] mobiles =
                    new[]
                {
                    "midp", "j2me", "avant", "docomo", 
                    "novarra", "palmos", "palmsource", 
                    "240x320", "opwv", "chtml",
                    "pda", "windows ce", "mmp/", 
                    "blackberry", "mib/", "symbian", 
                    "wireless", "nokia", "hand", "mobi",
                    "phone", "cdm", "up.b", "audio", 
                    "SIE-", "SEC-", "samsung", "HTC", 
                    "mot-", "mitsu", "sagem", "sony"
                    , "alcatel", "lg", "eric", "vx", 
                    "NEC", "philips", "mmm", "xx", 
                    "panasonic", "sharp", "wap", "sch",
                    "rover", "pocket", "benq", "java", 
                    "pt", "pg", "vox", "amoi", 
                    "bird", "compal", "kg", "voda",
                    "sany", "kdd", "dbt", "sendo", 
                    "sgh", "gradi", "jb", "dddi", 
                    "moto", "iphone"
                };

                //Loop through each item in the list created above 
                //and check if the header contains that text
                foreach (string s in mobiles)
                {
                    if (context.Request.ServerVariables["HTTP_USER_AGENT"].
                                                        ToLower().Contains(s.ToLower()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool CheckCaptcha()
        {
            try
            {
                string challenge = "";
                string response = "";

                if (POST("recaptcha_challenge_field") != "")
                    challenge = POST("recaptcha_challenge_field");
                else
                    challenge = GET("recaptcha_challenge_field");

                if (POST("recaptcha_response_field") != "")
                    response = POST("recaptcha_response_field");
                else
                    response = GET("recaptcha_response_field");

                string Content = "privatekey=6LdrmskSAAAAAKLVqC1U31nHe-s7OdtQw-cyLgmO&remoteip=" + HttpContext.Current.Request.UserHostAddress + "&challenge=" + challenge + "&response=" + response;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.google.com/recaptcha/api/verify");
                request.UserAgent = "BlazeGames WebClient";
                request.Method = "POST";
                byte[] PostData = Encoding.Default.GetBytes(Content);
                request.ContentLength = PostData.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                request.GetRequestStream().Write(PostData, 0, PostData.Length);
                HttpWebResponse resp = (HttpWebResponse)request.GetResponse();

                StreamReader contentReader = new StreamReader(resp.GetResponseStream());
                return Convert.ToBoolean(contentReader.ReadLine());
            }
            catch { return false; }
        }

        public static byte[] ReadStreamToEnd(System.IO.Stream stream)
        {
            long originalPosition = stream.Position;
            stream.Position = 0;

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        #region Email
        public static void SendEmail(string Email, string Subject, string Body, bool IsBodyHtml=false)
        {
            SmtpClient mailClient = new SmtpClient("127.0.0.1", 25);
            MailMessage message = new MailMessage("Blaze Games <no-reply@blaze-games.com>", Email, Subject, Body);
            message.IsBodyHtml = IsBodyHtml;
            message.BodyEncoding = System.Text.Encoding.Default;
            message.SubjectEncoding = System.Text.Encoding.Default;

            mailClient.Send(message);
        }
        #endregion
        #region Mobile Notification
        public static void SendMobileNotification(string UDID, string Message)
        {
            var notificationService = new NotificationService(true, HttpContext.Current.Server.MapPath(".") + "/ios_sandbox.p12", "sedona7289", 1);

            var notification = new Notification(UDID);
            notification.Payload.Sound = "default";
            notification.Payload.Alert.Body = Message;

            notificationService.QueueNotification(notification);

            notificationService.Close();
            notificationService.Error += notificationService_Error;
            notificationService.Dispose();

        }

        static void notificationService_Error(object sender, Exception ex)
        {
            ErrorManager.Fatal(ex);
        }
        #endregion
    }
}