using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Web;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace BlazeGames.Web.Core
{
    public class Page
    {
        public const int CurrentPageVersion = 1;

        private CodeDomProvider CodeCompiler;
        private CompilerParameters CodeParameters;

        public string Url,
            Domain;

        private string _Code,
            _PageCSS,
            _PageJS,
            _PageHTML;

        private bool CodeChanged = false;
        public string Code
        {
            get { return _Code; }
            set { if (_Code != value) { CodeChanged = true; } _Code = value; }
        }

        public string PageCSS
        {
            get { return _PageCSS; }
            set { if (_PageCSS != value) { CodeChanged = true; } _PageCSS = value; }
        }

        public string PageJS
        {
            get { return _PageJS; }
            set { if (_PageJS != value) { CodeChanged = true; } _PageJS = value; }
        }

        public string PageHTML
        {
            get { return _PageHTML; }
            set { if (_PageHTML != value) { CodeChanged = true; } _PageHTML = value; }
        }

        public string[] References;

        public int ID,
            WarnLevel,
            MinimumAuthorization,
            PageVersion,
            IsEditing;

        public bool RequireSecure,
            InDev,
            GenerateInMemory,
            Exists,
            HtmlPage,
            Compiled,
            Locked;

        public byte[] CompiledCode;

        public DateTime LastUpdate;

        private MySqlConnection SqlConnection;

        public Page(string Url, string Domain, MySqlConnection SqlConnection)
        {
            this.SqlConnection = SqlConnection;
            this.Url = Url;

            if (Domain.ToLower().StartsWith("www."))
                this.Domain = Domain.Substring(4, Domain.Length - 4);
            else
                this.Domain = Domain;

            MySqlCommand PageIDQuery = new MySqlCommand("SELECT ID FROM pages WHERE @Url LIKE `Url` AND @Domain LIKE `Domain`", this.SqlConnection);
            PageIDQuery.Parameters.AddWithValue("@Url", this.Url);
            PageIDQuery.Parameters.AddWithValue("@Domain", this.Domain);
            using (MySqlDataReader PageIDReader = PageIDQuery.ExecuteReader())
            {
                if (PageIDReader.Read())
                    this.ID = PageIDReader.GetInt32("ID");
                else
                    this.ID = 0;
            }

            this.Load();
        }

        public Page(int ID, MySqlConnection SqlConnection)
        {
            this.SqlConnection = SqlConnection;
            this.ID = ID;

            this.Load();
        }

        public void Load()
        {
            MySqlCommand PageLoadQuery = new MySqlCommand("SELECT * FROM pages WHERE ID=@ID", this.SqlConnection);
            PageLoadQuery.Parameters.AddWithValue("@ID", this.ID);
            using (MySqlDataReader PageLoadReader = PageLoadQuery.ExecuteReader())
            {
                if (PageLoadReader.Read())
                {
                    this.Url = PageLoadReader.GetString("Url");
                    this.Domain = PageLoadReader.GetString("Domain");
                    this._Code = PageLoadReader.GetString("Code");
                    this._PageCSS = PageLoadReader.GetString("PageCSS");
                    this._PageJS = PageLoadReader.GetString("PageJS");
                    this._PageHTML = PageLoadReader.GetString("PageHTML");

                    this.References = PageLoadReader.GetString("References").Split(',');

                    this.ID = PageLoadReader.GetInt32("ID");
                    this.WarnLevel = PageLoadReader.GetInt32("WarnLevel");
                    this.MinimumAuthorization = PageLoadReader.GetInt32("MinimumAuthorization");
                    this.PageVersion = PageLoadReader.GetInt32("PageVersion");

                    this.RequireSecure = PageLoadReader.GetBoolean("RequireSecure");
                    this.InDev = PageLoadReader.GetBoolean("InDev");
                    this.GenerateInMemory = PageLoadReader.GetBoolean("GenerateInMemory");
                    this.HtmlPage = PageLoadReader.GetBoolean("HtmlPage");
                    this.Compiled = PageLoadReader.GetBoolean("Compiled");
                    this.Locked = PageLoadReader.GetBoolean("Locked");
                    this.IsEditing = PageLoadReader.GetInt32("IsEditing");

                    this.CompiledCode = (byte[])PageLoadReader["CompiledCode"];

                    this.LastUpdate = PageLoadReader.GetDateTime("LastUpdate");

                    this.Exists = true;
                    this.CodeChanged = false;
                }
                else
                {
                    this.Url = "/404/";
                    this.Code = "";

                    this.References = "".Split(',');

                    this.ID = 0;
                    this.WarnLevel = 0;
                    this.MinimumAuthorization = 0;

                    this.RequireSecure = false;
                    this.InDev = false;
                    this.GenerateInMemory = false;
                    this.Exists = false;
                }
            }
        }

        public void Save()
        {
            if(CodeChanged)
                this.LastUpdate = DateTime.Now;

            MySqlCommand PageSaveQuery = new MySqlCommand("UPDATE pages SET Url=@Url, Domain=@Domain, Code=@Code, `References`=@References, WarnLevel=@WarnLevel, MinimumAuthorization=@MinimumAuthorization, RequireSecure=@RequireSecure, InDev=@InDev, GenerateInMemory=@GenerateInMemory, HtmlPage=@HtmlPage, Compiled=@Compiled, CompiledCode=@CompiledCode, LastUpdate=@LastUpdate, Locked=@Locked, PageVersion=@PageVersion, PageCSS=@PageCSS, PageJS=@PageJS, PageHTML=@PageHTML, IsEditing=@IsEditing WHERE ID=@ID", SqlConnection);

            PageSaveQuery.Parameters.AddWithValue("@ID", this.ID);
            PageSaveQuery.Parameters.AddWithValue("@Url", this.Url);
            PageSaveQuery.Parameters.AddWithValue("@Domain", this.Domain);
            PageSaveQuery.Parameters.AddWithValue("@Code", this.Code);
            PageSaveQuery.Parameters.AddWithValue("@References", String.Join(",", this.References));
            PageSaveQuery.Parameters.AddWithValue("@WarnLevel", this.WarnLevel);
            PageSaveQuery.Parameters.AddWithValue("@MinimumAuthorization", this.MinimumAuthorization);
            PageSaveQuery.Parameters.AddWithValue("@RequireSecure", this.RequireSecure);
            PageSaveQuery.Parameters.AddWithValue("@InDev", this.InDev);
            PageSaveQuery.Parameters.AddWithValue("@GenerateInMemory", this.GenerateInMemory);
            PageSaveQuery.Parameters.AddWithValue("@HtmlPage", this.HtmlPage);
            PageSaveQuery.Parameters.AddWithValue("@Compiled", this.Compiled);
            PageSaveQuery.Parameters.AddWithValue("@CompiledCode", this.CompiledCode);
            PageSaveQuery.Parameters.AddWithValue("@LastUpdate", this.LastUpdate);
            PageSaveQuery.Parameters.AddWithValue("@Locked", this.Locked);
            PageSaveQuery.Parameters.AddWithValue("@PageVersion", this.PageVersion);
            PageSaveQuery.Parameters.AddWithValue("@PageCSS", this.PageCSS);
            PageSaveQuery.Parameters.AddWithValue("@PageJS", this.PageJS);
            PageSaveQuery.Parameters.AddWithValue("@PageHTML", this.PageHTML);
            PageSaveQuery.Parameters.AddWithValue("@IsEditing", this.IsEditing);

            PageSaveQuery.ExecuteNonQuery();
        }

        public void Delete()
        {
            MySqlCommand PageDeleteQuery = new MySqlCommand("DELETE FROM pages WHERE ID=@ID", SqlConnection);
            PageDeleteQuery.Parameters.AddWithValue("@ID", this.ID);
            PageDeleteQuery.ExecuteNonQuery();

            this.Load();
        }

        public CompilerErrorCollection Compile()
        {
            if (!this.HtmlPage)
            {
                CodeCompiler = CodeDomProvider.CreateProvider("CSharp");
                CodeParameters = new CompilerParameters();
                CodeParameters.CompilerOptions = "/lib:C:\\inetpub\\wwwroot\\bin";
                CodeParameters.GenerateInMemory = this.GenerateInMemory;

                CodeParameters.ReferencedAssemblies.AddRange(this.References);
                CodeParameters.WarningLevel = this.WarnLevel;

                /* Build the assembly */
                string tmpcode = this.Code;
                BlazeGames_CodeCompiler BGxCodeCompiler = new BlazeGames_CodeCompiler();
                tmpcode = BGxCodeCompiler.CompileToCSharp(tmpcode);

                CompilerResults CodeCompiled = CodeCompiler.CompileAssemblyFromSource(CodeParameters, tmpcode);

                /* Check for errors */
                if (CodeCompiled.Errors.HasErrors)
                    return CodeCompiled.Errors;

                this.CompiledCode = System.IO.File.ReadAllBytes(CodeCompiled.PathToAssembly);
                this.Compiled = true;
                this.Save();
            }
            else
                this.Save();

            return null;
        }

        public static Page Create(MySqlConnection SqlConnection, string Url, string Domain, string Code, string[] References, int MinimumAuthorization, bool RequireSecure, string PageCSS, string PageJS, string PageHTML)
        {
            if (!Url.StartsWith("/"))
                Url = "/" + Url;
            if (!Url.EndsWith("/"))
                if (!Url.EndsWith("%"))
                    Url = Url + "/";
            MySqlCommand PageCreateQuery = new MySqlCommand(@"INSERT INTO pages (`Url`, `Code`, `References`, `WarnLevel`, `MinimumAuthorization`, `RequireSecure`, `InDev`, `GenerateInMemory`, `HtmlPage`, `Compiled`, `CompiledCode`, `LastUpdate`, `Locked`, `PageCSS`, `PageJS`, `PageHTML`)
VALUES (@Url, @Code, @References, 4, @MinimumAuthorization, @RequireSecure, 0, 0, 0, 0, 0, @LastUpdate, 0, @PageCSS, @PageJS, @PageHTML);", SqlConnection);
            PageCreateQuery.Parameters.AddWithValue("@Url", Url);
            PageCreateQuery.Parameters.AddWithValue("@Code", Code);
            PageCreateQuery.Parameters.AddWithValue("@References", String.Join(",", References));
            PageCreateQuery.Parameters.AddWithValue("@MinimumAuthorization", MinimumAuthorization);
            PageCreateQuery.Parameters.AddWithValue("@RequireSecure", RequireSecure);
            PageCreateQuery.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
            PageCreateQuery.Parameters.AddWithValue("@PageCSS", PageCSS);
            PageCreateQuery.Parameters.AddWithValue("@PageJS", PageJS);
            PageCreateQuery.Parameters.AddWithValue("@PageHTML", PageHTML);

            PageCreateQuery.ExecuteNonQuery();
            return new Page(Url, Domain, SqlConnection);
        }

        public static string DefaultCode = @"using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using BlazeGames.Web.Core;
using MySql.Data.MySqlClient;

namespace BlazeGames.Web
{
    public class DynamicPage : CodePage
    {
        public override void onPageInitialize()
        {
            // TODO: Page Initialize Code Here
        }
        
        public override void onPageLoad()
        {
            // TODO: Page Load Code Here
        }
    }
}";
        public static string DefaultHTML = @"<div id='contents'><div id='main' class='container htmlpage'>
    <!--{PageCode}-->
</div></div>";

        public static string DefaultCSS = @"";

        public static string DefaultJS = @"";
    }
}