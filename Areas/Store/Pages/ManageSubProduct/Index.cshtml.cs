using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Jovera.Data;
using Jovera.Models;
using Jovera.DataTables;
using NToastNotify;
using QRCoder;
using System.Drawing;
using System.Linq.Dynamic.Core;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Imaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace Jovera.Areas.Store.Pages.ManageSubProduct
{
    public class IndexModel : PageModel
    {
		private CRMDBContext _context;
		private readonly IWebHostEnvironment _hostEnvironment;
		private readonly IToastNotification _toastNotification;
		private readonly UserManager<ApplicationUser> _userManager;

		public string QRScan { get; set; }
		public string url { get; set; }
		public string QRCodeText { get; set; }

		[BindProperty]
		public SubProduct addSubProduct { get; set; }


		
		public static int _itemId = 0;
       // [BindProperty]
        public SubProduct addSubProductObj { get; set; }
		
		[BindProperty]
		public DataTablesRequest DataTablesRequest { get; set; }
		public IndexModel(CRMDBContext context, IWebHostEnvironment hostEnvironment,
											IToastNotification toastNotification, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_hostEnvironment = hostEnvironment;
			_toastNotification = toastNotification;
			_userManager = userManager;
			addSubProduct = new SubProduct();
            addSubProductObj = new SubProduct();
		}
		public void OnGet(int itemId)
		{
			var item = _context.Items.Where(e=>e.ItemId==itemId).FirstOrDefault();
			if (item == null)
			{
				Redirect("/PageNotFound");
			}
            _itemId = itemId;
			url = $"{this.Request.Scheme}://{this.Request.Host}";
		}

		

		public async Task<JsonResult> OnPostAsync()
		{
			

			var recordsTotal = _context.SubProducts.Where(e => e.ItemId == _itemId).Count();

			var customersQuery = _context.SubProducts.Include(e=>e.Color).Include(e=>e.Size).Include(e=>e.Item).Where(e => e.ItemId == _itemId).Select(i => new
			{
				ItemId = i.ItemId,
				SubProductId = i.SubProductId,
				ColorId = i.ColorId,
				ColorTLAR = i.Color.ColorTLAR,
				ColorTLEN = i.Color.ColorTLEN,
				SizeId = i.SizeId,
				SizeTLAR = i.Size.SizeTLAR,
				SizeTLEN = i.Size.SizeTLEN,
				Quantity = i.Quantity,
				ItemTitleAr = i.Item.ItemTitleAr,
				ItemTitleEn = i.Item.ItemTitleEn,
			}).AsQueryable();


			var searchText = DataTablesRequest.Search.Value?.ToUpper();
			if (!string.IsNullOrWhiteSpace(searchText))
			{
				customersQuery = customersQuery.Where(s =>
					s.ColorTLAR.ToUpper().Contains(searchText) ||
					s.SizeTLAR.ToUpper().Contains(searchText)
				);
			}

			var recordsFiltered = customersQuery.Count();

			var sortColumnName = DataTablesRequest.Columns.ElementAt(DataTablesRequest.Order.ElementAt(0).Column).Name;
			var sortDirection = DataTablesRequest.Order.ElementAt(0).Dir.ToLower();

			// using System.Linq.Dynamic.Core
			customersQuery = customersQuery.OrderBy($"{sortColumnName} {sortDirection}");

			var skip = DataTablesRequest.Start;
			var take = DataTablesRequest.Length;
			var data = await customersQuery
				.Skip(skip)
				.Take(take)
				.ToListAsync();

			return new JsonResult(new
			{
				draw = DataTablesRequest.Draw,
				recordsTotal = recordsTotal,
				recordsFiltered = recordsFiltered,
				data = data
			});
		}

        public async Task<IActionResult> OnPostAddSubProduct()
        {

            try
            {
				var existedSubProduct = _context.SubProducts.Where(e => e.ItemId ==_itemId && e.ColorId == addSubProduct.ColorId && e.SizeId == addSubProduct.SizeId).FirstOrDefault();
				if (existedSubProduct != null)
				{
                    _toastNotification.AddErrorToastMessage("SubProduct in Already Exist Before,Can Update In Its Quantity");
                    return Redirect($"/Store/ManageSubProduct/Index?itemid={_itemId}");
                }
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
				addSubProduct.ItemId = _itemId;
				addSubProduct.StoreId = vendor.StoreId;
                _context.SubProducts.Add(addSubProduct);
                _context.SaveChanges();
                 GenerateBarCode(addSubProduct.SubProductId);



            }
            catch (Exception)
            {

                _toastNotification.AddErrorToastMessage("Something went wrong");
            }
            return Redirect($"/Store/ManageSubProduct/Index?itemid={_itemId}");
        }
        public void GenerateBarCode(int ItemId)
        {
            var Event = _context.SubProducts.Where(e => e.SubProductId == ItemId).FirstOrDefault();
            if (Event != null)
            {
                Event.ItemQRCode = ItemId.ToString();

            }
            QRCodeText = $"{this.Request.Scheme}://{this.Request.Host}/?Id=" + ItemId + "&HasSubProd=" + true;
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
            //nurseryMember.Banner = uniqePictureName;
            Event.ItemQRCode = "Images/Item/" + uniqePictureName;
            var UpdatedEvent = _context.SubProducts.Attach(Event);
            UpdatedEvent.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            _toastNotification.AddSuccessToastMessage("SubProduct Added Successfully");



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
