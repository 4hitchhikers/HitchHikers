using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hitchhikers.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using System.Web;
using System.Drawing;
using System.Net.Http.Headers;
using System.IO;
using Microsoft.AspNetCore.Hosting.Internal;

namespace Hitchhikers.Controllers
{
    public class TravelController : Controller
    {
        private TravelContext _dbcontext;
        private Random rnd = new Random();
        private readonly IHostingEnvironment hostingEnvironment;

        //Constructor
        public TravelController(TravelContext context,IHostingEnvironment environment)
        {
            _dbcontext = context;
            hostingEnvironment = environment;

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
            ViewBag.CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");
            var currentUser = _dbcontext.Users.Where(User => User.Userid == (int)HttpContext.Session.GetInt32("CurrentUserID"))
                                    .Include(pic => pic.Uploaded).ToList();
            // var userstates = _dbcontext.Users.Where(User=>User.Userid == (int)HttpContext.Session.GetInt32("CurrentUserID"))
            //             .Include(pic=>pic.Uploaded.GroupBy(p=>p.States)).ToList();
            //             foreach(var item in userstates){
            //                 System.Console.WriteLine(item.Uploaded.Count());

            //             }
            var uploaded = _dbcontext.Pictures.Where(user => user.UploaderId == (int)HttpContext.Session.GetInt32("CurrentUserID")).GroupBy(s => s.States).ToList();
            var alluploaded = _dbcontext.Pictures.Where(user => user.UploaderId == (int)HttpContext.Session.GetInt32("CurrentUserID")).ToList();
            ViewBag.CurrentUser = currentUser;
            ViewBag.AllUploaded = alluploaded;

            // var states = _dbcontext.Pictures.SingleOrDefault(v => v.UploaderId == (int)HttpContext.Session.GetInt32("CurrentUserID"));
            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            var states = _dbcontext.Pictures.Where(v => v.UploaderId == (int)HttpContext.Session.GetInt32("CurrentUserID")).ToList();
            ViewBag.MyStates = JsonConvert.SerializeObject(states, jss);
            return View("Dashboard");
        }

        // [HttpGet]
        // [Route("viewPicture/{userID}")]
        // public IActionResult ViewPicture(int UserID)
        // {
        //     if (!CheckLoggedIn())
        //     {
        //         return RedirectToAction("SignIn", "Home");
        //     }
        //     var Photo = _dbcontext.Users.Where(User => User.Userid == (int)HttpContext.Session.GetInt32("CurrentUserID"))
        //                             .Include(pic => pic.Uploaded).ToList();
        //     // var userstates = _dbcontext.Users.Where(User=>User.Userid == (int)HttpContext.Session.GetInt32("CurrentUserID"))
        //     //             .Include(pic=>pic.Uploaded.GroupBy(p=>p.States)).ToList();
        //     //             foreach(var item in userstates){
        //     //                 System.Console.WriteLine(item.Uploaded.Count());

        //     //             }

        //     var userPhoto =_dbcontext.Users.Where(User => User.Userid == UserID)
        //                             .Include(pic => pic.Uploaded).ToList();
        //     ViewBag.UserPhoto = userPhoto;
        //     return View("Dashboard");
        // }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            ViewBag.CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");
            List<string> StateList = new List<string>
            { "AL", "AK", "AS", "AZ", "AR", "CA", "CO", "CT", "DE", "DC","FM", "FL","GA", "GU","HI","ID","IL", "IN", "IA","KS","KY","LA","ME", "MH", "MD", "MA", "MI","MN","MS","MO", "MT", "NE","NV","NH","NJ", "NM", "NY", "NC", "ND","MP","OH","OK", "OR",  "PW", "PA","PR","RI", "SC", "SD", "TN","TX","UT","VT", "VI", "VA", "WA", "WV", "WI","WY"};
            ViewBag.all_states = StateList;

            return View("Create");
        }

        [HttpPost]
        [Route("AddPhoto")]
        public IActionResult AddPhoto(CreateViewModel model, List<IFormFile> PictName)
        {
            
            // if(ModelState.IsValid) 
            // {
                long size = PictName.Sum(f => f.Length);
                string strfullPath = "";
                // full path to file in temp location
                var filePath = Path.GetTempFileName();

                foreach (var formFile in PictName)
                {
                    var uploads = Path.Combine(hostingEnvironment.WebRootPath, "images");
                    var fullPath = Path.Combine(uploads, GetUniqueFileName(formFile.FileName));
                    strfullPath = GetUniqueFileName(formFile.FileName);
                    formFile.CopyTo(new FileStream(fullPath, FileMode.Create));
                }

                Picture NewPicture  = new Picture
                    {
                        UploaderId = (int)HttpContext.Session.GetInt32("CurrentUserID"),
                        States = model.States,
                        City = model.City,
                        PictName = strfullPath,
                        DateVisited = model.DateOfVisit,
                        Description = model.Description,
                    };
    
                    _dbcontext.Pictures.Add(NewPicture);
                    _dbcontext.SaveChanges();
                    return RedirectToAction("CollectivePhotos", new {state = model.States });
            // }
            // return View("Create");
        }
        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return fileName;
        }

        [HttpGet]
        [Route("ViewPicture/{photoID}")]
        public IActionResult ViewPicture(int photoID)
        {
            ViewBag.CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");
            var pic = _dbcontext.Pictures.Where(e => e.PictureId == photoID).Include(p => p.Uploader).SingleOrDefault();
            ViewBag.Pic = pic;
            return View("ViewPicture");
        }

        [HttpPost]
        [Route("comment/{photoID}")]
        public IActionResult Comment(int photoID, string comment)
        {
            Comment newComment = new Comment
            {
                CommentText = comment,
                SenderId = (int)HttpContext.Session.GetInt32("CurrentUSerID"),
                PictureId = photoID
            };
            _dbcontext.Comments.Add(newComment);
            _dbcontext.SaveChanges();
            return RedirectToAction("ViewPicture", new { photoID = photoID });

        }

        [HttpGet]
        [Route("CollectivePhotos/{state}")]
        public IActionResult CollectivePhotos(string state)
        {
            if (!CheckLoggedIn())
            {
                return RedirectToAction("SignIn", "Home");
            }
            ViewBag.CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");
            var Pictures = _dbcontext.Pictures.Include(users => users.Uploader).Where(states => states.States == state).ToList();
            ViewBag.DisplayPhoto = Pictures;
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
            ViewBag.CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");
            User user = _dbcontext.Users.Where(e => e.Userid == (int)HttpContext.Session.GetInt32("CurrentUserID")).SingleOrDefault();
            ViewBag.CurrentUser = user.FirstName;
            ViewBag.UserColor = Getcolor();
            return View("Chatroom");
        }

        private string Getcolor()
        {
            string color = "rgb(" + rnd.Next(150) +"," + rnd.Next(256) + "," + rnd.Next(256) +")";
            return color;
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
