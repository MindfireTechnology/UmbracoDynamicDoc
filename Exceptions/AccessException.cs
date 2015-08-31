using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoDynamicDoc.Exceptions
{
	[Serializable]
	public class AccessException : UmbracoDynamicDocException
	{
		public AccessException() { }
		public AccessException(string message) : base(message) { }
		public AccessException(string message, Exception inner) : base(message, inner) { }
		protected AccessException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
