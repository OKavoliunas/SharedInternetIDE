using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public class SignInModel : PageModel
{
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly UserManager<IdentityUser> userManager;

    public SignInModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager) 
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
    }
    public async Task<IActionResult> OnGetAsync(string userId) 
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user != null)
        {
            await signInManager.SignInAsync(user, isPersistent: false);
            return Redirect("~/");
        }
        else
        {
            return NotFound("User ");
        }
        
    }
}
