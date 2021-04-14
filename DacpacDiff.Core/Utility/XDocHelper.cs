using System;
using System.Linq;
using System.Xml.Linq;

namespace DacpacDiff.Core.Utility
{
	internal static class XDocHelper
	{
		public static XElement[] Find(this XElement root, XName elementName, params XName[] attributes)
		{
			return root.Elements(elementName)
				.Where(e => attributes.All(a => e.Attribute(a) != null))
				.ToArray();
		}
		public static XElement[] Find(this XElement root, XName elementName, params (XName name, string value)[] attributes)
			=> Find(root, false, elementName, attributes);
		public static XElement[] Find(this XElement root, XName elementName, params (XName name, Func<string?, bool> pred)[] attributePreds)
			=> Find(root, false, elementName, attributePreds);
		
		public static XElement[] Find(this XElement root, bool deep, XName elementName, params (XName name, string value)[] attributes)
		{
			var els = deep ? root.Descendants(elementName) : root.Elements(elementName);
			return els.Where(e => attributes.All(a => e.Attribute(a.name)?.Value == a.value))
				.ToArray();
		}
		public static XElement[] Find(this XElement root, bool deep, XName elementName, params (XName name, Func<string?, bool> pred)[] attributePreds)
		{
			var els = deep ? root.Descendants(elementName) : root.Elements(elementName);
			return els.Where(e => attributePreds.All(p => p.pred(e.Attribute(p.name)?.Value)))
				.ToArray();
		}
	}
}
