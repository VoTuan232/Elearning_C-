using BELibrary.Core.Entity;
using BELibrary.Entity;
using BELibrary.Utils;
using PagedList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ELearning.Controllers
{
    public class TeacherController : Controller
    {
        // GET: Teacher
        public ActionResult Index(int ?page)
        {
            using (var unitofwork = new UnitOfWork(new ELearningDBContext()))
            {
                var related = unitofwork.Courses.GetAll().OrderByDescending(x => x.ModifiedDate).Take(3).ToList();
                ViewBag.Related = related;

                var teachers = unitofwork.Account.Query(x => x.RoleId == RoleKey.Teacher && x.Status).ToList();
                var t = teachers.Count;

                int pageNumber = (page ?? 1);
                int pageSize = 8;
                var result = teachers.ToPagedList(pageNumber, pageSize);

                return View(result);
            }
              
        }
    }
}