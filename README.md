<h1 align="center">
	<img src="https://raw.githubusercontent.com/glienard/OpenAI.Net/main/logo.png" alt="OpenAI.Net" />
	<br/>
	OpenAI.Net
</h1>

<h4 align="center">An unofficial .NET API wrapper for OpenAI.</h4>

<div align="center">

[![Discord Bots](https://img.shields.io/nuget/v/OpenAI.NET)](https://www.nuget.org/packages/OpenAI.Net)

</div>  

## Installation
Stable builds are available through [NuGet](https://www.nuget.org/packages/OpenAI.Net).  
```
Install-Package OpenAI.Net
```

## Authentication
There are 3 ways to provide your API keys, in order of precedence:
1.  Pass keys directly to `APIAuthentication(string key)` constructor
2.  Set environment var for OPENAI_KEY
3.  Include a config file in the local directory or in your user directory named `.openai` and containing the line:
```shell
OPENAI_KEY=sk-yourapikey
```

You use the `APIAuthentication` when you initialize the API as shown:
```csharp
var api = new OpenAIAPI("YOUR_API_KEY_HERE");
// or
var api = new OpenAIAPI(new APIAuthentication("sk-yourapikey")); // create object manually
// or
var api = new OpenAIAPI(APIAuthentication LoadFromEnv()); // use env vars
// or
var api = new OpenAIAPI(APIAuthentication LoadFromPath()); // use config file (optionally specify where to look)
// or
var api = new OpenAIAPI(); // uses default, env, or config file
```

## Examples

You can view built examples in the [samples](https://github.com/glienard/OpenAI.Net/tree/main/samples) folder.  These examples include very basic access of endpoints, as well as versions of [OpenAI's example applications](https://platform.openai.com/examples) using this wrapper.  

### ChatGPT
ChatGPT was recently released so it can be used using the API. Note that the only way you can use the ChatGPT engine is by using the Engine ChatGPT (gpt-3.5-turbo) and make use of the ChatCompletions. Given a prompt, the model will return a completion. [View on OpenAI](https://platform.openai.com/docs/guides/chat).  

```csharp
var api = new OpenAIAPI(apiKeys: "YOUR_API_KEY_HERE", engine: Engine.ChatGPT);

var request = new ChatCompletionRequestBuilder()
     .WithPrompt("Where is the Yankee stadium?")
     .WithMaxTokens(50)
     .Build();

var result = await api.ChatCompletions.CreateChatCompletionAsync(request);
Console.WriteLine(result.ToString());
// Should print something like "The Yankee Stadium is located in the Bronx borough of New York City, United States."
```

### Completions
Given a prompt, the model will return one or more predicted completions, and can also return the probabilities of alternative tokens at each position. [View on OpenAI](https://platform.openai.com/docs/api-reference/completions).  

```csharp
var api = new OpenAIAPI(apiKeys: "YOUR_API_KEY_HERE", engine: Engine.Davinci);

var request = new CompletionRequestBuilder()
    .WithPrompt("Once upon a time")
    .WithMaxTokens(5)
    .Build();

var result = await api.Completions.CreateCompletionAsync(request);
Console.WriteLine(result.ToString());
// Should print something like ", there was a girl who"
```

### Searches
Given a query and a set of documents or labels, the model ranks each document based on its semantic similarity to the provided query. [View on OpenAI](https://platform.openai.com/docs/api-reference/searches).  

```csharp
var api = new OpenAIAPI(apiKeys: "YOUR_API_KEY_HERE", engine: Engine.Davinci);

var request = new SearchRequestBuilder()
    .WithQuery("the president")
    .WithDocuments(new List<string>
    {
        "White House",
        "hospital",
        "school"
    })
    .Build();

var result = await api.Search.GetBestMatchAsync(request);
Console.WriteLine(result);
// Should print "White House"
```

### Classifications
Given a query and a set of labeled examples, the model will predict the most likely label for the query. Useful as a drop-in replacement for any ML classification or text-to-label task. [View on OpenAI](https://platform.openai.com/docs/api-reference/classifications).  

```csharp
var api = new OpenAIAPI(apiKeys: "YOUR_API_KEY_HERE", engine: Engine.Davinci);

var request = new ClassificationRequestBuilder()
    .WithExamples(new List<List<string>>
    {
        new List<string> { "A happy moment", "Positive" },
        new List<string> { "I am sad.", "Negative" },
        new List<string> { "I am feeling awesome", "Positive" }
    })
    .WithLabels(new List<string>
    {
        "Positive", "Negative", "Neutral"
    })
    .WithQuery("It is a raining day :(")
    .WithSearchModel(Engine.Ada)
    .WithModel(Engine.Curie)
    .Build();

var result = await api.Classifications.CreateClassificationAsync(request);
Console.WriteLine(result.Label);
// Should print "Negative"
```

### Answers
Given a question, a set of documents, and some examples, the API generates an answer to the question based on the information in the set of documents. This is useful for question-answering applications on sources of truth, like company documentation or a knowledge base. [View on OpenAI](https://platform.openai.com/docs/api-reference/answers).  

```csharp
var api = new OpenAIAPI(apiKeys: "YOUR_API_KEY_HERE", engine: Engine.Davinci);

var request = new AnswerRequestBuilder()
    .WithDocuments(new List<string>
    {
    	"Puppy A is happy.", "Puppy B is sad."
    })
    .WithQuestion("which puppy is happy?")
    .WithSearchModel(Engine.Ada)
    .WithModel(Engine.Curie)
    .WithExamplesContext("In 2017, U.S. life expectancy was 78.6 years.")
    .WithExamples(new List<List<string>>
    {
    	new List<string> { "What is human life expectancy in the United States?", "78 years." }
    })
    .WithMaxTokens(5)
    .WithStop(new List<string>
    {
    	"\n", "<|endoftext|>"
    })
    .Build();

var result = await api.Answers.CreateAnswerAsync(request);
Console.WriteLine(result.Answers[0]);
// Should print something like "puppy A."
```

### Fine-tunes
Coming soon. Feel free to make a pull request.

## Streaming
Streaming allows you to get results as they are generated, which can help your application feel more responsive, especially on slow models like Davinci.

Using async iterators:
```csharp
IAsyncEnumerable<CompletionResult> StreamCompletionEnumerableAsync(CompletionRequest request)

// Example
await foreach (var token in api.Completions.StreamCompletionEnumerableAsync(new CompletionRequest("My name is Roger and I am a principal software engineer at Salesforce.  This is my resume:", 200, 0.5, presencePenalty: 0.1, frequencyPenalty: 0.1)))
{
	Console.Write(token);
}
```

Or if using .NET framework or C# <8.0:
```csharp
StreamCompletionAsync(CompletionRequest request, Action<CompletionResult> resultHandler)

// Example
await api.Completions.StreamCompletionAsync(
	new CompletionRequest("My name is Roger and I am a principal software engineer at Salesforce.  This is my resume:", 200, 0.5, presencePenalty: 0.1, frequencyPenalty: 0.1),
	res => ResumeTextbox.Text += res.ToString());
```

### Document Search
The Search API is accessed via `OpenAIAPI.Search`:

You can get all results as a dictionary using
```csharp
GetSearchResultsAsync(SearchRequest request)

// Example
var request = new SearchRequest()
{
	Query = "Washington DC",
	Documents = new List<string> { "Canada", "China", "USA", "Spain" }
};
var result = await api.Search.GetSearchResultsAsync(request);
// result["USA"] == 294.22
// result["Spain"] == 73.81
```

The returned dictionary maps documents to scores.  You can create your `SearchRequest` ahead of time or use one of the helper overloads for convenience, such as
```csharp
GetSearchResultsAsync(string query, params string[] documents)

// Example
var result = await api.Search.GetSearchResultsAsync("Washington DC", "Canada", "China", "USA", "Spain");
```

You can get only the best match using
```csharp
GetBestMatchAsync(request)
```

And if you only want the best match but still want to know the score, use
```csharp
GetBestMatchWithScoreAsync(request)
```
Each of those methods has similar convenience overloads to specify the request inline.

## Documentation

View the documentation on [OpenAI](https://platform.openai.com/docs/introduction/overview). Feel free to add me on Discord Reverse#0069 if you have any questions. Better documentation may come later.

## Credits
- OkGoDoIt - Original [fork](https://github.com/OkGoDoIt/OpenAI-API-dotnet/tree/e07bfe2ddeea40380beb923f36fca9853830d7d7) from December 22, 2020.
- WilliamWelsh [fork](https://github.com/WilliamWelsh/OpenAI.Net)
