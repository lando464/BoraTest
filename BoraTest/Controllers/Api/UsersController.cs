using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using BoraTest.Models;
using System.Net.Mail;

namespace BoraTest.Controllers.Api
{
    public class UsersController : ApiController
    {
        private BoraTestEntities db;

        public UsersController()
        {
            db = new BoraTestEntities();
        }


        [HttpGet]
        public IHttpActionResult Login(string username, string pass)
        {
            var usr = db.Users.Where(p => (p.Username.ToUpper() == username.ToUpper() || p.Email.ToUpper() == username.ToUpper()) && p.Password == pass);

            if (usr.Any())
            {
                if (usr.FirstOrDefault().Active == false)
                {
                    return Json("please Verify your email First");
                }
                else
                    return Json("Success login");
            }

            return Json("Username|Email or password is incorrect");
        }
        [HttpPost]
        public IHttpActionResult Register(User user)
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
                    return Json("Please fill the required fields");
                }
                if (!IsValidEmail(user.Email))
                {
                    return Json("Please Write Valid Email");
                }
                //check if username or email added before
                if (db.Users.Where(p => p.Username.ToUpper() == user.Username.ToUpper() || p.Email.ToUpper() == user.Email.ToUpper()).Any())
                {
                    return Json("Your username or email is added before please change it");
                }
                user.Active = false;
                db.Users.Add(user);
                db.SaveChanges();
                SendVerficationEmail(user.Email, user.Id);
                return Json("Successful register please Verify your email");
            }
            return Json("Input data not true");
        }
        [HttpPost]
        public IHttpActionResult Lostyourpassword(string Email)
        {
            if (!IsValidEmail(Email))
            {
                return Json("Please Write Valid Email");
            
            }
            if (!db.Users.Where(p => p.Email.ToUpper() == Email.ToUpper()).Any())
            {
                return Json("This Email does not belong to the site");
              
            }

            SendPassToEmail(Email);
            return Json("Data sent successfully to mail");
            
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
                Request.RequestUri.AbsoluteUri.Replace("Register", "") + "/VerficationEmail/" + UserID.ToString();
            
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