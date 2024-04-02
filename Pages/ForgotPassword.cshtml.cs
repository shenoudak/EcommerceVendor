using Jovera.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using MimeKit;
using NToastNotify;

namespace Jovera.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IToastNotification toastNotification, IEmailSender emailSender, IWebHostEnvironment hostEnvironment)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _hostEnvironment = hostEnvironment;
            _toastNotification = toastNotification;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Input.Email != null)
                    {
                        var user = await _userManager.FindByEmailAsync(Input.Email);
                        if (user == null)
                        {
                            _toastNotification.AddErrorToastMessage("Email Not Valid");
                            return Page();

                        }

                        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                        var webRoot = _hostEnvironment.WebRootPath;

                        var pathToFile = _hostEnvironment.WebRootPath
                               + Path.DirectorySeparatorChar.ToString()
                               + "Templates"
                               + Path.DirectorySeparatorChar.ToString()
                               + "EmailTemplate"
                               + Path.DirectorySeparatorChar.ToString()
                               + "ResetPassword.html";
                        var builder = new BodyBuilder();
                        using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
                        {

                            builder.HtmlBody = SourceReader.ReadToEnd();

                        }
                        string messageBody = string.Format(builder.HtmlBody,
                         user.UserName,
                         code,
                           string.Format("{0:dddd, d MMMM yyyy}", DateTime.Now)
                           );
                        await _emailSender.SendEmailAsync(
                            user.Email,
                            "Reset Password",
                           messageBody);
                        _toastNotification.AddSuccessToastMessage("Please check your email to reset your password.");


                    }
                    else
                    {
                        _toastNotification.AddErrorToastMessage("Enter Your Email");

                    }
                }
                catch (Exception ex)
                {
                    _toastNotification.AddErrorToastMessage(ex.Message);

                }
            }

            return Page();
        }
    }
}

