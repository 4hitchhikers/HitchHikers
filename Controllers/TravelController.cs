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
        [Route("Dashboard/{state}")]
        public IActionResult Dashboard(string state)
        {
            ViewBag.state = state;
            ViewBag.Email = HttpContext.Session.GetString("email");
            ViewBag.CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");
            
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
            return View();
        }

        [HttpGet]
        [Route("CollectivePhotos")]
        public IActionResult CollectivePhotos()
        {
            return View();
        }

        [HttpGet]
        [Route("StartChat")]
        public IActionResult StartChat()
        {
            if (!CheckLoggedIn())
            {
                return RedirectToAction("Login", "Home");
            }
            ViewBag.CurrentUser = _dbcontext.Users.Where(e=>e.userid == (int)HttpContext.Session.GetInt32("CurrentUserID"));
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
