using OpenAI;
using System;
using System.Threading.Tasks;

namespace ChatCompletions
{
    class Program
    {
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            // Initialize the API
            var api = new OpenAIAPI(apiKeys: "YOUR_API_KEY_HERE", engine: Engine.ChatGPT);

            // Set up a search request
            // https://platform.openai.com/docs/api-reference/chat
            var request = new ChatCompletionRequestBuilder()
                .WithPrompt("Where is the Yankee stadium?")
                .WithMaxTokens(50)
                .Build();

            var result = await api.ChatCompletions.CreateChatCompletionAsync(request);

            // Print the result
            Console.WriteLine(result.ToString());

            // Should print something like "The Yankee Stadium is located in the Bronx borough of New York City, United States."
        }
    }
}
