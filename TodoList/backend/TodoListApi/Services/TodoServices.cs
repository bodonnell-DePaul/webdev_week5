using System.Text.Json;
using TodoListApi.Models;

namespace TodoListApi.Services;

public interface ITodoService
{
    Task<IEnumerable<Todo>> GetAllTodosAsync();
    Task<Todo?> GetTodoByIdAsync(string id);
    Task<Todo> CreateTodoAsync(Todo todo);
    Task<Todo?> UpdateTodoAsync(string id, Todo updatedTodo);
    Task<bool> DeleteTodoAsync(string id);
    Task<TodoStats> GetStatsAsync();
}

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(string id);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category?> UpdateCategoryAsync(string id, Category updatedCategory);
    Task<bool> DeleteCategoryAsync(string id);
}

public class InMemoryTodoService : ITodoService
{
    private readonly List<Todo> _todos;
    private readonly ICategoryService _categoryService;

    public InMemoryTodoService(ICategoryService categoryService)
    {
        _categoryService = categoryService;
        _todos = new List<Todo>
        {
            new Todo
            {
                Id = "1",
                Title = "Complete project proposal",
                Description = "Write and submit the final project proposal for CSC436",
                Priority = Priority.High,
                Category = "Academic",
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                DueDate = DateTime.UtcNow.AddDays(11),
                Tags = new List<string> { "project", "academic", "deadline" }
            },
            new Todo
            {
                Id = "2",
                Title = "Grocery shopping",
                Description = "Buy groceries for the week including fruits and vegetables",
                Priority = Priority.Medium,
                Category = "Personal",
                IsCompleted = true,
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddDays(1),
                Tags = new List<string> { "shopping", "food", "weekly" }
            },
            new Todo
            {
                Id = "3",
                Title = "Team meeting preparation",
                Description = "Prepare slides and agenda for the weekly team meeting",
                Priority = Priority.High,
                Category = "Work",
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                DueDate = DateTime.UtcNow.AddDays(4),
                Tags = new List<string> { "meeting", "presentation", "team" }
            },
            new Todo
            {
                Id = "4",
                Title = "Exercise routine",
                Description = "Complete 30-minute workout including cardio and strength training",
                Priority = Priority.Medium,
                Category = "Health",
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow,
                Tags = new List<string> { "fitness", "health", "routine" }
            },
            new Todo
            {
                Id = "5",
                Title = "Read React documentation",
                Description = "Study advanced React patterns including Context API and custom hooks",
                Priority = Priority.Low,
                Category = "Learning",
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow,
                Tags = new List<string> { "learning", "react", "documentation" }
            }
        };
    }

    public Task<IEnumerable<Todo>> GetAllTodosAsync()
    {
        return Task.FromResult(_todos.AsEnumerable());
    }

    public Task<Todo?> GetTodoByIdAsync(string id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        return Task.FromResult(todo);
    }

    public Task<Todo> CreateTodoAsync(Todo todo)
    {
        todo.Id = Guid.NewGuid().ToString();
        todo.CreatedDate = DateTime.UtcNow;
        _todos.Add(todo);
        return Task.FromResult(todo);
    }

    public Task<Todo?> UpdateTodoAsync(string id, Todo updatedTodo)
    {
        var existingTodo = _todos.FirstOrDefault(t => t.Id == id);
        if (existingTodo == null)
            return Task.FromResult<Todo?>(null);

        existingTodo.Title = updatedTodo.Title;
        existingTodo.Description = updatedTodo.Description;
        existingTodo.Priority = updatedTodo.Priority;
        existingTodo.Category = updatedTodo.Category;
        existingTodo.IsCompleted = updatedTodo.IsCompleted;
        existingTodo.DueDate = updatedTodo.DueDate;
        existingTodo.Tags = updatedTodo.Tags;

        return Task.FromResult<Todo?>(existingTodo);
    }

    public Task<bool> DeleteTodoAsync(string id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo == null)
            return Task.FromResult(false);

