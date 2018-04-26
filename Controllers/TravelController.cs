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

        [HttpPost]
        [HttpGet]
        [Route("Create")]
        public IActionResult Create(CreateViewModel model)
        {
            return View(model);
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
