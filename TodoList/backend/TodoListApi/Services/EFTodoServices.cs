using System;
using TodoListApi.Models;
using TodoListApi.Data;
using Microsoft.EntityFrameworkCore;

namespace TodoListApi.Services;

public class EfTodoService : ITodoService
{
    private readonly TodoDbContext _context;

    public EfTodoService(TodoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Todo>> GetAllTodosAsync()
    {
        return await _context.Todos.ToListAsync();
    }

    public async Task<Todo?> GetTodoByIdAsync(string id)
    {
        return await _context.Todos.FindAsync(id);
    }

    public async Task<Todo> CreateTodoAsync(Todo todo)
    {
        todo.Id = Guid.NewGuid().ToString();
        todo.CreatedDate = DateTime.UtcNow;

        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        return todo;
    }

    public async Task<Todo?> UpdateTodoAsync(string id, Todo updatedTodo)
    {
        var existingTodo = await _context.Todos.FindAsync(id);
        if (existingTodo == null)
            return null;

        existingTodo.Title = updatedTodo.Title;
        existingTodo.Description = updatedTodo.Description;
        existingTodo.Priority = updatedTodo.Priority;
        existingTodo.Category = updatedTodo.Category;
        existingTodo.IsCompleted = updatedTodo.IsCompleted;
        existingTodo.DueDate = updatedTodo.DueDate;
        existingTodo.Tags = updatedTodo.Tags;

        await _context.SaveChangesAsync();
        return existingTodo;
    }

    public async Task<bool> DeleteTodoAsync(string id)
    {
        var todo = await _context.Todos.FindAsync(id);
        if (todo == null)
            return false;

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TodoStats> GetStatsAsync()
    {
        var todos = await _context.Todos.ToListAsync();
        var categories = await _context.Categories.ToListAsync();

        var completedTodos = todos.Count(t => t.IsCompleted);
        var overdueTodos = todos.Count(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate < DateTime.UtcNow);

        var todosByCategory = todos
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Count());

        var todosByPriority = todos
            .GroupBy(t => t.Priority.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return new TodoStats
        {
            TotalTodos = todos.Count,
            CompletedTodos = completedTodos,
            PendingTodos = todos.Count - completedTodos,
            TotalCategories = categories.Count,
            TodosByCategory = todosByCategory,
            TodosByPriority = todosByPriority,
            OverdueTodos = overdueTodos
        };
    }
}

public class EfCategoryService : ICategoryService
{
    private readonly TodoDbContext _context;

    public EfCategoryService(TodoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories.ToListAsync();

        // Update todo counts for each category
        foreach (var category in categories)
        {
            category.TodoCount = await _context.Todos
                .CountAsync(t => t.Category == category.Name);
        }

        return categories;
    }

    public async Task<Category?> GetCategoryByIdAsync(string id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            category.TodoCount = await _context.Todos
                .CountAsync(t => t.Category == category.Name);
        }
        return category;
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        category.Id = Guid.NewGuid().ToString();
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateCategoryAsync(string id, Category updatedCategory)
    {
        var existingCategory = await _context.Categories.FindAsync(id);
        if (existingCategory == null)
            return null;

        existingCategory.Name = updatedCategory.Name;
        existingCategory.Description = updatedCategory.Description;
        existingCategory.Color = updatedCategory.Color;

        await _context.SaveChangesAsync();
        return existingCategory;
    }

    public async Task<bool> DeleteCategoryAsync(string id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}
