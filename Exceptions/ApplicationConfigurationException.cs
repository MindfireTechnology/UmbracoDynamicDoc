using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoDynamicDoc.Exceptions
{
	[Serializable]
	public class ApplicationConfigurationException : UmbracoDynamicDocException
	{
		public ApplicationConfigurationException() { }
		public ApplicationConfigurationException(string message) : base(message) { }
		public ApplicationConfigurationException(string message, Exception inner) : base(message, inner) { }
		protected ApplicationConfigurationException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
