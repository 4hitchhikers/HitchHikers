using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hitchhikers.Models
{
    public class User
    {
        public int userid { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string nickname { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string profile_pict { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public List<Place> visited { get; set; }
        public List<Picture> uploaded { get; set; }
        public List<Comment> my_comments { get; set; }
        public User()
        {
            this.created_at = DateTime.Now;
            this.updated_at = DateTime.Now;
            visited = new List<Place>();
            uploaded = new List<Picture>();
            my_comments = new List<Comment>();
        }
    }
}

