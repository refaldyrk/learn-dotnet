using BookAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BookDb>(opt => opt.UseInMemoryDatabase("Bookist"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//GET: All Book
app.MapGet("/book", async (BookDb db) => await db.Books.ToListAsync()).WithOpenApi();

app.MapGet("/book/{id}", async (int id,BookDb db) => 
        await db.Books.FindAsync(id) 
        is Book book 
        ? Results.Ok(book)
        : Results.NotFound()
).WithOpenApi();

app.MapPost("/book", async (Book book, BookDb db) =>
{
    //No Duplicate ID
    if (await db.Books.FindAsync(book.Id) is Book books)
    {
        return Results.Conflict();
    }
    
    
    db.Books.Add(book);

    await db.SaveChangesAsync();

    return Results.Created($"/book/{book.Id}", book);
}).WithOpenApi();

app.MapPut("/book/{id}", async (int Id, Book inputBook, BookDb db) =>
{
    //Not Found
    if (await db.Books.FindAsync(Id) is Book books)
    {
        books.Author = inputBook.Author;
        books.Name = inputBook.Name;
        books.Page = inputBook.Page;

        await db.SaveChangesAsync();
        return Results.Ok();
    }

    return Results.NotFound();
}).WithOpenApi();

app.MapDelete("/book/{id}", async (int id, BookDb db) =>
{
    if (await db.Books.FindAsync(id) is Book book)
    {
        db.Books.Remove(book);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
}).WithOpenApi();

app.Run();