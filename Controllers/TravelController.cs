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
        [Route("/Dashboard")]
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
            var alluploaded = _dbcontext.Pictures.Where(user => user.UploaderId == UserID).ToList();
            User userState = _dbcontext.Users.Where(u => u.Userid == (int)HttpContext.Session.GetInt32("CurrentUserID"))
                                    .Include(pic => pic.Uploaded).SingleOrDefault();
            var states = _dbcontext.Pictures.Where(v => v.UploaderId == (int)HttpContext.Session.GetInt32("CurrentUserID")).ToList();

            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            ViewBag.MostVisted = MostVisted(userState).Take(5); ;
            ViewBag.MyStates = JsonConvert.SerializeObject(states, jss);
            ViewBag.AllUploaded = alluploaded;
            ViewBag.Count = alluploaded.Count;
            ViewBag.User = User;
            return View("Dashboard");
        }

        [HttpGet]
        [Route("/Create")]
        public IActionResult Create(string Page)
        {
            if (!CheckLoggedIn())
            {
                return RedirectToAction("SignIn", "Home");
            }
            HttpContext.Session.SetString("PageName", Page);
            ViewBag.CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");
            List<string> StateList = new List<string>
            { "AL", "AK", "AS", "AZ", "AR", "CA", "CO", "CT", "DE", "DC","FM", "FL","GA", "GU","HI","ID","IL", "IN", "IA","KS","KY","LA","ME", "MH", "MD", "MA", "MI","MN","MS","MO", "MT", "NE","NV","NH","NJ", "NM", "NY", "NC", "ND","MP","OH","OK", "OR",  "PW", "PA","PR","RI", "SC", "SD", "TN","TX","UT","VT", "VI", "VA", "WA", "WV", "WI","WY"};
            ViewBag.all_states = StateList;
            return View("Create");
        }

        [HttpPost]
        [Route("/AddPhoto")]
        public IActionResult AddPhoto(CreateViewModel model, List<IFormFile> PictName)
        {
            System.Console.WriteLine(model.States);
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
                Address = model.Address
            };

            _dbcontext.Pictures.Add(NewPicture);
            _dbcontext.SaveChanges();
            string PageName = (string)HttpContext.Session.GetString("PageName");

            if (PageName != "Dashboard")
            {
                return RedirectToAction("CollectivePhotos", new { state = model.States });
            }
            return RedirectToAction("Dashboard");
        }
        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return fileName;
        }

        [HttpGet]
        [Route("/CollectivePhotos/viewPicture/{picID}")]
        public IActionResult ViewPicture(int picID)
        {
            ViewBag.CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");
            if (!CheckLoggedIn())
            {
                return RedirectToAction("SignIn", "Home");
            }
            Picture photo = _dbcontext.Pictures.Where(e => e.PictureId == picID).Include(p => p.Uploader).SingleOrDefault();
            ViewBag.Pic = photo;

            var comment = _dbcontext.Comments.Include(u => u.Sender).Where(c => c.PictureId == picID).Include(u => u.Sender).ToList();
            ViewBag.Comment = comment;
            return View("ViewPicture");
        }


        [HttpGet]
        [Route("/CollectivePhotos/viewUser/{userID}")]
        [Route("/CollectivePhotos/viewUser/viewUser/{userID}")]
        public IActionResult ViewUser(int UserID)
        {
            if (!CheckLoggedIn())
            {
                return RedirectToAction("SignIn", "Home");
            }
            var User = _dbcontext.Users.Where(u => u.Userid == UserID)
                                    .Include(pic => pic.Uploaded).ToList();
            ViewBag.CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");

            User userState = _dbcontext.Users.Where(u => u.Userid == (int)HttpContext.Session.GetInt32("CurrentUserID"))
                                    .Include(pic => pic.Uploaded).SingleOrDefault();
            ViewBag.MostVisted = MostVisted(userState).Take(5);

            var uploaded = _dbcontext.Pictures.Where(user => user.UploaderId == UserID).GroupBy(s => s.States).ToList();
            var alluploaded = _dbcontext.Pictures.Where(user => user.UploaderId == UserID).ToList();
            int count = _dbcontext.Pictures.Where(user => user.UploaderId == UserID).Count();
            var states = _dbcontext.Pictures.Where(v => v.UploaderId == UserID).ToList();

            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            ViewBag.AllUploaded = alluploaded;
            ViewBag.Count = count;
            ViewBag.User = User;
            ViewBag.MyStates = JsonConvert.SerializeObject(states, jss);

            return View("Dashboard");
        }

        // [HttpGet]
        // [Route("/CollectivePhotos/viewUser/{UserID}")]
        // public IActionResult ViewViewUser(int UserID)
        // {
        //     User userState = _dbcontext.Users.Where(u => u.Userid == (int)HttpContext.Session.GetInt32("CurrentUserID"))
        //                             .Include(pic => pic.Uploaded).SingleOrDefault();
        //     ViewBag.MostVisted = MostVisted(userState).Take(5); ;
        //     return RedirectToAction("ViewUser", new { UserID = UserID });
        // }

        [HttpPost]
        [Route("/comment")]
        public IActionResult Comment(int photoID, string comment)
        {
            Comment newComment = new Comment
            {
                CommentText = comment,
                SenderId = (int)HttpContext.Session.GetInt32("CurrentUserID"),
                PictureId = photoID
            };
            _dbcontext.Comments.Add(newComment);
            _dbcontext.SaveChanges();
            // return RedirectToAction("ViewPicture", new { photoID = photoID });
            return Redirect($"/CollectivePhotos/viewPicture/{photoID}");
        }

        // route needs to be configure 
        [HttpGet]
        [Route("/CollectivePhotos/viewPicture/delete/{Cmtid}")]
        public IActionResult DeleteCmt(int Cmtid)
        {
            int CurrentUserID = (int)HttpContext.Session.GetInt32("CurrentUserID");
            Comment deleteCmt = _dbcontext.Comments.Where(e => e.Commentid == Cmtid).SingleOrDefault();
            if (deleteCmt.SenderId != CurrentUserID)
            {
                return Redirect($"/CollectivePhotos/ViewPicture/{deleteCmt.PictureId}");
            }
            _dbcontext.Comments.Remove(deleteCmt);
            _dbcontext.SaveChanges();
            return Redirect($"/CollectivePhotos/ViewPicture/{deleteCmt.PictureId}");
        }


        [HttpGet]
        [Route("/CollectivePhotos/{state}")]
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
        // [HttpGet]
        // [Route("/CollectivePhotos/{state}")]
        // public IActionResult RedirectCollectivePhotos(string state)
        // {
        //     return RedirectToAction("CollectivePhotos", new { state = state });
        // }

        [HttpGet]
        [Route("/StartChat")]
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

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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

        public List<Tuple<int, string>> MostVisted(User user)
        {
            Dictionary<string, int> data = new Dictionary<string, int>()
                {{"AL", 0},{"AK", 0},{"AS", 0},{"AZ", 0},{"AR", 0},{"CA", 0},{"CO", 0},{"CT", 0},{"DE", 0},{"DC", 0},{"FM", 0},{"FL", 0},{"GA", 0},{"GU", 0},{"HI", 0},{"ID", 0},{"IL", 0},{"IN", 0},{"IA", 0},{"KS", 0},{"KY", 0},{"LA", 0},{"ME", 0},{"MH", 0},{"MD", 0},{"MA", 0},{"MI", 0},{"MN", 0},{"MS", 0},{"MO", 0},{"MT", 0},{"NE", 0},{"NV", 0},{"NH", 0},{"NJ", 0},{"NM", 0},{"NY", 0},{"NC", 0},{"ND", 0},{"MP", 0},{"OH", 0},{"OK", 0},{"OR", 0},{"PW", 0},{"PA", 0},{"PR", 0},{"RI", 0},{"SC", 0},{"SD", 0},{"TN", 0},{"TX", 0},{"UT", 0},{"VT", 0},{"VI", 0},{"VA", 0},{"WA", 0},{"WV", 0},{"WI", 0},{"WY", 0} };
            var keys = new List<string>(data.Keys);
            var vistedState = _dbcontext.Pictures.Include(e => e.Uploader).Where(p => p.Uploader.Userid == (int)HttpContext.Session.GetInt32("CurrentUserID")).ToList();
            var list = new List<Tuple<int, string>>();
            foreach (string i in keys)
            {
                foreach (var state in vistedState)
                {
                    if (i == state.States)
                    {
                        data[i] += 1;
                    }
                }
                list = new List<Tuple<int, string>>();
                foreach (KeyValuePair<string, int> kvp in data)
                {
                    if (kvp.Value != 0)
                    {
                        list.Add(Tuple.Create(kvp.Value, kvp.Key));
                    }
                }
            }
            return list;
        }
    }
}
