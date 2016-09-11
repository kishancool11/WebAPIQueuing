using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using WebAPIQueue.Models;
using WebAPIQueue.Models.DataModel;

namespace WebAPIQueue.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }


        public ActionResult Create()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult Create(string Name)
        {
            if (ModelState.IsValid)
            {
                WebDB db = new WebDB();
                AppUser u = new AppUser
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Name = Name
                };


                db.AppUsers.Add(u);
                db.SaveChanges();

                QueueRequest r = new QueueRequest
                {
                    UserId = u.Id
                };

                db.QueueRequests.Add(r);
                db.SaveChanges();

                var Identifier = new APIViewModel { Identifier = u.Identifier };

                // Call Local API for Queue 
                HttpClient client = new HttpClient();
                client.BaseAddress =  new Uri("http://localhost:50317/");
                string uri = "api/Case/requestSave";
                client.PostAsJsonAsync<APIViewModel>(uri, Identifier);

                return PartialView();
            }
            else
            {
                return PartialView(Name);
            }
        }
    }
}
