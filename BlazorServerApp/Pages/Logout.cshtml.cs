using System.Threading.Tasks;
using BlazorServerApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> signInManager;

    public LogoutModel(SignInManager<ApplicationUser> signInManager)
    {
        this.signInManager = signInManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync()
    {
        await signInManager.SignOutAsync();
        return LocalRedirect("~/");
    }
}
