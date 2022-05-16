using Jering;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorPages();
// Add Jering Node services
builder.Services.ConfigureNodejsService();

var app = builder.Build();

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
// app.UseResponseCompression();


app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

if (hostEnvironment != null)
{
    app.UseNodejsService(hostEnvironment, "./build/client");
    app.UseMiddleware<NodejsMiddleware>();
}



app.Run();
