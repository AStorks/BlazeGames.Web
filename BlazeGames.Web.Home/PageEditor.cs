using System;
using System.IO;
using System.Web;
using BlazeGames.Web.Core;
using MySql.Data.MySqlClient;
using System.CodeDom.Compiler;
using BlazeGames;
using System.Net.Mail;
using System.Collections.Generic;

namespace BlazeGames.Web
{
    public class DynamicPage : CodePage
    {
        public override void onPageInitialize()
        {
            HttpHead.AddCSS("https://blaze-games.com/codemirror/codemirror.css");
            HttpHead.AddCSS("https://blaze-games.com/codemirror/ambiance.css");
            HttpHead.AddCSS("https://blaze-games.com/codemirror/dialog.css");
            HttpHead.AddCSS("https://blaze-games.com/datatable/datatable.css");
            HttpHead.AddCSS("https://blaze-games.com/datatable/ui_custom.css");

            HttpHead.AddJS("https://ajax.googleapis.com/ajax/libs/jqueryui/1.8/jquery-ui.min.js");
            HttpHead.AddJS("https://blaze-games.com/codemirror/codemirror.js");
            HttpHead.AddJS("https://blaze-games.com/codemirror/clike.js");
            HttpHead.AddJS("https://blaze-games.com/codemirror/css.js");
            HttpHead.AddJS("https://blaze-games.com/codemirror/xml.js");
            HttpHead.AddJS("https://blaze-games.com/codemirror/htmlmixed.js");
            HttpHead.AddJS("https://blaze-games.com/codemirror/javascript.js");
            HttpHead.AddJS("https://blaze-games.com/codemirror/continuecomment.js");
            HttpHead.AddJS("https://blaze-games.com/codemirror/dialog.js");
            HttpHead.AddJS("https://blaze-games.com/codemirror/search.js");
            HttpHead.AddJS("https://blaze-games.com/codemirror/searchcursor.js");
            HttpHead.AddJS("https://blaze-games.com/datatable/datatable.js");

            if (Utilities.POST("Act") == "GetInfo" && Utilities.POST("ID") != "")
            {
                string PageInfo = "";
                Page InfoPage = new Page(Convert.ToInt32(Utilities.POST("ID")), SqlConnection);

                if (!InfoPage.Exists)
                    PageInfo += "No page found!";
                else
                {
                    TimeSpan LastUpdate = DateTime.Now - InfoPage.LastUpdate;
                    PageInfo += "ID: " + InfoPage.ID + "\r\n";
                    PageInfo += "Url: " + InfoPage.Url + "\r\n";
                    PageInfo += "Last Update: " + Utilities.GetTimeSpan(LastUpdate);
                }

                Http.Response.Write(PageInfo);
                Http.Response.End();
            }

            if (Utilities.POST("Act") == "CompileAll")
            {
                MySqlCommand PageFetchQuery = new MySqlCommand("SELECT ID FROM pages ORDER BY LastUpdate DESC", SqlConnection);
                MySqlDataReader PageFetchReader = PageFetchQuery.ExecuteReader();
                List<int> Pages = new List<int>();

                while (PageFetchReader.Read())
                {
                    int ID = PageFetchReader.GetInt32("ID");
                    Pages.Add(ID);
                }

                PageFetchReader.Close();

                foreach (int PageID in Pages)
                {
                    Page page = new Page(PageID, SqlConnection);
                    if(page.Locked)
                        Http.Response.Write(PageID + " locked.\r\n");
                    else if (page.Compile() == null)
                        Http.Response.Write(PageID + " compiled.\r\n");
                    else
                        Http.Response.Write(PageID + " compile failed.\r\n");
                }

                Http.Response.End();
            }

            if (Utilities.POST("Act") == "UpdatePage")
            {
                try
                {
                    if (Utilities.POST("ID") != "" && Utilities.POST("Url") != "" && Utilities.POST("Domain") != "" && Utilities.POST("References") != "" && Utilities.POST("MinimumAuthorization") != "" && Utilities.POST("RequireSecure") != "" && Utilities.POST("Code") != "")
                    {
                        Page EditPage = new Page(Convert.ToInt32(Utilities.POST("ID")), SqlConnection);
                        if (EditPage.Exists)
                        {
                            if (EditPage.IsEditing != LoggedInMember.ID)
                                Http.Response.Write("4");
                            else
                            {
                                EditPage.Url = Utilities.POST("Url").ToLower();
                                EditPage.Domain = Utilities.POST("Domain").ToLower();
                                EditPage.References = Utilities.POST("References").Split(',');
                                EditPage.MinimumAuthorization = Convert.ToInt32(Utilities.POST("MinimumAuthorization"));
                                EditPage.RequireSecure = Convert.ToBoolean(Utilities.POST("RequireSecure"));
                                EditPage.Code = Utilities.POST("Code");
                                EditPage.PageCSS = Utilities.POST("Code_CSS");
                                EditPage.PageJS = Utilities.POST("Code_JS");
                                EditPage.PageHTML = Utilities.POST("Code_HTML");

                                if (EditPage.Compile() == null)
                                    Http.Response.Write("0");
                                else 
                                    Http.Response.Write("3");
                            }
                        }
                        else
                            Http.Response.Write("1");

                    }
                    else
                        Http.Response.Write("2");
                }
                catch (Exception ex) { Http.Response.Write(ex.Message); }

                Http.Response.End();
            }

            if (Utilities.POST("Act") == "CreatePage")
            {
                if (Utilities.POST("Url") != "" && Utilities.POST("References") != "" && Utilities.POST("MinimumAuthorization") != "" && Utilities.POST("RequireSecure") != "" && Utilities.POST("Code") != "")
                {
                    Page CreatedPage = Page.Create(SqlConnection, Utilities.POST("Url").ToLower(), Utilities.POST("Domain").ToLower(), Utilities.POST("Code"), Utilities.POST("References").Split(','), Convert.ToInt32(Utilities.POST("MinimumAuthorization")), Convert.ToBoolean(Utilities.POST("RequireSecure")), Utilities.POST("Code_CSS"), Utilities.POST("Code_JS"), Utilities.POST("Code_HTML"));
                    Http.Response.Write(CreatedPage.ID.ToString());
                }
                else
                    Http.Response.Write("0");

                Http.Response.End();
            }

            if (Utilities.POST("Act") == "DeletePage" && Utilities.POST("ID") != "")
            {
                Page DeletePage = new Page(Convert.ToInt32(Utilities.POST("ID")), SqlConnection);
                DeletePage.Delete();
                Http.Response.End();
            }

            if (Utilities.POST("Act") == "UnlockPage" && Utilities.POST("ID") != "")
            {
                Page EditPage = new Page(Convert.ToInt32(Utilities.POST("ID")), SqlConnection);
                if (EditPage.Exists && EditPage.IsEditing != 0)
                {
                    EditPage.IsEditing = 0;
                    EditPage.Save();
                }

                Http.Response.End();
            }

            if (Utilities.POST("Act") == "VerifyCode")
            {
                if (Utilities.POST("Code") != "" && Utilities.POST("References") != "" && Utilities.POST("Type") != "")
                {
                    if (Utilities.POST("Type") == "text/x-csharp")
                    {
                        CodeDomProvider CodeCompiler = CodeDomProvider.CreateProvider("CSharp");
                        CompilerParameters CodeParameters = new CompilerParameters();
                        CodeParameters.CompilerOptions = "/lib:C:\\inetpub\\wwwroot\\bin";
                        CodeParameters.GenerateInMemory = false;

                        CodeParameters.ReferencedAssemblies.AddRange(Utilities.POST("References").Split(','));
                        CodeParameters.WarningLevel = 4;


                        /* Build the assembly */
                        string Code = Utilities.POST("Code");
                        BlazeGames_CodeCompiler BGxCodeCompiler = new BlazeGames_CodeCompiler();
                        Code = BGxCodeCompiler.CompileToCSharp(Code);

                        CompilerResults CodeCompiled = CodeCompiler.CompileAssemblyFromSource(CodeParameters, Code);

                        /* Check for errors */
                        if (CodeCompiled.Errors.HasErrors)
                        {
                            foreach (CompilerError Error in CodeCompiled.Errors)
                            {
                                if (Error.IsWarning)
                                    Http.Response.Write("2:");
                                else
                                    Http.Response.Write("1:");

                                Http.Response.Write(Error.Line + ":");
                                Http.Response.Write(Error.Column + ":");
                                Http.Response.Write(Error.ErrorText + "\r\n");
                            }
                        }
                        else
                            Http.Response.Write("0");
                    }
                    else if (Utilities.POST("Type") == "text/x-html")
                        Http.Response.Write("0");
                    else
                        Http.Response.Write("3");
                }
                else
                    Http.Response.Write("3");
                Http.Response.End();
            }
        }

