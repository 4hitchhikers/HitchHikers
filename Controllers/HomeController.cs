using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hitchhikers.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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
            return View("Index");
        }

        [HttpGet]
        [Route("signin")]
        public IActionResult SignIn()
        {
            return View("Login");
        }

        [HttpPost]
        [Route("register")]
        public IActionResult Register(RegisterViewModel model)
        {
            // User user = _dbcontext.Users.Where(e=>e.Email == model.Email).SingleOrDefault();
            // if(user != null){
            //     ViewBag.Error ="Email already registered";
            //     return View("Login");
            // }
            if (ModelState.IsValid)
            {
                User NewUser  = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Nickname = model.Nickname,
                    Email = model.Email,
                    Password = model.Password

                };
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                NewUser.Password = Hasher.HashPassword(NewUser, NewUser.Password);
                _dbcontext.Users.Add(NewUser);
                _dbcontext.SaveChanges();
                var loginUser = _dbcontext.Users.SingleOrDefault(User => User.Email == model.Email);
                HttpContext.Session.SetInt32("CurrentUserID", loginUser.Userid);
                return RedirectToAction("Create", "Travel");
            }
            return View("Login");
        }


        [HttpPost]
        [Route("login")]
        public IActionResult Login(string Email, string loginpw)
        {
            PasswordHasher<User> Hasher = new PasswordHasher<User>();

            var loginUser = _dbcontext.Users.SingleOrDefault(User => User.Email == Email);
            if (loginUser != null)
            {
                var hashedPw = Hasher.VerifyHashedPassword(loginUser, loginUser.Password, loginpw);
                if (hashedPw == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetInt32("CurrentUserID", loginUser.Userid);
                    return RedirectToAction("Create", "Travel");
                }
            }

            ViewBag.Error = "Email address or Password is not matching";
            return View("Login");
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
