using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Real_Estate_Services.Models;
using Real_Estate_Services.db;

namespace Real_Estate_Services.Controllers
{
    public class UsersController : Controller
    {
        private readonly DB context;

        public UsersController(DB mydb)
        {
            this.context = mydb;
        }
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult SignUp(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var data = new Users()
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = EncryptPassword(model.Password),
                    IsActive = model.IsActive,
                };
                context.Users.Add(data);
                context.SaveChanges();
                TempData["successMessage"] = "You are eligible to login, please type your login credential";
                return RedirectToAction("Login");
            }
            else
            {
                TempData["error"] = "Empty form can't be submitted";
                return View(model);
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]

        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var data = context.Users.SingleOrDefault(e => e.Username == model.Username);

                if (data != null)
                {

                    DateTime now = DateTime.Now;

                    // Check if user is currently locked
                    if (data.LastLogoutEndTime.HasValue && data.LastLogoutEndTime > now)
                    {
                        TempData["AccountError"] = $"Account is locked. Try again at {data.LastLogoutEndTime.Value.ToString("hh:mm:ss tt")}";
                        return View(model);
                    }

                    // Check if it's a new day and reset LoginAttempt
                    if (data.LastLoginAttemptTime.HasValue && data.LastLoginAttemptTime.Value.Date != now.Date)
                    {
                        data.LoginAttempt = 0;
                        context.SaveChanges();
                    }

                    bool isValid = data.Username == model.Username &&
                                   DecryptPassword(data.Password) == model.Password &&
                                   data.IsActive == true;

                    if (isValid)
                    {
                        // Reset login attempt on success
                        data.LoginAttempt = 0;
                        data.LastLoginAttemptTime = null;
                        data.LastLogoutEndTime = null;
                        context.SaveChanges();

                        var identity = new ClaimsIdentity(new[] {
                            new Claim(ClaimTypes.Name, model.Username)
                        }, CookieAuthenticationDefaults.AuthenticationScheme);

                        var principal = new ClaimsPrincipal(identity);

                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        HttpContext.Session.SetString("Username", model.Username);

                        return RedirectToAction("Index", "admin");
                    }
                    else if (data.IsActive == false)
                    {
                        TempData["AccountError"] = "Account is Not Active";
                    }

                    else
                    {
                        // Increase failed attempt count
                        data.LoginAttempt += 1;
                        data.LastLoginAttemptTime = now;

                        // Lock account if 2 failed attempts today
                        if (data.LoginAttempt >= 4)
                        {
                            data.LastLogoutEndTime = now.AddMinutes(5);
                            TempData["Errorpassword"] = "Too many failed attempts. Account locked for 5 minutes.";
                        }
                        else
                        {
                            TempData["Errorpassword"] = "Invalid password.";
                        }

                        context.SaveChanges();
                        return View(model);
                    }
                }
                else
                {
                    TempData["ErrorUsername"] = "Username not found.";
                    return View(model);
                }
            }

            return View(model);
        }

        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var stroedCookies = Request.Cookies.Keys;

            foreach (var cookies in stroedCookies)
            {
                Response.Cookies.Delete(cookies);
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult EditUser(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }

            var obj = context.Users.Find(Id);

            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult EditUser(Users model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Username = model.Username;
                    model.Email = model.Email;
                    model.Password = EncryptPassword(model.Password);
                    model.IsActive = model.IsActive;
                }
                ;
                context.Users.Update(model);
                context.SaveChanges();
                TempData["successMessage"] = "Your are eligible to login, please type your login credential";
                return RedirectToAction("GetUsers", "Admin");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("Post", "Get")]
        public IActionResult UsernameIsExist(string Username)
        {
            var data = context.Users.Where(x => x.Username == Username)
                .SingleOrDefault();

            if (data != null)
            {
                return Json($"Username {Username} is already taken.");
            }
            else
            {
                return Json(true);
            }

            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Handle form submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1) Look up the user by identifier (username or email)
            var user = context.Users
                .FirstOrDefault(u => u.Username == model.Identifier
                                  || u.Email == model.Identifier);
            if (user == null)
            {
                ModelState.AddModelError(nameof(model.Identifier), "User not found.");
                return View(model);
            }


            // 2) Verify old password
            var decrypted = DecryptPassword(user.Password);
            if (decrypted != model.OldPassword)
            {
                ModelState.AddModelError(nameof(model.OldPassword), "Current password is incorrect.");
                return View(model);
            }

            // 3) Everything’s good — update to the new password
            user.Password = EncryptPassword(model.NewPassword);
            context.SaveChanges();

            TempData["SuccessMessage"] = "Your password has been updated. Please log in.";
            return RedirectToAction("Login");
        }


        //Encrypt Passrword
        public static string EncryptPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return null;
            }
            else
            {
                byte[] storePasseword = ASCIIEncoding.ASCII.GetBytes(password);
                string encryptedPassword = Convert.ToBase64String(storePasseword);
                return encryptedPassword;
            }
        }

        //Decrypt Password
        public static string DecryptPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return null;
            }
            else
            {
                byte[] storePasseword = Convert.FromBase64String(password);
                string decryptedPassword = ASCIIEncoding.ASCII.GetString(storePasseword);
                return decryptedPassword;
            }
        }
    }
}
