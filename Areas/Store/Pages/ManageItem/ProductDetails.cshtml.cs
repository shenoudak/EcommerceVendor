using Jovera.Data;
using Jovera.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NToastNotify;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Hosting;

namespace Jovera.Areas.Store.Pages.ManageItem
{
    public class ProductDetailsModel : PageModel
    {
        private CRMDBContext _context;
        private readonly IToastNotification _toastNotification;
        public IRequestCultureFeature locale;
        public string BrowserCulture;
        public string url { get; set; }
        private readonly IWebHostEnvironment _hostEnvironment;
        [BindProperty]
        public Item ItemDetails { get; set; }
        public int AvailableQuantityInStore = 0;

        public ProductDetailsModel(CRMDBContext context, IToastNotification toastNotification, IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            _context = context;
            _toastNotification = toastNotification;
        }


        public IActionResult OnGet(int ItemId)
        {
            locale = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            BrowserCulture = locale.RequestCulture.UICulture.ToString();
            url = $"{this.Request.Scheme}://{this.Request.Host}";
            ItemDetails = _context.Items.Include(e => e.ItemImages).Include(e=>e.MiniSubCategory).Include(e => e.SubProducts).ThenInclude(e=>e.StepOne).ThenInclude(e=>e.StepTwos).FirstOrDefault(a => a.ItemId == ItemId);
            if (ItemDetails.HasSubProduct)
            {
                AvailableQuantityInStore = _context.SubProducts.Where(e => e.ItemId == ItemId).Sum(e => e.Quantity.Value);

			}
            else
            {
                AvailableQuantityInStore = ItemDetails.Quantity;

			}
            return Page();
        }
        public IActionResult OnGetSingleUpdateItemQuantityForEdit(int ItemId)
        {
            var item = _context.Items.Where(c => c.ItemId == ItemId).FirstOrDefault();

            return new JsonResult(item);
        }
        public async Task<IActionResult> OnPostUpdateItemQuantity(int ItemId)
        {

            try
            {
                var model = _context.Items.Where(c => c.ItemId == ItemId).FirstOrDefault();
                if (model == null)
                {
                    _toastNotification.AddErrorToastMessage("Item Not Found");
                    return Page();
                    
                }


               

                model.Quantity = ItemDetails.Quantity;
                


                var UpdatedItem = _context.Items.Attach(model);

                UpdatedItem.State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                _context.SaveChanges();

                _toastNotification.AddSuccessToastMessage("Item Edited successfully");

                return Redirect($"/Store/ManageItem/ProductDetails?ItemId={ItemId}");

            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Something went Error");

            }
            return Redirect($"/Store/ManageItem/ProductDetails?ItemId={ItemDetails.ItemId}");
        }

    }
}
