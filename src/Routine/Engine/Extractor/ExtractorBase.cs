﻿namespace Routine.Engine.Extractor
{
	public abstract class ExtractorBase : IIdExtractor, IValueExtractor
	{
		protected abstract string Extract(object obj);

		#region IIdExtractor implementation

		string IValueExtractor.GetValue(object obj) { return Extract(obj); }

		#endregion

		#region IValueExtractor implementation

		string IIdExtractor.GetId(object obj) { return Extract(obj); }

		#endregion
	}
}