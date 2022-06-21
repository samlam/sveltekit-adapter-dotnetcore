using Jering;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// Add Jering Node services
builder.Services.ConfigureNodejsService(builder.Configuration.GetSection("NodejsOptions").Get<NodejsOptions>());

builder.Services.AddResponseCompression(options => options.EnableForHttps = true);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

IHostEnvironment? hostEnvironment = app.Services.GetService<IHostEnvironment>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseResponseCompression();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

if (hostEnvironment != null)
{
    app.UseNodejsService(hostEnvironment, "./build/client");
    app.UseWhen(
        httpContext => 
            httpContext.Request.Path.StartsWithSegments("/joke") ||
            httpContext.Request.Path.StartsWithSegments("/about"), 
        appBuilder => appBuilder.UseMiddleware<NodejsMiddleware>());
}

app.Run();
