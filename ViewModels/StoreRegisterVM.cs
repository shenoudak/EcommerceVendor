using System.ComponentModel.DataAnnotations;
namespace Jovera.ViewModels
{
    public class StoreRegisterVM
    {
        [Required(ErrorMessage = "Enter your Store Name")]
        [Display(Name = "Store Name")]
        public string StoreName { get; set; }
        public string Address { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }
        [Required(ErrorMessage = "Enter your Email")]
        [EmailAddress(ErrorMessage = "Not a vaild Email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

    }
}
