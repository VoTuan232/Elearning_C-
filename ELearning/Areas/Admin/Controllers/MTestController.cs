using BELibrary.Core.Entity;
using BELibrary.Entity;
using System;
using System.Linq;
using System.Web.Mvc;
using BELibrary.Utils;
using System.Net.Mail;
using System.Threading;
using System.Net;
using System.IO;
using Microsoft.Extensions.FileProviders;
using System.Web;
using System.Net.Mime;

namespace ELearning.Areas.Admin.Controllers
{
    public class MTestController : BaseController
    {
        private readonly string keyElement = "Kiểm tra";

        public ActionResult Index(int lectureId)
        {
            ViewBag.Element = keyElement;
            using (var workScope = new UnitOfWork(new ELearningDBContext()))
            {
                var user = GetCurrentUser();
                if (user.RoleId == RoleKey.Admin)
                {
                    var listData = workScope.StudentTests.Include(x => x.User).Where(x => x.LectureId == lectureId).ToList();
                    return View(listData);
                }
                else
                {
                    var subjectAssign =
                        workScope.TeacherSubjects.Query(x => x.Username.ToLower() == user.Username.ToLower());

                    var subjects = workScope.Subjects.GetAll().Join(
                        subjectAssign,
                        sj => sj.Id,
                        sja => sja.SubjectId,
                        (sj, sja) => new { sj }).Select(x => x.sj).Where(x => x.Status).ToList();

                    var lectures = workScope.Lectures.GetAll().Join(
                        subjects,
                        l => l.SubjectId,
                        sj => sj.Id,
                        (l, sj) => new { l }).Select(x => x.l).ToList();

                    var studentTests = workScope.StudentTests.GetAll().Join(
                        lectures,
                        st => st.LectureId,
                        l => l.Id,
                        (st, l) => new { st }).Select(x => x.st).ToList();

                    return View(studentTests);
                }
            }
        }

        public ActionResult Update(int id)
        {
            ViewBag.isEdit = true;
            ViewBag.key = "Cập nhật";
            ViewBag.Element = keyElement;
            using (var workScope = new UnitOfWork(new ELearningDBContext()))
            {
                var user = GetCurrentUser();
                if (user.RoleId == RoleKey.Admin)
                {
                    var student = workScope.StudentTests.FirstOrDefault(x => x.Id == id);

                    return View(student);
                }
                else
                {
                    var subjectAssign =
                        workScope.TeacherSubjects.Query(x => x.Username.ToLower() == user.Username.ToLower());

                    var subjects = workScope.Subjects.GetAll().Join(
                        subjectAssign,
                        sj => sj.Id,
                        sja => sja.SubjectId,
                        (sj, sja) => new { sj }).Select(x => x.sj).Where(x => x.Status).ToList();

                    var lectures = workScope.Lectures.GetAll().Join(
                        subjects,
                        l => l.SubjectId,
                        sj => sj.Id,
                        (l, sj) => new { l }).Select(x => x.l).ToList();

                    var studentTests = workScope.StudentTests.GetAll().Join(
                        lectures,
                        st => st.LectureId,
                        l => l.Id,
                        (st, l) => new { st }).Select(x => x.st);

                    var studentTest = studentTests.FirstOrDefault(x => x.Id == id);

                    if (studentTest == null)
                    {
                        return RedirectToAction("E401", "Dashboard");
                    }
                    return View(studentTest);
                }
            }
        }

        [HttpPost, ValidateInput(false)]
        public JsonResult CreateOrEdit(StudentTest input, bool isEdit, bool isSendMail, string LectureName)
        {
            try
            {
                if (!isEdit) return Json(new { status = false, mess = "Có lỗi xảy ra: " });
                using (var workScope = new UnitOfWork(new ELearningDBContext()))
                {
                    var elm = workScope.StudentTests.Get(input.Id);
                    if (elm != null) //update
                    {
                        elm.Point = input.Point;
                        elm.ReplyFile = input.ReplyFile;
                        if (isSendMail)
                        {
                            var lecture = workScope.Lectures.FirstOrDefault(x => x.Id == input.LectureId);
                            var client = new SmtpClient
                            {
                                Host = "smtp.gmail.com",
                                Port = 587,
                                UseDefaultCredentials = false,
                                DeliveryMethod = SmtpDeliveryMethod.Network,
                                Credentials =
                            new NetworkCredential(
                                "votuanbk2302@gmail.com",
                                "@Vat883279"),
                                EnableSsl = true,
                            };
                            var from = new MailAddress("votuanbk2302@gmail.com", "Admin Elearning");
                            var to = new MailAddress(input.Email);
                            var mail = new MailMessage(from, to)
                            {
                                Subject = "Điểm môn học!",
                                Body = "Xin chào " + "<span style='color: red;'>" + input.Username + "</span>" + "!" + "<br/> Điểm bài học " + lecture.Name + " của bạn là: " + input.Point,
                                IsBodyHtml = true,
                            };
                                Attachment data = new Attachment(
                             Server.MapPath("~/" + input.ReplyFile),
                             MediaTypeNames.Application.Octet);
                            // your path may look like Server.MapPath("~/file.ABC")
                            mail.Attachments.Add(data);
                            client.Send(mail);
                        }
                        workScope.StudentTests.Put(elm, elm.Id);
                        workScope.Complete();
                        return Json(new { status = true, mess = "Cập nhập thành công" });
                    }
                    else
                    {
                        return Json(new { status = false, mess = "Không tồn tại " + keyElement });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, mess = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Del(int id)
        {
            try
            {
                using (var workScope = new UnitOfWork(new ELearningDBContext()))
                {
                    var elm = workScope.StudentTests.Get(id);
                    if (elm != null) //update
                    {
                        workScope.StudentTests.Remove(elm);
                        workScope.Complete();
                        return Json(new { status = true, mess = "Xóa thành công " + keyElement });
                    }
                    else
                    {
                        return Json(new { status = false, mess = "Không tồn tại " + keyElement });
                    }
                }
            }
            catch
            {
                return Json(new { status = false, mess = "Thất bại" });
            }
        }
    }
}