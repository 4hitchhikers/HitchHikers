using System;
using System.Collections.Generic;
using System.Web;

namespace Hitchhikers.Models
{
    public class Picture
    {
        public int PictureId { get; set; }
        public string PictName { get; set; }
        public string Description { get; set; }
        public string States { get; set; }
        public string City { get; set; }
        public DateTime DateVisited { get; set; }
        public string Address { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
        public int UploaderId { get; set; }
        public User Uploader { get; set; }
        public List<Comment> PictComments { get; set; }

        public Picture()
        {
            this.Created_At = DateTime.Now;
            this.Updated_At = DateTime.Now;
            PictComments = new List<Comment>();
        }
       
    }
}
