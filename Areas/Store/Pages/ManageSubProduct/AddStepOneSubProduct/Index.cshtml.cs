using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Jovera.Data;
using Jovera.Models;
using Jovera.DataTables;
using NToastNotify;
using System.Linq.Dynamic.Core;
using System.Drawing.Imaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Jovera.Areas.Store.Pages.ManageSubProduct.AddStepOneSubProduct
{
    public class IndexModel : PageModel
    {
        private CRMDBContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IToastNotification _toastNotification;
        private readonly UserManager<ApplicationUser> _userManager;
        public string url { get; set; }
        public int notStatictemId { get; set; }
        public static int staticItemId { get; set; }

        [BindProperty]
        public Jovera.Models.SubProductStepOne addStepOne { get; set; }
        public Jovera.Models.SubProductStepOne addStepOneObj { get; set; }

        [BindProperty]
        public DataTablesRequest DataTablesRequest { get; set; }
        public IndexModel(CRMDBContext context, IWebHostEnvironment hostEnvironment,
                                            IToastNotification toastNotification, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _toastNotification = toastNotification;
            _userManager = userManager;
            addStepOne = new Jovera.Models.SubProductStepOne();
            addStepOneObj = new Jovera.Models.SubProductStepOne();
        }
        public IActionResult OnGet(int ItemId)
        {
            var ItemExist = _context.Items.Where(e => e.ItemId == ItemId && e.IsDeleted == false && e.HasSubProduct == true).FirstOrDefault();
            if (ItemExist == null)
            {
                Redirect("/Store/PageNotFound");
            }
            staticItemId = ItemId;
            notStatictemId = ItemId;
            url = $"{this.Request.Scheme}://{this.Request.Host}";
            return Page();
        }



      

        public async Task<IActionResult> OnPostAddSubProductStepOne(IFormFile file)
        {

            try
            {
                var supbProdStepOneExist = _context.SubProductStepOnes.Where(e => e.ItemId == staticItemId && e.StepOneId == addStepOne.StepOneId).FirstOrDefault();
                if (supbProdStepOneExist != null)
                {
                    _toastNotification.AddErrorToastMessage("Already Exists");
                    return Redirect($"/Store/ManageSubProduct/AddStepOneSubProduct/index?ItemId={staticItemId}");
                }
                if (file != null)
                {
                    string folder = "Images/Item/";
                    addStepOne.Icon = UploadImage(folder, file);
                }
                addStepOne.ItemId = staticItemId;
                _context.SubProductStepOnes.Add(addStepOne);
                _context.SaveChanges();
                _toastNotification.AddSuccessToastMessage("Added Successfully");
            }
            catch (Exception)
            {

                _toastNotification.AddErrorToastMessage("Something went wrong");
            }
            return Redirect($"/Store/ManageSubProduct/AddStepOneSubProduct/index?ItemId={staticItemId}");
        }
        //public IActionResult OnGetSingleStepOneForEdit(int StepOneId)
        //{
        //    addStepOne = _context.SubProductStepOnes.Where(c => c.SubProductStepOneId == StepOneId).FirstOrDefault();

        //    return new JsonResult(addStepOne);
        //}
        //public async Task<IActionResult> OnPostEditStepOne(int StepOneId)
        //{

        //    try
        //    {
        //        var model = _context.StepOnes.Where(c => c.StepOneId == StepOneId).FirstOrDefault();
        //        if (model == null)
        //        {
        //            _toastNotification.AddErrorToastMessage("Object Not Found");

        //            return Redirect("/crm/configurations/Managesubproduct/managestepone/index");
        //        }
        //        model.StepOneTLAR = addStepOne.StepOneTLAR;
        //        model.StepOneTLEN = addStepOne.StepOneTLEN;
        //        var UpdatedStepOne = _context.StepOnes.Attach(model);

        //        UpdatedStepOne.State = Microsoft.EntityFrameworkCore.EntityState.Modified;

        //        _context.SaveChanges();

        //        _toastNotification.AddSuccessToastMessage("Edited Successfully");



        //    }
        //    catch (Exception)
        //    {
        //        _toastNotification.AddErrorToastMessage("Something went Error");

        //    }
        //    return Redirect("/crm/configurations/Managesubproduct/managestepone/index");
        //}
        //public IActionResult OnGetSingleStepOneForView(int StepOneId)
        //{
        //    var Result = _context.StepOnes.Where(c => c.StepOneId == StepOneId).FirstOrDefault();
        //    return new JsonResult(Result);
        //}
        //public IActionResult OnGetSingleStepOneForDelete(int StepOneId)
        //{
        //    var Result = _context.StepOnes.Where(c => c.StepOneId == StepOneId).FirstOrDefault();
        //    return new JsonResult(Result);
        //}
        //public async Task<IActionResult> OnPostDeleteStepOne(int StepOneId)
        //{
        //    try
        //    {
        //        var model = _context.StepOnes.Where(c => c.StepOneId == StepOneId).FirstOrDefault();
        //        if (model == null)
        //        {
        //            _toastNotification.AddErrorToastMessage("Object Not Found");

        //            return Redirect("/crm/configurations/Managesubproduct/managestepone/index");
        //        }
        //        model.IsDeleted = true;
        //        var UpdatedStepOne = _context.StepOnes.Attach(model);

        //        UpdatedStepOne.State = Microsoft.EntityFrameworkCore.EntityState.Modified;

        //        _context.SaveChanges();

        //        _toastNotification.AddSuccessToastMessage("Step Deleted Successfully");



        //    }
        //    catch (Exception)
        //    {
        //        _toastNotification.AddErrorToastMessage("Something went Error");

        //    }

        //    return Redirect("/crm/configurations/Managesubproduct/managestepone/index");

        //}

        private string UploadImage(string folderPath, IFormFile file)
        {

            folderPath += Guid.NewGuid().ToString() + "_" + file.FileName;

            string serverFolder = Path.Combine(_hostEnvironment.WebRootPath, folderPath);

            file.CopyToAsync(new FileStream(serverFolder, FileMode.Create));

            return folderPath;
        }

    }
}
