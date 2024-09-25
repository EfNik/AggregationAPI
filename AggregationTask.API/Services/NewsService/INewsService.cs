using AggregationTask.API.Models.News;

namespace AggregationTask.API.Services.NewsService;
public interface INewsService
{
    Task<NewsData> GetNewsAsync(string category, DateTime? From);
}
