using CleaningScheduleBokkingManagementSystem.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CleaningScheduleBokkingManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        BookingScheduleManagementDBEntities2 dc = new BookingScheduleManagementDBEntities2();
       
        public int residentId;
        // GET: Home
        public ActionResult Index()
        {
            if (Session["Resident_Id"] != null)
            {

                residentId = (int)Session["Resident_Id"];

            }
            return View();
        }
        public JsonResult GetInitialSchedules()
        {
            residentId = (int)Session["Resident_Id"];
            int groupId = (int)(dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId)?.Group_Id);

            var events = dc.CLEANINGSCHEDULEs
   .Join(dc.RESIDENTS, cs => cs.Resident_Id, r => r.Resident_Id, (cs, r) => new { cs, r })
    .Where(joinedData => joinedData.cs.Group_Id == groupId)
    .AsEnumerable()
    .Select(joinedData => new
    {
        joinedData.cs.WeekNumber,
        joinedData.cs.SlotNumber,
        joinedData.cs.Start_Date,
        joinedData.cs.End_Date,
        joinedData.cs.Is_Cleaned,
        joinedData.cs.Is_Verified,
        joinedData.cs.Theme_Colour,
        joinedData.cs.Is_FullDay,
        joinedData.cs.Group_Id,
        ResidentName = (joinedData.cs.Start_Date != null && joinedData.cs.Resident_Id == null) ? "Choose a slot" : joinedData.r.Full_Name,
       
    })
   
    .ToList();





            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };


        }
        [HttpPost]
        public JsonResult ChooseScedule(CLEANINGSCHEDULE e)
        {
            var Status = false;
            // using (BookingScheduleManagementSystemEntities dc = new BookingScheduleManagementSystemEntities())
            //{
            if (e.WeekNumber > 0)
            {
                // Update schedule
                var v = dc.CLEANINGSCHEDULEs.Where(a => (a.WeekNumber == e.WeekNumber && a.SlotNumber == e.SlotNumber)).FirstOrDefault();
                if (v != null)


                {
                    v.Resident_Id = (int?)Session["Resident_Id"];
                    v.Theme_Colour = "#AA336A";
                    v.Is_FullDay = true;
                }
            }

            else
            {
                dc.CLEANINGSCHEDULEs.Add(e);
            }
            dc.SaveChanges();
            Status = true;
            //}
            return new JsonResult { Data = new { status = Status } };
        }

        //public JsonResult DeleteSchedule(int weekNumber, int slotNumber)
        //{

        //    int residentId = (int)Session["Resident_Id"];

        //    //bool isAdmin = (bool)(dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId)?.Is_Admin);
        //    var Status = false;
        //    var currentUser = getCurrentUser(); // Get the current user (you need to implement this method)

        //    var v = dc.CLEANINGSCHEDULEs.FirstOrDefault(a => a.WeekNumber == weekNumber && a.SlotNumber == slotNumber);

        //    if (v != null)
        //    {
        //        if ( v.Resident_Id == currentUser.Resident_Id)
        //        {
        //            v.Resident_Id = 0;
        //            v.Theme_Colour = "#AA336A";
        //            dc.SaveChanges();
        //            Status = true;
        //        }
        //        else
        //        {

        //            Status = false;
        //        }
        //    }
        //    else
        //    {

        //        Status = false;
        //    }

        //    return Json(new { status = Status });
        //}
        [HttpPost]
        public JsonResult DeleteSchedule(int weekNumber, int slotNumber)
        {
            try
            {
                var currentUser = getCurrentUser(); // Get the current user (you need to implement this method)

                // Check if the current user is an admin
                if (currentUser != null && currentUser.Is_Admin)
                {
                    // Admin can delete any schedule
                    var schedule = dc.CLEANINGSCHEDULEs.FirstOrDefault(a => a.WeekNumber == weekNumber && a.SlotNumber == slotNumber);
                    if (schedule != null)
                    {
                        schedule.Resident_Id = 1;
                        schedule.Theme_Colour = "#388e3c";
                        dc.SaveChanges();
                        return Json(new { status = true });
                        
                    }
                }
                else
                {
                    // Non-admin user can only delete their own schedule
                    var schedule = dc.CLEANINGSCHEDULEs.FirstOrDefault(a => a.WeekNumber == weekNumber && a.SlotNumber == slotNumber && a.Resident_Id == currentUser.Resident_Id);
                    if (schedule != null)
                    {
                        schedule.Resident_Id = 1;
                        
                        schedule.Theme_Colour = "#388e3c";
                        dc.SaveChanges();
                        return Json(new { status = true });
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during deletion
                return Json(new { status = false, message = "An error occurred: " + ex.Message });
            }

            // If the schedule is not found or user does not have permission, return false
            return Json(new { status = false, message = "Unauthorized or schedule not found." });
        }
        [HttpPost]
        public JsonResult DeleteAddedSchedule(int weekNumber, int slotNumber)
        {
            try
            {
                var currentUser = getCurrentUser(); // Get the current user (you need to implement this method)

                // Check if the current user is an admin
                if (currentUser != null && currentUser.Is_Admin)
                {
                    // Admin can delete any schedule
                    var schedule = dc.CLEANINGSCHEDULEs.FirstOrDefault(a => a.WeekNumber == weekNumber && a.SlotNumber == slotNumber);
                    if (schedule != null)
                    {
                        dc.CLEANINGSCHEDULEs.Remove(schedule);
                        dc.SaveChanges();
                        return Json(new { status = true });
                    }
                }
                else
                {
                   
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during deletion
                return Json(new { status = false, message = "An error occurred: " + ex.Message });
            }

            // If the schedule is not found or user does not have permission, return false
            return Json(new { status = false, message = "Unauthorized or schedule not found." });
        }
        // POST: Calendar/AddSchedule
        [HttpPost]
        public ActionResult AddSchedule(DateTime eventDate)
        {
            try
            {
                int? residentId = (int?)Session["Resident_Id"];
                if (residentId == null)
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                using (var dc = new BookingScheduleManagementDBEntities2())
                {
                    bool isAdmin = dc.RESIDENTS
                                        .Where(r => r.Resident_Id == residentId)
                                        .Select(r => r.Is_Admin)
                                        .FirstOrDefault();

                    if (!isAdmin)
                    {
                        return Json(new { success = false, message = "Unauthorized access." });
                    }

                    Calendar calendar = CultureInfo.CurrentCulture.Calendar;
                    int weekNumber = calendar.GetWeekOfYear(eventDate, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);

                    var resident = dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId);
                    if (resident == null)
                    {
                        return HttpNotFound();
                    }

                    int groupId = resident.Group_Id;
                    var newSlot = new CLEANINGSCHEDULE
                    {
                        WeekNumber = weekNumber,
                        Start_Date = eventDate,
                        End_Date = eventDate,
                        Is_FullDay = true,
                        Theme_Colour = "#388e3c",
                        Resident_Id = 1,
                        Group_Id = groupId,
                    };

                    dc.CLEANINGSCHEDULEs.Add(newSlot);
                    dc.SaveChanges();

                    return Json(new { success = true, message = "Slot added successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }



        // GET: Home/CheckAdminStatus
        public ActionResult CheckAdminStatus()
        {
            try
            {
                int residentId = (int)Session["Resident_Id"];

                bool Admin = (bool)(dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId)?.Is_Admin);
                if (Admin == true)
                {
                    bool isAdmin = true;
                    return Json(new { isAdmin }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    bool isAdmin = false;
                    return Json(new { isAdmin }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                // Handle any exceptions and return an error message
                return Json(new { success = false, message = "An error occurred while checking admin status: " + ex.Message });
            }
        }
        public RESIDENT getCurrentUser()
        {
            // Check if session contains the resident ID
            int residentId = (int)Session["Resident_Id"];

            if (residentId != null)
            {
               
                var currentUser = dc.RESIDENTS.FirstOrDefault(u => u.Resident_Id == residentId);

                return currentUser;
            }
            else
            {
                
                return null;
            }
        }


        public JsonResult SchedulePlaning()
        {
            var events = dc.CLEANINGSCHEDULEs
    .Join(dc.RESIDENTS, cs => cs.Resident_Id, r => r.Resident_Id,
        (cs, r) => new { cs, r })
    .Select(joinedData => new
    {
        joinedData.cs.WeekNumber,
        joinedData.cs.SlotNumber,
        joinedData.cs.Start_Date,
        joinedData.cs.End_Date,
        joinedData.cs.Is_Cleaned,
        joinedData.cs.Is_Verified,
        joinedData.cs.Theme_Colour,
        joinedData.cs.Is_FullDay,
        joinedData.cs.Group_Id,
        ResidentName = (joinedData.cs.Start_Date != null && joinedData.cs.Resident_Id == null) ? "Choose a slot" : joinedData.r.Full_Name
    })
    .ToList();

            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        public ActionResult GetEventsForDate(string date)
        {
            DateTime selectedDate;
            if (DateTime.TryParse(date, out selectedDate))
            {
                var eventsForDate = dc.CLEANINGSCHEDULEs
                    .Where(e => e.Start_Date.HasValue && e.Start_Date.Value.Date == selectedDate.Date)
                    .ToList();


                var eventsData = eventsForDate.Select(e => new
                {
                    residentId = 0,
                    start = e.Start_Date.HasValue ? e.Start_Date.Value.ToString("yyyy-MM-ddTHH:mm:ss") : "", // Conditional formatting
                    weekNumber = GetWeekNumber(e.Start_Date),
                    slotNumber = GetSlotNumber(e.Start_Date, selectedDate.Date),
                });

                return Json(eventsData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { error = "Invalid date format" }, JsonRequestBehavior.AllowGet);
            }
        }

        // Helper method to get the week number for a given date
        private int GetWeekNumber(DateTime? date)
        {
            if (date.HasValue)
            {
                return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date.Value, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            }
            return 0; // Return 0 if date is null
        }

        // Helper method to get the slot number for a given event date and selected date
        private int GetSlotNumber(DateTime? eventDate, DateTime selectedDate)
        {
            if (eventDate.HasValue && eventDate.Value.Date == selectedDate)
            {
                // Assuming slot number is determined by the number of events on the same day
                var eventsOnSameDay = dc.CLEANINGSCHEDULEs.Count(e => e.Start_Date.HasValue && e.Start_Date.Value.Date == selectedDate);
                return eventsOnSameDay + 1;
            }
            return 0; // Return 0 if eventDate is null or different from selectedDate
        }

        public ActionResult GroupDetails()
        {
            var resident = dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == (int)Session["Resident_Id"]);
            if (resident == null)
            {
                return HttpNotFound();
            }

            int groupId = resident.Group_Id;
            //load same group members
            var users = (from r in dc.RESIDENTS
                         join g in dc.GROUPs on r.Group_Id equals g.Group_Id
                         where r.Group_Id == groupId
                         select new UserDetailsViewModel
                         {
                             FullName = r.Full_Name,
                             ContactNo = r.Contact_No,
                             GroupName = g.Group_Name
                         }).ToList();

            return View(users);
        }
        public ActionResult Register()
        {

            return View();


        }
        public ActionResult RegisterUser(RESIDENT RESIDENTS)
        {
            var Status = false;

            using (var db = new BookingScheduleManagementDBEntities2())
            {
                RESIDENTS.Group_Id = 1;
                RESIDENTS.Is_Admin = false;
                db.RESIDENTS.Add(RESIDENTS);
                db.SaveChanges();

                Status = true;
            }


            return RedirectToAction("Login", "Login");
        }
        public ActionResult Group()
        {
            //if (Session["Resident_Id"] != null)
            //{

            //    residentId = (int)Session["Resident_Id"];
            //    ViewBag.ResidentId = residentId;

            //}


            return View();

        }

        public JsonResult GetGroupMembers()
        {
            try
            {
                int residentId = (int)Session["Resident_Id"];

                bool isAdmin = (bool)(dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId)?.Is_Admin);

                int groupId = (int)(dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId)?.Group_Id);
                if (groupId != 1)
                {
                    var groupMembers = dc.RESIDENTS.Where(r => r.Group_Id == groupId)
                                                   .Select(r => new
                                                   {
                                                       r.Full_Name,
                                                       r.Email,
                                                       r.Contact_No,
                                                       r.Resident_Id,
                                                       r.Is_Admin
                                                   })
                                                   .ToList();
                    return Json(new { success = true, members = groupMembers, isAdmin = isAdmin }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, members = new List<object>(), isAdmin = isAdmin }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult LoadGroupName()
        {
            try
            {
                int GroupresidentId = (int)Session["Resident_Id"];
                int groupId = (int)(dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == GroupresidentId)?.Group_Id);

                // Check if the group ID is equal to 1
                if (groupId == 1)
                {
                    // If the group ID is 1, set the group name as an empty string or handle it accordingly
                    string groupName = ""; // Or any default value
                    return Json(new { success = true, groupName }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // If the group ID is not 1, retrieve the group name from the database based on the group ID
                    var group = dc.GROUPs.FirstOrDefault(g => g.Group_Id == groupId);

                    if (group != null)
                    {
                        // If the group is found, retrieve its name
                        string groupName = group.Group_Name;
                        return Json(new { success = true, groupName }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        // If the group is not found, handle the scenario accordingly
                        return Json(new { success = false, message = "Group not found." }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the process
                return Json(new { success = false, message = "An error occurred: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Home/Create
        [HttpPost]
        public ActionResult Create(string Group_Name)
        {
            try
            {
                // Create a new group
                var group = new GROUP { Group_Name = Group_Name };
                dc.GROUPs.Add(group);
                dc.SaveChanges();

                // Get the current user's resident ID
                int residentId = (int)Session["Resident_Id"];

                // Update the current user's group ID and set Is_Admin to true
                var resident = dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId);
                if (resident != null)
                {
                    resident.Group_Id = group.Group_Id;
                    resident.Is_Admin = true;
                    dc.SaveChanges();
                }

                return Json(new { success = true, message = "Group created successfully: " });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        [Route("Delete/{residentId}")]
        public ActionResult Delete(int residentId)
        {
            try
            {
                if (Session["Resident_Id"] != null)
                {
                    var loggedInResidentId = (int)Session["Resident_Id"];

                    var resident = dc.RESIDENTS.Find(loggedInResidentId);

                    if (resident != null)
                    {
                        if (resident.Is_Admin && resident.Resident_Id == residentId) // Check if the logged-in resident is the admin
                        {

                            var group = dc.GROUPs.FirstOrDefault(g => g.Group_Id == resident.Group_Id);
                            if (group != null)
                            {
                                var membersToUpdate = dc.RESIDENTS.Where(r => r.Group_Id == group.Group_Id);
                                foreach (var member in membersToUpdate)
                                {
                                    member.Group_Id = 1; // Set group ID to 1
                                    member.Is_Admin = false; // Set isAdmin to false
                                }
                                // Remove the group from the database
                                dc.GROUPs.Remove(group);
                                dc.SaveChanges();

                                // Update group ID of all members to 1 and set isAdmin to false

                                dc.SaveChanges();

                                return Json(new { success = true });
                            }

                            else
                            {
                                return Json(new { success = false, message = "Group not found." });
                            }
                        }
                        else
                        {
                            var group = dc.GROUPs.FirstOrDefault(g => g.Group_Id == resident.Group_Id);
                            var memberToUpdate = dc.RESIDENTS.FirstOrDefault(r => r.Group_Id == group.Group_Id && r.Resident_Id == residentId);
                            memberToUpdate.Group_Id = 1; // Set group ID to 1
                            dc.SaveChanges();

                            return Json(new { success = true });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Resident not found." });
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }


        // POST: Home/Search
        [HttpPost]
        public ActionResult Search(string Email)
        {
            try
            {
                int residentId = (int)Session["Resident_Id"];

                bool isAdmin = (bool)(dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId)?.Is_Admin);
                // Query residents based on email
                var residents = dc.RESIDENTS
                                   .Where(r => r.Email.Contains(Email))
                                   .Select(r => new
                                   {
                                       r.Resident_Id,
                                       r.Full_Name,
                                       r.Contact_No,
                                       r.Email
                                   })
                                   .ToList();

                return Json(new { success = true, residents, isAdmin = isAdmin });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpPost]
        public ActionResult RemoveMember(int residentId)
        {
            // Retrieve the resident from the database
            var resident = dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId);

            if (resident != null)
            {
                // Remove the resident from the database
                dc.RESIDENTS.Remove(resident);
                dc.SaveChanges(); // Save changes to the database

                // Return a success message
                return Json(new { success = true });
            }
            else
            {
                // Return an error message if the resident is not found
                return Json(new { success = false, message = "Resident not found." });
            }
        }


        // POST: Home/AddMember
        [HttpPost]
        public ActionResult AddMember(int residentId)
        {
            try
            {
                var resident = dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId);

                if (resident != null)
                {
                    int GroupresidentId = (int)Session["Resident_Id"];
                    int groupId = (int)(dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == GroupresidentId)?.Group_Id);

                    // Check if the member is already part of another group
                    if (resident.Group_Id != 1 && resident.Group_Id != null)
                    {
                        // Member is already in another group, return a message
                        return Json(new { success = false, message = "Member is already part of another group." });
                    }
                    else
                    {
                        resident.Group_Id = groupId;

                        dc.SaveChanges();

                        // Get all group members after adding the new member
                        var groupMembers = dc.RESIDENTS.Where(r => r.Group_Id == groupId).Select(r => new
                        {
                            r.Resident_Id,
                            r.Full_Name,
                            r.Email,
                            r.Contact_No
                        }).ToList();
                        // Serialize the response object to a JSON string
                        string jsonResponse = JsonConvert.SerializeObject(new { success = true, members = groupMembers, message = "Successfully add member to the group" });

                        // Render JavaScript to set response variable and refresh the page
                        string script = "<script>var response = " + jsonResponse + ";" + "window.location.reload();</script>";

                        // Return the JavaScript code as Content result
                        return Content(script, "text/html");
                    }

                }
                else
                {
                    return Json(new { success = false, message = "Resident not found." });
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return Json(new { success = false, message = "An error occurred while adding the member: " + ex.Message });
            }
        }

        public string GetCurrentUserName()
        {
            // Get the session ID
            int residentId = (int)Session["Resident_Id"];

            // Check if residentId is null or invalid
            if (residentId == null)
            {
                // Handle the case where there is no resident ID in the session
                return "Guest";
            }

            // Query your database to find the resident with the given ID
            using (var db = new BookingScheduleManagementDBEntities2())
            {
                var resident = db.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId);
                if (resident != null)
                {

                    return resident.Full_Name;
                }
                else
                {
                    // Handle the case where the resident is not found in the database
                    return "Guest";
                }
            }


            }
        public string GetCurrentUserAdmin()
        {
            // Get the session ID
            int residentId = (int)Session["Resident_Id"];

            // Check if residentId is null or invalid
            if (residentId == null)
            {
                // Handle the case where there is no resident ID in the session
                return "Guest";
            }

            // Query your database to find the resident with the given ID
            using (var db = new BookingScheduleManagementDBEntities2())
            {
                var resident = db.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId);
                if (resident != null)
                {
                   if(resident.Is_Admin == true) {
                        return "(Admin)" ;
                    }
                    else
                    {
                        return "(Member)";
                    }
                    
                }
                else
                {
                    // Handle the case where the resident is not found in the database
                    return "Guest";
                }
            }


        }
    }
}