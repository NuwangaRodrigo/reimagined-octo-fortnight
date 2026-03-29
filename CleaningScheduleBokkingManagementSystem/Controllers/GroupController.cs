using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CleaningScheduleBokkingManagementSystem.Controllers
{
    public class GroupController : Controller
    {
        
        public int residentId;
        public ActionResult Group()
        {
            return View();
        }
       

    }
}