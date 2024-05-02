using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PressTheButton.Context;
using PressTheButton.Models;
using PressTheButton.ViewModels;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace PressTheButton.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _config;
        private string _filePath;
        private readonly AppDbContext _context;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration config, IWebHostEnvironment env, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _filePath = env.WebRootPath;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistroViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    Email = model.Email,
                    UserName = model.Email,
                    EmailConfirmed = false,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await SendEmailConfirm(user);
                    return View("MailConfirmation");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SendEmailConfirm(IdentityUser user)
        {
            if (user == null)
            {
                return View("Error");
            }
            else
            {
                string fromAddress = _config["SmtpStrings:Smtp:FromMail"];
                string toAddress = user.Email;
                string smtpUsername = _config["SmtpStrings:Smtp:CrMail"];
                string smtpPassword = _config["SmtpStrings:Smtp:CrPass"];

                if (fromAddress != null && toAddress != null && smtpUsername != null && smtpPassword != null)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);

                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                    };

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = "PressTheButton - Confirmar Conta",
                        Body = $"Clique no link a seguir para confirmar seu e-mail: <a href='{confirmationLink}'>Confirmar E-mail</a>",
                        IsBodyHtml = true
                    })
                    {
                        try
                        {
                            smtpClient.Send(message);
                            return View();
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    return View("Error");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return View("Error");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return View("EmailConfirmed");
            }
            else
            {
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    return RedirectToAction("Register");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(
                    user, model.Password, false);

                if (user.EmailConfirmed && result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                else if (result.Succeeded)
                {
                    return View("ConfirmYourAccount");
                }

                ModelState.AddModelError(string.Empty, "Dados de Login inválidos");
            }
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Login");
            }

            var profilePicture = await _context.ProfilePictures.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profilePicture == null || profilePicture.Path == null)
            {
                ViewBag.ProfilePicturePath = "profile.jpg";

                return View(user);
            }

            var filePath = Path.Combine(_filePath, "images", "profilePicture", profilePicture.Path);

            if (System.IO.File.Exists(filePath))
            {
                ViewBag.ProfilePicturePath = profilePicture.Path;
            }
            else
            {
                ViewBag.ProfilePicturePath = "profile.jpg";
            }

            return View(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditAccount(string id, string newUserName)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return View("Login");
            }

            user.UserName = newUserName;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("EditAccount");
            }

            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> NewProfilePicture(IFormFile profilePicture)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return View("Login");
            }

            if (ModelState.IsValid)
            {
                if (!ImageIsValid(profilePicture))
                {
                    return View("Error");
                }

                var userWithPicture = await _context.ProfilePictures.FirstOrDefaultAsync(p => p.UserId == user.Id);

                if (userWithPicture == null)
                {
                    var name = await SaveFile(profilePicture);

                    ProfilePicture newProfilePicture = new ProfilePicture
                    {
                        UserId = user.Id,
                        Path = name
                    };

                    _context.Add(newProfilePicture);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("EditAccount");
                }
                else
                {
                    DeleteFile(userWithPicture.Path);

                    var name = await SaveFile(profilePicture);

                    userWithPicture.Path = name;

                    _context.Update(userWithPicture);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("EditAccount");
                }
            }
            return View("Index");
        }

        public bool ImageIsValid(IFormFile profilePicture)
        {
            if (profilePicture == null || profilePicture.Length == 0)
            {
                return false;
            }

            switch (profilePicture.ContentType)
            {
                case "image/jpeg":
                    return true;
                case "image/bmp":
                    return true;
                case "image/gif":
                    return true;
                case "image/png":
                    return true;
                default:
                    return false;
            }
        }

        public async Task<string> SaveFile(IFormFile profilePicture)
        {
            var name = Guid.NewGuid().ToString() + Path.GetExtension(profilePicture.FileName);
            var filePath = Path.Combine(_filePath, "images", "profilePicture");

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            var fullPath = Path.Combine(filePath, name);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(stream);
            }

            return name;
        }

        private void DeleteFile(string fileName)
        {
            var filePath = Path.Combine(_filePath, "images", "profilePicture", fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
