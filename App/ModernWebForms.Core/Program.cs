
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.ResponseCompression;
using Yarp.ReverseProxy.Forwarder;

#pragma warning disable CS8604

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters();

// Disable compression to resolve the following warning
// warn: Microsoft.WebTools.BrowserLink.Net.BrowserLinkMiddleware[4]
// Unable to configure Browser Link script injection on the response. This may have been caused by the response's Content-Encoding: 'gzip'. Consider disabling response compression.
//
// builder.Services.AddHttpForwarder();

builder.Services.AddReverseProxy()
    .ConfigureHttpClient((context, handler) =>
    {
        handler.AutomaticDecompression = System.Net.DecompressionMethods.All;
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.UseSystemWebAdapters();

app.MapDefaultControllerRoute();
app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);

app.Run();
