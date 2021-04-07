using System;
using System.Collections.Generic;
using System.Linq;

namespace Routine.Engine.Converter
{
	public abstract class ConverterBase<TConcrete> : IConverter
		where TConcrete : ConverterBase<TConcrete>
	{
		private object ConvertInner(object @object, IType from, IType to)
		{
			try
			{
				return Convert(@object, from, to);
			}
			catch (CannotConvertException)
			{
				throw;
			}
			catch (Exception ex)
			{
				if (!GetTargetTypes(from).Any(t => Equals(t, to)))
				{
					throw new CannotConvertException(@object, to, ex);
				}

				throw;
			}
		}

		protected abstract List<IType> GetTargetTypes(IType type);
		protected abstract object Convert(object @object, IType from, IType to);

		#region IConverter implementation

		object IConverter.Convert(object @object, IType from, IType to) { return ConvertInner(@object, from, to); }

		#endregion

		List<IType> IConverter.GetTargetTypes(IType type) { return GetTargetTypes(type); }
	}

}
