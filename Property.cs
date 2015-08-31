using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoDynamicDoc
{
	public class Property
	{
		public string Name { get; set; }
		public string Alias { get; set; }
		public string Description { get; set; }
		public bool Required { get; set; }
		public string Validation { get; set; }
		public string DataType { get; set; }
		public string Value { get; set; }
	}
}
