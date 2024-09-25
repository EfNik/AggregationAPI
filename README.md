# API Aggregation Service

This service aggregates data from multiple external APIs (news, weather, GitHub) and provides a single endpoint to fetch combined data. The service includes caching and request statistics tracking for optimal performance.

## Features
- **Aggregated API:** Fetches news, weather, and GitHub repositories data.
- **Provides Statistics:** Tracks the total number of requests and response times for each external API.
- **Error Handling:** Implements fallback mechanisms if external APIs are unavailable.
- **Caching:** Improves performance by caching responses.

## Setup Instructions

### Prerequisites
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- API keys for:
  - **News API**
  - **Weather API**
  - **GitHub API** (if needed)
  
### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/your-repository-url.git
   ```
2. Navigate to the project directory:
   ```bash
   cd AggregationTask
   ```  
3. Add your API keys to appsettings.json(or use user secrets):
   ```json
   {
    "APIKeys": {
      "News": {
        "Key": "your-news-api-key"
      },
      "Weather": {
        "Key": "your-weather-api-key"
      }
    }
   }
   ```
5. Build and run the project:
   ```bash
   dotnet build
   dotnet run
   ```

## Swagger Documentation
Once the service is running, you can access the detailed API documentation via Swagger:

-  Open your browser and navigate to:
  [swagger](https://localhost:7035/swagger)
- Here you will find a detailed description of the API's endpoints, input parameters, and response formats.

## Usage
### Aggregated Data Endpoint

### GET /api/aggregate

Fetches aggregated data from News, Weather, and GitHub APIs.

### Example request parameters:
- **city**: The city for which to fetch weather data (e.g., London).
- **category**: The category of news to fetch (e.g., technology).
- **githubUser**: The GitHub username to fetch repositories for.

### Example Request:
```bash
   GET https://localhost:7035/api/aggregate?city=London&category=technology&githubUser=EfNik
   ```  

### Example Response:
```json
   {
    "weather": {
      "city": "London",
      "forecasts": [
        {
          "dt": 1695690000,
          "temp": 15.2,
          "feelsLike": 14.5,
          "description": "clear sky",
          "windSpeed": 5.7,
          "cloudiness": 0,
          "humidity": 72
        }
      ]
    },
    "news": {
      "totalResults": 50,
      "articles": [
        {
          "sourceName": "BBC",
          "author": "John Doe",
          "title": "Tech Innovations in 2024",
          "description": "A detailed report on the upcoming tech trends in 2024.",
          "url": "https://bbc.com/tech-innovations-2024",
          "urlToImage": "https://bbc.com/image.jpg",
          "publishedAt": "2024-09-23T14:45:00Z",
          "content": "Technology continues to shape the future..."
        }
      ]
    },
    "repositories": [
      {
        "id": 111111111,
        "name": "my-awesome-repo",
        "description": "A repository of awesome projects.",
        "homepage": "",
        "watchers_Count": 0,
        "stargazers_Count": 0,
        "size": 5018,
        "full_Name": "EfNik/my-awesome-repo",
        "created_At": "2023-12-01T15:53:53Z",
        "updated_At": "2024-01-30T07:55:31Z",
        "has_Downloads": true,
        "fork": false,
        "url": "https://api.github.com/repos/EfNik/my-awesome-repo",
        "html_Url": "https://github.com/EfNik/my-awesome-repo",
        "private": false,
        "parent": null
      }
    ]
  }

  ```

### GET /api/statistics
Retrieves request statistics for external APIs, including total requests and average response times.

- **Fast:** Requests that complete in less than 100ms.
- **Average:** Requests that take between 100ms and 200ms.
- **Slow:** Requests that take more than 200ms.
  
### Example Response:
```bash
   GET https://localhost:7035/api/statistics
   ```  

### Example Request:
```json
   [
    {
      "apiName": "News",
      "totalRequests": 20,
      "averageResponseTime": 150.75,
      "fastRequests": 10,
      "averageRequests": 7,
      "slowRequests": 3
    },
    {
      "apiName": "Weather",
      "totalRequests": 15,
      "averageResponseTime": 120.40,
      "fastRequests": 12,
      "averageRequests": 3,
      "slowRequests": 0
    }
  ]

  ```

## Additional Considerations
### Error Handling
In case any external API is down or returns an error, the service logs the issue and continues with partial data from available APIs. No exceptions are thrown to disrupt the aggregated response.


### Example Response:
News Service is unavailable
```json
   {
    "weather": {
      "city": "London",
      "forecasts": [
        {
          "dt": 1695690000,
          "temp": 15.2,
          "feelsLike": 14.5,
          "description": "clear sky",
          "windSpeed": 5.7,
          "cloudiness": 0,
          "humidity": 72
        }
      ]
    },
    "news": {
        "totalResults": 0,
        "articles": [
            {
                "sourceName": null,
                "author": null,
                "title": "Service unavailable",
                "description": "Unable to fetch news at the moment.",
                "url": null,
                "urlToImage": null,
                "publishedAt": "0001-01-01T00:00:00",
                "content": null
            }
        ]
    },
    "repositories": [
      {
        "id": 111111111,
        "name": "my-awesome-repo",
        "description": "A repository of awesome projects.",
        "homepage": "",
        "watchers_Count": 0,
        "stargazers_Count": 0,
        "size": 5018,
        "full_Name": "EfNik/my-awesome-repo",
        "created_At": "2023-12-01T15:53:53Z",
        "updated_At": "2024-01-30T07:55:31Z",
        "has_Downloads": true,
        "fork": false,
        "url": "https://api.github.com/repos/EfNik/my-awesome-repo",
        "html_Url": "https://github.com/EfNik/my-awesome-repo",
        "private": false,
        "parent": null
      }
    ]
  }

  ```

## Testing
There are unit tests for each of the 3 services (News, Weather, Github) and the Aggregation controller. To run the unit tests:
```bash
   dotnet test
   ```  















