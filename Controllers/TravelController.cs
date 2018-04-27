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
        public TravelController(TravelContext context, IHostingEnvironment environment)
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
            ViewBag.CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");
            int UserID = ViewBag.CurrentUserID;
            var User = _dbcontext.Users.Where(u => u.Userid == (int)HttpContext.Session.GetInt32("CurrentUserID"))
                                    .Include(pic => pic.Uploaded).ToList();

            var uploaded = _dbcontext.Pictures.Where(user => user.UploaderId == UserID).GroupBy(s => s.States).ToList();
            var alluploaded = _dbcontext.Pictures.Where(user => user.UploaderId == UserID).ToList();
            int count = _dbcontext.Pictures.Where(user => user.UploaderId == UserID).Count();
            ViewBag.AllUploaded = alluploaded;
            ViewBag.Count = count;
            ViewBag.User = User;
            User userState = _dbcontext.Users.Where(u => u.Userid == (int)HttpContext.Session.GetInt32("CurrentUserID"))
                                    .Include(pic => pic.Uploaded).SingleOrDefault();
            // MostVisted(userState);

            // var states = _dbcontext.Pictures.SingleOrDefault(v => v.UploaderId == (int)HttpContext.Session.GetInt32("CurrentUserID"));
            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            var states = _dbcontext.Pictures.Where(v => v.UploaderId == (int)HttpContext.Session.GetInt32("CurrentUserID")).ToList();
            ViewBag.MyStates = JsonConvert.SerializeObject(states, jss);
            return View("Dashboard");
        }

        [HttpGet]
        [Route("CollectivePhotos/viewPicture/{picID}")]
        public IActionResult ViewPicture(int picID)
        {
            if (!CheckLoggedIn())
            {
                return RedirectToAction("SignIn", "Home");
            }
            ViewBag.CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");

            Picture photo = _dbcontext.Pictures.Where(e => e.PictureId == picID).Include(p => p.Uploader).SingleOrDefault();
            ViewBag.Pic = photo;

            return View("ViewPicture");
        }

        [HttpGet]
        [Route("CollectivePhotos/viewUser/{userID}")]
        public IActionResult ViewUser(int UserID)
        {
            if (!CheckLoggedIn())
            {
                return RedirectToAction("SignIn", "Home");
            }
            var User = _dbcontext.Users.Where(u => u.Userid == UserID)
                                    .Include(pic => pic.Uploaded).ToList();
            ViewBag.CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");

            var uploaded = _dbcontext.Pictures.Where(user => user.UploaderId == UserID).GroupBy(s => s.States).ToList();
            var alluploaded = _dbcontext.Pictures.Where(user => user.UploaderId == UserID).ToList();
            int count = _dbcontext.Pictures.Where(user => user.UploaderId == UserID).Count();
            ViewBag.AllUploaded = alluploaded;
            ViewBag.Count = count;
            ViewBag.User = User;

            // var states = _dbcontext.Pictures.SingleOrDefault(v => v.UploaderId == (int)HttpContext.Session.GetInt32("CurrentUserID"));
            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            var states = _dbcontext.Pictures.Where(v => v.UploaderId == UserID).ToList();
            ViewBag.MyStates = JsonConvert.SerializeObject(states, jss);
            
            return View("Dashboard");
        }

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

            Picture NewPicture = new Picture
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
            return RedirectToAction("CollectivePhotos", new { state = model.States });
            // }
            // return View("Create");
        }
        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return fileName;
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
        [Route("CollectivePhotos/viewUser/CollectivePhotos/{state}")]
        public IActionResult RedirectCollectivePhotos(string state){
            return RedirectToAction("CollectivePhotos", new{state = state});
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
            string color = "rgb(" + rnd.Next(150) + "," + rnd.Next(256) + "," + rnd.Next(256) + ")";
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

        // public Dictionary<string, int> MostVisted(User user)
        // {
        //     Dictionary<string, int> data = new Dictionary<string, int>()
        //         {{"AL", 0},{"AK", 0},{"AS", 0},{"AZ", 0},{"AR", 0},{"CA", 0},{"CO", 0},{"CT", 0},{"DE", 0},{"DC", 0},{"FM", 0},{"FL", 0},{"GA", 0},{"GU", 0},{"HI", 0},{"ID", 0},{"IL", 0},{"IN", 0},{"IA", 0},{"KS", 0},{"KY", 0},{"LA", 0},{"ME", 0},{"MH", 0},{"MD", 0},{"MA", 0},{"MI", 0},{"MN", 0},{"MS", 0},{"MO", 0},{"MT", 0},{"NE", 0},{"NV", 0},{"NH", 0},{"NJ", 0},{"NM", 0},{"NY", 0},{"NC", 0},{"ND", 0},{"MP", 0},{"OH", 0},{"OK", 0},{"OR", 0},{"PW", 0},{"PA", 0},{"PR", 0},{"RI", 0},{"SC", 0},{"SD", 0},{"TN", 0},{"TX", 0},{"UT", 0},{"VT", 0},{"VI", 0},{"VA", 0},{"WA", 0},{"WV", 0},{"WI", 0},{"WY", 0} };
        //     var keys = new List<string>(data.Keys);
        //     var vistedState = user.Uploaded.
        //     foreach (string i in keys)
        //     {
        //         foreach (var state in ViewBag.states)
        //         {
        //             if (i == state.States)
        //             {
        //                 data[i] += 1;
        //             }
        //         }
        //     }
        //     foreach(var b in data)
        // {
        // b.Ke b.Value </ p >
        // }
        // }



    }
}
