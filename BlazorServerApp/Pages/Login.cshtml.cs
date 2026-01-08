using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BlazorServerApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> signInManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager)
    {
        this.signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync()
    {
        Console.WriteLine("OnPostAsync called");

        if (!ModelState.IsValid)
        {
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine("Validation Error: " + error.ErrorMessage);
                }
            }
            return Page();
        }
        var result = await signInManager.PasswordSignInAsync(
            Input.Email,
            Input.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return LocalRedirect("~/");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
