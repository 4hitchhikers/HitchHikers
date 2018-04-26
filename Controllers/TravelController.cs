using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hitchhikers.Models;

namespace Hitchhikers.Controllers
{
    public class TravelController : Controller
    {
        private TravelContext _dbcontext;

        //Constructor
        public TravelController(TravelContext context)
        {
            _dbcontext = context;
        }
        [HttpGet]
        [Route("Dashboard")]
        public IActionResult Dashboard(string state)
        {
            ViewBag.state = state;
            ViewBag.Email = HttpContext.Session.GetString("email");
            ViewBag.CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");
            
            return View("Dashboard");
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            Dictionary<string, int> StateList = new Dictionary<string, int>
            {
                { "AL", 1 },{ "AK", 2 },{ "AS", 3 },{ "AZ", 4 },{ "AR", 5 },{ "CA", 6 },
                { "CO", 7 },{ "CT", 8 },{ "DE", 9 },{ "DC", 10 },{ "FM", 11 },{ "FL", 12 },
                { "GA", 13 },{ "GU", 14 },{ "HI", 15 },{ "ID", 16 },{ "IL", 17 },{ "IN", 18 },
                { "IA", 19 },{ "KS", 20 },{ "KY", 21 },{ "LA", 22 },{ "ME", 23 },{ "MH", 24 },
                { "MD", 25 },{ "MA", 26 },{ "MI", 27 },{ "MN", 28 },{ "MS", 29 },{ "MO", 30 },
                { "MT", 31 },{ "NE", 32 },{ "NV", 33 },{ "NH", 34 },{ "NJ", 35 },{ "NM", 36 },
                { "NY", 37 },{ "NC", 38 },{ "ND", 37 },{ "MP", 38 },{ "OH", 39 },{ "OK", 40 },
                { "OR", 41 },{ "PW", 42 },{ "PA", 43 },{ "PR", 44 },{ "RI", 45 },{ "SC", 46 },
                { "SD", 47 },{ "TN", 48 },{ "TX", 49 },{ "UT", 50 },{ "VT", 51 },{ "VI", 52 },
                { "VA", 53 },{ "WA", 54 },{ "WV", 55 },{ "WI", 56 },{ "WY", 57 },
            };       
            ViewBag.all_states = StateList;

          return View("Create");
        }

        [HttpPost]
        [Route("AddPhoto")]
        public IActionResult AddPhoto(CreateViewModel model, int state)
        {
            System.Console.WriteLine((int)HttpContext.Session.GetInt32("CurrentUserID"));

           if(ModelState.IsValid) {

               Picture NewPicture  = new Picture
                {
                    UploaderId = (int)HttpContext.Session.GetInt32("CurrentUserID"),
                    StateId = state,
                    PictName = model.PictName,
                    DateVisited = model.DateOfVisit,
                    Description = model.Description,
                };
 
                _dbcontext.Pictures.Add(NewPicture);
                _dbcontext.SaveChanges();

                State newstate = new State{
                    States = model.States,
                    City = model.City
                };
                _dbcontext.States.Add(newstate);
                _dbcontext.SaveChanges(); 
                return View("Dashboard");    
            }
        return View("Create");
        }

        [HttpGet]
        [Route("ViewPicture")]
        public IActionResult ViewPicture()
        {
            return View("");
        }

        [HttpGet]
        [Route("CollectivePhotos")]
        public IActionResult CollectivePhotos()
        {
            return View("CollectivePhotos");
        }

        [HttpGet]
        [Route("StartChat")]
        public IActionResult StartChat()
        {
            if (!CheckLoggedIn())
            {
                return RedirectToAction("Login", "Home");
            }
            ViewBag.CurrentUser = _dbcontext.Users.Where(e=>e.Userid == (int)HttpContext.Session.GetInt32("CurrentUserID"));
            return View("Chatroom");
        }
        
        public bool CheckLoggedIn()
        {
            if (HttpContext.Session.GetInt32("CurrentUserID") == null)
            {
                return false;
            }
            return true;
        }
    }
}
