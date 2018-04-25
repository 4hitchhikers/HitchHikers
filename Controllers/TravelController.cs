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
        [HttpGet]
        [Route("Dashboard/{state}")]
        public IActionResult Dashboard(string state)
        {
            ViewBag.state = state;
            ViewBag.Email = HttpContext.Session.GetString("email");
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
        [Route("CollectivePhotos")]
        public IActionResult CollectivePhotos()
        {
            return View("CollectivePhotos");
        }

        [HttpGet]
        [Route("StartChat")]
        public IActionResult StartChat()
        {
            return View("Chatroom");
        }

        [HttpGet]
        [Route("EnlargePhoto")]
        public IActionResult EnlargePhoto()
        {
            return View("EnlargePhoto");
        }
    }
}
