﻿using System;

namespace Routine.Engine.Virtual
{
	public class ProxyParameter : IParameter
	{
		private readonly IParameter real;
		private readonly int index;

		public IParametric Owner { get; private set; }

		public ProxyParameter(IParameter real, IParametric owner) : this(real, owner, 0) { }
		public ProxyParameter(IParameter real, IParametric owner, int index)
		{
			if (real == null) { throw new ArgumentNullException("real"); }
			if (owner == null) { throw new ArgumentNullException("owner"); }
			if (index < 0) { throw new ArgumentOutOfRangeException("index", index, "'index' cannot be less than zero"); }

			this.real = real;
			this.index = index;

			Owner = owner;
		}

		public object[] GetCustomAttributes() { return real.GetCustomAttributes(); }
		public string Name { get { return real.Name; } }
		public IType ParentType { get { return Owner.ParentType; } }
		public int Index { get { return index; } }
		public IType ParameterType { get { return real.ParameterType; } }
	}
}