using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Jovera.Data;
using Jovera.Models;
using NToastNotify;
using QRCoder;
using System.Drawing;
using System.Linq.Dynamic.Core;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Identity;
using Jovera.ViewModels;

namespace Jovera.Areas.Store.Pages.ManageItem
{
    public class AddItemModel : PageModel
    {
        private CRMDBContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IToastNotification _toastNotification;
        private readonly UserManager<ApplicationUser> _userManager;
        public string QRScan { get; set; }
        public string url { get; set; }
        public string QRCodeText { get; set; }
        public IRequestCultureFeature locale;
        public string BrowserCulture;

        [BindProperty]
        public Item AddItem { get; set; }
        [BindProperty]
        public AddSubProductVm AddSubProductVm  { get; set; }

        public AddItemModel(CRMDBContext context, IWebHostEnvironment hostEnvironment,
                                            IToastNotification toastNotification,UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _toastNotification = toastNotification;
            _userManager = userManager;
            AddItem = new Item();
           
        }
        public async Task<IActionResult> OnGet()
        {
            locale = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            BrowserCulture = locale.RequestCulture.UICulture.ToString();
            url = $"{this.Request.Scheme}://{this.Request.Host}";
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Login");

            }
            var vendor = _context.Stores.Where(e => e.Email == user.Email).FirstOrDefault();
            if (vendor == null)
            {
                return Redirect("/Login");
            }
            AddItem.StoreId = vendor.StoreId;
            return Page();
        }
        public async Task<IActionResult> OnPost(IFormFile file, IFormFileCollection MorePhoto)
        {

            try
            {

                if (file != null)
                {
                    string folder = "Images/Item/";
                    AddItem.ItemImage = UploadImage(folder, file);
                }
                List<ItemImage> itemImagesList = new List<ItemImage>();


                if (MorePhoto.Count != 0)
                {
                    foreach (var item in MorePhoto)
                    {
                        var itemImageObj = new ItemImage();
                        string folder = "Images/Item/";
                        itemImageObj.ImageName = UploadImage(folder, item);
                        itemImagesList.Add(itemImageObj);


                    }
                    AddItem.ItemImages = itemImagesList;
                }
                AddItem.PublishedDate = DateTime.Now;
                AddItem.ItemStatusId = 1;
                

                _context.Items.Add(AddItem);
                _context.SaveChanges();
                if (!AddItem.HasSubProduct)
                {
                    GenerateBarCode(AddItem.ItemId);
                }



            }
            catch (Exception)
            {

                _toastNotification.AddErrorToastMessage("Something went wrong");
            }
            return Redirect("/Store/ManageItem/Index");
        }
        public void GenerateBarCode(int ItemId)
        {
            var Event = _context.Items.Where(e => e.ItemId == ItemId).FirstOrDefault();
            if (Event != null)
            {
                Event.BarCode = ItemId.ToString();

            }
            QRCodeText = $"{this.Request.Scheme}://{this.Request.Host}/?Id=" + ItemId + "&HasSubProd=" + false;
            string QRCodeName = QRCodeText;
            QRCodeGenerator QrGenerator = new QRCodeGenerator();
            QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(QRCodeText, QRCodeGenerator.ECCLevel.Q);
            QRCode QrCode = new QRCode(QrCodeInfo);
            Bitmap QrBitmap = QrCode.GetGraphic(60);
            byte[] BitmapArray = BitmapToByteArray(QrBitmap);
            string uploadFolder = Path.Combine(_hostEnvironment.WebRootPath, "Images/Item");
            string uniqePictureName = Guid.NewGuid() + ".jpeg";
            string uploadedImagePath = Path.Combine(uploadFolder, uniqePictureName);
            using (var imageFile = new FileStream(uploadedImagePath, FileMode.Create))
            {
                imageFile.Write(BitmapArray, 0, BitmapArray.Length);
                imageFile.Flush();
            }
            
            Event.BarCode = "Images/Item/" + uniqePictureName;
            Event.ItemStatusId = 2;
            Event.Quantity = 0;
            var UpdatedEvent = _context.Items.Attach(Event);
            UpdatedEvent.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            _toastNotification.AddSuccessToastMessage("Item Added Successfully");



        }
        private byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
        private string UploadImage(string folderPath, IFormFile file)
        {

            folderPath += Guid.NewGuid().ToString() + "_" + file.FileName;

            string serverFolder = Path.Combine(_hostEnvironment.WebRootPath, folderPath);

            file.CopyToAsync(new FileStream(serverFolder, FileMode.Create));

            return folderPath;
        }

    }
}
