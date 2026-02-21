using System.Net;

namespace MusicStreamerBackend.Helpers;

public static class DiscogsApiHelpers
{
    /// <summary>
    /// Discogs rate limits requests via a sliding window to 60 requests per minute.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request"></param>
    /// <param name="maxRetries"></param>
    /// <param name="waitSeconds"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<HttpResponseMessage> SendWithRateLimitHandlingAsync(
        HttpClient client, 
        HttpRequestMessage request,
        int maxRetries = 3,
        int waitSeconds = 60)
    {
        int attempts = 0;
        var clonedRequest = await CloneRequestAsync(request);
        while (attempts < maxRetries)
        {
            var response = await client.SendAsync(clonedRequest);
            int remainingRequests = 60;
            response.Headers.TryGetValues("X-Discogs-Ratelimit-Remaining", out var remainingRequestsHeaderVal);
            if (remainingRequestsHeaderVal != null)
            {
                int.TryParse(remainingRequestsHeaderVal.FirstOrDefault(), out remainingRequests);    
            }
            
            if (response.StatusCode == HttpStatusCode.TooManyRequests || remainingRequests < 1) // 429
            {
                attempts++;
                Console.WriteLine($"Rate limited. Waiting {waitSeconds}s before retry {attempts}/{maxRetries}...");
                await Task.Delay(TimeSpan.FromSeconds(waitSeconds));
                continue;
            }
            response.EnsureSuccessStatusCode();
            return response;
        }
        throw new Exception($"Request failed after {maxRetries} retries due to rate limiting.");
    }

    // HttpRequestMessage isn't reusable, so you need to clone it
    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content != null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);
            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }
    
}