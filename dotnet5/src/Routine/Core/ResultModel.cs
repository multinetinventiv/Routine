using System.Collections.Generic;

namespace Routine.Core
{
	public class ResultModel
	{
		public string ViewModelId { get; set; }
		public bool IsList { get; set; }
		public bool IsVoid { get; set; }

		public ResultModel()
			: this(new Dictionary<string, object>
			{
				{"ViewModelId", null},
				{"IsList", false},
				{"IsVoid", false}
			}) { }
		public ResultModel(IDictionary<string, object> model)
		{
			ViewModelId = (string)model["ViewModelId"];
			IsList = (bool)model["IsList"];
			IsVoid = (bool)model["IsVoid"];
		}

		#region ToString & Equality

		public override string ToString()
		{
			return string.Format("[ResultModel: [ViewModelId: {0}, IsList: {1}, IsVoid: {2}]]", ViewModelId, IsList, IsVoid);
		}

		protected bool Equals(ResultModel other)
		{
			return string.Equals(ViewModelId, other.ViewModelId) && IsList == other.IsList && IsVoid == other.IsVoid;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return Equals((ResultModel)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (ViewModelId != null ? ViewModelId.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ IsList.GetHashCode();
				hashCode = (hashCode * 397) ^ IsVoid.GetHashCode();
				return hashCode;
			}
		}

		#endregion
	}
}
