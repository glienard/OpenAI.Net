﻿using System.Collections.Generic;

namespace OpenAI
{
    public class ChatCompletionRequestBuilder
    {
        public string Prompt { get; set; }

        public int? MaxTokens { get; set; }

        public double Temperature { get; set; }

        public int? BestOf { get; set; }

        public double? TopP { get; set; }

        public double? PresencePenalty { get; set; }

        public double? FrequencyPenalty { get; set; }

        public int? NumChoicesPerPrompt { get; set; }

        public int? Logprobs { get; set; }

        public bool? Echo { get; set; }

        public string? User { get; set; }

        public List<string>? Stop { get; set; }

        /// <summary>
        /// The prompt(s) to generate ChatCompletions for, encoded as a string, a list of strings, or a list of token lists.
        /// </summary>
        public ChatCompletionRequestBuilder WithPrompt(string prompt)
        {
            Prompt = prompt;
            return this;
        }

        /// <summary>
        /// The maximum number of tokens to generate in the ChatCompletion.
        /// </summary>
        public ChatCompletionRequestBuilder WithMaxTokens(int maxTokens)
        {
            MaxTokens = maxTokens;
            return this;
        }

        /// <summary>
        /// What sampling temperature to use. Higher values means
        /// the model will take more risks. Try 0.9 for more creative applications,
        /// and 0 (argmax sampling) for ones with a well-defined answer. It is generally recommend to use this or <see cref="TopP"/> but not both.
        /// </summary>
        public ChatCompletionRequestBuilder WithTemperature(double temperature)
        {
            Temperature = temperature;
            return this;
        }

        /// <summary>
        /// How many different ChatCompletions to generate server-side before returning the "best" (the one with the highest log probability per token).
        /// </summary>
        public ChatCompletionRequestBuilder WithBestOf(int best_of)
        {
            BestOf = best_of;
            return this;
        }

        /// <summary>
        /// An alternative to sampling with temperature, called nucleus sampling,
        /// where the model considers the results of the tokens with top_p probability mass.
        /// So 0.1 means only the tokens comprising the top 10% probability mass are considered.
        /// It is generally recommend to use this or <see cref="Temperature"/> but not both.
        /// </summary>
        public ChatCompletionRequestBuilder WithTopP(double topP)
        {
            TopP = topP;
            return this;
        }

        /// <summary>
        /// The scale of the penalty applied if a token is already present at all.  Should generally be between 0 and 1, although negative numbers are allowed to encourage token reuse.
        /// </summary>
        public ChatCompletionRequestBuilder WithPresencePenalty(double presencePenalty)
        {
            PresencePenalty = presencePenalty;
            return this;
        }

        /// <summary>
        /// The scale of the penalty for how often a token is used.  Should generally be between 0 and 1, although negative numbers are allowed to encourage token reuse.
        /// </summary>
        public ChatCompletionRequestBuilder WithFrequencyPenalty(double frequencyPenalty)
        {
            FrequencyPenalty = frequencyPenalty;
            return this;
        }

        /// <summary>
        /// How many different choices to request for each prompt.
        /// </summary>
        public ChatCompletionRequestBuilder WithNumChoicesPerPrompt(int n)
        {
            NumChoicesPerPrompt = n;
            return this;
        }

        /// <summary>
        /// Include the log probabilities on the logprobs most likely tokens, which can be found
        /// in <see cref="ChatCompletionResult.Choices"/> -> <see cref="Choice.Logprobs"/>. So for example,
        /// if logprobs is 10, the API will return a list of the 10 most likely tokens. If logprobs is supplied,
        /// the API will always return the logprob of the sampled token, so there may be up to logprobs+1 elements in the response.
        /// </summary>
        public ChatCompletionRequestBuilder WithLogprobs(int logProbs)
        {
            Logprobs = logProbs;
            return this;
        }

        /// <summary>
        /// Echo back the prompt in addition to the ChatCompletion.
        /// </summary>
        public ChatCompletionRequestBuilder WithEcho(bool echo)
        {
            Echo = echo;
            return this;
        }

        /// <summary>
        /// A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse
        /// </summary>
        public ChatCompletionRequestBuilder WithUser(string user)
        {
            User = user;
            return this;
        }

        /// <summary>
        /// Up to 4 sequences where the API will stop generating further tokens. The returned text will not contain the stop sequence.
        /// </summary>
        public ChatCompletionRequestBuilder WithStop(List<string> stop)
        {
            Stop = stop;
            return this;
        }

        /// <summary>
        /// Build into a ChatCompletionRequest
        /// </summary>
        public ChatCompletionRequest Build()
        {
            return new ChatCompletionRequest(prompt: Prompt, max_tokens: MaxTokens, temperature: Temperature, top_p: TopP, presencePenalty: PresencePenalty, frequencyPenalty: FrequencyPenalty, numOutputs: NumChoicesPerPrompt, logProbs: Logprobs, echo: Echo, stop: Stop, best_of: BestOf, user: User);
        }
    }
}