        public override void onPageLoad()
        {
            if (Utilities.GET("Compile") == "true")
            {
                echo("<h1>Welcome " + LoggedInMember.Nickname + "</h1>");
                echo("<h3 style='margin-left:10px;'>There is currently " + Log.TotalVisitors() + " users viewing this website and " + Log.TotalMembers() + " registered members.</h3><br />");
                echo("<div style='float: right;'><a href='/Admin/Pages/'>Back</a></div><br /><br />");
                int PageID = Convert.ToInt32(Utilities.GET("ID"));
                Page EditPage = new Page(PageID, SqlConnection);

                System.CodeDom.Compiler.CompilerErrorCollection Errors = EditPage.Compile();
                if (Errors != null)
                {
                    foreach (System.CodeDom.Compiler.CompilerError Error in Errors)
                    {
                        if (Error.IsWarning)
                            echo("<span style='color:yellow'>" + Error.ErrorText + "</span><br />");
                        else
                            echo("<span style='color:red'>" + Error.ErrorText + " on line " + Error.Line + "</span><br />");
                    }
                }
                else
                    echo("<span style='color:green'>Compiled Successfully!</span>");
            }
            else if (Utilities.GET("NewPage") == "true")
            {
                echo(string.Format(@"<div class='boxcontainer'>
	<span class='header'>Welcome, {0}</span>
	
	<span class='links'>
		<a href='#' onclick='setFullScreen(editor, true);return false;'>Fullscreen</a>
		<a href='#' onclick='CreatePage(); return false;'>Create</a>
		<a href='/Admin/Pages/'>Quit</a>
	</span>
	
	<br />
	<br />
	There is currently {1} users viewing the website and {2} registered members.
</div>", LoggedInMember.Nickname, Log.TotalVisitors(), Log.TotalMembers()));

                echo("<div class='boxcontainer'>");
                echo("<a href='#' onclick='$(\".PageSettings\").animate({ height: \"toggle\" }); return false;'>Edit Settings</a><br /><span class='PageSettings'>");
                echo("Page Url:<br /><input style='width:150px;' class='txtbar' type='text' id='Url' onkeypress='Edited();' onchange='Edited();' value='' /><br />");
                echo("Page Domain:<br /><input style='width:150px;' class='txtbar' type='text' id='Domain' onkeypress='Edited();' onchange='Edited();' value='blaze-games.com' /><br />");
                echo("Page References:<br /><input style='width:150px;' class='txtbar' type='text' id='new_reference' />&nbsp;&nbsp;&nbsp;&nbsp;<a href='#' onclick='AddReference();return false;' title='Add Reference'>+</a> | <a href='#' onclick='RemoveReference();return false;' title='Remove Reference'>x</a><br />");
                echo("<select style='width:200px;' multiple='multiple' class='dropdown' id='References'>");
                echo("<option value='System.dll'>System.dll</option>");
                echo("<option value='System.dll'>System.Core.dll</option>");
                echo("<option value='System.Web.dll'>System.Web.dll</option>");
                echo("<option value='System.Data.dll'>System.Data.dll</option>");
                echo("<option value='mysql.data.dll'>mysql.data.dll</option>");
                echo("<option value='BlazeGames.Web.dll'>BlazeGames.Web.dll</option>");
                echo("</select><br />");
                echo("Page Authorization:<br /><input style='width:20px;' class='txtbar' type='text' id='MinimumAuthorization' onkeypress='Edited();' onchange='Edited();' value='0' /><br />");
                echo("RequireSecure:<br /><select style='width:75px;' class='dropdown' id='RequireSecure' onchange='Edited();'>");
                echo("<option value='False'>False</option><option value='True'>True</option>");
                echo("</select><br /><br />");
                //echo("HtmlPage: <input style='width:200px;' class='txtbar' type='text' name='' value='" + EditPage.HtmlPage + "' /><br />");
                echo("Page Code:<br /></span>");
                echo(Utilities.CodeMirror("maincode", Page.DefaultCode, "text/x-csharp", true, "ambiance"));
                echo("Page CSS:<br />");
                echo(Utilities.CodeMirror("csscode", Page.DefaultCSS, "text/css", false, "ambiance"));
                echo("Page Javascript:<br />");
                echo(Utilities.CodeMirror("jscode", Page.DefaultJS, "text/javascript", false, "ambiance"));
                echo("Page Html:<br />");
                echo(Utilities.CodeMirror("htmlcode", Page.DefaultHTML, "text/html", false, "ambiance"));
                echo("</div>");
            }
            else if (Utilities.GET("EditPage") == "true" && Utilities.GET("ID") != "")
            {
                int PageID = Convert.ToInt32(Utilities.GET("ID"));
                Page EditPage = new Page(PageID, SqlConnection);

                if (EditPage.IsEditing == 0)
                {
                    EditPage.IsEditing = LoggedInMember.ID;
                    EditPage.Save();
                }

                echo(string.Format(@"<div class='boxcontainer'>
	<span class='header'>Welcome, {0}</span>
	
	<span class='links'>
		<a href='#' onclick='setFullScreen(editor, true);return false;'>Fullscreen</a>
        <a href='#' onclick='UnlockPageNoRedirect({3}); return false;'>Unlock</a>
		<a href='#' onclick='DeletePage({3}); return false;'>Delete</a>
		<a href='#' onclick='CompilePage(false); return false;'>Compile</a>
		<a href='#' onclick='CompilePage(true); return false;'>Compile And Quit</a>
		<a href='/Admin/Pages/'>Quit</a>
	</span>
	
	<br />
	<br />
	There is currently {1} users viewing the website and {2} registered members.
</div>", LoggedInMember.Nickname, Log.TotalVisitors(), Log.TotalMembers(), EditPage.ID));

                if (EditPage.Exists)
                {
                    echo("<div class='boxcontainer'>");
                    echo("<a href='#' onclick='$(\".PageSettings\").animate({ height: \"toggle\" }); return false;'>Edit Settings</a><br /><span class='PageSettings'>");
                    echo("<span style='display:none;' id='ID'>" + EditPage.ID + "</span>");
                    echo("Page Url:<br /><input style='width:150px;' class='txtbar' type='text' id='Url' onkeypress='Edited();' onchange='Edited();' value='" + EditPage.Url + "' /><br />");
                    echo("Page Domain:<br /><input style='width:150px;' class='txtbar' type='text' id='Domain' onkeypress='Edited();' onchange='Edited();' value='" + EditPage.Domain + "' /><br />");
                    echo("Page References:<br /><input style='width:150px;' class='txtbar' type='text' id='new_reference' />&nbsp;&nbsp;&nbsp;&nbsp;<a href='#' onclick='AddReference();return false;' title='Add Reference'>+</a> | <a href='#' onclick='RemoveReference();return false;' title='Remove Reference'>x</a><br />");
                    echo("<select style='width:200px;' multiple='multiple' class='dropdown' id='References'>");
                    foreach (string Reference in EditPage.References)
                        echo("<option value='" + Reference + "'>" + Reference + "</option>");
                    echo("</select><br />");
                    echo("Page Authorization:<br /><input style='width:20px;' class='txtbar' type='text' id='MinimumAuthorization' onkeypress='Edited();' onchange='Edited();' value='" + EditPage.MinimumAuthorization + "' /><br />");
                    echo("RequireSecure:<br /><select style='width:75px;' class='dropdown' id='RequireSecure' onchange='Edited();'>");
                    echo("<option value='" + EditPage.RequireSecure + "'>" + EditPage.RequireSecure + "</option><option value='" + (!EditPage.RequireSecure) + "'>" + (!EditPage.RequireSecure) + "</option>");
                    echo("</select><br /><br />");
                    //echo("HtmlPage: <input style='width:200px;' class='txtbar' type='text' name='' value='" + EditPage.HtmlPage + "' /><br />");
                    echo("Page Code:<br /></span>");
                    echo(Utilities.CodeMirror("maincode", EditPage.Code, "text/x-csharp", true, "ambiance"));
                    echo("Page CSS:<br />");
                    echo(Utilities.CodeMirror("csscode", EditPage.PageCSS, "text/css", false, "ambiance"));
                    echo("Page Javascript:<br />");
                    echo(Utilities.CodeMirror("jscode", EditPage.PageJS, "text/javascript", false, "ambiance"));
                    echo("Page Html:<br />");
                    echo(Utilities.CodeMirror("htmlcode", EditPage.PageHTML, "text/html", false, "ambiance"));
                    echo("</form><script>$('.PageSettings').animate({ height: 'toggle' });</script>");
                    echo("</div>");
                }
                else
                    echo("<div class='boxcontainer'>Unable to find the page you want to edit.</div>");
            }
            else
            {
                echo(string.Format(@"<div class='boxcontainer'>
	<span class='header'>Welcome, {0}</span>
	
	<span class='links'>
		<a href='/Admin/'>Admin Home</a>
		<a href='/Admin/Pages/NewPage/'>New Page</a>
        <a href='#' onclick='CompileAll(); return false;'>Compile All Pages</a>
	</span>
	
	<br />
	<br />
	There is currently {1} users viewing the website and {2} registered members.
</div>", LoggedInMember.Nickname, Log.TotalVisitors(), Log.TotalMembers()));

                MySqlCommand PageFetchQuery = new MySqlCommand("SELECT * FROM pages ORDER BY LastUpdate DESC", SqlConnection);
                MySqlDataReader PageFetchReader = PageFetchQuery.ExecuteReader();
                List<object[]> Pages = new List<object[]>();

                while (PageFetchReader.Read())
                {
                    int ID = PageFetchReader.GetInt32("ID");
                    string Url = PageFetchReader.GetString("Url");
                    string Domain = PageFetchReader.GetString("Domain");
                    DateTime Updated = PageFetchReader.GetDateTime("LastUpdate");
                    TimeSpan LastUpdate = DateTime.Now - Updated;
                    int MinimumAuthorization = PageFetchReader.GetInt32("MinimumAuthorization");
                    int PageVersion = PageFetchReader.GetInt32("PageVersion");
                    int IsEditing = PageFetchReader.GetInt32("IsEditing");

                    Pages.Add(new object[] { ID, Url, Domain, Updated, MinimumAuthorization, PageVersion, IsEditing });
                }

                PageFetchReader.Close();

                echo("<style type='text/css'>.table_row:hover{ background-color:#CCCCCC; }</style>"); 
                echo("<div class='boxcontainer'><br /><br /><table style='width:100%;' class='display pagelist'><thead><tr><th>ID</th><th>Domain</th><th>Url</th><th>Last Update</th><th>Update Sort</th><th>Auth</th><th>Locked To</th></tr></thead><tbody>");

                foreach (object[] PageObj in Pages)
                {
                    int ID = Convert.ToInt32(PageObj[0]);
                    string Url = PageObj[1] as string;
                    string Domain = PageObj[2] as string;
                    DateTime Updated = Convert.ToDateTime(PageObj[3]);
                    TimeSpan LastUpdate = DateTime.Now - Updated;
                    int MinimumAuthorization = Convert.ToInt32(PageObj[4]);
                    int PageVersion = Convert.ToInt32(PageObj[5]);
                    int IsEditing = Convert.ToInt32(PageObj[6]);
                    string LockedBy = "Unlocked";

                    if (IsEditing > 0)
                    {
                        Member LockedByMember = new Member(IsEditing, SqlConnection);
                        LockedBy = LockedByMember.FirstName + " " + LockedByMember.LastName;
                    }

                    string RowStyle = "";
                    if (IsEditing > 0)
                        RowStyle = "background-color:#DCECF2;";
                    else if (PageVersion != Page.CurrentPageVersion)
                        RowStyle = "background-color:#EAE7D3;";

                    if (IsEditing != 0 && IsEditing != LoggedInMember.ID)
                        echo("<tr class='table_row' pageid='" + ID + "' style='" + RowStyle + "'><td>" + ID + "</td><td>" + Domain + "</td><td>" + Url + "</td><td>" + Utilities.GetTimeSpan(LastUpdate) + "</td><td>" + Utilities.GetSortableDate(Updated) + "</td><td>" + MinimumAuthorization + "</td><td>" + LockedBy + "</td></tr>");
                    else
                        echo("<tr class='table_row' pageid='" + ID + "' style='cursor:pointer;" + RowStyle + "'><td onclick=\"window.location='./EditPage/ID-" + ID + "/';\">" + ID + "</td><td onclick=\"window.location='./EditPage/ID-" + ID + "/';\">" + Domain + "</td><td onclick=\"window.location='./EditPage/ID-" + ID + "/';\">" + Url + "</td><td onclick=\"window.location='./EditPage/ID-" + ID + "/';\">" + Utilities.GetTimeSpan(LastUpdate) + "</td><td>" + Utilities.GetSortableDate(Updated) + "</td><td onclick=\"window.location='./EditPage/ID-" + ID + "/';\">" + MinimumAuthorization + "</td><td onclick=\"window.location='./EditPage/ID-" + ID + "/';\">" + LockedBy + "</td></tr>");
                }

                echo("</tbody></table></div>");
            }
        }
    }
}