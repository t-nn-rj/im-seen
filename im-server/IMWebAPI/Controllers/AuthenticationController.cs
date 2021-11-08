﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IMWebAPI.Data;
using IMWebAPI.Helpers;
using IMWebAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IMWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IM_API_Context _context;
        private readonly IEmailer _emailer;
        //private readonly JwtBearerTokenSettings jwtBearerTokenSettings;

        public AuthenticationController(UserManager<ApplicationUser> userManager, IM_API_Context context, IEmailer emailer)
        {
            _userManager = userManager;
            _context = context;
            _emailer = emailer;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(string email, string password, string role, string job, string firstname, string lastname)
        {
            if (!ModelState.IsValid || email == null)
                return new BadRequestObjectResult(new { Message = "User Registration failed." });

            var appUser = new ApplicationUser() { UserName = email, Email = email, JobTitle = job, FirstName = firstname, LastName = lastname };
            bool sendRegLink = false;
            if (string.IsNullOrEmpty(password))
            {
                // adding user from admin UI - create a temporary password and send a registration link
                sendRegLink = true;
                password = Guid.NewGuid().ToString();
            }

            var result = await _userManager.CreateAsync(appUser, password);
            if (!result.Succeeded)
                return new BadRequestObjectResult(new { Message = "User Registration failed. Could not create new account." });
            if (role == null || (role != "Administrator" && role != "PrimaryActor" && role != "SupportingActor"))
            { 
                result = await _userManager.AddToRoleAsync(appUser, "Observer");
                appUser.Role = "Observer";
            }
            else
            {
                result = await _userManager.AddToRoleAsync(appUser, role);
                appUser.Role = role;
            }
            
            if (!result.Succeeded)
                return new BadRequestObjectResult(new { Message = "User Registration Successful, but could not assign the appropriate role." });


            if (sendRegLink)
            {
                _emailer.Send(email, "Create web portal account", "You have been added as a user on Canyons School District's IMseen web portal. Please follow the link below to activate your account.\n\n"
                     + $"{{webUrl}}/register?username={email}&token={password}");
            }

            return Ok(new { Message = "User Registration Successful" });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (!ModelState.IsValid || username == null || password == null)
                return new BadRequestObjectResult(new { Message = "Login failed." });

            var appUser = await _userManager.FindByNameAsync(username);
            if (appUser == null)
                return new BadRequestObjectResult(new { Message = "Login failed. Email not recognized." });

            var passResult = _userManager.PasswordHasher.VerifyHashedPassword(appUser, appUser.PasswordHash, password);
            if (passResult == PasswordVerificationResult.Failed)
                return new BadRequestObjectResult(new { Message = "Login failed. Incorrect password." });

            var jwtGenerator = new JwtGenerator();

            jwtGenerator.AddClaim(new Claim(ClaimTypes.Email, appUser.Email));
            jwtGenerator.AddClaim(new Claim(ClaimTypes.Name, appUser.UserName));


            // Add more claims as necessary (ROLES)
            foreach (var role in await _userManager.GetRolesAsync(appUser))
            {
                jwtGenerator.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            var accessToken = jwtGenerator.GetAccessToken();
            var refreshToken = jwtGenerator.GetRefreshToken();

            appUser.RefreshToken = refreshToken;
            appUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(5); // CHANGE THIS VALUE AFTER TESTING
            var result = await _userManager.UpdateAsync(appUser);

            if (!result.Succeeded)
                return new BadRequestObjectResult(new { Message = "Login failed. Token not generated." });

            return Ok(new { Token = accessToken, RefreshToken = refreshToken, Message = "You are logged in." });
        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(string username, string currentPassword, string password, string firstname, string lastname, string job)
        {
            if (!ModelState.IsValid || username == null || password == null || currentPassword == null)
                return new BadRequestObjectResult(new { Message = "Missing parameters" });

            var appUser = await _userManager.FindByNameAsync(username);
            if (appUser == null)
                return new BadRequestObjectResult(new { Message = "Login failed. Email not recognized." });

            var result = await _userManager.ChangePasswordAsync(appUser, currentPassword, password);
            if (!result.Succeeded)
                return new BadRequestObjectResult(new { Message = "Login failed. Password incorrect or does not meet requirements." });

            if (!string.IsNullOrEmpty(firstname) || !string.IsNullOrEmpty(lastname) || !string.IsNullOrEmpty(job))
            {
                var user = _context.Users.Where(u => u.UserName == username).First();
                if (!string.IsNullOrEmpty(firstname))
                {
                    user.FirstName = firstname;
                }
                if (!string.IsNullOrEmpty(lastname))
                {
                    user.LastName = lastname;
                }
                if (!string.IsNullOrEmpty(job))
                {
                    user.JobTitle = job;
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new { Message = "Password changed successfully" });
        }

        [HttpGet, Authorize(Roles = "Administrator, PrimaryActor, SupportingActor")]
        [Route("User")]
        public async Task<IActionResult> GetUserInfo()
        {
            var username = User.Identity.Name;
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return BadRequest();
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new { User = user, Role = roles.First() });
        }

        [HttpPost]
        [Route("Logout")]
        public IActionResult Logout()
        {
            return Ok(new { Message = "Logout Successful" });
        }
    }
}
