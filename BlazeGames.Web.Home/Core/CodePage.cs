using System;
using System.Collections.Generic;
using System.Web;
using MySql.Data.MySqlClient;
using BlazeGames;

namespace BlazeGames.Web.Core
{
    public class CodePage
    {
        public PageBuffer pageBuffer = new PageBuffer();
        public MySqlConnection SqlConnection;
        public HttpContext Http;
        public Member LoggedInMember;
        public Logging Log;
        public Page CurrentPage;
        public HttpHeader HttpHead;
        public Events Event;

        public void echo(string Html)
        {
            pageBuffer.Write(Html);
        }

        public void die(string Html)
        {
            HttpContext.Current.Response.Write(Html);
            exit();
        }

        public void exit()
        {
            HttpContext.Current.Response.End();
        }

        public virtual void onPagePreInitialize(MySqlConnection SQLConn, HttpContext Http, Member LoggedInMember, Logging Log, Page CurrentPage, HttpHeader HttpHead, Events Event)
        {
            this.SqlConnection = SQLConn;
            this.Http = Http;
            this.LoggedInMember = LoggedInMember;
            this.Log = Log;
            this.CurrentPage = CurrentPage;
            this.HttpHead = HttpHead;
            this.Event = Event;
        }

        public virtual bool onPageAuthenticate()
        {
            if (CurrentPage.MinimumAuthorization > LoggedInMember.Authority)
                return false;
            else
                return true;
        }

        public virtual void onPageInitialize()
        {
            
        }

        public virtual void onPageLoad()
        {
            ErrorManager.Error("onPageLoad() was not found!");
        }

        public virtual void onPageUnLoad()
        {

        }

        public virtual string onPageReturn()
        {
            return pageBuffer.ToString();
        }
    }

    public class PageBuffer
    {
        private string BufferData = "";

        public void Clear()
        {
            BufferData = "";
        }

        public void Write(string Html)
        {
            BufferData += Html + "\r\n";
        }

        public string Get()
        {
            return BufferData;
        }

        public override string ToString()
        {
            return BufferData;
        }
    }
}