        _todos.Remove(todo);
        return Task.FromResult(true);
    }

    public Task<TodoStats> GetStatsAsync()
    {
        var completedTodos = _todos.Count(t => t.IsCompleted);
        var overdueTodos = _todos.Count(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate < DateTime.UtcNow);

        var todosByCategory = _todos
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Count());

        var todosByPriority = _todos
            .GroupBy(t => t.Priority.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var stats = new TodoStats
        {
            TotalTodos = _todos.Count,
            CompletedTodos = completedTodos,
            PendingTodos = _todos.Count - completedTodos,
            TotalCategories = todosByCategory.Keys.Count,
            TodosByCategory = todosByCategory,
            TodosByPriority = todosByPriority,
            OverdueTodos = overdueTodos
        };

        return Task.FromResult(stats);
    }
}

public class InMemoryCategoryService : ICategoryService
{
    private readonly List<Category> _categories;

    public InMemoryCategoryService()
    {
        _categories = new List<Category>
        {
            new Category { Id = "1", Name = "Academic", Description = "School and university related tasks", Color = "#3b82f6" },
            new Category { Id = "2", Name = "Personal", Description = "Personal life and household tasks", Color = "#10b981" },
            new Category { Id = "3", Name = "Work", Description = "Professional and career related tasks", Color = "#f59e0b" },
            new Category { Id = "4", Name = "Health", Description = "Health and fitness related activities", Color = "#ef4444" },
            new Category { Id = "5", Name = "Learning", Description = "Learning and skill development", Color = "#8b5cf6" }
        };
    }

    public Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return Task.FromResult(_categories.AsEnumerable());
    }

    public Task<Category?> GetCategoryByIdAsync(string id)
    {
        var category = _categories.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(category);
    }

    public Task<Category> CreateCategoryAsync(Category category)
    {
        category.Id = Guid.NewGuid().ToString();
        _categories.Add(category);
        return Task.FromResult(category);
    }

    public Task<Category?> UpdateCategoryAsync(string id, Category updatedCategory)
    {
        var existingCategory = _categories.FirstOrDefault(c => c.Id == id);
        if (existingCategory == null)
            return Task.FromResult<Category?>(null);

        existingCategory.Name = updatedCategory.Name;
        existingCategory.Description = updatedCategory.Description;
        existingCategory.Color = updatedCategory.Color;

        return Task.FromResult<Category?>(existingCategory);
    }

    public Task<bool> DeleteCategoryAsync(string id)
    {
        var category = _categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
            return Task.FromResult(false);

        _categories.Remove(category);
        return Task.FromResult(true);
    }
}

// Weather Service Interface - Defines the contract for weather-related operations
// This interface allows us to easily test our code by providing mock implementations
public interface IWeatherService
{
    // Get current weather information for a specific city
    // Returns null if the city is not found or if there's an API error
    Task<WeatherInfo?> GetCurrentWeatherAsync(string city);
    
    // Get 5-day weather forecast for a specific city
    // Returns null if the city is not found or if there's an API error
    Task<WeatherForecast5Day?> GetForecastAsync(string city);
    
    // Get todos that might be affected by weather conditions
    // This demonstrates combining external API data with our local todo data
    Task<IEnumerable<Todo>> GetWeatherSensitiveTodosAsync();
}

