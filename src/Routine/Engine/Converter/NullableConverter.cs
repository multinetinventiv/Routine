﻿using System;
using System.Collections.Generic;

namespace Routine.Engine.Converter
{
	public class NullableConverter : ConverterBase<NullableConverter>
	{
		protected override List<IType> GetTargetTypes(IType type)
		{
			if (type == null) { return new List<IType>(); }
			if (!type.IsValueType) { return new List<IType>(); }
			if (type.IsVoid) { return new List<IType>(); }
			if (type.IsGenericType) { return new List<IType>(); }
			if (!(type is TypeInfo)) { return new List<IType>(); }

			var typeInfo = type as TypeInfo;

			return new List<IType> { typeof(Nullable<>).MakeGenericType(typeInfo.GetActualType()).ToTypeInfo() };
		}

		protected override object Convert(object @object, IType from, IType to)
		{
			var targetTypeInfo = (TypeInfo) to;

			return Activator.CreateInstance(targetTypeInfo.GetActualType(), @object);
		}
	}
}