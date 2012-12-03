using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Linq;
using MySql.Data.MySqlClient;

namespace BlazeGames.Web.Core
{
    public class Member
    {
        public string LoginName,
            PasswordHash,
            PasswordMD5,
            Nickname,
            BirthDay,
            Email,
            FirstName,
            LastName,
            StreetAddress,
            State,
            City,
            Country,
            VerificationHash,
            Notes,
            MemberData,
            SecurityQuestion,
            SecurityAnswer,
            WebSessionKey,
            UserHostAddress,
            PIN;

        public int ID,
            ZIP,
            Authority,
            Cash;

        public bool EmailVerified,
            IsValid,
            RequestSecure,
            GlobalSession,
            MobileNotifications,
            EmailNotifications;

        public List<string> LinkedDevices = new List<string>();

        public List<string> PendingFriends = new List<string>();
        public List<string> BlockedFriends = new List<string>();
        public List<string> Friends = new List<string>();

        private MySqlConnection SqlConnection;

        public Member()
        {
            this.ID = 0;
        }

        public Member(string LoginName, string PasswordHash, int MemberID, string PIN, MySqlConnection SqlConnection)
        {
            this.LoginName = LoginName;
            this.PasswordHash = PasswordHash;
            this.PIN = PIN;

            this.SqlConnection = SqlConnection;

            MySqlCommand MemberLoadQuery = new MySqlCommand("SELECT ID FROM members WHERE LoginName=@LoginName AND PasswordHash=@PasswordHash AND PIN=@PIN, AND GlobalSession=true;", SqlConnection);
            MemberLoadQuery.Parameters.AddWithValue("@LoginName", this.LoginName);
            MemberLoadQuery.Parameters.AddWithValue("@PasswordHash", this.PasswordHash);
            MemberLoadQuery.Parameters.AddWithValue("@PIN", this.PIN);
            MySqlDataReader MemberLoadReader = MemberLoadQuery.ExecuteReader();

            if (MemberLoadReader.Read())
                this.ID = MemberLoadReader.GetInt32("ID");
            else
                this.ID = 0;

            MemberLoadReader.Close();

            this.Load();
        }

        public Member(string WebSessionKey, string UserHostAddress, MySqlConnection SqlConnection)
        {
            this.WebSessionKey = WebSessionKey;
            this.UserHostAddress = UserHostAddress;

            this.SqlConnection = SqlConnection;

            if (WebSessionKey.Trim() == "" || UserHostAddress.Trim() == "")
            {
                ID = 0;
                return;
            }

            MySqlCommand MemberLoadQuery = new MySqlCommand("SELECT ID FROM members WHERE WebSessionKey=@WebSessionKey AND UserHostAddress=@UserHostAddress", SqlConnection);
            MemberLoadQuery.Parameters.AddWithValue("@UserHostAddress", this.UserHostAddress);
            MemberLoadQuery.Parameters.AddWithValue("@WebSessionKey", this.WebSessionKey);
            using (MySqlDataReader MemberLoadReader = MemberLoadQuery.ExecuteReader())
            {
                if (MemberLoadReader.Read())
                    this.ID = MemberLoadReader.GetInt32("ID");
                else
                    this.ID = 0;
            }

            this.Load();
        }

        public Member(int ID, MySqlConnection SqlConnection)
        {
            this.ID = ID;

            this.SqlConnection = SqlConnection;

            this.Load();
        }

        public Member(string Account, MySqlConnection SqlConnection)
        {
            this.SqlConnection = SqlConnection;

            MySqlCommand MemberLoadQuery = new MySqlCommand("SELECT ID FROM members WHERE LoginName=@Account OR Email=@Account", SqlConnection);
            MemberLoadQuery.Parameters.AddWithValue("@Account", Account);
            using (MySqlDataReader MemberLoadReader = MemberLoadQuery.ExecuteReader())
            {
                if (MemberLoadReader.Read())
                    this.ID = MemberLoadReader.GetInt32("ID");
                else
                    this.ID = 0;
            }

            this.Load();
        }

