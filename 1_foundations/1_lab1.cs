using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Lab1
{
    static async Task<string> CallChatCompletion(HttpClient client, string apiKey, string baseUrl, string model, object messages)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        var payload = new { model, messages };
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task Main(string[] args)
    {
        string openaiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        string openrouterApiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

        if (!string.IsNullOrEmpty(openrouterApiKey))
            Console.WriteLine($"OpenRouter API Key exists and begins {openrouterApiKey.Substring(0, Math.Min(8, openrouterApiKey.Length))}");
        else
            Console.WriteLine("OpenRouter API Key not set - please head to the troubleshooting guide in the setup folder");

        if (!string.IsNullOrEmpty(openaiApiKey))
            Console.WriteLine($"OpenAI API Key exists and begins {openaiApiKey.Substring(0, Math.Min(8, openaiApiKey.Length))}");
        else
            Console.WriteLine("OpenAI API Key not set - please head to the troubleshooting guide in the setup folder");

        if (string.IsNullOrEmpty(openaiApiKey)) return;

        var client = new HttpClient();

        var messages = new[] { new { role = "user", content = "What is 2+2?" } };
        try
        {
            string response = await CallChatCompletion(client, openaiApiKey, "https://api.openai.com/v1", "gpt-4.1-nano", messages);
            using var doc = JsonDocument.Parse(response);
            Console.WriteLine(doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred with OpenAI: {ex.Message}");
        }

        if (!string.IsNullOrEmpty(openrouterApiKey))
        {
            try
            {
                string response = await CallChatCompletion(client, openrouterApiKey, "https://api.openrouter.ai/v1", "deepseek/deepseek-r1-0528:free", messages);
                using var doc = JsonDocument.Parse(response);
                Console.WriteLine(doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred with OpenRouter: {ex.Message}");
            }
        }

        string questionPrompt = "Please propose a hard, challenging question to assess someone's IQ. Respond only with the question.";
        messages = new[] { new { role = "user", content = questionPrompt } };
        string question;
        try
        {
            string response = await CallChatCompletion(client, openaiApiKey, "https://api.openai.com/v1", "gpt-4.1-mini", messages);
            using var doc = JsonDocument.Parse(response);
            question = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            Console.WriteLine(question);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating question: {ex.Message}");
            return;
        }

        messages = new[] { new { role = "user", content = question } };
        try
        {
            string response = await CallChatCompletion(client, openaiApiKey, "https://api.openai.com/v1", "gpt-4.1-mini", messages);
            using var doc = JsonDocument.Parse(response);
            string answer = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            Console.WriteLine(answer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error answering question: {ex.Message}");
        }
    }
}
