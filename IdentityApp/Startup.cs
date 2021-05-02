using IdentityApp.ClaimProviders;
using IdentityApp.CustomValidations;
using IdentityApp.Models;
using IdentityApp.Requirements;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace IdentityApp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IAuthorizationHandler, ExpireDateExchangeHandler>();
            services.AddDbContext<AppIdentityDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SqlConStr"));
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("SomeCityPolicy", policy =>
                {
                    policy.RequireClaim("city", "KOCAELİ");
                });
                options.AddPolicy("ViolencePolicy", policy =>
                {
                    policy.RequireClaim("violence");
                });
                options.AddPolicy("ExchangePolicy", policy =>
                {
                    policy.AddRequirements(new ExpireDateExchangeRequirement());
                });
            });

            services.AddAuthentication().AddFacebook(options =>
            {
                options.AppId = Configuration["Authentication:Facebook:AppId"].ToString();
                options.AppSecret = Configuration["Authentication:Facebook:AppSecret"].ToString();
            }).AddGoogle(options =>
            {
                options.ClientId = Configuration["Authentication:Google:ClientId"].ToString();
                options.ClientSecret = Configuration["Authentication:Google:ClientSecret"].ToString();
            }).AddMicrosoftAccount(options =>
            {
                options.ClientId = Configuration["Authentication:Microsoft:ClientId"].ToString();
                options.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"].ToString();
            });


            services.AddIdentity<UserApp, RoleApp>(options =>
            {
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;


                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcçdefgğhıijklmnoöpqrsştuüvwxyzABCÇDEFGĞHIİJKLMNOÖPQRSÞTUÜVWXYZ0123456789-._";


            }).AddPasswordValidator<CustomPasswordValidator>().AddUserValidator<CustomUserValidator>().AddErrorDescriber<CustomIdentityErrorDescriber>().AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders();

            CookieBuilder cookieBuilder = new CookieBuilder
            {
                Name = "MyWebSite",
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,  //SameSiteMode.Strict : Only this site
                SecurePolicy = CookieSecurePolicy.SameAsRequest //CookieSecurePolicy.Always : Request is only HTTPS || CookieSecurePolicy.SameAsRequest : HTTP - HTTP OR HTTPS - HTTPS || CookieSecurePolicy.None : Only HTTP 
            };

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = new PathString("/Home/LogIn");
                options.LogoutPath = new PathString("/Member/LogOut");
                options.AccessDeniedPath = new PathString("/Member/AccessDenied");
                options.Cookie = cookieBuilder;
                options.SlidingExpiration = true; // Expiration Time Half = Expiration Time Half + Expiration Time
                options.ExpireTimeSpan = TimeSpan.FromDays(2);
            });


            services.AddScoped<IClaimsTransformation, ClaimProvider>();

            services.AddControllersWithViews();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseStatusCodePages();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
