using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public class LogoutModel : PageModel
{
    private readonly SignInManager<IdentityUser> signInManager;

    public LogoutModel(SignInManager<IdentityUser> signInManager)
    {
        this.signInManager = signInManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPost()
    {
        await signInManager.SignOutAsync();
        return LocalRedirect("~/");
    }
}
