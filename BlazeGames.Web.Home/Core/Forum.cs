using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

namespace BlazeGames.Web.Core
{
    public class Forum
    {
        private MySqlConnection SqlConnection;
        public int ID,
            Posts,
            Threads,
            MinimumAuthorization,
            Display;

        public string ForumName,
            ForumDescription;

        public bool Exists;

        public Forum(int ID, MySqlConnection SqlConnection)
        {
            this.ID = ID;
            this.SqlConnection = SqlConnection;
        }

        public void Load()
        {
            MySqlCommand ForumsFetchQuery = new MySqlCommand("SELECT * FROM forums WHERE ID=@ID;", SqlConnection);
            ForumsFetchQuery.Parameters.AddWithValue("@ID", this.ID);
            MySqlDataReader ForumsFetchReader = ForumsFetchQuery.ExecuteReader();

            if (ForumsFetchReader.Read())
            {
                this.ID = ForumsFetchReader.GetInt32("ID");
                this.MinimumAuthorization = ForumsFetchReader.GetInt32("MinimumAuthorization");
                this.Display = ForumsFetchReader.GetInt32("Display");

                this.ForumName = ForumsFetchReader.GetString("ForumName");
                this.ForumDescription = ForumsFetchReader.GetString("ForumDescription");

                this.Exists = true;
            }
            else
            {
                this.Threads = 0;
                this.Posts = 0;
                this.MinimumAuthorization = 0;
                this.Display = 0;

                this.ForumName = "";
                this.ForumDescription = "";

                this.Exists = false;
            }

            ForumsFetchReader.Close();

            if (this.Exists)
            {
                this.Threads = Convert.ToInt32(new MySqlCommand("SELECT COUNT(*) FROM threads WHERE ForumID=" + this.ID, this.SqlConnection).ExecuteScalar());
                this.Posts = Convert.ToInt32(new MySqlCommand("SELECT COUNT(*) FROM posts WHERE ForumID=" + this.ID, this.SqlConnection).ExecuteScalar()) - this.Threads;

                if (this.Posts < 0)
                    this.Posts = 0;
            }
        }

        public Thread[] AllThreads()
        {
            List<Thread> Threads = new List<Thread>();
            MySqlCommand ThreadFetchQuery = new MySqlCommand("SELECT ID FROM threads WHERE ForumID=@ForumID ORDER BY ThreadUpdateDate DESC;", SqlConnection);
            ThreadFetchQuery.Parameters.AddWithValue("@ForumID", this.ID);
            MySqlDataReader ThreadFetchReader = ThreadFetchQuery.ExecuteReader();

            while (ThreadFetchReader.Read())
                Threads.Add(new Thread(ThreadFetchReader.GetInt32("ID"), SqlConnection));
            ThreadFetchReader.Close();

            foreach (Thread thread in Threads)
                thread.Load();

            return Threads.ToArray();
        }

        public static Forum[] AllForums(MySqlConnection SqlConnection)
        {
            List<Forum> Forums = new List<Forum>();
            MySqlCommand ForumsFetchQuery = new MySqlCommand("SELECT ID FROM forums ORDER BY Display;", SqlConnection);
            MySqlDataReader ForumsFetchReader = ForumsFetchQuery.ExecuteReader();

            while (ForumsFetchReader.Read())
                Forums.Add(new Forum(ForumsFetchReader.GetInt32("ID"), SqlConnection));
            ForumsFetchReader.Close();

            foreach (Forum forum in Forums)
                forum.Load();

            return Forums.ToArray();
        }
    }

    public class Thread
    {
        private MySqlConnection SqlConnection;

        public int ID,
            ForumID,
            ThreadOwner,
            Replies;

        public DateTime ThreadStartDate,
            ThreadUpdateDate;

        public string ThreadTitle,
            ThreadOwnerName;

        public bool PromotedArticle,
            Exists;

        public Thread(int ID, MySqlConnection SqlConnection)
        {
            this.ID = ID;
            this.SqlConnection = SqlConnection;
        }

