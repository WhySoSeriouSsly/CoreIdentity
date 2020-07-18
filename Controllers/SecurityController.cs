using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Core_Identity.Models;
using Core_Identity.Identity;
using Microsoft.AspNetCore.Identity;

namespace Core_Identity.Controllers
{
    public class SecurityController : Controller
    {
        private UserManager<CustomIdentityUser> _userManager;
        private SignInManager<CustomIdentityUser> _signInManager;
        public SecurityController(UserManager<CustomIdentityUser> userManager,SignInManager<CustomIdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager=signInManager;
        }

          }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }
            var user = await _userManager.FindByNameAsync(loginViewModel.UserName);
            if (user != null)
            {
                if(!await _userManager.IsEmailConfirmedAsync(user))//kullanıcı emaili doğrulamışşmı
                {
                    ModelState.AddModelError(String.Empty, "Confirm Your Email Please");
                    return View(loginViewModel);
                }
            }
            var result = await _signInManager.PasswordSignInAsync(
                loginViewModel.UserName, loginViewModel.Password,
                false,false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Student");

            }
            ModelState.AddModelError(String.Empty, "Login failed");
            return View(loginViewModel);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Student");
        }
        public IActionResult AccessDenided()//GİRİŞ YETKİSİ OLMAYAN YER
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerViewModel);
            }
            var user = new AppIdentityUser
            {
                UserName = registerViewModel.UserName,
                Email = registerViewModel.Email,
                Age = registerViewModel.Age
            };
            var result = await _userManager.CreateAsync(user, registerViewModel.Password);
            if (result.Succeeded)
            {
                var confirmationCode = _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callBackUrl = Url.Action("ConfirmEmail", "Security", new {
                    userId = user.Id, code = confirmationCode.Result });

                //send Email
                return RedirectToAction("Index", "Student");
            }
            return View(registerViewModel);//o ana kadar girdiği bilgiler kaybolmasın diye.

        }
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if(userId==null||code==null)
            {
                return RedirectToAction("Index", "Student");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user==null)
            {
                throw new ApplicationException("Unable to find the user");

            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return View("ConfirmEmail");
            }
            return RedirectToAction("Index", "Student");

        }

        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if(string.IsNullOrEmpty(email))
            {
                return View();
            }
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return View();
            }
            var confirmationCode = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callBackUrl = Url.Action("ResetPassword", "Security", new { userId = user.Id, code = confirmationCode });

            //send callback url with email 
            return RedirectToAction("ForgotPasswordEmailSent");

        }
        public IActionResult ForgotPasswordEmailSent()
        {
            return View();
        }
        public IActionResult ResetPassword(string userId,string code)
        {
            if (userId == null || code == null)
            {
                throw new ApplicationException("UserId or Code Must be supplied for password reset");
            }

            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(resetPasswordViewModel);
            }
            var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }
            var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Code, resetPasswordViewModel.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirm");

            }
            return View();
        }
        public IActionResult ResetPasswordConfirm()
        {
            return View();
        }
    }
}
