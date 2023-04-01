using Microsoft.EntityFrameworkCore;
using StudentAPI;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



// Get All 
app.MapGet("/students", async (DataContext context) => 
    await context.Students.ToListAsync());

// Get 
app.MapGet("/students/{id}", async (DataContext context, int id) =>
    await context.Students.FindAsync(id) is Student student ? 
        Results.Ok(student) :
        Results.NotFound("Student not found"));

// Post
app.MapPost("/students", async(DataContext context, Student student) =>
{
    context.Students.Add(student);
    await context.SaveChangesAsync();
    return Results.Ok(student);
});

// Put
app.MapPut("/students/{id}", async(DataContext context, Student student, int id) =>
{ 
    var dbStudent = await context.Students.FindAsync(id);
    if (dbStudent == null) return Results.NotFound("Student not found");
    dbStudent.FirstName = student.FirstName;
    dbStudent.LastName = student.LastName;
    await context.SaveChangesAsync();
    return Results.Ok(dbStudent);
});

// Delete
app.MapDelete("/students/{id}", async(DataContext context, int id) =>
{
    var dbStudent = await context.Students.FindAsync(id);
    if (dbStudent == null) return Results.NotFound("Student not found");
    context.Students.Remove(dbStudent);
    await context.SaveChangesAsync();
    return Results.Ok(dbStudent);

});



app.UseAuthorization();

app.MapControllers();

app.Run();
