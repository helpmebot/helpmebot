namespace Helpmebot.WebUI.Controllers
{
    using System.Threading.Tasks;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebUI.Models;
    using Helpmebot.WebUI.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    public class AuthenticationController : ControllerBase
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly UserStore userStore;

        public AuthenticationController(IApiService apiService, SignInManager<User> signInManager, UserManager<User> userManager, IUserStore<User> userStore) : base(apiService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.userStore = (UserStore)userStore;
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
            if (!this.signInManager.IsSignedIn(this.User))
            {
                return Redirect("/");
            }
            
            var user = await this.userManager.FindByNameAsync(this.User.Identity.Name);

            if (user != null)
            {
                this.ApiService.InvalidateToken(user.Token.Split(':')[0]);
                this.userStore.LogoutUser(user);
            }

            await this.signInManager.SignOutAsync();
            return Redirect("/");
        }
    }
}