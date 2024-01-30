using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
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

                if (user.EmailConfirmed)
                {
                    var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, false);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    return View("ConfirmYourAccount");
                }

                ModelState.AddModelError(string.Empty, "Dados de Login inválidos");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
