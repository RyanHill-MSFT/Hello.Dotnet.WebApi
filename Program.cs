using Hello.Dotnet.WebApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddDbContext<TodoDb>(options => options.UseInMemoryDatabase("TodoList"))
    .AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

var todoItems = app.MapGroup("/todo/items");

todoItems.MapGet("/", async (TodoDb db) => await db.Todos.ToListAsync());
todoItems.MapGet("/complete", async (TodoDb db) => await db.Todos.Where(x => x.IsComplete).ToListAsync());
todoItems.MapGet("/{id}", async (int id, TodoDb db) => await db.Todos.FindAsync(id) is Todo todo ? Results.Ok(todo) : Results.NotFound());
todoItems.MapPost("/", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todo/items/{todo.Id}", todo);
});
todoItems.MapPut("/{id}", async (int id, Todo item, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    todo.Name = item.Name;
    todo.IsComplete = item.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
todoItems.MapDelete("/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();
