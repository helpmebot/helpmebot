namespace Helpmebot.WebUI.Controllers
{
    using System.Threading.Tasks;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebUI.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    public class AuthenticationController : ControllerBase
    {
        private readonly SignInManager<User> signInManager;

        public AuthenticationController(IApiService apiService, SignInManager<User> signInManager) : base(apiService)
        {
            this.signInManager = signInManager;
        }

        [HttpGet("/login")]
        public IActionResult Login()
        {
            var loginToken = this.ApiService.GetLoginToken();
            
            return View(new LoginRequest { LoginToken = loginToken });
        }
        
        [HttpPost("/login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var token = this.ApiService.GetAuthToken(request.LoginToken);
            HttpContext.Session.SetString("AuthenticationController.account", token.IrcAccount);
            HttpContext.Session.SetString("AuthenticationController.token", token.Token);

            await this.signInManager.SignInAsync(new User { Token = token.Token, Account = token.IrcAccount }, false);
            
            return Redirect("/");
        }

        [HttpPost("/logout")]
        public async Task<IActionResult> Logout()
        {
            await this.signInManager.SignOutAsync();
            return Redirect("/");
        }
    }
}