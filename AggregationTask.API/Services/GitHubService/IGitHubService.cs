using ServiceStack;

namespace AggregationTask.API.Services.GitHubService;
public interface IGitHubService
{
    Task<List<GithubRepo>> GetRepositoriesAsync(string username, string sortBy = null, string filterBy = null);
}
