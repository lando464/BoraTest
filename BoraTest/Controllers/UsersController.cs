using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BoraTest.Models;
using System.Net.Mail;

namespace BoraTest.Controllers
{
    public class UsersController : Controller
    {
        private BoraTestEntities db;

        public UsersController()
        {
            db = new BoraTestEntities();
        }
        public ActionResult Index()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Index(User model)
        {
            var usr = db.Users.Where(p => (p.Username.ToUpper() == model.Username.ToUpper() || p.Email.ToUpper() == model.Username.ToUpper()) && p.Password == model.Password);
            if (usr.Any())
            {
                if (usr.FirstOrDefault().Active == false)
                {
                    ViewBag.Error = "please Verify your email First";
                }
                else
                    ViewBag.Success = "Success login";
            }
            else
            {
                ViewBag.Error = "Username|Email or password is incorrect";
            }

            return View();
        }


        public ActionResult Lostyourpassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Lostyourpassword(string Email)
        {
            if (!IsValidEmail(Email))
            {
                ViewBag.Error = "Please Write Valid Email";
                return View();
            }
            if (!db.Users.Where(p => p.Email.ToUpper() == Email.ToUpper()).Any())
            {
                ViewBag.Error = "This Email does not belong to the site";
                return View();
            }

            SendPassToEmail(Email);
            ViewBag.Success = "Data sent successfully to mail";
            return View();
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "Id,Email,Username,Password,Phone")] User user)
        {
            if (ModelState.IsValid)
            {
                if (
                    string.IsNullOrEmpty(user.Username)
                    || string.IsNullOrEmpty(user.Email)
                    || string.IsNullOrEmpty(user.Password)
                    || string.IsNullOrEmpty(user.Phone)
                    )
                {
                    ViewBag.Error = "Please fill the required fields";
                    return View();
                }
                if (!IsValidEmail(user.Email))
                {
                    ViewBag.Error = "Please Write Valid Email";
                    return View();
                }
                //check if username or email added before
                if (db.Users.Where(p => p.Username.ToUpper() == user.Username.ToUpper() || p.Email.ToUpper() == user.Email.ToUpper()).Any())
                {
                    ViewBag.Error = "Your username or email is added before please change it";
                    return View();
                }
                user.Active = false;
                db.Users.Add(user);
                db.SaveChanges();
                SendVerficationEmail(user.Email, user.Id);
                ViewBag.Success = "Successful register please Verify your email";
                return View();
            }
            return View();
        }

        [NonAction]
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        [NonAction]
        private void SendVerficationEmail(string emailTo, int UserID)
        {

            string smtpAddress = "smtp.gmail.com";
            int portNumber = 587;
            bool enableSSL = true;
            string emailFrom = "vipsoft454@gmail.com";
            string password = "Ahmed@12345678900";
            string subject = "Verify your User email address (BoraTest)";

            string x = "Verify your account email address Link    \n   " +
                Request.Url.AbsoluteUri.Replace("Register", "") + "/VerficationEmail/" + UserID.ToString();
            string body = x.Replace("\n", System.Environment.NewLine);
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(emailFrom, "BoraTest");
                mail.To.Add(emailTo);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                    smtp.Credentials = new NetworkCredential(emailFrom, password);
                    smtp.EnableSsl = enableSSL;
                    smtp.Send(mail);
                }
            }
        }
        [NonAction]
        private bool CHKIsUserEmailActiveBefore(int UserID)
        {
            var user = db.Users.Where(p => p.Id == UserID).FirstOrDefault();
            if (user != null)
            {
                return user.Active;
            }
            return true;
        }
        [NonAction]
        private void SendPassToEmail(string emailTo)
        {
            var usr = db.Users.Where(p => p.Email.ToUpper() == emailTo.ToUpper()).FirstOrDefault();
            string usrname = usr.Username;
            string pass = usr.Password;
            string smtpAddress = "smtp.gmail.com";
            int portNumber = 587;
            bool enableSSL = true;
            string emailFrom = "vipsoft454@gmail.com";
            string password = "Ahmed@12345678900";
            string subject = "You receive your username and password from BoraTest";
            string x = "Username Is:" + usrname +
                " & \n Your Password Is: " +
                pass;
            string body = x.Replace("\n", System.Environment.NewLine);
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(emailFrom, "BoraTest");
                mail.To.Add(emailTo);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                    smtp.Credentials = new NetworkCredential(emailFrom, password);
                    smtp.EnableSsl = enableSSL;
                    smtp.Send(mail);
                }
            }
        }

        public ActionResult VerficationEmail(int Id)
        {
            if (CHKIsUserEmailActiveBefore(Id))
            {
                ViewBag.Error = "Your email address has been verified before";
                return View();
            }
            var user = db.Users.Where(p => p.Id == Id).FirstOrDefault();
            user.Active = true;
            db.SaveChanges();
            ViewBag.Success = "Congratulations, your email address has been verified";
            return View();
        }





        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }




    }
}
