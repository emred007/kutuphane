using LibraryManagement.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var dataDirectory = LibraryPaths.ResolveDataDirectory();
var bootstrap = new LibraryAppBootstrap(dataDirectory);
bootstrap.Initialize();
builder.Services.AddSingleton(bootstrap);
builder.Services.AddSingleton(bootstrap.Library);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.Use(async (context, next) =>
{
    var library = context.RequestServices.GetRequiredService<LibraryService>();
    library.ReloadIfChanged();
    await next();
});

app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
