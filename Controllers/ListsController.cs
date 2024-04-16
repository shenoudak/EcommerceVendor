using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jovera.Data;
using Jovera.Models;

namespace Jovera.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ListsController : Controller
    {
        private CRMDBContext _context;

        public ListsController(CRMDBContext context)
        {
            _context = context;
        }

      
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SoicialMidiaLink>>> GetSocialLinks()
        {
            var data = await _context.SoicialMidiaLinks.Select(i => new
            {
                Facebook = i.facebooklink,
                Instgram = i.Instgramlink,
                Twitter = i.TwitterLink,
                WhatsApp = i.WhatsApplink,
                LinkedIn = i.LinkedInlink,
                Youtube = i.YoutubeLink,
                SocialMediaLinkId = i.id,

            }).ToListAsync();


            return Ok(new { data });
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contact>>> GetMessages()
        {
            var data = await _context.Contacts.Select(i => new
            {
                FullName = i.FullName,
                TransDate = i.SendingDate.Value.ToShortDateString(),
                Email = i.Email,
                ContactId = i.ContactId,
                Msg = i.Message

            }).ToListAsync();


            return Ok(new { data });
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetProducts()
        {
            var data = await _context.Items.Select(i => new
            {
                ItemId = i.ItemId,
                ItemTitleAr = i.ItemTitleAr,
                ItemTitleEn = i.ItemTitleEn,
                ItemImage = i.ItemImage,
                ItemPrice = i.ItemPrice,
                OrderIndex = i.OrderIndex,
                IsActive = i.IsActive,
                HasSubProduct = i.HasSubProduct,
                

            }).ToListAsync();


            return Ok(new { data });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            var data = await _context.Categories.Select(i => new
            {
                CategoryTLAR = i.CategoryTLAR,
                CategoryTLEN = i.CategoryTLEN,
                CategoryPic = i.CategoryPic,
                CategoryId = i.CategoryId,
                OrderIndex = i.OrderIndex,
                IsActive = i.IsActive
            }).ToListAsync();


            return Ok(new { data });
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubProductStepOne>>> GetSupProductsStepOne(int itemId)
        {
            var data = await _context.SubProductStepOnes.Include(e=>e.StepOne).Include(e => e.Item).Where(e=>e.ItemId==itemId).Select(i => new
            {
                SubProductStepOneId = i.SubProductStepOneId,
                StepOneId = i.StepOneId,
                Icon = i.Icon,
                StepOneTLAR = i.StepOne.StepOneTLAR,
                StepOneTLEN = i.StepOne.StepOneTLEN,
                ItemTitleAr = i.Item.ItemTitleAr,
                ItemTitleEn = i.Item.ItemTitleEn,
                IsDeleted = i.IsDeleted
            }).ToListAsync();


            return Ok(new { data });
        }
        [HttpGet]
        public object GetImagesForItem([FromQuery] int id)
        {
            var productimages = _context.ItemImages.Where(p => p.ItemId == id)
                                .Select(i => new
                                {
                                    i.ItemImageId,
                                    i.ImageName,
                                    i.ItemId
                                });

            return productimages;
        }
        [HttpPost]
        public async Task<int> RemoveImageById([FromQuery] int id)
        {
            var itemPic = await _context.ItemImages.FirstOrDefaultAsync(p => p.ItemImageId == id);
            _context.ItemImages.Remove(itemPic);
            _context.SaveChanges();
            return id;
        }
        [HttpGet]
        public object GetImagesForStoreSampleItem([FromQuery] int id)
        {
            var productimages = _context.StoreProfileImages.Where(p => p.StoreId == id)
                                .Select(i => new
                                {
                                    i.StoreProfileImageId,
                                    i.ImageName,
                                    i.StoreId
                                });

            return productimages;
        }
        [HttpPost]
        public async Task<int> RemoveStoreSampleImageById([FromQuery] int id)
        {
            var itemPic = await _context.StoreProfileImages.FirstOrDefaultAsync(p => p.StoreProfileImageId == id);
            _context.StoreProfileImages.Remove(itemPic);
            _context.SaveChanges();
            return id;
        }
    }
}
