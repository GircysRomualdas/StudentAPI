using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using StudentAPI;
using System;

var builder = WebApplication.CreateBuilder(args);


IdentityModelEventSource.ShowPII = true;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => 
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
     {
         c.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
         c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
         {
             ValidAudience = builder.Configuration["Auth0:Audience"],
             ValidIssuer = $"{builder.Configuration["Auth0:Domain"]}"
         };
     });


builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("todo:read-write", p => p.
        RequireAuthenticatedUser().
        RequireClaim("scope", "todo:read-write"));
});


builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthentication();
app.UseAuthorization();


app.UseHttpsRedirection();



// Get All 
app.MapGet("/students", async (DataContext context) => 
    await context.Students.ToListAsync()).RequireAuthorization("todo:read-write");

// Get 
app.MapGet("/students/{id}", async (DataContext context, int id) =>
    await context.Students.FindAsync(id) is Student student ? 
        Results.Ok(student) :
        Results.NotFound("Student not found")).RequireAuthorization("todo:read-write");

// Post
app.MapPost("/students", async(DataContext context, Student student) =>
{
    context.Students.Add(student);
    await context.SaveChangesAsync();
    return Results.Ok(student);
}).RequireAuthorization("todo:read-write").RequireAuthorization("todo:read-write");

// Put
app.MapPut("/students/{id}", async(DataContext context, Student student, int id) =>
{ 
    var dbStudent = await context.Students.FindAsync(id);
    if (dbStudent == null) return Results.NotFound("Student not found");
    dbStudent.FirstName = student.FirstName;
    dbStudent.LastName = student.LastName;
    await context.SaveChangesAsync();
    return Results.Ok(dbStudent);
}).RequireAuthorization("todo:read-write");

// Delete
app.MapDelete("/students/{id}", async(DataContext context, int id) =>
{
    var dbStudent = await context.Students.FindAsync(id);
    if (dbStudent == null) return Results.NotFound("Student not found");
    context.Students.Remove(dbStudent);
    await context.SaveChangesAsync();
    return Results.Ok(dbStudent);

}).RequireAuthorization("todo:read-write");



app.UseAuthorization();

app.MapControllers();

app.Run();
