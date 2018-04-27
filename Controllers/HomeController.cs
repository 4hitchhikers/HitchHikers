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
using Newtonsoft.Json;

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
            var states = _dbcontext.Pictures.ToList();
            ViewBag.MyStates = JsonConvert.SerializeObject(states);
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
        public IActionResult Register(RegisterViewModel model, string state)
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
                return RedirectToAction("Index");
            }
            return View("Login");
        }


        [HttpPost]
        [Route("login")]
        public IActionResult Login(string login_email, string loginpw)
        {

            PasswordHasher<User> Hasher = new PasswordHasher<User>();

            var loginUser = _dbcontext.Users.SingleOrDefault(User => User.Email == login_email);
            if (loginUser != null)
            {
                var hashedPw = Hasher.VerifyHashedPassword(loginUser, loginUser.Password, loginpw);
                if (hashedPw == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetInt32("CurrentUserID", loginUser.Userid);
                    return RedirectToAction("Index");
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



    //                var max = 0,
    //     min = Number.MAX_VALUE,
    //     cc,
    //     endColor = [88, 6, 94],
    //     startColor = [202, 10, 10],
    //     colors = {},
    //     hex;

    // find maximum and minimum values
    // for (cc in gdpData)
    // {
    //     if (parseFloat(gdpData[cc]) > max)
    //     {
    //         max = parseFloat(gdpData[cc]);
    //     }
    //     if (parseFloat(gdpData[cc]) < min)
    //     {
    //     min = parseFloat(gdpData[cc]);
    //     }
    // }
    // console.log(max + "MAXIMUM");
    // console.log(min + "MINIMUM");
    //  set colors according to values of GDP
    // for (cc in gdpData)
    // {
    //     if (gdpData[cc] > 0)
    //     {
    //         colors[cc] = '#';
    //         for (var i = 0; i<3; i++)
    //         {
    //             hex = Math.round(startColor[i]
    //                 + (endColor[i]
    //                 - startColor[i])
    //                 * (gdpData[cc] / (max - min))).toString(16);
    //             console.log("**********");
    //             console.log(hex);
    //             console.log(endColor[i]);
    //             console.log(startColor[i]);
    //             console.log(gdpData[cc]);
    //             console.log("**********");
    //             if (hex.length == 1)
    //             {
    //                hex = '0'+hex;
    //        }

    //            colors[cc] += (hex.length == 1 ? '0' : '') + hex;













    
// @{
//         Dictionary<string, int> data = new Dictionary<string, int>() 
//         {
//           {"AL", 0},
//           {"AK", 0},
//           {"AS", 0},
//           {"AZ", 0},
//           {"AR", 0},
//           {"CA", 0},
//           {"CO", 0},
//           {"CT", 0},
//           {"DE", 0},
//           {"DC", 0},
//           {"FM", 0},
//           {"FL", 0},
//           {"GA", 0},
//           {"GU", 0},
//           {"HI", 0},
//           {"ID", 0},
//           {"IL", 0},
//           {"IN", 0},
//           {"IA", 0},
//           {"KS", 0},
//           {"KY", 0},
//           {"LA", 0},
//           {"ME", 0},
//           {"MH", 0},
//           {"MD", 0},
//           {"MA", 0},
//           {"MI", 0},
//           {"MN", 0},
//           {"MS", 0},
//           {"MO", 0},
//           {"MT", 0},
//           {"NE", 0},
//           {"NV", 0},
//           {"NH", 0},
//           {"NJ", 0},
//           {"NM", 0},
//           {"NY", 0},
//           {"NC", 0},
//           {"ND", 0},
//           {"MP", 0},
//           {"OH", 0},
//           {"OK", 0},
//           {"OR", 0},
//           {"PW", 0},
//           {"PA", 0},
//           {"PR", 0},
//           {"RI", 0},
//           {"SC", 0},
//           {"SD", 0},
//           {"TN", 0},
//           {"TX", 0},
//           {"UT", 0},
//           {"VT", 0},
//           {"VI", 0},
//           {"VA", 0},
//           {"WA", 0},
//           {"WV", 0},
//           {"WI", 0},
//           {"WY", 0} 
//    };
//    var keys = new List<string>(data.Keys);
//  }  



//     @foreach(string i in keys)
//     {
//         @foreach(var state in @ViewBag.states)
//         {
//             if(@i == @state.States)
//             {
//                 data[i] += 1;
//             }
//         }
//     }

//     @foreach( var b in data)
//     {
//         <p>@b.Key and @b.Value</p>
//     }


// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


//     jQuery(document).ready(function() {

//         jQuery('#usa').vectorMap(

//         {

//             map: 'usa_en',
//             backgroundColor: 'white',
//             borderColor: '#FF9900',
//             borderOpacity: 0.60,
//             borderWidth: 2,
//             color: '#1076C8',
//             hoverColor: '#0A4C82',
//             selectedColor: '#FF9900',

//         }

//     );

// });