        public void Load()
        {
            MySqlCommand ThreadFetchQuery = new MySqlCommand("SELECT * FROM threads WHERE ID=@ID", SqlConnection);
            ThreadFetchQuery.Parameters.AddWithValue("@ID", this.ID);
            MySqlDataReader ThreadFetchReader = ThreadFetchQuery.ExecuteReader();

            if (ThreadFetchReader.Read())
            {
                this.ForumID = ThreadFetchReader.GetInt32("ForumID");
                this.ThreadOwner = ThreadFetchReader.GetInt32("ThreadOwner");
                this.ThreadStartDate = ThreadFetchReader.GetDateTime("ThreadStartDate");
                this.ThreadUpdateDate = ThreadFetchReader.GetDateTime("ThreadUpdateDate");
                this.ThreadTitle = ThreadFetchReader.GetString("ThreadTitle");
                this.PromotedArticle = ThreadFetchReader.GetBoolean("PromotedArticle");
                this.Exists = true;
            }
            else
            {
                this.Exists = false;
            }

            ThreadFetchReader.Close();

            if (this.Exists)
            {
                this.Replies = Convert.ToInt32(new MySqlCommand("SELECT COUNT(*) FROM posts WHERE ThreadID=" + this.ID, this.SqlConnection).ExecuteScalar()) - 1;
                if (this.Replies < 0)
                    this.Replies = 0;

                this.ThreadOwnerName = Convert.ToString(new MySqlCommand("SELECT Nickname FROM members WHERE ID=" + this.ThreadOwner, this.SqlConnection).ExecuteScalar());
            }
        }

        public Post[] AllPosts()
        {
            List<Post> Posts = new List<Post>();
            MySqlCommand PostsFetchQuery = new MySqlCommand("SELECT ID FROM posts WHERE ThreadID=@ThreadID ORDER BY PostDate;", SqlConnection);
            PostsFetchQuery.Parameters.AddWithValue("@ThreadID", this.ID);
            MySqlDataReader PostsFetchReader = PostsFetchQuery.ExecuteReader();

            while (PostsFetchReader.Read())
                Posts.Add(new Post(PostsFetchReader.GetInt32("ID"), SqlConnection));
             
            PostsFetchReader.Close();

            foreach (Post post in Posts)
                post.Load();

            return Posts.ToArray();
        }

        public Post[] PagePosts(int PageNum)
        {
            List<Post> Posts = new List<Post>();
            int PostsPrPage = 6;
            MySqlCommand PostsFetchQuery = new MySqlCommand("SELECT ID FROM posts WHERE ThreadID=@ThreadID ORDER BY PostDate LIMIT @Limit1,@Limit2;", SqlConnection);
            PostsFetchQuery.Parameters.AddWithValue("@ThreadID", this.ID);
            PostsFetchQuery.Parameters.AddWithValue("@Limit1", PostsPrPage * PageNum - PostsPrPage);
            PostsFetchQuery.Parameters.AddWithValue("@Limit2", PostsPrPage * PageNum - 1);
            MySqlDataReader PostsFetchReader = PostsFetchQuery.ExecuteReader();

            while (PostsFetchReader.Read())
                Posts.Add(new Post(PostsFetchReader.GetInt32("ID"), SqlConnection));

            PostsFetchReader.Close();

            foreach (Post post in Posts)
                post.Load();

            return Posts.ToArray();
        }
    }

    public class Post
    {
        private MySqlConnection SqlConnection;

        public int ID,
            ForumID,
            ThreadID,
            PostOwner,
            PostReputation;

        public DateTime PostDate;

        public string PostTitle,
            PostContent;

        public bool Exists;

        public Member PostOwnerMember;

        public Post(int ID, MySqlConnection SqlConnection)
        {
            this.ID = ID;
            this.SqlConnection = SqlConnection;
        }

        public void Load()
        {
            MySqlCommand PostFetchQuery = new MySqlCommand("SELECT * FROM posts WHERE ID=@ID", SqlConnection);
            PostFetchQuery.Parameters.AddWithValue("@ID", this.ID);
            MySqlDataReader PostFetchReader = PostFetchQuery.ExecuteReader();

            if (PostFetchReader.Read())
            {
                this.ForumID = PostFetchReader.GetInt32("ForumID");
                this.ThreadID = PostFetchReader.GetInt32("ThreadID");
                this.PostOwner = PostFetchReader.GetInt32("PostOwner");
                this.PostReputation = PostFetchReader.GetInt32("PostReputation");

                this.PostDate = PostFetchReader.GetDateTime("PostDate");

                this.PostTitle = PostFetchReader.GetString("PostTitle");
                this.PostContent = PostFetchReader.GetString("PostContent");
                this.Exists = true;
            }
            else
            {
                this.Exists = false;
            }

            PostFetchReader.Close();

            if (this.Exists)
                this.PostOwnerMember = new Member(this.PostOwner, SqlConnection);
        }

        public void Save()
        {
            throw new NotImplementedException("fuck this for now");
        }
    }
}