using LBox.Common.Shared.Contracts;
using LBox.Common.WebApi.Middleware;
using LBox.Common.WebApi.Services;
using LitArchive.Infrastructure.Contracts;
using LitArchive.Infrastructure.Models;
using LitArchive.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using System.Net;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddControllers(options =>
    {
        //options.Filters.Add<HttpResponseExceptionFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// CORS config
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var archiveOptionsSection = builder.Configuration.GetSection("ArchiveOptions");
var archiveOptions = archiveOptionsSection.Get<ArchiveOptions>();
builder.Services.Configure<ArchiveOptions>(archiveOptionsSection);


builder.Services.AddScoped<IAuthTokenProvider, AuthTokenProvider>();

builder.Services.AddScoped<IArchiveDataService, ArchiveDataService>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("corsapp");

app.UseMiddleware<AuthorizationMiddleware>();
//app.UseMiddleware<ExceptionMiddleware>();

//app.UseHttpsRedirection();

app.UseAuthorization();


app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream",
    //FileProvider = new PhysicalFileProvider("/root/"),
    FileProvider = new PhysicalFileProvider(archiveOptions.Root),
    RequestPath = "/files",
    OnPrepareResponse = ctx =>
    {
        var accessKey = ctx.Context.Request.Query["access"].FirstOrDefault();
        if (!String.IsNullOrWhiteSpace(accessKey))
        {
            var service = ctx.Context.RequestServices.GetService<IArchiveDataService>();
            if (service != null && service.ValidateAccessKey(accessKey))
            {
                // token is linked to the user => access allowed
                // client can store the file in private (browser) cache for 7 days 7 * 24 * 60 * 60 = 604800 seconds
                ctx.Context.Response.Headers.Add("Cache-Control", "private, max-age=604800");
                return;
            }
        }
        // respond HTTP 401 Unauthorized, and...
        ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

        // Append following 2 lines to drop body from static files middleware!
        ctx.Context.Response.ContentLength = 0;
        ctx.Context.Response.Body = Stream.Null;

    }
});


app.MapControllers();

app.Run();
