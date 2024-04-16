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
using Jovera.ViewModels;

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
        public List<StepTwo> StepTwosList = new List<StepTwo>();

        [BindProperty]
		public AddSubProductVm addSubProduct { get; set; }


		
		public static int staticStepOneSubProdId = 0;
		public static int staticStepOneId = 0;
		public static int staticItemIdForUpdateStatus = 0;
       // [BindProperty]
        public MiniSubProduct addSubProductObj { get; set; }
		
		[BindProperty]
		public DataTablesRequest DataTablesRequest { get; set; }
		public IndexModel(CRMDBContext context, IWebHostEnvironment hostEnvironment,
											IToastNotification toastNotification, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_hostEnvironment = hostEnvironment;
			_toastNotification = toastNotification;
			_userManager = userManager;
			addSubProduct = new AddSubProductVm();
            addSubProductObj = new MiniSubProduct();
		}
		public void OnGet(int stepOneSubProductId)
		{
			var SubProductStepObj = _context.SubProductStepOnes.Where(e=>e.SubProductStepOneId== stepOneSubProductId&&e.IsDeleted==false).FirstOrDefault();
			if (SubProductStepObj == null)
			{
				Redirect("/PageNotFound");
			}
            
            staticStepOneSubProdId = stepOneSubProductId;
			url = $"{this.Request.Scheme}://{this.Request.Host}";
            addSubProduct.SubProductStepOneId =staticStepOneSubProdId;
            addSubProduct.StepOneId = SubProductStepObj.StepOneId;
            staticStepOneId = SubProductStepObj.StepOneId;
            staticItemIdForUpdateStatus = SubProductStepObj.ItemId;
            //StepTwosList = _context.StepTwos.Where(e => e.StepOneId == SubProductStepObj.StepOneId&&e.IsDeleted==false).ToList();

        }

		

		public async Task<JsonResult> OnPostAsync()
		{
			

			var recordsTotal = _context.MiniSubProducts.Where(e => e.SubProductStepOneId == staticStepOneSubProdId&&e.IsDeleted==false).Count();

			var customersQuery = _context.MiniSubProducts.Include(e=>e.StepTwo).Include(e=>e.SubProductStepOne).ThenInclude(e=>e.StepOne).Where(e => e.SubProductStepOneId == staticStepOneSubProdId&&e.IsDeleted == false).Select(i => new
			{
                MiniSubProductId = i.MiniSubProductId,
                IsDeleted = i.IsDeleted,
                ItemQRCode = i.ItemQRCode,
                StepOneTLAR = i.SubProductStepOne.StepOne.StepOneTLAR,
                StepOneTLEN = i.SubProductStepOne.StepOne.StepOneTLEN,
                SubProductStepOneId = i.SubProductStepOneId,
                StepTwoTLAR = i.StepTwo.StepTwoTLAR,
                StepTwoTLEN = i.StepTwo.StepTwoTLEN,
				Quantity = i.Quantity,
			}).AsQueryable();


			var searchText = DataTablesRequest.Search.Value?.ToUpper();
			if (!string.IsNullOrWhiteSpace(searchText))
			{
				customersQuery = customersQuery.Where(s =>
					s.StepTwoTLEN.ToUpper().Contains(searchText) ||
					s.StepTwoTLAR.ToUpper().Contains(searchText)
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
        public IActionResult OnGetSingleMiniSubProductForView(int MiniSubProductId)
        {
            var Result = _context.MiniSubProducts.Include(e=>e.StepTwo).Include(e=>e.SubProductStepOne).ThenInclude(e=>e.StepOne).Where(c => c.MiniSubProductId == MiniSubProductId).Select(i => new
            {
                ItemQRCode= i.ItemQRCode,
                StepTwoTLAR = i.StepTwo.StepTwoTLAR,
                StepTwoTLEN = i.StepTwo.StepTwoTLEN,
                StepOneTLAR = i.SubProductStepOne.StepOne.StepOneTLAR,
                StepOneTLEN = i.SubProductStepOne.StepOne.StepOneTLEN,
                Quantity = i.Quantity,
            }).FirstOrDefault();
            return new JsonResult(Result);
        }
        public IActionResult OnGetSingleMiniSubProductForEdit(int MiniSubProductId)
        {
            var MiniSubProduct = _context.MiniSubProducts.Where(c => c.MiniSubProductId == MiniSubProductId).FirstOrDefault();
            addSubProduct.Quantity = MiniSubProduct.Quantity;
            addSubProduct.StepTwoId = MiniSubProduct.StepTwoId;
            addSubProduct.SubProductStepOneId = MiniSubProduct.SubProductStepOneId;
            addSubProduct.MiniSubProductId = MiniSubProduct.MiniSubProductId;
            addSubProduct.StepOneId = staticStepOneId;
            return new JsonResult(addSubProduct);
        }
        public async Task<IActionResult> OnPostEditSubProduct(int MiniSubProductId)
        {

            try
            {
                var model = _context.MiniSubProducts.Where(c => c.MiniSubProductId == MiniSubProductId).FirstOrDefault();
                if (model == null)
                {
                    _toastNotification.AddErrorToastMessage("Object Not Found");

                    return Redirect($"/Store/ManageSubProduct/Index?stepOneSubProductId={staticStepOneSubProdId}");

                }
                model.Quantity = addSubProduct.Quantity;
                model.StepTwoId = addSubProduct.StepTwoId;
                //model.MiniSubProductId = addSubProduct.MiniSubProductId;
                //model.SubProductStepOneId = addSubProduct.SubProductStepOneId;
                var UpdatedMiniSubProduct = _context.MiniSubProducts.Attach(model);

                UpdatedMiniSubProduct.State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                _context.SaveChanges();

                _toastNotification.AddSuccessToastMessage("Sub Product Edited Successfully");



            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Something went Error");

            }
            return Redirect($"/Store/ManageSubProduct/Index?stepOneSubProductId={staticStepOneSubProdId}");
        }
        public IActionResult OnGetSingleMiniSubProductForDelete(int MiniSubProductId)
        {
            var Result = _context.MiniSubProducts.Where(c => c.MiniSubProductId == MiniSubProductId).FirstOrDefault();
            return new JsonResult(Result);
        }
        public async Task<IActionResult> OnPostDeleteSubProduct(int MiniSubProductId)
        {
            try
            {
                var model = _context.MiniSubProducts.Where(c => c.MiniSubProductId == MiniSubProductId).FirstOrDefault();
                if (model == null)
                {
                    _toastNotification.AddErrorToastMessage("Object Not Found");

                    return Redirect($"/Store/ManageSubProduct/Index?stepOneSubProductId={staticStepOneSubProdId}");

                }
                model.IsDeleted = true;
                var UpdatedMini = _context.MiniSubProducts.Attach(model);

                UpdatedMini.State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                _context.SaveChanges();

                _toastNotification.AddSuccessToastMessage("Sub Product Deleted Successfully");



            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Something went Error");

            }

            return Redirect($"/Store/ManageSubProduct/Index?stepOneSubProductId={staticStepOneSubProdId}");


        }
        public async Task<IActionResult> OnPostAddSubProduct()
        {

            try
            {
				var existedSubProduct = _context.MiniSubProducts.Where(e => e.StepTwoId ==addSubProduct.StepTwoId && e.SubProductStepOneId ==staticStepOneSubProdId&&e.IsDeleted==false).FirstOrDefault();
				if (existedSubProduct != null)
				{
                    _toastNotification.AddErrorToastMessage("SubProduct in Already Exist Before,Can Update In Its Quantity");
                    return Redirect($"/Store/ManageSubProduct/Index?stepOneSubProductId={staticStepOneSubProdId}");
                }
                //var user = await _userManager.GetUserAsync(User);
                //if (user == null)
                //{
                //    return Redirect("/Login");

                //}
                //var vendor = _context.Stores.Where(e => e.Email == user.Email).FirstOrDefault();
                //if (vendor == null)
                //{
                //    return Redirect("/Login");
                //}
                var addMiniSub = new MiniSubProduct()
                {
                    Quantity= addSubProduct.Quantity,
                    StepTwoId=addSubProduct.StepTwoId,
                    SubProductStepOneId=staticStepOneSubProdId


                };
				
                _context.MiniSubProducts.Add(addMiniSub);
                _context.SaveChanges();
                 GenerateBarCode(addMiniSub.MiniSubProductId);
                


            }
            catch (Exception)
            {

                _toastNotification.AddErrorToastMessage("Something went wrong");
            }
            return Redirect($"/Store/ManageSubProduct/Index?stepOneSubProductId={staticStepOneSubProdId}");
        }
        public void GenerateBarCode(int ItemId)
        {
            var Event = _context.MiniSubProducts.Where(e => e.MiniSubProductId == ItemId).FirstOrDefault();
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
            var UpdatedEvent = _context.MiniSubProducts.Attach(Event);
            UpdatedEvent.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            var ItemModel = _context.Items.Where(e => e.ItemId == staticItemIdForUpdateStatus).FirstOrDefault();
            ItemModel.ItemStatusId = 2;
            var UpdatedItem = _context.Items.Attach(ItemModel);
            UpdatedItem.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
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
