﻿using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Routine.Core.Rest
{
	public class RestRequest
	{
		public static readonly RestRequest Empty = new RestRequest(string.Empty);

		public string Body { get; private set; }
		public Dictionary<string, string> Headers { get; private set; }
		public Dictionary<string, string> UrlParameters { get; private set; }

		public RestRequest(string body)
		{
			Body = body;
			Headers = new Dictionary<string, string>();
			UrlParameters = new Dictionary<string, string>();
		}

		public RestRequest WithHeaders(IDictionary<string, string> headers)
		{
			foreach (var key in headers.Keys)
			{
				Headers.Add(key, headers[key]);
			}

			return this;
		}

		public RestRequest WithUrlParameters(IDictionary<string, string> urlParameters)
		{
			foreach (var key in urlParameters.Keys)
			{
				UrlParameters.Add(key, urlParameters[key]);
			}

			return this;
		}

		public string BuildUrlParameters()
		{
			return string.Join("&",
				UrlParameters.Select(kvp => string.Format("{0}={1}", kvp.Key, HttpUtility.UrlEncode(kvp.Value)))
			);
		}

		#region ToString & Equality

		public override string ToString()
		{
			return string.Format("Body: {0}, Headers: {1}, UrlParameters: {2}", Body, Headers.ToKeyValueString(), UrlParameters.ToKeyValueString());
		}

		protected bool Equals(RestRequest other)
		{
			return string.Equals(Body, other.Body) && Headers.KeyValueEquals(other.Headers) && UrlParameters.KeyValueEquals(other.UrlParameters);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((RestRequest)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Body != null ? Body.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Headers != null ? Headers.GetKeyValueHashCode() : 0);
				hashCode = (hashCode * 397) ^ (UrlParameters != null ? UrlParameters.GetKeyValueHashCode() : 0);
				return hashCode;
			}
		}

		#endregion
	}
}