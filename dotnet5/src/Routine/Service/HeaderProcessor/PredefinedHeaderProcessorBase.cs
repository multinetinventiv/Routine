﻿using System;
using System.Collections.Generic;

namespace Routine.Service.HeaderProcessor
{
	public abstract class PredefinedHeaderProcessorBase<TConcrete> : IHeaderProcessor
		where TConcrete : PredefinedHeaderProcessorBase<TConcrete>
	{
		private readonly string[] headerKeys;
		private Action<List<string>> processorDelegate;

		protected PredefinedHeaderProcessorBase(params string[] headerKeys)
		{
			this.headerKeys = headerKeys;

			processorDelegate = list => { };
		}

		protected void Process(IDictionary<string, string> responseHeaders)
		{
			var headers = new List<string>();
			foreach (var headerKey in headerKeys)
			{
				string header;
				if (!responseHeaders.TryGetValue(headerKey, out header))
				{
					header = string.Empty;
				}

				headers.Add(header);
			}

			processorDelegate(headers);
		}

		protected TConcrete Do(Action<List<string>> processorDelegate) { this.processorDelegate = processorDelegate; return (TConcrete)this; }

		#region IHeaderProcessor implementation

		void IHeaderProcessor.Process(IDictionary<string, string> responseHeaders) { Process(responseHeaders); }

		#endregion
	}
}