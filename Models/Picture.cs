using System;
using System.Collections.Generic;



namespace Hitchhikers.Models
{
    public class Picture
    {

        public int pictureid { get; set; }
        public string pict_name { get; set; }
        public DateTime date_visited { get; set; }
        public string description { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int userid { get; set; }
        public User user { get; set; }
        public List<Place> places { get; set; }
        public List<Comment> pict_comments { get; set; }



        public Picture()
        {
            this.created_at = DateTime.Now;
            this.updated_at = DateTime.Now;
            places = new List<Place>();
            pict_comments = new List<Comment>();

        }
    }
}
