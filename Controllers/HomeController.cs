using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BankAccounts.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private YourContext dbContext;
        
        // here we can "inject" our context service into the constructor
        public HomeController(YourContext context)
        {
            dbContext = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }

        [Route("create")]
        [HttpPost]
        public IActionResult Create(User yourSurvey)
        {
            // Handle your form submission here
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == yourSurvey.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided
                    // error message
                    ModelState.AddModelError("Email", "Email already in use!");
                    
                    // You may consider returning to the View at this point
                    return View("Index");
                }
                // do somethng!  maybe insert into db?  then we will redirect
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                yourSurvey.Password = Hasher.HashPassword(yourSurvey, yourSurvey.Password);
                dbContext.Add(yourSurvey); //Adds new user into database
                dbContext.SaveChanges();
                // Console.WriteLine("Success");
                // HttpContext.Session.SetString("Active", "true");
                HttpContext.Session.SetInt32("id", yourSurvey.UserId); //Set current user to current session
                return Redirect("account/" + yourSurvey.UserId);
            }
            else
            {
                // Oh no!  We need to return a ViewResponse to preserve the ModelState, and the errors it now contains!
                // Console.WriteLine("FAIL");
                return View("Index");
            }
        }
        [Route("trylogin")]
        [HttpPost]
        public IActionResult TryLogin(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                // If no user exists with provided email
                if(userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                
                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();
                
                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
                
                // result can be compared to 0 for failure
                if(result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                // HttpContext.Session.SetString("Active", "true");
                HttpContext.Session.SetInt32("id", userInDb.UserId); //Successfully logged in, sets user to current session
                return Redirect("account/" + userInDb.UserId);
            }
            return View("Login");
        }

        [HttpGet]
        [Route("account/{userId}")]
        public IActionResult Success(int userId)
        {
            // User UserwithTransactions = dbContext.Users
            //     .Include(user => user.Transactions)
            //     .FirstOrDefault(user => user.UserId == (int)HttpContext.Session.GetInt32("id"));

            if (HttpContext.Session.GetInt32("id") == null)
            {
                return RedirectToAction("Login");
            }
            var UserId = HttpContext.Session.GetInt32("id");
            var userInDb = dbContext.Users
                .Include(user => user.Transactions)
                .FirstOrDefault(t => t.UserId == UserId);
            ViewBag.User = userInDb;

            // string LocalVariable = HttpContext.Session.GetString("Active");
            // if (LocalVariable != "true") {
            //     return RedirectToAction("Index");
            // }
            return View();

            
        }

        [HttpPost]
        [Route("PerformTransaction")]
        public IActionResult PerformTransaction(TransactionModel trans)
        {
            if (ModelState.IsValid) {
                var userInDb = dbContext.Users.Include(user => user.Transactions).FirstOrDefault(t => t.UserId == (int)HttpContext.Session.GetInt32("id"));
                float temp = userInDb.Balance + trans.Amount;
                if (temp < 0) {
                    ModelState.AddModelError("Amount", "Cannot withdraw more than your balance you FOOL!");
                    @ViewBag.User = userInDb;
                    return View("Success");
                } else {
                    var newTrans = new Transaction()
                    {
                        Amount = trans.Amount,
                        UserId = (int)HttpContext.Session.GetInt32("id")
                    };
                    dbContext.Transactions.Add(newTrans);
                    userInDb.Balance += trans.Amount;
                    dbContext.SaveChanges();
                    return Redirect("account/" + (int)HttpContext.Session.GetInt32("id"));
                }

            } else {
                return Redirect("account/" + (int)HttpContext.Session.GetInt32("id"));
            }
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            TempData["logout"] = "You have been logged out";
            return RedirectToAction("Login");
        }
    }
}
