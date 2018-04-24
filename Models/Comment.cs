using System;
using System.Collections.Generic;



namespace Hitchhikers.Models
{
    public class Comment
    {

        public int commentid { get; set; }
        public string comment { get; set; }
        public int senderid { get; set; }
        public User sender { get; set; }
        public int pictureid { get; set; }
        public Picture picture { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }


        public Comment()
        {
            this.created_at = DateTime.Now;
            this.updated_at = DateTime.Now;

        }
    }
}
