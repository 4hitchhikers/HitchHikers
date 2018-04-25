using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hitchhikers.Models
{
    public class User
    {
        public int Userid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ProfilePict { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
        public List<State> Visited { get; set; }
        public List<Picture> Uploaded { get; set; }
        public List<Comment> MyComments { get; set; }
        public User()
        {
            this.Created_At = DateTime.Now;
            this.Updated_At = DateTime.Now;
            Visited = new List<State>();
            Uploaded = new List<Picture>();
            MyComments = new List<Comment>();
        }
    }
}