        public void Save()
        {
            MySqlCommand MemberSaveQuery = new MySqlCommand(@"UPDATE members SET LoginName=@LoginName, PasswordHash=@PasswordHash, Nickname=@Nickname, BirthDay=@BirthDay, Email=@Email, FirstName=@FirstName, LastName=@LastName, StreetAddress=@StreetAddress, State=@State, City=@City, ZIP=@ZIP, Country=@Country, EmailVerified=@EmailVerified, VerificationHash=@VerificationHash, Authority=@Authority, Notes=@Notes, MemberData=@MemberData, SecurityQuestion=@SecurityQuestion, SecurityAnswer=@SecurityAnswer, Cash=@Cash, RequestSecure=@RequestSecure, LinkedDevices=@LinkedDevices, MobileNotifications=@MobileNotifications, EmailNotifications=@EmailNotifications, PendingFriends=@PendingFriends, BlockedFriends=@BlockedFriends, Friends=@Friends WHERE ID=@ID", SqlConnection);

            MemberSaveQuery.Parameters.AddWithValue("@LoginName", this.LoginName);
            MemberSaveQuery.Parameters.AddWithValue("@PasswordHash", this.PasswordHash);
            MemberSaveQuery.Parameters.AddWithValue("@Nickname", this.Nickname);
            MemberSaveQuery.Parameters.AddWithValue("@BirthDay", this.BirthDay);
            MemberSaveQuery.Parameters.AddWithValue("@Email", this.Email);
            MemberSaveQuery.Parameters.AddWithValue("@FirstName", this.FirstName);
            MemberSaveQuery.Parameters.AddWithValue("@LastName", this.LastName);
            MemberSaveQuery.Parameters.AddWithValue("@StreetAddress", this.StreetAddress);
            MemberSaveQuery.Parameters.AddWithValue("@State", this.State);
            MemberSaveQuery.Parameters.AddWithValue("@City", this.City);
            MemberSaveQuery.Parameters.AddWithValue("@ZIP", this.ZIP);
            MemberSaveQuery.Parameters.AddWithValue("@Country", this.Country);
            MemberSaveQuery.Parameters.AddWithValue("@EmailVerified", this.EmailVerified);
            MemberSaveQuery.Parameters.AddWithValue("@VerificationHash", this.VerificationHash);
            MemberSaveQuery.Parameters.AddWithValue("@Authority", this.Authority);
            MemberSaveQuery.Parameters.AddWithValue("@Notes", this.Notes);
            MemberSaveQuery.Parameters.AddWithValue("@MemberData", this.MemberData);
            MemberSaveQuery.Parameters.AddWithValue("@SecurityQuestion", this.SecurityQuestion);
            MemberSaveQuery.Parameters.AddWithValue("@SecurityAnswer", this.SecurityAnswer);
            MemberSaveQuery.Parameters.AddWithValue("@Cash", this.Cash);
            MemberSaveQuery.Parameters.AddWithValue("@RequestSecure", this.RequestSecure);
            MemberSaveQuery.Parameters.AddWithValue("@GlobalSession", this.GlobalSession);
            MemberSaveQuery.Parameters.AddWithValue("@PIN", this.PIN);
            MemberSaveQuery.Parameters.AddWithValue("@ID", this.ID);
            MemberSaveQuery.Parameters.AddWithValue("@LinkedDevices", String.Join(",", this.LinkedDevices.ToArray()));
            MemberSaveQuery.Parameters.AddWithValue("@MobileNotifications", MobileNotifications);
            MemberSaveQuery.Parameters.AddWithValue("@EmailNotifications", EmailNotifications);
            MemberSaveQuery.Parameters.AddWithValue("@PendingFriends", String.Join(",", this.PendingFriends.ToArray()));
            MemberSaveQuery.Parameters.AddWithValue("@BlockedFriends", String.Join(",", this.BlockedFriends.ToArray()));
            MemberSaveQuery.Parameters.AddWithValue("@Friends", String.Join(",", this.Friends.ToArray()));

            MemberSaveQuery.ExecuteNonQuery();
        }

