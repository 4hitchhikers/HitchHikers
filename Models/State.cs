using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hitchhikers.Models
{
    public class State
    {
        public int StateId { get; set; }
        public string States { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public List<Picture> PictPlace { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }


        public State()
        {
            this.Created_At = DateTime.Now;
            this.Updated_At = DateTime.Now;
            PictPlace = new List<Picture>();

        }
    }
}
