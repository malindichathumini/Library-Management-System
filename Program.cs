using System.Security.Claims;
using BookNest.Data;
using BookNest.model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure cookies to support authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None; // Allows cross-site cookies
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Requires HTTPS
});

// Add CORS policy for React app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // React app URL
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // Needed for cookies
        });
});

// Configure authentication and authorization
builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);
builder.Services.AddAuthorizationBuilder();

// Configure database context
builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure identity services
builder.Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();
app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

// Map identity API
app.MapIdentityApi<User>();

// Steps to Fix the 401 Unauthorized Error
/*
1. **Ensure User Authentication:**
   - Verify that the user is logged in. If not, implement a login mechanism (e.g., calling the `/api/account/signin` endpoint).
   - Ensure cookies or bearer tokens are passed with the request.

2. **Verify Authentication Middleware:**
   - Ensure `app.UseAuthentication()` and `app.UseAuthorization()` are included in the correct order in the middleware pipeline.

3. **Check CORS Configuration:**
   - If your frontend is running on a different port (e.g., `http://localhost:5173`), confirm that the `AllowReactApp` CORS policy is properly configured.
   - Include `.AllowCredentials()` in the CORS configuration if cookies are required.

4. **Add Headers in Postman:**
   - Add the `Cookie` or `Authorization` header to your Postman requests:
     - For cookies, copy the authentication cookie from the browser after login.
     - For bearer tokens, retrieve the token from the `/api/account/signin` endpoint and include it as `Authorization: Bearer <token>`.

5. **Cross-Site Request Issues:**
   - If using cookies for authentication, ensure that the `SameSite` and `Secure` settings are correctly configured in the `ConfigureApplicationCookie` method.

6. **Check User Identity:**
   - Verify that the `user.Identity.Name` is being set correctly when authenticated.
   - If it is null or empty, it may indicate a problem with the authentication setup.
*/

app.MapGet("/hello", (ClaimsPrincipal user) => user.Identity!.Name).RequireAuthorization();

app.MapPost("/api/account/signout", async (SignInManager<User> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
}).RequireCors("AllowReactApp");

// Endpoint to create a new book
app.MapPost("/api/books", async (AppDbContext db, Book book, ClaimsPrincipal user) =>
{
    var userName = user.Identity?.Name;

    if (string.IsNullOrEmpty(userName))
    {
        return Results.Unauthorized();
    }

    // Set the CreatedBy property to the user's name
    book.CreatedBy = userName;

    db.Books.Add(book);
    await db.SaveChangesAsync();

    return Results.Created($"/api/books/{book.Id}", book);
}).RequireAuthorization()
.RequireCors("AllowReactApp");

// Endpoint to get books created by the current user
app.MapGet("/api/books", async (AppDbContext db, ClaimsPrincipal user) =>
{
    var userName = user.Identity?.Name;

    if (string.IsNullOrEmpty(userName))
    {
        return Results.Unauthorized();
    }

    var books = await db.Books.Where(b => b.CreatedBy == userName).ToListAsync();
    return Results.Ok(books);
}).RequireCors("AllowReactApp");

// Endpoint to get a specific book by ID
app.MapGet("/api/books/{id}", async (int id, AppDbContext db, ClaimsPrincipal user) =>
{
    var userName = user.Identity?.Name;

    if (string.IsNullOrEmpty(userName))
    {
        return Results.Unauthorized();
    }

    var book = await db.Books.FindAsync(id);

    if (book == null || book.CreatedBy != userName)
    {
        return Results.NotFound();
    }

    return Results.Ok(book);
}).RequireAuthorization();

// Endpoint to update a book
app.MapPut("/api/books/{id}", async (int id, AppDbContext db, Book updatedBook, ClaimsPrincipal user) =>
{
    var userName = user.Identity?.Name;

    if (string.IsNullOrEmpty(userName))
    {
        return Results.Unauthorized();
    }

    var book = await db.Books.FindAsync(id);

    if (book == null || book.CreatedBy != userName)
    {
        return Results.NotFound();
    }

    // Update book properties
    book.Title = updatedBook.Title;
    book.Author = updatedBook.Author;
    book.Description = updatedBook.Description;

    await db.SaveChangesAsync();

    return Results.NoContent();
}).RequireAuthorization();

// Endpoint to delete a book
app.MapDelete("/api/books/{id}", async (int id, AppDbContext db, ClaimsPrincipal user) =>
{
    var userName = user.Identity?.Name;

    if (string.IsNullOrEmpty(userName))
    {
        return Results.Unauthorized();
    }

    var book = await db.Books.FindAsync(id);

    if (book == null || book.CreatedBy != userName)
    {
        return Results.NotFound();
    }

    db.Books.Remove(book);
    await db.SaveChangesAsync();

    return Results.NoContent();
}).RequireAuthorization();

app.Run();
