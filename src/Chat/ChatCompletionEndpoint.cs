﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace OpenAI
{
	/// <summary>
	/// Text generation is the core function of the API. You give the API a prompt, and it generates a ChatCompletion. The way you “program” the API to do a task is by simply describing the task in plain english or providing a few written examples. This simple approach works for a wide range of use cases, including summarization, translation, grammar correction, question answering, chatbots, composing emails, and much more (see the prompt library for inspiration).
	/// </summary>
	public class ChatCompletionEndpoint
	{
		private OpenAIAPI Api;

		/// <summary>
		/// This allows you to set default parameters for every request, for example to set a default temperature or max tokens.  For every request, if you do not have a parameter set on the request but do have it set here as a default, the request will automatically pick up the default value.
		/// </summary>
		public ChatCompletionRequest DefaultChatCompletionRequestArgs { get; set; } = new ChatCompletionRequest();

		/// <summary>
		/// Constructor of the api endpoint.  Rather than instantiating this yourself, access it through an instance of <see cref="OpenAIAPI"/> as <see cref="OpenAIAPI.ChatCompletions"/>.
		/// </summary>
		/// <param name="api"></param>
		internal ChatCompletionEndpoint(OpenAIAPI api)
		{
			this.Api = api;
		}

		#region Non-streaming

		/// <summary>
		/// Ask the API to complete the prompt(s) using the specified request.  This is non-streaming, so it will wait until the API returns the full result.
		/// </summary>
		/// <param name="request">The request to send to the API.  This does not fall back to default values specified in <see cref="DefaultChatCompletionRequestArgs"/>.</param>
		/// <returns>Asynchronously returns the ChatCompletion result.  Look in its <see cref="ChatCompletionResult.Choices"/> property for the ChatCompletions.</returns>
		public async Task<ChatCompletionResult> CreateChatCompletionAsync(ChatCompletionRequest request)
		{
			if (Api.Auth?.ApiKey is null)
			{
				throw new AuthenticationException("You must provide API authentication.  Please refer to https://github.com/glienard/OpenAI.Net#authentication for details.");
			}
            if (!Api.UsingEngine.EngineName.StartsWith("gpt-"))
                throw new NotImplementedException($"{Api.UsingEngine.EngineName} does not implement chat completion. Please refer to https://github.com/glienard/OpenAI.Net#chatgpt for details. ");

			request.Model = Api.UsingEngine.EngineName;
            request.Stream = false;
            var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Api.Auth.ApiKey);
			client.DefaultRequestHeaders.Add("User-Agent", "glienard/openai-dotnet");

            var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
			var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var chatCompletionsEndPoint = $"https://api.openai.com/v1/chat/completions";

            var response = await client.PostAsync(chatCompletionsEndPoint, stringContent);
			if (response.IsSuccessStatusCode)
			{
                var resultAsString = await response.Content.ReadAsStringAsync();

				var res = JsonConvert.DeserializeObject<ChatCompletionResult>(resultAsString);
				try
				{
					res.Organization = response.Headers.GetValues("Openai-Organization").FirstOrDefault();
					res.RequestId = response.Headers.GetValues("X-Request-ID").FirstOrDefault();
					res.ProcessingTime = TimeSpan.FromMilliseconds(int.Parse(response.Headers.GetValues("Openai-Processing-Ms").First()));
				}
				catch (Exception) { }


				return res;
			}

            throw new HttpRequestException("Error calling OpenAi API to get ChatCompletion.  HTTP status code: " + response.StatusCode + ". Request body: " + jsonContent);
        }


		/// <summary>
		/// Ask the API to complete the prompt(s) using the specified request and a requested number of outputs.  This is non-streaming, so it will wait until the API returns the full result.
		/// </summary>
		/// <param name="request">The request to send to the API.  This does not fall back to default values specified in <see cref="DefaultChatCompletionRequestArgs"/>.</param>
		/// <param name="numOutputs">Overrides <see cref="ChatCompletionRequest.NumChoicesPerPrompt"/> as a convenience.</param>
		/// <returns>Asynchronously returns the ChatCompletion result.  Look in its <see cref="ChatCompletionResult.Choices"/> property for the ChatCompletions, which should have a length equal to <paramref name="numOutputs"/>.</returns>
		public Task<ChatCompletionResult> CreateChatCompletionsAsync(ChatCompletionRequest request, int numOutputs = 5)
		{
			request.NumChoicesPerPrompt = numOutputs;
			return CreateChatCompletionAsync(request);
		}

        /// <summary>
        /// Ask the API to complete the prompt(s) using the specified parameters.  This is non-streaming, so it will wait until the API returns the full result.  Any non-specified parameters will fall back to default values specified in <see cref="DefaultChatCompletionRequestArgs"/> if present.
        /// </summary>
        /// <param name="prompt">The prompt to generate from</param>
        /// <param name="max_tokens">How many tokens to complete to. Can return fewer if a stop sequence is hit.</param>
        /// <param name="temperature">What sampling temperature to use. Higher values means the model will take more risks. Try 0.9 for more creative applications, and 0 (argmax sampling) for ones with a well-defined answer. It is generally recommend to use this or <paramref name="top_p"/> but not both.</param>
        /// <param name="top_p">An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered. It is generally recommend to use this or <paramref name="temperature"/> but not both.</param>
        /// <param name="numOutputs">How many different choices to request for each prompt.</param>
        /// <param name="presencePenalty">The scale of the penalty applied if a token is already present at all.  Should generally be between 0 and 1, although negative numbers are allowed to encourage token reuse.</param>
        /// <param name="frequencyPenalty">The scale of the penalty for how often a token is used.  Should generally be between 0 and 1, although negative numbers are allowed to encourage token reuse.</param>
        /// <param name="logProbs">Include the log probabilities on the logprobs most likely tokens, which can be found in <see cref="ChatCompletionResult.Choices"/> -> <see cref="Choice.Logprobs"/>. So for example, if logprobs is 10, the API will return a list of the 10 most likely tokens. If logprobs is supplied, the API will always return the logprob of the sampled token, so there may be up to logprobs+1 elements in the response.</param>
        /// <param name="echo">Echo back the prompt in addition to the ChatCompletion.</param>
        /// <param name="user">A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.</param>
        /// <param name="stopSequences">One or more sequences where the API will stop generating further tokens. The returned text will not contain the stop sequence.</param>
        /// <returns>Asynchronously returns the ChatCompletion result.  Look in its <see cref="ChatCompletionResult.Choices"/> property for the ChatCompletions.</returns>
        public Task<ChatCompletionResult> CreateChatCompletionAsync(string prompt,
			int? max_tokens = null,
			double? temperature = null,
			double? top_p = null,
			int? numOutputs = null,
			double? presencePenalty = null,
			double? frequencyPenalty = null,
			int? logProbs = null,
			bool? echo = null,
			string? user = null,
			params string[] stopSequences
			)
		{
			var request = new ChatCompletionRequest(DefaultChatCompletionRequestArgs)
			{
				Message = prompt,
				MaxTokens = max_tokens ?? DefaultChatCompletionRequestArgs.MaxTokens,
				Temperature = temperature ?? DefaultChatCompletionRequestArgs.Temperature,
				TopP = top_p ?? DefaultChatCompletionRequestArgs.TopP,
				NumChoicesPerPrompt = numOutputs ?? DefaultChatCompletionRequestArgs.NumChoicesPerPrompt,
				PresencePenalty = presencePenalty ?? DefaultChatCompletionRequestArgs.PresencePenalty,
				FrequencyPenalty = frequencyPenalty ?? DefaultChatCompletionRequestArgs.FrequencyPenalty,
				Logprobs = logProbs ?? DefaultChatCompletionRequestArgs.Logprobs,
				Echo = echo ?? DefaultChatCompletionRequestArgs.Echo,
				User = user ?? DefaultChatCompletionRequestArgs.User,
				MultipleStopSequences = stopSequences ?? DefaultChatCompletionRequestArgs.MultipleStopSequences
			};
			return CreateChatCompletionAsync(request);
		}

        /// <summary>
        /// Ask the API to complete the prompt(s) using the specified parameters.  This is non-streaming, so it will wait until the API returns the full result.  Any non-specified parameters will fall back to default values specified in <see cref="DefaultChatCompletionRequestArgs"/> if present.
        /// </summary>
        /// <param name="prompt">The prompt to generate from</param>
        /// <param name="max_tokens">How many tokens to complete to. Can return fewer if a stop sequence is hit.</param>
        /// <param name="temperature">What sampling temperature to use. Higher values means the model will take more risks. Try 0.9 for more creative applications, and 0 (argmax sampling) for ones with a well-defined answer. It is generally recommend to use this or <paramref name="top_p"/> but not both.</param>
        /// <param name="best_of">How many different ChatCompletions to generate server-side before returning the "best" (the one with the highest log probability per token).</param>
        /// <param name="top_p">An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered. It is generally recommend to use this or <paramref name="temperature"/> but not both.</param>
        /// <param name="numOutputs">How many different choices to request for each prompt.</param>
        /// <param name="presencePenalty">The scale of the penalty applied if a token is already present at all.  Should generally be between 0 and 1, although negative numbers are allowed to encourage token reuse.</param>
        /// <param name="frequencyPenalty">The scale of the penalty for how often a token is used.  Should generally be between 0 and 1, although negative numbers are allowed to encourage token reuse.</param>
        /// <param name="logProbs">Include the log probabilities on the logprobs most likely tokens, which can be found in <see cref="ChatCompletionResult.Choices"/> -> <see cref="Choice.Logprobs"/>. So for example, if logprobs is 10, the API will return a list of the 10 most likely tokens. If logprobs is supplied, the API will always return the logprob of the sampled token, so there may be up to logprobs+1 elements in the response.</param>
        /// <param name="echo">Echo back the prompt in addition to the ChatCompletion.</param>
        /// <param name="user">A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.</param>
        /// <param name="stopSequences">One or more sequences where the API will stop generating further tokens. The returned text will not contain the stop sequence.</param>
        /// <returns>Asynchronously returns the ChatCompletion result.  Look in its <see cref="ChatCompletionResult.Choices"/> property for the ChatCompletions.</returns>
        public Task<ChatCompletionResult> CreateChatCompletionAsync(string prompt,
			int? max_tokens = null,
			double? temperature = null,
			int? best_of = null,
			double? top_p = null,
			int? numOutputs = null,
			double? presencePenalty = null,
			double? frequencyPenalty = null,
			int? logProbs = null,
			bool? echo = null,
			string? user = null,
			params string[] stopSequences
			)
		{
			var request = new ChatCompletionRequest(DefaultChatCompletionRequestArgs)
			{
				Message = prompt,
				MaxTokens = max_tokens ?? DefaultChatCompletionRequestArgs.MaxTokens,
				Temperature = temperature ?? DefaultChatCompletionRequestArgs.Temperature,
				BestOf = best_of ?? DefaultChatCompletionRequestArgs.BestOf,
				TopP = top_p ?? DefaultChatCompletionRequestArgs.TopP,
				NumChoicesPerPrompt = numOutputs ?? DefaultChatCompletionRequestArgs.NumChoicesPerPrompt,
				PresencePenalty = presencePenalty ?? DefaultChatCompletionRequestArgs.PresencePenalty,
				FrequencyPenalty = frequencyPenalty ?? DefaultChatCompletionRequestArgs.FrequencyPenalty,
				Logprobs = logProbs ?? DefaultChatCompletionRequestArgs.Logprobs,
				Echo = echo ?? DefaultChatCompletionRequestArgs.Echo,
				User = user ?? DefaultChatCompletionRequestArgs.User,
				MultipleStopSequences = stopSequences ?? DefaultChatCompletionRequestArgs.MultipleStopSequences
			};
			return CreateChatCompletionAsync(request);
		}

		/// <summary>
		/// Ask the API to complete the prompt(s) using the specified promptes, with other paramets being drawn from default values specified in <see cref="DefaultChatCompletionRequestArgs"/> if present.  This is non-streaming, so it will wait until the API returns the full result.
		/// </summary>
		/// <param name="prompts">One or more prompts to generate from</param>
		/// <returns></returns>
		public Task<ChatCompletionResult> CreateChatCompletionAsync(List<ChatMessage> prompts)
		{
            var request = new ChatCompletionRequest(DefaultChatCompletionRequestArgs)
			{
				MultipleMessages = prompts
			};
			return CreateChatCompletionAsync(request);
		}

		#endregion

		#region Streaming

		/// <summary>
		/// Ask the API to complete the prompt(s) using the specified request, and stream the results to the <paramref name="resultHandler"/> as they come in.
		/// If you are on the latest C# supporting async enumerables, you may prefer the cleaner syntax of <see cref="StreamChatCompletionEnumerableAsync(ChatCompletionRequest)"/> instead.
		/// </summary>
		/// <param name="request">The request to send to the API.  This does not fall back to default values specified in <see cref="DefaultChatCompletionRequestArgs"/>.</param>
		/// <param name="resultHandler">An action to be called as each new result arrives, which includes the index of the result in the overall result set.</param>
		public async Task StreamChatCompletionAsync(ChatCompletionRequest request, Action<int, ChatCompletionResult> resultHandler)
		{
			if (Api.Auth?.ApiKey is null)
			{
				throw new AuthenticationException("You must provide API authentication.  Please refer to https://github.com/glienard/OpenAI.Net#authentication for details.");
			}

			request = new ChatCompletionRequest(request) { Stream = true };
            var client = new HttpClient();

            var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
			var stringContent = new StringContent(jsonContent, UnicodeEncoding.UTF8, "application/json");

			using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, $"https://api.openai.com/v1/engines/{Api.UsingEngine.EngineName}/ChatCompletions"))
			{
				req.Content = stringContent;
				req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Api.Auth.ApiKey); ;
				req.Headers.Add("User-Agent", "glienard/openai-dotnet");

				var response = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

				if (response.IsSuccessStatusCode)
				{
					int index = 0;

					using (var stream = await response.Content.ReadAsStreamAsync())
					using (StreamReader reader = new StreamReader(stream))
					{
						string line;
						while ((line = await reader.ReadLineAsync()) != null)
						{
							if (line.StartsWith("data: "))
								line = line.Substring("data: ".Length);

							if (line == "[DONE]")
							{
								return;
							}

                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                index++;
                                var res = JsonConvert.DeserializeObject<ChatCompletionResult>(line.Trim());
                                try
                                {
                                    res.Organization = response.Headers.GetValues("Openai-Organization").FirstOrDefault();
                                    res.RequestId = response.Headers.GetValues("X-Request-ID").FirstOrDefault();
                                    res.ProcessingTime = TimeSpan.FromMilliseconds(int.Parse(response.Headers.GetValues("Openai-Processing-Ms").First()));
                                }
                                catch (Exception) { }

                                resultHandler(index, res);
                            }
                        }
					}
				}
				else
				{
					throw new HttpRequestException("Error calling OpenAi API to get ChatCompletion.  HTTP status code: " + response.StatusCode.ToString() + ". Request body: " + jsonContent);
				}
			}
		}

		/// <summary>
		/// Ask the API to complete the prompt(s) using the specified request, and stream the results to the <paramref name="resultHandler"/> as they come in.
		/// If you are on the latest C# supporting async enumerables, you may prefer the cleaner syntax of <see cref="StreamChatCompletionEnumerableAsync(ChatCompletionRequest)"/> instead.
		/// </summary>
		/// <param name="request">The request to send to the API.  This does not fall back to default values specified in <see cref="DefaultChatCompletionRequestArgs"/>.</param>
		/// <param name="resultHandler">An action to be called as each new result arrives.</param>
		public async Task StreamChatCompletionAsync(ChatCompletionRequest request, Action<ChatCompletionResult> resultHandler)
		{
			await StreamChatCompletionAsync(request, (i, res) => resultHandler(res));
		}

		/// <summary>
		/// Ask the API to complete the prompt(s) using the specified request, and stream the results to the <paramref name="resultHandler"/> as they come in.
		/// If you are not using C# 8 supporting async enumerables or if you are using the .NET Framework, you may need to use <see cref="StreamChatCompletionAsync(ChatCompletionRequest, Action{ChatCompletionResult})"/> instead.
		/// </summary>
		/// <param name="request">The request to send to the API.  This does not fall back to default values specified in <see cref="DefaultChatCompletionRequestArgs"/>.</param>
		/// <returns>An async enumerable with each of the results as they come in.  See <seealso cref="https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8#asynchronous-streams"/> for more details on how to consume an async enumerable.</returns>
		public async IAsyncEnumerable<ChatCompletionResult> StreamChatCompletionEnumerableAsync(ChatCompletionRequest request)
		{
			if (Api.Auth?.ApiKey is null)
			{
				throw new AuthenticationException("You must provide API authentication.  Please refer to https://github.com/glienard/OpenAI.Net#authentication for details.");
			}

			request = new ChatCompletionRequest(request) { Stream = true };
            var client = new HttpClient();

            var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
			var stringContent = new StringContent(jsonContent, UnicodeEncoding.UTF8, "application/json");

			using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, $"https://api.openai.com/v1/engines/{Api.UsingEngine.EngineName}/ChatCompletions"))
			{
				req.Content = stringContent;
				req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Api.Auth.ApiKey); ;
				req.Headers.Add("User-Agent", "glienard/openai-dotnet");

				var response = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

				if (response.IsSuccessStatusCode)
				{
					using (var stream = await response.Content.ReadAsStreamAsync())
					using (StreamReader reader = new StreamReader(stream))
					{
						string line;
						while ((line = await reader.ReadLineAsync()) != null)
						{
							if (line.StartsWith("data: "))
								line = line.Substring("data: ".Length);

							if (line == "[DONE]")
							{
								yield break;
							}

                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                var res = JsonConvert.DeserializeObject<ChatCompletionResult>(line.Trim());
                                yield return res;
                            }
                        }
					}
				}
				else
				{
					throw new HttpRequestException("Error calling OpenAi API to get ChatCompletion.  HTTP status code: " + response.StatusCode.ToString() + ". Request body: " + jsonContent);
				}
			}
		}

        /// <summary>
        /// Ask the API to complete the prompt(s) using the specified parameters. 
        /// Any non-specified parameters will fall back to default values specified in <see cref="DefaultChatCompletionRequestArgs"/> if present.
        /// If you are not using C# 8 supporting async enumerables or if you are using the .NET Framework, you may need to use <see cref="StreamChatCompletionAsync(ChatCompletionRequest, Action{ChatCompletionResult})"/> instead.
        /// </summary>
        /// <param name="prompt">The prompt to generate from</param>
        /// <param name="max_tokens">How many tokens to complete to. Can return fewer if a stop sequence is hit.</param>
        /// <param name="temperature">What sampling temperature to use. Higher values means the model will take more risks. Try 0.9 for more creative applications, and 0 (argmax sampling) for ones with a well-defined answer. It is generally recommend to use this or <paramref name="top_p"/> but not both.</param>
        /// <param name="top_p">An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered. It is generally recommend to use this or <paramref name="temperature"/> but not both.</param>
        /// <param name="numOutputs">How many different choices to request for each prompt.</param>
        /// <param name="presencePenalty">The scale of the penalty applied if a token is already present at all.  Should generally be between 0 and 1, although negative numbers are allowed to encourage token reuse.</param>
        /// <param name="frequencyPenalty">The scale of the penalty for how often a token is used.  Should generally be between 0 and 1, although negative numbers are allowed to encourage token reuse.</param>
        /// <param name="logProbs">Include the log probabilities on the logprobs most likely tokens, which can be found in <see cref="ChatCompletionResult.Choices"/> -> <see cref="Choice.Logprobs"/>. So for example, if logprobs is 10, the API will return a list of the 10 most likely tokens. If logprobs is supplied, the API will always return the logprob of the sampled token, so there may be up to logprobs+1 elements in the response.</param>
        /// <param name="echo">Echo back the prompt in addition to the ChatCompletion.</param>
        /// <param name="user">A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.</param>
        /// <param name="stopSequences">One or more sequences where the API will stop generating further tokens. The returned text will not contain the stop sequence.</param>
        /// <returns>An async enumerable with each of the results as they come in.  See <see href="https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8#asynchronous-streams">the C# docs</see> for more details on how to consume an async enumerable.</returns>
        public IAsyncEnumerable<ChatCompletionResult> StreamChatCompletionEnumerableAsync(string prompt,
			int? max_tokens = null,
			double? temperature = null,
			double? top_p = null,
			int? numOutputs = null,
			double? presencePenalty = null,
			double? frequencyPenalty = null,
			int? logProbs = null,
			bool? echo = null,
			string? user = null,
			params string[] stopSequences)
		{
            var request = new ChatCompletionRequest(DefaultChatCompletionRequestArgs)
			{
				Message = prompt,
				MaxTokens = max_tokens ?? DefaultChatCompletionRequestArgs.MaxTokens,
				Temperature = temperature ?? DefaultChatCompletionRequestArgs.Temperature,
				TopP = top_p ?? DefaultChatCompletionRequestArgs.TopP,
				NumChoicesPerPrompt = numOutputs ?? DefaultChatCompletionRequestArgs.NumChoicesPerPrompt,
				PresencePenalty = presencePenalty ?? DefaultChatCompletionRequestArgs.PresencePenalty,
				FrequencyPenalty = frequencyPenalty ?? DefaultChatCompletionRequestArgs.FrequencyPenalty,
				Logprobs = logProbs ?? DefaultChatCompletionRequestArgs.Logprobs,
				Echo = echo ?? DefaultChatCompletionRequestArgs.Echo,
				User = user ?? DefaultChatCompletionRequestArgs.User,
				MultipleStopSequences = stopSequences ?? DefaultChatCompletionRequestArgs.MultipleStopSequences,
				Stream = true
			};
			return StreamChatCompletionEnumerableAsync(request);
		}
		#endregion

		#region Helpers

		/// <summary>
		/// Simply returns a string of the prompt followed by the best ChatCompletion
		/// </summary>
		/// <param name="request">The request to send to the API.  This does not fall back to default values specified in <see cref="DefaultChatCompletionRequestArgs"/>.</param>
		/// <returns>A string of the prompt followed by the best ChatCompletion</returns>
		public async Task<string> CreateAndFormatChatCompletion(ChatCompletionRequest request)
		{
            var prompt = request.Message;
			var result = await CreateChatCompletionAsync(request);
			return prompt + result;
		}

		#endregion
	}
}
