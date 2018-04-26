using System;
using System.Collections.Generic;



namespace Hitchhikers.Models
{
    public class Comment
    {

        public int Commentid { get; set; }
        public string CommentText { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; }
        public int PictureId { get; set; }
        public Picture Picture { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }


        public Comment()
        {
            this.Created_At = DateTime.Now;
            this.Updated_At = DateTime.Now;

        }
    }
}