        public void Load()
        {
            if (this.ID == 0)
            {
                this.LoginName = "Guest";
                this.PasswordHash = "";
                this.Nickname = "";
                this.BirthDay = "";
                this.Email = "";
                this.FirstName = "";
                this.LastName = "";
                this.StreetAddress = "";
                this.State = "";
                this.City = "";
                this.Country = "";
                this.VerificationHash = "";
                this.Notes = "";
                this.MemberData = "";
                this.SecurityQuestion = "";
                this.SecurityAnswer = "";
                this.PIN = "0000";

                this.ID = 0;
                this.ZIP = 0;
                this.Authority = 0;
                this.Cash = 0;

                this.EmailVerified = false;
                this.RequestSecure = false;
                this.GlobalSession = false;
                this.IsValid = false;

                return;
            }

            MySqlCommand MemberLoadQuery = new MySqlCommand("SELECT * FROM members WHERE ID=@ID", SqlConnection);
            MemberLoadQuery.Parameters.AddWithValue("@ID", this.ID);
            using (MySqlDataReader MemberLoadReader = MemberLoadQuery.ExecuteReader())
            {
                if (MemberLoadReader.Read())
                {
                    this.LoginName = MemberLoadReader.GetString("LoginName");
                    this.PasswordHash = MemberLoadReader.GetString("PasswordHash");
                    this.PasswordMD5 = MemberLoadReader.GetString("PasswordMD5");
                    this.Nickname = MemberLoadReader.GetString("Nickname");
                    this.BirthDay = MemberLoadReader.GetString("BirthDay");
                    this.Email = MemberLoadReader.GetString("Email");
                    this.FirstName = MemberLoadReader.GetString("FirstName");
                    this.LastName = MemberLoadReader.GetString("LastName");
                    this.StreetAddress = MemberLoadReader.GetString("StreetAddress");
                    this.State = MemberLoadReader.GetString("State");
                    this.City = MemberLoadReader.GetString("City");
                    this.Country = MemberLoadReader.GetString("Country");
                    this.VerificationHash = MemberLoadReader.GetString("VerificationHash");
                    this.Notes = MemberLoadReader.GetString("Notes");
                    this.MemberData = MemberLoadReader.GetString("MemberData");
                    this.SecurityQuestion = MemberLoadReader.GetString("SecurityQuestion");
                    this.SecurityAnswer = MemberLoadReader.GetString("SecurityAnswer");
                    this.PIN = MemberLoadReader.GetString("PIN");

                    this.ID = MemberLoadReader.GetInt32("ID");
                    this.ZIP = MemberLoadReader.GetInt32("ZIP");
                    this.Authority = MemberLoadReader.GetInt32("Authority");
                    this.Cash = MemberLoadReader.GetInt32("Cash");

                    this.LinkedDevices.AddRange(MemberLoadReader.GetString("LinkedDevices").Split(','));

                    this.PendingFriends.AddRange(MemberLoadReader.GetString("PendingFriends").Split(','));
                    this.BlockedFriends.AddRange(MemberLoadReader.GetString("BlockedFriends").Split(','));
                    this.Friends.AddRange(MemberLoadReader.GetString("Friends").Split(','));

                    this.EmailVerified = MemberLoadReader.GetBoolean("EmailVerified");
                    this.RequestSecure = MemberLoadReader.GetBoolean("RequestSecure");
                    this.GlobalSession = MemberLoadReader.GetBoolean("GlobalSession");
                    this.MobileNotifications = MemberLoadReader.GetBoolean("MobileNotifications");
                    this.EmailNotifications = MemberLoadReader.GetBoolean("EmailNotifications");
                    this.IsValid = true;
                }
                else
                {
                    this.LoginName = "Guest";
                    this.PasswordHash = "";
                    this.Nickname = "";
                    this.BirthDay = "";
                    this.Email = "";
                    this.FirstName = "";
                    this.LastName = "";
                    this.StreetAddress = "";
                    this.State = "";
                    this.City = "";
                    this.Country = "";
                    this.VerificationHash = "";
                    this.Notes = "";
                    this.MemberData = "";
                    this.SecurityQuestion = "";
                    this.SecurityAnswer = "";
                    this.PIN = "0000";

                    this.ID = 0;
                    this.ZIP = 0;
                    this.Authority = 0;
                    this.Cash = 0;

                    this.EmailVerified = false;
                    this.RequestSecure = false;
                    this.GlobalSession = false;
                    this.IsValid = false;
                }
            }
        }

        public bool SendNotification(string Title, string Message)
        {
            if (!EmailNotifications && !MobileNotifications)
                return false;
            else
            {
                if (EmailNotifications)
                    Utilities.SendEmail(Email, Title, Message);

                //if(MobileNotifications)
                    
                return true;
            }
        }

        public string GetProfileImage()
        {

            if (HttpContext.Current.Request.IsSecureConnection)
                return "https://c328395.ssl.cf1.rackcdn.com/" + Nickname.ToLower() + ".png";
            else
                return "http://c328395.r95.cf1.rackcdn.com/" + Nickname.ToLower() + ".png";
        }

        public static string HashPassword(string Password)
        {
            return Utilities.MD5("BGxSecure" + Utilities.MD5(Password));
        }

