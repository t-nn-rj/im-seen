using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMLibrary.Data;
using IMLibrary.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using IMWebAPI.Controllers;
using System.Security.Claims;

namespace IMWebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddControllers();

            services.AddDbContext<IM_API_Context>(options =>
                    options.UseMySQL(Configuration.GetConnectionString("IM_API_Context")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<IM_API_Context>();

            services.Configure<IdentityOptions>(options =>
            {
                // Default Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 2;
                options.Password.RequiredUniqueChars = 0;
            });


            // authentication settings
            services.AddAuthentication(options =>
            {
                // set schemes to jwt defaults
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.IncludeErrorDetails = true;
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "IMWEBAPI",//jwtBearerTokenSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = "IMWEBAPI",//jwtBearerTokenSettings.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("IMWEBAPI_SECRETKEY")),//jwtBearerTokenSettings.Key)),
                        ValidateLifetime = true,
                        RequireAudience = false,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ClockSkew = TimeSpan.Zero // USED FOR TESTING
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async (ctx) =>
                        {
                            // add roles and role claims to principal, since we don't want to store them all in the jwt
                            // note: a role MUST have at least one role claim to be added here
                            var db = ctx.HttpContext.RequestServices.GetRequiredService<IM_API_Context>();
                            string username = ctx.Principal.FindFirst(ClaimTypes.Name).Value;
                            var result = await (from u in db.Users
                                         where u.UserName == username
                                         join ur in db.UserRoles on u.Id equals ur.UserId
                                         join r in db.Roles on ur.RoleId equals r.Id
                                         join rc in db.RoleClaims on r.Id equals rc.RoleId
                                         select new { Role = r.Name, Claim = new Claim(rc.ClaimType, rc.ClaimValue) }).ToListAsync();
                            var claims = result.Select(r => r.Claim).Append(new Claim(ClaimTypes.Role, result.FirstOrDefault()?.Role));
                            ctx.Principal.AddIdentity(new ClaimsIdentity(claims));
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                // role/claim based access control
                // each controller adds their own access policies
                AuthenticationController.AddPolicies(options);
                DashboardsController.AddPolicies(options);
                DelegationsController.AddPolicies(options);
                ObservationsController.AddPolicies(options);
                ReportsController.AddPolicies(options);
                StudentGroupsController.AddPolicies(options);
                StudentsController.AddPolicies(options);
                SupportersController.AddPolicies(options);
                TokensController.AddPolicies(options);
                UsersController.AddPolicies(options);
            });

            // Register JwtSettings from appsettings.json to JwtSettings options pattern
            var jwtSettingsSection = Configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettingsSection);

            // Library dependencies
            IMLibrary.Helpers.DependencyConfig.AddDependencies(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(builder => builder
            .SetIsOriginAllowed(origin => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            //.WithOrigins("http://localhost:8080", "http://im.godandanime.tv")
            .AllowCredentials());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
