using UnityEngine;
using System.Collections;

namespace UnityToolbag
{
	/// <summary>
	/// Display a List/Array as a sortable list in the inspector
	/// </summary>
	public class ReorderableAttribute : PropertyAttribute
	{
		public string ElementHeader { get; protected set; }
		public bool HeaderZeroIndex { get; protected set; }

		public ReorderableAttribute()
		{
			ElementHeader = string.Empty;
			HeaderZeroIndex = false;
		}

		public ReorderableAttribute(string headerString, bool isZeroIndex = true)
		{
			ElementHeader = headerString;
			HeaderZeroIndex = isZeroIndex;
		}
	}
}