        public static bool Login(string LoginName, string Password, string WebSessionKey, string UserHostAddress, MySqlConnection SqlConnection)
        {
            bool LoginValid = false;

            if (WebSessionKey == "" || UserHostAddress == "")
                return false;

            MySqlCommand LoginCheckQuery = new MySqlCommand("SELECT ID FROM members WHERE LoginName=@LoginName AND PasswordHash=@PasswordHash AND EmailVerified=true OR Email=@LoginName AND PasswordHash=@PasswordHash AND EmailVerified=true", SqlConnection);
            LoginCheckQuery.Parameters.AddWithValue("@LoginName", LoginName);
            LoginCheckQuery.Parameters.AddWithValue("@PasswordHash", HashPassword(Password));
            using (MySqlDataReader LoginCheckReader = LoginCheckQuery.ExecuteReader())
            {
                if (LoginCheckReader.Read())
                    LoginValid = true;
                else
                    return false;
            }

            if (LoginValid)
            {
                MySqlCommand LoginUpdateQuery = new MySqlCommand("UPDATE members SET WebSessionKey=@WebSessionKey, UserHostAddress=@UserHostAddress, PasswordMD5=@PasswordMD5 WHERE LoginName=@LoginName AND PasswordHash=@PasswordHash OR Email=@LoginName AND PasswordHash=@PasswordHash", SqlConnection);
                LoginUpdateQuery.Parameters.AddWithValue("@LoginName", LoginName);
                LoginUpdateQuery.Parameters.AddWithValue("@PasswordHash", HashPassword(Password));
                LoginUpdateQuery.Parameters.AddWithValue("@PasswordMD5", Utilities.MD5(Password));
                LoginUpdateQuery.Parameters.AddWithValue("@WebSessionKey", WebSessionKey);
                LoginUpdateQuery.Parameters.AddWithValue("@UserHostAddress", UserHostAddress);
                LoginUpdateQuery.ExecuteNonQuery();

                return true;
            }
            else
                return false;
        }

        public static bool CheckEmailExists(string Email, MySqlConnection SqlConnection)
        {
            MySqlCommand LoginCheckQuery = new MySqlCommand("SELECT COUNT(*) FROM members WHERE Email=@Eamil", SqlConnection);
            LoginCheckQuery.Parameters.AddWithValue("@Eamil", Email);
            if ((long)LoginCheckQuery.ExecuteScalar() >= 1) return true;
            else return false;
        }

        public static bool CheckLoginNameExists(string LoginName, MySqlConnection SqlConnection)
        {
            MySqlCommand LoginCheckQuery = new MySqlCommand("SELECT COUNT(*) FROM members WHERE LoginName=@LoginName", SqlConnection);
            LoginCheckQuery.Parameters.AddWithValue("@LoginName", LoginName);
            if ((long)LoginCheckQuery.ExecuteScalar() >= 1) return true;
            else return false;
        }

        public static bool CheckNicknameExists(string Nickname, MySqlConnection SqlConnection)
        {
            MySqlCommand LoginCheckQuery = new MySqlCommand("SELECT COUNT(*) FROM members WHERE Nickname=@Nickname", SqlConnection);
            LoginCheckQuery.Parameters.AddWithValue("@Nickname", Nickname);
            if ((long)LoginCheckQuery.ExecuteScalar() >= 1) return true;
            else return false;
        }

        public static bool CheckEmailValid(string Email)
        {
            Regex re = new Regex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,6}\b");
            return re.IsMatch(Email);
        }

