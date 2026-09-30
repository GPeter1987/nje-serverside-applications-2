using Microsoft.EntityFrameworkCore;
using TodoApi;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddOpenApi();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.ToListAsync());

app.MapGet("/todoitems/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound());

app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.MapPatch("/todoitems/{id}", async (int id, TodoPatchDto inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    if (inputTodo.Name is not null) todo.Name = inputTodo.Name;
    if (inputTodo.IsComplete is not null) todo.IsComplete = inputTodo.IsComplete.Value;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();

/*
    API	                |        Description	        |    Request body	           |        Response body
==================================================================================================================
GET /todoitems	        |    Get all to-do items	    |       None	               |    Array of to-do items
==================================================================================================================
GET /todoitems/complete	|    Get completed to-do items	|       None	               |    Array of to-do items
==================================================================================================================
GET /todoitems/{id}	    |    Get an item by ID	        |       None	               |        To-do item
==================================================================================================================
POST /todoitems	        |    Add a new item	            |       To-do item	           |        To-do item
==================================================================================================================
PUT /todoitems/{id}	    |    Update an existing item  	|       To-do item	           |            None
==================================================================================================================
PATCH /todoitems/{id}	|    Partially update an item  	|       Partial to-do item	   |            None
==================================================================================================================
DELETE /todoitems/{id}  |  	Delete an item    	        |       None	               |            None
==================================================================================================================
 */