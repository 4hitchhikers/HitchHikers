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

            var checkEmail = _dbcontext.Users.SingleOrDefault(e => e.Email == model.Email);
            if (ModelState.IsValid)
            {
                if(checkEmail != null)
                {
                    TempData["error"] = "Email already in use";
                    return View("Login");
                }
            
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

                HttpContext.Session.SetInt32("CurrentUserID", NewUser.Userid);
                return RedirectToAction("Dashboard", "Travel");
            }
            return View("Login");
        }


        [HttpPost]
        [Route("login")]
        public IActionResult Login(string email, string loginpw)
        {

            PasswordHasher<User> Hasher = new PasswordHasher<User>();

            var loginUser = _dbcontext.Users.SingleOrDefault(User => User.Email == email);
            if (loginUser != null)
            {
                var hashedPw = Hasher.VerifyHashedPassword(loginUser, loginUser.Password, loginpw);
                if (hashedPw == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetInt32("CurrentUserID", loginUser.Userid);
                    return RedirectToAction("Dashboard", "Travel");
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
            return RedirectToAction("Login");
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}



            // var checkEmail = _dbcontext.Users.SingleOrDefault(e => e.Email == email);
            // if(checkEmail==null)
            // {
            //     ViewBag.email_error = "Please check your email otherwie go to register";
            //     return View("Login");
            // }

            // if(checkEmail!=null && loginpw!= null)
            // {
            //     var hasher = new PasswordHasher<User>();
            //     if(0 != hasher.VerifyHashedPassword(checkEmail, checkEmail.Password, loginpw))
            //     {
            //         HttpContext.Session.SetInt32("CurrentUserID", checkEmail.Userid);
            //         // var id = HttpContext.Session.GetInt32("userid");
            //         return RedirectToAction("Dashboard", "Travel");
            //     }
            // }
            // ViewBag.psw_error = "Password is incorrect";
            // return View("Login");