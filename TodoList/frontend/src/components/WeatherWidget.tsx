import React, { useState, useEffect } from 'react';
import { WeatherForecast5Day } from '../types';
import { weatherApi } from '../services/api';

interface WeatherWidgetProps {
  defaultCity?: string;
}

const WeatherWidget: React.FC<WeatherWidgetProps> = ({ defaultCity = 'Chicago' }) => {
  const [forecast, setForecast] = useState<WeatherForecast5Day | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [city, setCity] = useState(defaultCity);

  useEffect(() => {
    const fetchForecast = async () => {
      try {
        setLoading(true);
        setError(null);
        const forecastData = await weatherApi.getForecast(city);
        setForecast(forecastData);
      } catch (err) {
        console.error('Failed to fetch weather forecast:', err);
        setError('Failed to load weather data');
      } finally {
        setLoading(false);
      }
    };

    fetchForecast();
  }, [city]);

  // Get weather icon based on condition code
  const getWeatherIcon = (conditionCode: string): string => {
    // Weather condition codes from wttr.in API
    const code = parseInt(conditionCode);
    
    if (code === 113) return '☀️'; // Sunny
    if (code === 116) return '⛅'; // Partly cloudy
    if (code === 119 || code === 122) return '☁️'; // Cloudy/Overcast
    if (code === 143 || code === 248) return '🌫️'; // Mist/Fog
    if (code >= 176 && code <= 200) return '🌦️'; // Light rain
    if (code >= 227 && code <= 240) return '❄️'; // Snow
    if (code >= 263 && code <= 284) return '🌧️'; // Rain
    if (code >= 293 && code <= 302) return '🌧️'; // Heavy rain
    if (code >= 308 && code <= 317) return '⛈️'; // Thunderstorm
    if (code >= 320 && code <= 377) return '🌨️'; // Snow/Sleet
    if (code >= 386 && code <= 395) return '⛈️'; // Thunderstorm
    
    return '🌤️'; // Default
  };

  // Format date for display
  const formatDate = (dateStr: string): string => {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-US', { 
      weekday: 'short', 
      month: 'short', 
      day: 'numeric' 
    });
  };

  // Get day name for display
  const getDayName = (dateStr: string): string => {
    const date = new Date(dateStr);
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(today.getDate() + 1);
    
    if (date.toDateString() === today.toDateString()) return 'Today';
    if (date.toDateString() === tomorrow.toDateString()) return 'Tomorrow';
    
    return date.toLocaleDateString('en-US', { weekday: 'short' });
  };

  if (loading) {
    return (
      <div className="bg-gradient-to-r from-blue-400 to-blue-600 text-white p-4 rounded-lg shadow-lg mb-6">
        <div className="animate-pulse">
          <div className="flex items-center justify-between mb-4">
            <div className="h-6 bg-blue-300 rounded w-32"></div>
            <div className="h-6 bg-blue-300 rounded w-20"></div>
          </div>
          <div className="grid grid-cols-5 gap-2">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="bg-blue-300 rounded p-3 h-24"></div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-6">
        <div className="flex items-center">
          <span className="mr-2">⚠️</span>
          <span>{error}</span>
          <button 
            onClick={() => window.location.reload()}
            className="ml-auto text-red-600 hover:text-red-800 underline"
          >
            Retry
          </button>
        </div>
      </div>
    );
  }

  if (!forecast || !forecast.days.length) {
    return null;
  }

  return (
    <div className="bg-gradient-to-r from-blue-400 to-blue-600 text-white p-6 rounded-lg shadow-lg mb-6">
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-xl font-semibold flex items-center">
            🌤️ 5-Day Weather Forecast
          </h2>
          <p className="text-blue-100 text-sm">{forecast.location}</p>
        </div>
        <div className="text-right">
          <input
            type="text"
            value={city}
            onChange={(e) => setCity(e.target.value)}
            onKeyPress={(e) => {
              if (e.key === 'Enter') {
                // City will update via useEffect
              }
            }}
            className="bg-white/20 text-white placeholder-blue-200 border border-white/30 rounded px-3 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-white/50"
            placeholder="Enter city..."
          />
        </div>
      </div>

      <div className="grid grid-cols-2 sm:grid-cols-5 gap-3">
        {forecast.days.map((day, index) => (
          <div 
            key={`${day.date}-${index}`}
            className="bg-white/20 backdrop-blur-sm rounded-lg p-3 text-center hover:bg-white/30 transition-colors"
          >
            <div className="font-medium text-sm mb-1">
              {getDayName(day.date)}
            </div>
            <div className="text-xs text-blue-100 mb-2">
              {formatDate(day.date)}
            </div>
            <div className="text-2xl mb-2">
              {getWeatherIcon(day.conditionCode)}
            </div>
            <div className="text-xs font-medium mb-1">
              {day.maxTemperature}° / {day.minTemperature}°
            </div>
            <div className="text-xs text-blue-100 mb-1 leading-tight">
              {day.description}
            </div>
            <div className="text-xs text-blue-100 space-y-0.5">
              <div>💧 {day.chanceOfRain}%</div>
              <div>💨 {day.windSpeedKmh} km/h</div>
            </div>
          </div>
        ))}
      </div>

      <div className="text-xs text-blue-100 text-center mt-3">
        Last updated: {new Date(forecast.retrievedAt).toLocaleString()}
      </div>
    </div>
  );
};

export default WeatherWidget;