using Jovera.Data;
using Jovera.Models;
using Jovera.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NToastNotify;

namespace Jovera.Areas.Store.Pages
{
    public class IndexModel : PageModel
    {
        private CRMDBContext _context;
        private readonly IToastNotification _toastNotification;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public int storeStatus { get; set; }
        public IndexModel(CRMDBContext context, IToastNotification toastNotification, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _toastNotification = toastNotification;
            _signInManager = signInManager;
            _userManager = userManager;


        }
        public async Task<IActionResult> OnGet()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Login");

            }
            var store = _context.Stores.Where(e => e.Email == user.Email).FirstOrDefault();
            if (store == null)
            {
                return Redirect("/Login");
            }
            storeStatus = store.StoreProfileStatusId;




            return Page();
        }
    }
}