// Implementation of the Weather Service using the wttr.in API
// This class handles HTTP communication with external weather services
public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;           // For making HTTP requests to external APIs
    private readonly ITodoService _todoService;        // To access our todo data for filtering
    private readonly ILogger<WeatherService> _logger;  // For logging errors and information
    
    // Dependency Injection Constructor
    // The framework will automatically provide instances of these dependencies
    public WeatherService(HttpClient httpClient, ITodoService todoService, ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _todoService = todoService;
        _logger = logger;
        
        // Configure HttpClient with a reasonable timeout for external API calls
        // External APIs can be slow, but we don't want to hang forever
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }
    
    public async Task<WeatherInfo?> GetCurrentWeatherAsync(string city)
    {
        try
        {
            _logger.LogInformation("Fetching weather data for city: {City}", city);
            
            // Build the API URL - wttr.in provides weather data in JSON format
            // The format parameter specifies we want JSON, not the default ASCII art
            var url = $"https://wttr.in/{Uri.EscapeDataString(city)}?format=j1";
            
            // Make the HTTP GET request to the weather API
            // ConfigureAwait(false) is a best practice for library code to avoid deadlocks
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            
            // Check if the API call was successful
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Weather API returned error status: {StatusCode} for city: {City}", 
                    response.StatusCode, city);
                return null;
            }
            
            // Read the JSON response from the API
            var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            // Deserialize the JSON into our model classes
            // We use System.Text.Json which is built into .NET and performs well
            var apiResponse = System.Text.Json.JsonSerializer.Deserialize<WttrApiResponse>(jsonContent, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // Convert the API response format to our internal WeatherInfo format
            // This mapping isolates our code from changes in the external API structure
            return MapApiResponseToWeatherInfo(apiResponse, city);
        }
        catch (HttpRequestException ex)
        {
            // Network-related errors (no internet, DNS issues, etc.)
            _logger.LogError(ex, "Network error when fetching weather for city: {City}", city);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            // Timeout or cancellation errors
            _logger.LogError(ex, "Timeout when fetching weather for city: {City}", city);
            return null;
        }
        catch (System.Text.Json.JsonException ex)
        {
            // JSON parsing errors (malformed response from API)
            _logger.LogError(ex, "Failed to parse weather API response for city: {City}", city);
            return null;
        }
        catch (Exception ex)
        {
            // Catch any other unexpected errors
            _logger.LogError(ex, "Unexpected error when fetching weather for city: {City}", city);
            return null;
        }
    }
    
    public async Task<WeatherForecast5Day?> GetForecastAsync(string city)
    {
        try
        {
            _logger.LogInformation("Fetching 5-day forecast for city: {City}", city);
            
            // Build the API URL - wttr.in provides weather forecast data in JSON format
            var url = $"https://wttr.in/{Uri.EscapeDataString(city)}?format=j1";
            
            // Make the HTTP GET request to the weather API
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            
            // Check if the API call was successful
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Weather API returned error status: {StatusCode} for city: {City}", 
                    response.StatusCode, city);
                return null;
            }
            
            // Read the JSON response from the API
            var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            // Deserialize the JSON into our model classes
            var apiResponse = System.Text.Json.JsonSerializer.Deserialize<WttrApiResponse>(jsonContent, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // Convert the API response format to our internal forecast format
            return MapApiResponseToForecast(apiResponse, city);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error when fetching forecast for city: {City}", city);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout when fetching forecast for city: {City}", city);
            return null;
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse forecast API response for city: {City}", city);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when fetching forecast for city: {City}", city);
            return null;
        }
    }
    
    public async Task<IEnumerable<Todo>> GetWeatherSensitiveTodosAsync()
    {
        try
        {
            _logger.LogInformation("Identifying weather-sensitive todos");
            
            // Get all todos from our todo service
            var allTodos = await _todoService.GetAllTodosAsync();
            
            // Define keywords that indicate a todo might be weather-sensitive
            // In a real application, this could be stored in configuration or database
            var weatherKeywords = new[]
            {
                "outdoor", "outside", "hiking", "running", "walk", "jog", "bike", "cycling",
                "picnic", "barbecue", "bbq", "gardening", "sports", "soccer", "football",
                "tennis", "golf", "swimming", "beach", "park", "camping", "fishing"
            };
            
            // Filter todos to find weather-sensitive ones
            // We look in both title and description, and only include incomplete todos
            var weatherSensitiveTodos = allTodos.Where(todo =>
                !todo.IsCompleted && // Only incomplete todos are relevant
                (ContainsWeatherKeyword(todo.Title, weatherKeywords) ||
                 ContainsWeatherKeyword(todo.Description, weatherKeywords))
            ).ToList();
            
            _logger.LogInformation("Found {Count} weather-sensitive todos", weatherSensitiveTodos.Count);
            
            return weatherSensitiveTodos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when identifying weather-sensitive todos");
            return Enumerable.Empty<Todo>();
        }
    }
    
    // Private helper method to map the external API response to our internal model
    // This abstraction protects us from changes in the external API structure
    private WeatherInfo? MapApiResponseToWeatherInfo(WttrApiResponse? apiResponse, string requestedCity)
    {
        // Validate that we have the required data from the API
        if (apiResponse?.Current_condition?.Length == 0 || apiResponse?.Current_condition?[0] == null)
        {
            _logger.LogWarning("Invalid or empty weather data received for city: {City}", requestedCity);
            return null;
        }
        
        var currentCondition = apiResponse.Current_condition[0];
        
        // Extract location name from API response, fallback to requested city name
        var locationName = apiResponse.Nearest_area?.FirstOrDefault()?.AreaName?.FirstOrDefault()?.Value 
                          ?? requestedCity;
        
        // Parse weather description from the API response
        var weatherDescription = currentCondition.WeatherDesc?.FirstOrDefault()?.Value ?? "Unknown";
        
        // Create and return our internal weather model
        return new WeatherInfo
        {
            Location = locationName,
            Temperature = ParseIntSafely(currentCondition.Temp_C, 0),
            Description = weatherDescription,
            ConditionCode = currentCondition.WeatherCode,
            Humidity = ParseIntSafely(currentCondition.Humidity, 0),
            WindSpeedKmh = ParseIntSafely(currentCondition.WindspeedKmph, 0),
            RetrievedAt = DateTime.UtcNow
        };
    }
    
    // Helper method to safely parse strings to integers
    // External APIs sometimes return unexpected data, so we need defensive parsing
    private static int ParseIntSafely(string value, int defaultValue)
    {
        return int.TryParse(value, out var result) ? result : defaultValue;
    }
    
    // Private helper method to map the external API response to our forecast model
    private WeatherForecast5Day? MapApiResponseToForecast(WttrApiResponse? apiResponse, string requestedCity)
    {
        // Validate that we have the required forecast data from the API
        if (apiResponse?.Weather?.Length == 0 || apiResponse?.Weather == null)
        {
            _logger.LogWarning("Invalid or empty forecast data received for city: {City}", requestedCity);
            return null;
        }
        
        // Extract location name from API response, fallback to requested city name
        var locationName = apiResponse?.Nearest_area?.FirstOrDefault()?.AreaName?.FirstOrDefault()?.Value 
                          ?? requestedCity;
        
        var forecast = new WeatherForecast5Day
        {
            Location = locationName,
            RetrievedAt = DateTime.UtcNow,
            Days = new List<WeatherForecastDay>()
        };
        
        // Process each forecast day (typically 5 days)
        foreach (var day in apiResponse.Weather.Take(5))
        {
            if (DateTime.TryParse(day.Date, out var forecastDate))
            {
                // Use the first hourly entry for general day conditions
                var dayConditions = day.Hourly?.FirstOrDefault();
                
                forecast.Days.Add(new WeatherForecastDay
                {
                    Date = forecastDate,
                    MaxTemperature = ParseIntSafely(day.MaxtempC, 0),
                    MinTemperature = ParseIntSafely(day.MintempC, 0),
                    Description = dayConditions?.WeatherDesc?.FirstOrDefault()?.Value ?? "Unknown",
                    ConditionCode = dayConditions?.WeatherCode ?? "0",
                    Humidity = ParseIntSafely(dayConditions?.Humidity ?? "0", 0),
                    WindSpeedKmh = ParseIntSafely(dayConditions?.WindspeedKmph ?? "0", 0),
                    ChanceOfRain = ParseIntSafely(dayConditions?.ChanceOfRain ?? "0", 0)
                });
            }
        }
        
        return forecast;
    }
    
    // Helper method to check if text contains any weather-related keywords
    // Uses case-insensitive comparison for better matching
    private static bool ContainsWeatherKeyword(string text, string[] keywords)
    {
        if (string.IsNullOrEmpty(text)) return false;
        
        return keywords.Any(keyword => 
            text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
