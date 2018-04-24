using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hitchhikers.Models;
using Microsoft.AspNetCore.Http;

namespace Hitchhikers.Controllers
{
    public class HomeController : Controller
    {
        private TravelContext _dbcontext;

        //Constructor
        public HomeController(TravelContext context)
        {
            _dbcontext = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View("login");
        }

        [HttpPost]
        [HttpGet]
        [Route("register")]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                HttpContext.Session.SetString("email", model.Email);
                HttpContext.Session.SetString("firstname", model.FirstName);

                var user = new User
                {
                    first_name = model.FirstName,
                    last_name = model.LastName,
                    email = model.Email,
                    password = model.Password,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow

                };

                _dbcontext.Users.Add(user);
                _dbcontext.SaveChanges();

                return RedirectToAction("Dashboard", "Travel");
            }
            return View(model);
        }

        [HttpGet]
        [HttpPost]
        [Route("login")]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _dbcontext.Users.SingleOrDefault(dbUser => dbUser.email == model.Email && dbUser.password == model.Password);

                if (user != null)
                {
                    HttpContext.Session.SetString("email", model.Email);
                    return RedirectToAction("Dashboard", "Travel");
                }
                else
                {
                    ViewBag.UserOrPasswordIsWrong = "Username or password is not valid";
                }
            }
            return View(model);
        }

        [HttpGet]
        [Route("logoff")]
        public IActionResult Logoff()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
