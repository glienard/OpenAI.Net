﻿using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace OpenAI
{
    /// <summary>
    /// Given a question, a set of documents, and some examples, the API generates
    /// an answer to the question based on the information in the set of documents.
    /// This is useful for question-answering applications on sources of truth, like company documentation or a knowledge base.
    /// </summary>
    public class AnswerEndpoint
    {
        private OpenAIAPI Api;

        internal AnswerEndpoint(OpenAIAPI api)
        {
            Api = api;
        }

        public async Task<AnswerResult> CreateAnswerAsync(AnswerRequest request)
        {
            if (Api.Auth?.ApiKey is null)
            {
                throw new AuthenticationException("You must provide API authentication.  Please refer to https://github.com/WilliamWelsh/OpenAI.Net#authentication for details.");
            }

            //request.Stream = false;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Api.Auth.ApiKey);
            client.DefaultRequestHeaders.Add("User-Agent", "okgodoit/dotnet_openai_api");

            var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            var stringContent = new StringContent(jsonContent, UnicodeEncoding.UTF8, "application/json");

            var response = await client.PostAsync($"https://api.openai.com/v1/answers", stringContent);
            if (response.IsSuccessStatusCode)
            {
                var resultAsString = await response.Content.ReadAsStringAsync();

                var res = JsonConvert.DeserializeObject<AnswerResult>(resultAsString);
                try
                {
                    //res.Organization = response.Headers.GetValues("Openai-Organization").FirstOrDefault();
                    //res.RequestId = response.Headers.GetValues("X-Request-ID").FirstOrDefault();
                    //res.ProcessingTime = TimeSpan.FromMilliseconds(int.Parse(response.Headers.GetValues("Openai-Processing-Ms").First()));
                }
                catch (Exception) { }


                return res;
            }
            else
            {
                throw new HttpRequestException("Error calling OpenAi API to get completion.  HTTP status code: " + response.StatusCode.ToString() + ". Request body: " + jsonContent);
            }
        }

        public Task<AnswerResult> CreateAnswerAsync(string question, string[][] examples, string examplesContext)
        {
            var request = new AnswerRequest(question, examples, examplesContext);

            return CreateAnswerAsync(request);
        }
    }
}
