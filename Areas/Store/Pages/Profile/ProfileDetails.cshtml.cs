using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NToastNotify;
using Jovera.Data;
using Jovera.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Jovera.ViewModels;
using Jovera.ViewModel;

namespace Jovera.Areas.Store.Pages.Profile
{
    public class ProfileDetailsModel : PageModel
    {
        private CRMDBContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IToastNotification _toastNotification;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        public List<string> storeCatagories { get; set; }
        public List<string> otherCatagories { get; set; }
        [BindProperty]
        public ChangePasswordVM changePasswordVM { get; set; }
        public Jovera.Models.Store storDetails { get; set; }
        public ProfileDetailsModel(CRMDBContext context, ApplicationDbContext db, IWebHostEnvironment hostEnvironment, IToastNotification toastNotification, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _toastNotification = toastNotification;
            _signInManager = signInManager;
            _userManager = userManager;
            storDetails = new Jovera.Models.Store();
            _db = db;


        }
        public async Task<IActionResult> OnGet()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Login");

            }
            storDetails = _context.Stores.Include(e=>e.StoreProfileImages).Where(e => e.Email == user.Email).FirstOrDefault();
            
            if (storDetails == null)
            {
                return Redirect("/Login");
            }
            if (storDetails.CatagoriesTypes != null)
            {
                storeCatagories = storDetails.CatagoriesTypes.Split(",").ToList();


            }
            if (storDetails.OtherCatagories != null)
            {
                otherCatagories = storDetails.OtherCatagories.Split(",").ToList();


            }

            return Page();
        }
        public async Task<IActionResult> OnPost()
        {
            if (changePasswordVM.CurrentPassword == changePasswordVM.NewPassword)
            {
                ModelState.AddModelError("", "New password Must be Diffrent from Current Password..");
                return Page();
            }
            if (!ModelState.IsValid)
                return Page();
            var user = await _userManager.GetUserAsync(User);
            var result = await _userManager.ChangePasswordAsync(user, changePasswordVM.CurrentPassword, changePasswordVM.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }
            await _signInManager.RefreshSignInAsync(user);
            _toastNotification.AddSuccessToastMessage("Password Updated Successfully");
            return RedirectToPage("/Store/profile/ProfileDetails");

        }
    }
}