        public static string Create(string LoginName, string Password, string Nickname, string Birthday, string Email, string FirstName, string LastName, string Country, string State, MySqlConnection SqlConnection)
        {
            DateTime BirthDate;
            if (!DateTime.TryParse(Birthday, out BirthDate))
                return "Invalid birthday.";
            else if ((DateTime.Now - BirthDate).TotalDays < 4745)
                return "Due to our compliance with the requirements of COPPA (Childrens Online Privacy Protection Act), we do not collect any information from anyone under 13 years of age. Our website, products and services are all directed to people who are at least 13 years old or older.";
            else if (CheckLoginNameExists(LoginName, SqlConnection))
                return "Login already exists.";
            else if (CheckEmailExists(Email, SqlConnection))
                return "Email already exists.";
            else if (CheckNicknameExists(Nickname, SqlConnection))
                return "Nickname already exists.";
            else if (LoginName.Length < 5)
                return "Your login must be at least 5 characters.";
            else if (Password.Length < 6)
                return "Your password must be at least 6 characters.";
            else if (Nickname.Length < 3)
                return "your nickname must be at least 3 characters.";
            else if (CheckEmailValid(Email))
                return "The email you entered is invalid.";
            else
            {
                string VerificationHash = Guid.NewGuid().ToString();

                MySqlCommand MemberCreateQuery = new MySqlCommand("INSERT INTO `members` (`LoginName`, `PasswordHash`, `Nickname`, `BirthDay`, `Email`, `FirstName`, `LastName`, `StreetAddress`, `State`, `City`, `ZIP`, `Country`, `EmailVerified`, `VerificationHash`, `Authority`, `Notes`, `MemberData`, `SecurityQuestion`, `SecurityAnswer`, `Cash`, `WebSessionKey`, `UserHostAddress`, `RequestSecure`, `LinkedDevices`, `MobileNotifications`, `EmailNotifications`) VALUES (@LoginName, @PasswordHash, @Nickname, @BirthDay, @Email, @FirstName, @LastName, '', @State, '',  0, @Country, 0, @VerificationHash, 1, '', '', '', '', 0, 'NULL', '', 0, '', 1, 1);", SqlConnection);
                MemberCreateQuery.Parameters.AddWithValue("@LoginName", LoginName);
                MemberCreateQuery.Parameters.AddWithValue("@PasswordHash", HashPassword(Password));
                MemberCreateQuery.Parameters.AddWithValue("@Nickname", Nickname);
                MemberCreateQuery.Parameters.AddWithValue("@BirthDay", BirthDate);
                MemberCreateQuery.Parameters.AddWithValue("@Email", Email);
                MemberCreateQuery.Parameters.AddWithValue("@FirstName", FirstName);
                MemberCreateQuery.Parameters.AddWithValue("@LastName", LastName);
                MemberCreateQuery.Parameters.AddWithValue("@Country", Country);
                MemberCreateQuery.Parameters.AddWithValue("@State", State);
                MemberCreateQuery.Parameters.AddWithValue("@VerificationHash", VerificationHash);

                MemberCreateQuery.ExecuteNonQuery();

                Utilities.SendEmail(Email, "Blaze Games Account Activation", Nickname + @",
 This email has been sent from https://blaze-games.com/
  
 You have received this email because this email address
 was used during registration.
 If you did not register an account, please disregard this
 email. You do not need to unsubscribe or take any further action.
  
 ------------------------------------------------
 Activation Instructions
 ------------------------------------------------
  
 Thank you for registering.
 We require that you ""validate"" your registration to ensure that
 the email address you entered was correct. This protects against
 unwanted spam and malicious abuse.
  
 To activate your account, simply click on the following link:
  
 https://blaze-games.com/Account/Create/?Act=VerifyEmail&VerificationHash=" + VerificationHash + @"
  
 ------------------------------------------------
 Not working?
 ------------------------------------------------
  
 If you cannot validate your account, it's possible that the account has been removed.
 If this is the case, please contact an administrator to rectify the problem.
 
 Thank you for registering and enjoy your stay!");


                return "";
            }
        }

        public static bool VerifyAccount(string Hash, MySqlConnection SqlConnection)
        {
            MySqlCommand VerifyCheckQuery = new MySqlCommand("SELECT COUNT(*) FROM members WHERE EmailVerified=false AND VerificationHash=@VerificationHash", SqlConnection);
            VerifyCheckQuery.Parameters.AddWithValue("@VerificationHash", Hash);
            if ((long)VerifyCheckQuery.ExecuteScalar() >= 1)
            {
                MySqlCommand VerifyQuery = new MySqlCommand("UPDATE members SET EmailVerified=true WHERE EmailVerified=false AND VerificationHash=@VerificationHash", SqlConnection);
                VerifyQuery.Parameters.AddWithValue("@VerificationHash", Hash);
                VerifyQuery.ExecuteNonQuery();
                return true;
            }
            else return false;
        }

        public static bool TryLoginWithPassword(string LoginName, string Password, MySqlConnection SqlConnection, bool Hashed=false)
        {
            if (!Hashed)
                Password = HashPassword(Password);

            MySqlCommand LoginCheckQuery = new MySqlCommand("SELECT ID FROM members WHERE LoginName=@LoginName AND PasswordHash=@PasswordHash AND EmailVerified=true OR Email=@LoginName AND PasswordHash=@PasswordHash AND EmailVerified=true", SqlConnection);
            LoginCheckQuery.Parameters.AddWithValue("@LoginName", LoginName);
            LoginCheckQuery.Parameters.AddWithValue("@PasswordHash", Password);

            using (MySqlDataReader LoginCheckReader = LoginCheckQuery.ExecuteReader())
                return LoginCheckReader.Read();
        }

        public static Member Null()
        {
            return null;
        }
    }
}