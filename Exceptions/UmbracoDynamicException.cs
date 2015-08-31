using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoDynamicDoc.Exceptions
{
	[Serializable]
	public class UmbracoDynamicDocException : Exception
	{
		public UmbracoDynamicDocException() { }
		public UmbracoDynamicDocException(string message) : base(message) { }
		public UmbracoDynamicDocException(string message, Exception inner) : base(message, inner) { }
		protected UmbracoDynamicDocException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
