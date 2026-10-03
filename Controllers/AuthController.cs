using Azure.Core;
using CoreCrudWithJwt.Data;
using CoreCrudWithJwt.Dto;
using CoreCrudWithJwt.Models;
using CoreCrudWithJwt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreCrudWithJwt.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private JwtService _jwtservice;
        public AuthController(AppDbContext appDbContext, JwtService JwtService)
        {
            _context = appDbContext;
            _jwtservice= JwtService;
        }
        public async Task<IActionResult> Login(UserDto dto)
        {
            var isexists = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (isexists == null)
            {
                ViewBag.ErrorMessage = "User Not Exists";
                return View("Login");
            }
            else
            {
                if (isexists.Password == dto.Password)
                {
                    // Generate JWT only after successful authentication

                    var token = _jwtservice.GenerateToken(userId: 1, useremail: dto.Email, role: "Admin");
                  
                    // Store JWT in secure HTTP-only cookie
                    Response.Cookies.Append(
                        "access_token",
                        token,
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddMinutes(60)
                        });

                    return RedirectToAction("Index","Dashboard");
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid Password";
                    return View("Login");
                }
            }
        }

        public IActionResult Registration()
        {
            return View();
        }
        public async Task<IActionResult> CreateUser(UserDto dto)
        {
            var ExistingUser = _context.Users.FirstOrDefault(x => x.Email == dto.Email);
            if (ExistingUser == null)
            {

                var user = new Models.User
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    Password = dto.Password
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                ViewBag.ErrorMessage = "User Already There";
                return RedirectToAction("Register");
            }
            TempData["SuccessMessage"] = "Success, plz login";
            return RedirectToAction("Login");
        }
    }
}
