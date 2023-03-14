using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OpenAI
{
	/// <summary>
	/// Represents a result from calling the ChatCompletion API
	/// </summary>
	public class ChatCompletionResult
	{
		/// <summary>
		/// The identifier of the result, which may be used during troubleshooting
		/// </summary>
		[JsonProperty("id")]
		public string Id { get; set; }

		/// <summary>
		/// The time when the result was generated in unix epoch format
		/// </summary>
		[JsonProperty("created")]
		public int CreatedUnixTime { get; set; }

		/// The time when the result was generated
		[JsonIgnore]
		public DateTime Created => DateTimeOffset.FromUnixTimeSeconds(CreatedUnixTime).DateTime;

		/// <summary>
		/// Which model was used to generate this result.  Be sure to check <see cref="Engine.ModelRevision"/> for the specific revision.
		/// </summary>
		[JsonProperty("model")]
		public Engine Model { get; set; }

		/// <summary>
		/// The ChatCompletions returned by the API.  Depending on your request, there may be 1 or many choices.
		/// </summary>
		[JsonProperty("choices")]
		public List<Choice> ChatCompletions { get; set; }

		/// <summary>
		/// The server-side processing time as reported by the API.  This can be useful for debugging where a delay occurs.
		/// </summary>
		[JsonIgnore]
		public TimeSpan ProcessingTime { get; set; }

		/// <summary>
		/// The organization associated with the API request, as reported by the API.
		/// </summary>
		[JsonIgnore]
		public string Organization{ get; set; }

		/// <summary>
		/// The request id of this API call, as reported in the response headers.  This may be useful for troubleshooting or when contacting OpenAI support in reference to a specific request.
		/// </summary>
		[JsonIgnore]
		public string RequestId { get; set; }


		/// <summary>
		/// Gets the text of the first ChatCompletion, representing the main result
		/// </summary>
		public override string ToString()
        {
            if (ChatCompletions != null && ChatCompletions.Count > 0)
				return ChatCompletions[0].ToString();
            return $"ChatCompletionResult {Id} has no valid output";
        }
	}

  
}
