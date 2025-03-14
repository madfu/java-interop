using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.SourceWriter;

namespace generator.SourceWriters
{
	public class RegisterAttr : AttributeWriter
	{
		public string Name { get; set; }
		public string Signature { get; set; }
		public string Connector { get; set; }
		public bool DoNotGenerateAcw { get; set; }
		public string AdditionalProperties { get; set; }
		public bool UseGlobal { get; set; }	// TODO: Temporary for matching existing unit tests
		public bool UseShortForm { get; set; }  // TODO: Temporary for matching existing unit tests
		public bool AcwLast { get; set; }       // TODO: Temporary for matching existing unit tests

		public MemberTypes? MemberType { get; set; }

		public RegisterAttr (string name, string signature = null, string connector = null, bool noAcw = false, string additionalProperties = null)
		{
			Name = name;
			Signature = signature;
			Connector = connector;
			DoNotGenerateAcw = noAcw;
			AdditionalProperties = additionalProperties;
		}

		public override void WriteAttribute (CodeWriter writer)
		{
			if (MemberType.HasValue) {
				WriteJavaInterop1Attribute (writer);
				return;
			}
			var sb = new StringBuilder ();

			if (UseGlobal)
				sb.Append ((string) $"[global::Android.Runtime.Register (\"{Name}\"");
			else
				sb.Append ((string) $"[Register (\"{Name}\"");

			if ((Signature.HasValue () || Connector.HasValue ()) && !UseShortForm)
				sb.Append ((string) $", \"{Signature}\", \"{Connector}\"");

			if (DoNotGenerateAcw && !AcwLast)
				sb.Append (", DoNotGenerateAcw=true");

			if (AdditionalProperties.HasValue ())
				sb.Append (AdditionalProperties);

			if (DoNotGenerateAcw && AcwLast)
				sb.Append (", DoNotGenerateAcw=true");

			sb.Append (")]");

			writer.WriteLine (sb.ToString ());
		}

		private void WriteJavaInterop1Attribute (CodeWriter writer)
		{
			switch (MemberType) {
				case MemberTypes.TypeInfo:
					var invokerType = string.IsNullOrEmpty (Connector)
						? ""
						: $", InvokerType=typeof ({Connector.Replace ('/', '.')})";
					writer.WriteLine ($"[global::Java.Interop.JniTypeSignature (\"{Name}\", GenerateJavaPeer={(DoNotGenerateAcw ? "false" : "true")}{invokerType})]");
					break;
				case MemberTypes.Constructor:
					writer.WriteLine ($"[global::Java.Interop.JniConstructorSignature (\"{Signature}\")]");
					break;
				case MemberTypes.Method:
					writer.WriteLine ($"[global::Java.Interop.JniMethodSignature (\"{Name}\", \"{Signature}\")]");
					break;
				default:
					throw new NotImplementedException ($"Don't know how to write attribute for `{MemberType}`");
			}
		}
	}
}
