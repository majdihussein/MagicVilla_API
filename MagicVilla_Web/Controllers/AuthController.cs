using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MagicVilla_Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;


        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDTO obj = new();
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO obj)
        {
            APIResponse response = await _authService.LoginAsync<APIResponse>(obj);

            if (response != null && response.IsSuccess)
            {
                LoginResponseDTO model = JsonConvert.DeserializeObject<LoginResponseDTO>(Convert.ToString(response.Result)); // الان بدنا نتعامل مع التوكين بداخل الجلسات - session

                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(model.Token);

                // انشاء هوية كوكي للمستخدم للتنقل دون الحاجة للدخول مرات عديدة في كل حركة
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme); // انشاء هوية جديدة
                
                identity.AddClaim(new Claim(ClaimTypes.Name, jwt
                    .Claims.FirstOrDefault(u =>u.Type == "unique_name").Value));
                identity.AddClaim(new Claim(ClaimTypes.Role, jwt
                    .Claims.FirstOrDefault(u =>u.Type == "role").Value));

                var principal = new ClaimsPrincipal(identity); // انشاء كائن جديد من ClaimsPrincipal
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


                HttpContext.Session.SetString(SD.SessionToken, model.Token); // استدعينا الوسيط وخززنا التوكين في الجلسة المنشأة
                return RedirectToAction("Index", "Home"); // الى الكنترولر هوم ومن ثم الى الاندكس بداخل الهوم
            }
            else
            {
                ModelState.AddModelError("CustomError", response.ErrorMessage.FirstOrDefault());
                return View(obj);
            }

        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequestDTO obj)
        {
            APIResponse result = await _authService.RegisterAsync<APIResponse>(obj);
            if (result != null && result.IsSuccess)
            {
                return RedirectToAction(nameof(Login));
            }
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(); // Sign out from the current session
            HttpContext.Session.SetString(SD.SessionToken, ""); // Clear the session token
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
