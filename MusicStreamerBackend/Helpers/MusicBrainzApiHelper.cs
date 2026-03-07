using System.Net;

namespace MusicStreamerBackend.Helpers;

public class MusicBrainzApiHelper
{
    /// <summary>
    /// Sends an HTTP request with rate limit handling.
    /// Delays before every request and retries on 503 responses.
    /// By default does not throw on non-success status codes - caller should check response.
    /// </summary>
    public static async Task<HttpResponseMessage> SendWithRateLimitHandlingAsync(
        HttpClient client,
        HttpRequestMessage request,
        int maxRetries = 3,
        int secondsBetweenRequests = 1,
        int waitSeconds = 2,
        bool ensureSuccess = false)
    {
        int attempts = 0;
        while (attempts < maxRetries)
        {
            await Task.Delay(TimeSpan.FromSeconds(secondsBetweenRequests));
            var clonedRequest = await CloneRequestAsync(request);
            var response = await client.SendAsync(clonedRequest);

            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                attempts++;
                Console.WriteLine($"Rate limited. Waiting {waitSeconds}s before retry {attempts}/{maxRetries}...");
                await Task.Delay(TimeSpan.FromSeconds(waitSeconds));
                continue;
            }

            if (ensureSuccess)
                response.EnsureSuccessStatusCode();

            return response;
        }

        throw new Exception($"Request failed after {maxRetries} retries due to rate limiting.");
    }

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