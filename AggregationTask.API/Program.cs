using AggregationTask.API.Services.GitHubService;
using AggregationTask.API.Services.NewsService;
using AggregationTask.API.Services.Statistics;
using AggregationTask.API.Services.StatisticsService;
using AggregationTask.API.Services.WeatherService;
using ServiceStack;
using System.Net.Http.Headers;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setupAction =>
{
    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

    setupAction.IncludeXmlComments(xmlCommentsFullPath);
});

// Register the Statistics Service
builder.Services.AddSingleton<IStatisticsService, StatisticsService>();


// Register the Services
builder.Services.AddHttpClient<INewsService, NewsService>(client =>
{
    client.BaseAddress = new Uri("https://newsapi.org/v2/");
    client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");// News API requires user-agent
});
builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
{
    client.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/");
});
builder.Services.AddHttpClient<IGitHubService, GitHubService>(client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.UserAgent.TryParseAdd("request"); // GitHub API requires user-agent
});

builder.Services.AddMemoryCache(); // Add in-memory cache

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
