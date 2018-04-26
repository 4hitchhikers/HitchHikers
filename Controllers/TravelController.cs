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
        public IActionResult Dashboard()
        {
            if (!CheckLoggedIn())
            {
                return RedirectToAction("SignIn", "Home");
            }
            // ViewBag.state = state;
            // ViewBag.Email = HttpContext.Session.GetString("email");
            // ViewBag.CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");
            
            return View("Dashboard");
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            List<string> StateList = new List<string>
            { "AL", "AK", "AS", "AZ", "AR", "CA", "CO", "CT", "DE", "DC","FM", "FL","GA", "GU","HI","ID","IL", "IN", "IA","KS","KY","LA","ME", "MH", "MD", "MA", "MI","MN","MS","MO", "MT", "NE","NV","NH","NJ", "NM", "NY", "NC", "ND","MP","OH","OK", "OR",  "PW", "PA","PR","RI", "SC", "SD", "TN","TX","UT","VT", "VI", "VA", "WA", "WV", "WI","WY"};     
            ViewBag.all_states = StateList;

          return View("Create");
        }

        [HttpPost]
        [Route("AddPhoto")]
        public IActionResult AddPhoto(CreateViewModel model)
        {
            System.Console.WriteLine((int)HttpContext.Session.GetInt32("CurrentUserID"));

           if(ModelState.IsValid) {

               Picture NewPicture  = new Picture
                {
                    UploaderId = (int)HttpContext.Session.GetInt32("CurrentUserID"),
                    States = model.States,
                    City = model.City,
                    PictName = model.PictName,
                    DateVisited = model.DateOfVisit,
                    Description = model.Description,
                };
 
                _dbcontext.Pictures.Add(NewPicture);
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
        [Route("CollectivePhotos/{state}")]
        public IActionResult CollectivePhotos(string state)
        {
            if (!CheckLoggedIn())
            {
                return RedirectToAction("SignIn", "Home");
            }
            ViewBag.state = state;
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


        public bool checkLoggedIn()
        {
            if (HttpContext.Session.GetInt32("CurrentUserID") == null)
            {
                return false;
            }
            return true;
        }
    }
}
