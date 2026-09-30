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

var todoitems = app.MapGroup("/todoitems");

todoitems.MapGet("/", GetAllTodos);
todoitems.MapGet("/complete", GetCompleteTodos);
todoitems.MapGet("/{id}", GetTodo);
todoitems.MapPost("/", CreateTodo);
todoitems.MapPut("/{id}", UpdateTodo);
todoitems.MapPatch("/{id}", PatchTodo);
todoitems.MapDelete("/{id}", DeleteTodo);

static async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.ToArrayAsync());
}

static async Task<IResult> GetCompleteTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).ToListAsync());
}

static async Task<IResult> GetTodo(TodoDb db, int id)
{
    return await db.Todos.FindAsync(id)
        is Todo todo
            ? TypedResults.Ok(todo)
            : TypedResults.NotFound();
}

static async Task<IResult> CreateTodo(Todo todo, TodoDb db)
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/todoitems/{todo.Id}", todo);
}

static async Task<IResult> UpdateTodo(int id, Todo inputTodo, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return TypedResults.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> PatchTodo(int id, TodoPatchDto inputTodo, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return TypedResults.NotFound();

    if (inputTodo.Name is not null) todo.Name = inputTodo.Name;
    if (inputTodo.IsComplete is not null) todo.IsComplete = inputTodo.IsComplete.Value;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}

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