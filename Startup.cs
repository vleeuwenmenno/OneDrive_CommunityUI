using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneDrive_CSharp;

namespace OneDrive_CommunityUI
{
    public class Startup
    {
        public static OneDrive oneDrive;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            Console.WriteLine(Environment.CurrentDirectory + "/wwwroot/img/onedrive.png");

            var opt = new BrowserWindowOptions
            {
                Icon = Environment.CurrentDirectory + "/wwwroot/img/onedrive.png",
                Show = false,
                AlwaysOnTop = true,
                AutoHideMenuBar = true,
                Frame = false,
                Width = 450,
                Height = 650,
                SkipTaskbar = true,
                Fullscreenable = false
            };


            Task.Run(async () => await Electron.WindowManager.CreateWindowAsync(opt));

            MenuItem[] items = {
                new MenuItem {
                    Label = "Toggle window",
                    Enabled = true,
                    Click = () => toggleWindow()
                }
            };
            
            Electron.Tray.Show(Environment.CurrentDirectory + "/wwwroot/img/onedrive.png", items);
            Electron.Tray.SetToolTip("OneDrive Community UI");

            oneDrive = new OneDrive();

            oneDrive.authenticate ();
            oneDrive.startMonitorThread ();
            Console.WriteLine($"Started monitoring thread. [Authenticated: {oneDrive.hasAuthenticated}]");
            OneDrive_CSharp.Misc.unix_simple("notify-send", "\"OneDrive is hidden to tray\" \"It will continue to sync in the background.\" --icon=\""+Environment.CurrentDirectory + "/wwwroot/img/onedrive.png\"");
        }

        private async void toggleWindow()
        {
            if (await Electron.WindowManager.BrowserWindows.First().IsVisibleAsync())
                Electron.WindowManager.BrowserWindows.First().Hide();
            else
            {
                Display[] all = await Electron.Screen.GetAllDisplaysAsync();

                int h = all.Max(r => r.Size.Height);
                int w = all.Sum(r => r.Size.Width);

                int x = w - 450;
                int y = h - 650;

                Electron.WindowManager.BrowserWindows.First().SetPosition(x, y);
                Electron.WindowManager.BrowserWindows.First().Show();
            }
        }
    }
}
