using System;
using System.Collections.Generic;

namespace Routine.Engine.Locator
{
	public abstract class LocatorBase<TConcrete> : ILocator
		where TConcrete : LocatorBase<TConcrete>
	{
		private bool acceptNullResult;

		protected LocatorBase()
		{
			AcceptNullResult(true);
		}

		public TConcrete AcceptNullResult(bool acceptNullResult) { this.acceptNullResult = acceptNullResult; return (TConcrete)this; }

		private List<object> LocateInner(IType type, List<string> ids)
		{
			var result = Locate(type, ids) ?? new List<object>();

			if (!acceptNullResult && result.Contains(null))
			{
				throw new CannotLocateException(type, ids);
			}

			if (result.Count != ids.Count)
			{
				throw new CannotLocateException(type, ids,
					new InvalidOperationException(
						string.Format("Locate result count ({0}) cannot be different than id count ({1})",
							result.Count,
							ids.Count))
				);
			}

			return result;
		}

		protected abstract List<object> Locate(IType type, List<string> ids);

		#region ILocator implementation

		List<object> ILocator.Locate(IType type, List<string> ids) { return LocateInner(type, ids); }

		#endregion
	}

}
