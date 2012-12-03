using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

namespace BlazeGames.Web.Core
{
    public class Logging
    {
        private MySqlConnection SqlConnection;
        private Member LoggedInMember;
        private HttpContext Http;

        public Logging(MySqlConnection SqlConnection, Member LoggedInMember)
        {

            this.SqlConnection = SqlConnection;
            this.LoggedInMember = LoggedInMember;
            this.Http = HttpContext.Current;
            try
            {
                string IP = this.Http.Request.UserHostAddress;

                try
                {
                    IP += "," + System.Net.Dns.GetHostByAddress(this.Http.Request.UserHostAddress).HostName;
                }
                catch { }

                MySqlCommand LogVisitQuery = new MySqlCommand("INSERT INTO logs (IP, Time, MemberID) VALUES(@IP, @Time, @MemberID)", SqlConnection);
                LogVisitQuery.Parameters.AddWithValue("@IP", IP);
                LogVisitQuery.Parameters.AddWithValue("@Time", DateTime.Now);
                LogVisitQuery.Parameters.AddWithValue("@MemberID", this.LoggedInMember.ID);
                LogVisitQuery.ExecuteNonQuery();
            }
            catch { }
        }

        public int TotalVisitors()
        {
            List<string> Members = new List<string>();

            MySqlCommand MemberCountQuery = new MySqlCommand("SELECT IP FROM logs WHERE Time >= @Time", this.SqlConnection);
            MemberCountQuery.Parameters.AddWithValue("@Time", DateTime.Now.AddMinutes(-5));
            MySqlDataReader MemberCountReader = MemberCountQuery.ExecuteReader();

            while (MemberCountReader.Read())
            {
                string IP = MemberCountReader.GetString("IP");
                if(!Members.Contains(IP))
                    Members.Add(IP);
            }
            MemberCountReader.Close();

            return Members.Count();
        }

        public long TotalMembers()
        {
            MySqlCommand MemberCountQuery = new MySqlCommand("SELECT COUNT(*) FROM members", this.SqlConnection);
            return (long)MemberCountQuery.ExecuteScalar();
        }
    }
}