using System.ComponentModel.DataAnnotations;

namespace TodoListApi.Models;

public class Todo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public Priority Priority { get; set; } = Priority.Medium;
    
    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;
    
    public bool IsCompleted { get; set; } = false;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? DueDate { get; set; }

    public List<string> Tags { get; set; } = new();
    
    [StringLength(100)]
    public string? AssignedTo { get; set; } // New property for user assignment
}

public enum Priority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

// Weather-related models for external API integration
// These models represent the data we'll get from the wttr.in weather API
public class WeatherInfo
{
    // The city/location for which we retrieved weather data
    public string Location { get; set; } = string.Empty;
    
    // Current temperature in Celsius
    public int Temperature { get; set; }
    
    // Weather description (e.g., "Sunny", "Partly cloudy", "Light rain")
    public string Description { get; set; } = string.Empty;
    
    // When this weather data was retrieved - important for caching decisions
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
    
    // Weather condition code from the API - useful for categorizing weather
    public string ConditionCode { get; set; } = string.Empty;
    
    // Humidity percentage (0-100)
    public int Humidity { get; set; }
    
    // Wind speed in km/h
    public int WindSpeedKmh { get; set; }
}

// Represents a single day's forecast information
public class WeatherForecastDay
{
    // The date for this forecast
    public DateTime Date { get; set; }
    
    // Maximum temperature for the day in Celsius
    public int MaxTemperature { get; set; }
    
    // Minimum temperature for the day in Celsius
    public int MinTemperature { get; set; }
    
    // Weather description for the day
    public string Description { get; set; } = string.Empty;
    
    // Weather condition code
    public string ConditionCode { get; set; } = string.Empty;
    
    // Humidity percentage
    public int Humidity { get; set; }
    
    // Wind speed in km/h
    public int WindSpeedKmh { get; set; }
    
    // Probability of precipitation (0-100%)
    public int ChanceOfRain { get; set; }
}

// Container for 5-day weather forecast
public class WeatherForecast5Day
{
    // The city/location for this forecast
    public string Location { get; set; } = string.Empty;
    
    // Array of forecast days (typically 5 days)
    public List<WeatherForecastDay> Days { get; set; } = new();
    
    // When this forecast data was retrieved
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
}

// This model represents the raw JSON response from wttr.in API
// We'll deserialize the API response into this class first, then map to WeatherInfo
public class WttrApiResponse
{
    // The API returns current weather conditions in this property
    public CurrentCondition[]? Current_condition { get; set; }
    
    // The API also returns weather forecast, but we'll focus on current conditions
    public WeatherForecast[]? Weather { get; set; }
    
    // Nearest area information (contains the actual location name)
    public NearestArea[]? Nearest_area { get; set; }
}

// Represents current weather conditions from the API
public class CurrentCondition
{
    public string Temp_C { get; set; } = "0";           // Temperature in Celsius
    public string Humidity { get; set; } = "0";         // Humidity percentage  
    public string WindspeedKmph { get; set; } = "0";    // Wind speed in km/h
    public WeatherDesc[]? WeatherDesc { get; set; }     // Weather description array
    public string WeatherCode { get; set; } = "0";      // Weather condition code
}

// Weather description object from the API
public class WeatherDesc
{
    public string Value { get; set; } = string.Empty;   // The actual description text
}

// Forecast data structure from wttr.in API
public class WeatherForecast
{
    public string Date { get; set; } = string.Empty;
    public string MaxtempC { get; set; } = "0";
    public string MintempC { get; set; } = "0";
    public HourlyData[]? Hourly { get; set; }
}

// Hourly weather data within a forecast day
public class HourlyData
{
    public string Humidity { get; set; } = "0";
    public string WindspeedKmph { get; set; } = "0";
    public string ChanceOfRain { get; set; } = "0";
    public WeatherDesc[]? WeatherDesc { get; set; }
    public string WeatherCode { get; set; } = "0";
}

// Contains location information from the API response
public class NearestArea
{
    public AreaName[]? AreaName { get; set; }           // Array containing location names
    public CountryName[]? Country { get; set; }         // Array containing country information
}

// Country name object (similar to AreaName)
public class CountryName
{
    public string Value { get; set; } = string.Empty;   // The country name
}

// Location name object
public class AreaName
{
    public string Value { get; set; } = string.Empty;   // The city/area name
}

public class Category
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(7)]
    public string Color { get; set; } = "#3b82f6";
    
    public int TodoCount { get; set; } = 0;
}

public class TodoStats
{
    public int TotalTodos { get; set; }
    public int CompletedTodos { get; set; }
    public int PendingTodos { get; set; }
    public int TotalCategories { get; set; }
    public Dictionary<string, int> TodosByCategory { get; set; } = new();
    public Dictionary<string, int> TodosByPriority { get; set; } = new();
    public int OverdueTodos { get; set; }
}
