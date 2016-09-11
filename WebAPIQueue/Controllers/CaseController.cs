using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using WebAPIQueue.Models;
using WebAPIQueue.Models.DataModel;

namespace WebAPIQueue.Controllers
{
    public class CaseController : ApiController
    {

        // private static ConcurrentQueue<APIViewModel> cq = new ConcurrentQueue<APIViewModel>();
        // GET api/values

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);


        public IEnumerable<CaseViewModel> Get()
        {
            WebDB db = new WebDB();
            var result = db.UserCases.OrderByDescending(x => x.CaseId).Select(x => new CaseViewModel
            {
                Id = x.Id,
                CaseId = x.CaseId,
                CreatedDate = x.CreatedDate.ToString(),
                UserNewName = x.UserNewName
            }).ToList();
            return result;
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        [Route("api/Case/requestSave")]
        [HttpPost]
        public async void requestSave([FromBody]APIViewModel Identifier)
        {
            // Updating Identifier becuase not getting from post. TODO
            Identifier.Identifier = Guid.NewGuid().ToString();

            Debug.WriteLine("Request Came  : " + Identifier.Identifier + " At :   " + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));

            //Lock the code. Not using "lock" because it not accept any async request inside
            await semaphoreSlim.WaitAsync();

            Debug.WriteLine("Lock : " + Identifier.Identifier + " At :  " + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
            try
            {
                WebDB db = new WebDB();

                var res = new List<int>();
                HttpClient hc = new HttpClient();

                //Testing calling 3rd party api
                var d = await hc.GetAsync("https://restcountries.eu/rest/v1/all");

                //Delaying request by 10 second
                await Task.Delay(10000);

                Debug.WriteLine("After Delay : " + Identifier.Identifier + "  At :  " + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));

                var reuestList = db.QueueRequests.ToList();
                foreach (var req in reuestList)
                {
                    var user = db.AppUsers.FirstOrDefault(x => x.Id == req.UserId);

                    UserCase uc = new UserCase
                    {
                        CaseId = db.UserCases.Count() == 0 ? 1 : (db.UserCases.Max(x => x.CaseId) + 1),
                        CreatedDate = DateTime.Now,
                        UserNewName = user.Name + " " + Guid.NewGuid().ToString()
                    };
                    db.UserCases.Add(uc);
                    db.QueueRequests.Remove(req);
                    db.SaveChanges();

                    Debug.WriteLine("Saved : " + uc.CaseId + " : " + Identifier.Identifier + "   at :" + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
                }
            }
            catch(Exception e)
            {

            }
            finally
            {
                semaphoreSlim.Release();

                Debug.WriteLine("Release  : " + Identifier.Identifier + "   At : " + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
            }
        }

    }
}
