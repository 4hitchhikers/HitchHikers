using System;
using System.Collections.Generic;



namespace Hitchhikers.Models
{
    public class Place
    {

        public int placeid { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string address { get; set; }
        public int visitorid { get; set; }
        public User visitor { get; set; }
        public int place_pictid { get; set; }
        public Picture place_pict { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }


        public Place()
        {
            this.created_at = DateTime.Now;
            this.updated_at = DateTime.Now;

        }
    }
}
