namespace AggregationTask.API.Models.News;
/// <summary>
/// Represents news data retrieved for a specified category or time range.
/// </summary>
public class NewsData
{
    /// <summary>
    /// The total number of news articles found.
    /// </summary>
    public int TotalResults { get; set; }

    /// <summary>
    /// A list of news articles.
    /// </summary>
    public List<NewsArticleDto> Articles { get; set; }
}

/// <summary>
/// Represents a single news article.
/// </summary>
public class NewsArticleDto
{
    /// <summary>
    /// The name of the source that published the article (e.g., "BBC News").
    /// </summary>
    public string SourceName { get; set; }

    /// <summary>
    /// The author of the article, if available.
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// The title of the article.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// A brief description or summary of the article.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The URL of the article.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The URL of the image associated with the article, if available.
    /// </summary>
    public string UrlToImage { get; set; }

    /// <summary>
    /// The date and time the article was published.
    /// </summary>
    public DateTime PublishedAt { get; set; }

    /// <summary>
    /// The main content of the article.
    /// </summary>
    public string Content { get; set; }
}