using OpenAI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ListEngines
{
    class Program
    {
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            // Initialize the API
            var api = new OpenAIAPI(apiKeys: "YOUR_API_KEY_HERE");

            var engines = await OpenAI.EnginesEndpoint.GetEnginesAsync(api.Auth);
            foreach (var engine in engines.OrderBy(x=>x.EngineName))
            {
                Console.WriteLine(engine.EngineName);
            }
        }
    }
}
