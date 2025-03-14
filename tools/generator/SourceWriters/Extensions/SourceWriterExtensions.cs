using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Java.Interop.Tools.Generator;
using MonoDroid.Generation;
using Xamarin.SourceWriter;
using Xamarin.Android.Binder;

namespace generator.SourceWriters
{
	public static class SourceWriterExtensions
	{
		const int MINIMUM_API_LEVEL = 21;

		public static void AddField (TypeWriter tw, GenBase type, Field field, CodeGenerationOptions opt)
		{
			if (field.NeedsProperty)
				tw.Properties.Add (new BoundFieldAsProperty (type, field, opt));
			else
				tw.Fields.Add (new BoundField (type, field, opt));
		}

		public static bool AddFields (TypeWriter tw, GenBase gen, List<Field> fields, HashSet<string> seen, CodeGenerationOptions opt, CodeGeneratorContext context)
		{
			var needsProperty = false;

			foreach (var f in fields) {
				if (gen.ContainsName (f.Name)) {
					Report.LogCodedWarning (0, GetFieldCollisionMessage (gen, f), f, gen.FullName, f.Name, gen.JavaName);
					continue;
				}

				if (seen != null && seen.Contains (f.Name)) {
					Report.LogCodedWarning (0, Report.WarningDuplicateField, f, gen.FullName, f.Name, gen.JavaName);
					continue;
				}

				if (f.Validate (opt, gen.TypeParameters, context)) {
					if (seen != null)
						seen.Add (f.Name);

					needsProperty = needsProperty || f.NeedsProperty;
					AddField (tw, gen, f, opt);
				}
			}

			return needsProperty;
		}

		public static Report.LocalizedMessage GetFieldCollisionMessage (GenBase gen, Field f)
		{
			if (gen.HasNestedType (f.Name))
				return Report.WarningFieldNameCollision_NestedType;
			if (gen.ContainsProperty (f.Name, false))
				return Report.WarningFieldNameCollision_Property;

			return Report.WarningFieldNameCollision_Method;
		}

		public static void AddInterfaceListenerEventsAndProperties (TypeWriter tw, InterfaceGen iface, ClassGen target, CodeGenerationOptions opt)
		{
			var methods = target.Methods.Concat (target.Properties.Where (p => p.Setter != null).Select (p => p.Setter));
			var props = new HashSet<string> ();
			var refs = new HashSet<string> ();
			var eventMethods = methods.Where (m => m.IsListenerConnector && m.EventName != string.Empty && m.ListenerType == iface).OrderBy (m => m.Parameters.Count).GroupBy (m => m.Name).Select (g => g.First ()).Distinct ();

			foreach (var method in eventMethods) {
				var name = method.CalculateEventName (target.ContainsName);

				if (string.IsNullOrEmpty (name)) {
					Report.LogCodedWarning (0, Report.WarningEmptyEventName, method, iface.FullName, method.Name);
					continue;
				}

				if (opt.GetSafeIdentifier (name) != name) {
					Report.LogCodedWarning (0, Report.WarningInvalidEventName, method, iface.FullName, method.Name);
					continue;
				}

				var prop = target.Properties.FirstOrDefault (p => p.Setter == method);

				if (prop != null) {
					var setter = "__Set" + prop.Name;
					props.Add (prop.Name);
					refs.Add (setter);

					AddInterfaceListenerEventsAndProperties (tw, iface, target, name, setter,
						string.Format ("__v => {0} = __v", prop.Name),
						string.Format ("__v => {0} = null", prop.Name), opt, prop.Getter);
				} else {
					refs.Add (method.Name);
					string rm = null;
					string remove;

					if (method.Name.StartsWith ("Set", StringComparison.Ordinal))
						remove = string.Format ("__v => {0} (null)", method.Name);
					else if (method.Name.StartsWith ("Add", StringComparison.Ordinal) &&
						 (rm = "Remove" + method.Name.Substring ("Add".Length)) != null &&
						 methods.Where (m => m.Name == rm).Any ())
						remove = string.Format ("__v => {0} (__v)", rm);
					else
						remove = string.Format ("__v => {{throw new NotSupportedException (\"Cannot unregister from {0}.{1}\");}}",
							iface.FullName, method.Name);

					AddInterfaceListenerEventsAndProperties (tw, iface, target, name, method.Name,
						method.Name,
						remove, opt, method);
				}
			}

			foreach (var r in refs)
				tw.Fields.Add (new WeakImplementorField (r, opt));

			tw.Methods.Add (new CreateImplementorMethod (iface, opt));
		}

		// Parameter 'setListenerMethod' refers to the method used to set the listener, like 'addOnRoutingChangedListener'/'setOnRoutingChangedListener'.
		// This is used to determine what API level the listener setter is available on.
		public static void AddInterfaceListenerEventsAndProperties (TypeWriter tw, InterfaceGen iface, ClassGen target, string name, string connector_fmt, string add, string remove, CodeGenerationOptions opt, Method setListenerMethod)
		{
			if (!iface.IsValid)
				return;

			foreach (var method in iface.Methods) {
				var nameSpec = iface.Methods.Count > 1 ? method.EventName ?? method.AdjustedName : string.Empty;
				var nameUnique = string.IsNullOrEmpty (nameSpec) ? name : nameSpec;

				if (nameUnique.StartsWith ("On", StringComparison.Ordinal))
					nameUnique = nameUnique.Substring (2);

				if (target.ContainsName (nameUnique))
					nameUnique += "Event";

				AddInterfaceListenerEventOrProperty (tw, iface, method, target, nameUnique, connector_fmt, add, remove, opt, setListenerMethod);
			}
		}

		public static void AddInterfaceListenerEventOrProperty (TypeWriter tw, InterfaceGen iface, Method method, ClassGen target, string name, string connector_fmt, string add, string remove, CodeGenerationOptions opt, Method setListenerMethod)
		{
			if (method.EventName == string.Empty)
				return;

			var nameSpec = iface.Methods.Count > 1 ? method.AdjustedName : string.Empty;
			var idx = iface.FullName.LastIndexOf (".", StringComparison.Ordinal);
			var start = iface.Name.StartsWith ("IOn", StringComparison.Ordinal) ? 3 : 1;
			var full_delegate_name = iface.FullName.Substring (0, idx + 1) + iface.Name.Substring (start, iface.Name.Length - start - 8) + nameSpec;

			if (method.IsSimpleEventHandler)
				full_delegate_name = "EventHandler";
			else if (method.RetVal.IsVoid || method.IsEventHandlerWithHandledProperty)
				full_delegate_name = "EventHandler<" + iface.FullName.Substring (0, idx + 1) + iface.GetArgsName (method) + ">";
			else
				full_delegate_name += "Handler";

			if (method.RetVal.IsVoid || method.IsEventHandlerWithHandledProperty) {
				if (opt.GetSafeIdentifier (name) != name) {
					Report.LogCodedWarning (0, Report.WarningInvalidEventName2, method, iface.FullName, name);
					return;
				} else {
					var mt = target.Methods.Where (method => string.Compare (method.Name, connector_fmt, StringComparison.OrdinalIgnoreCase) == 0 && method.IsListenerConnector).FirstOrDefault ();
					var hasHandlerArgument = mt != null && mt.IsListenerConnector && mt.Parameters.Count == 2 && mt.Parameters [1].Type == "Android.OS.Handler";

					tw.Events.Add (new InterfaceListenerEvent (iface, setListenerMethod, name, nameSpec, full_delegate_name, connector_fmt, add, remove, hasHandlerArgument, opt));
				}
			} else {
				if (opt.GetSafeIdentifier (name) != name) {
					Report.LogCodedWarning (0, Report.WarningInvalidEventPropertyName, method, iface.FullName, name);
					return;
				}

				tw.Properties.Add (new InterfaceListenerPropertyImplementor (iface, name, opt));
				tw.Properties.Add (new InterfaceListenerProperty (iface, name, nameSpec, method.AdjustedName, full_delegate_name, opt));
			}
		}

		public static void AddMethodCustomAttributes (List<AttributeWriter> attributes, Method method)
		{
			if (method.GenericArguments != null && method.GenericArguments.Any ())
				attributes.Add (new CustomAttr (method.GenericArguments.ToGeneratedAttributeString ()));
			if (method.CustomAttributes != null)
				attributes.Add (new CustomAttr (method.CustomAttributes));
			if (method.Annotation != null)
				attributes.Add (new CustomAttr (method.Annotation));
		}

		public static void AddMethodParameters (this ITakeParameters method, ParameterList parameters, CodeGenerationOptions opt)
		{
			foreach (var p in parameters) {
				var para = new MethodParameterWriter (opt.GetSafeIdentifier (p.Name), new TypeReferenceWriter (opt.GetTypeReferenceName (p)));

				if (p.IsEnumified)
					para.Attributes.Add (new GeneratedEnumAttr ());
				if (p.Annotation != null)
					para.Attributes.Add (new CustomAttr (p.Annotation));

				method.Parameters.Add (para);
			}
		}

		// This replaces any `Java.Lang.ICharSequence` parameters with `string`.
		public static void AddMethodParametersStringOverloads (this MethodWriter method, ParameterList parameters, CodeGenerationOptions opt)
		{
			foreach (var p in parameters) {
				var para = new MethodParameterWriter (opt.GetSafeIdentifier (p.Name), new TypeReferenceWriter (opt.GetTypeReferenceName (p).Replace ("Java.Lang.ICharSequence", "string").Replace ("global::string", "string")));

				if (p.IsEnumified)
					para.Attributes.Add (new GeneratedEnumAttr ());
				if (p.Annotation != null)
					para.Attributes.Add (new CustomAttr (p.Annotation));

				method.Parameters.Add (para);
			}
		}

		public static string GetInvokeType (string type)
		{
			switch (type) {
				case "Bool": return "Boolean";
				case "Byte": return "SByte";
				case "Int": return "Int32";
				case "Short": return "Int16";
				case "Long": return "Int64";
				case "Float": return "Single";
				case "UInt": return "Int32";
				case "UShort": return "Int16";
				case "ULong": return "Int64";
				case "UByte": return "SByte";
				default: return type;
			}
		}

		public static void AddMethodBody (List<string> body, Method method, CodeGenerationOptions opt, string members = "_members")
		{
			body.Add ($"const string __id = \"{method.JavaName}.{method.JniSignature}\";");

			foreach (string prep in method.Parameters.GetCallPrep (opt))
				body.Add (prep);

			body.Add ("try {");

			AddMethodBodyTryBlock (body, method, opt, members);

			if (method.IsCompatVirtualMethod) {
				body.Add ("}");
				body.Add ("catch (Java.Lang.NoSuchMethodError) {");
				body.Add ("	throw new Java.Lang.AbstractMethodError (__id);");
			}

			body.Add ("} finally {");

			foreach (string cleanup in method.Parameters.GetCallCleanup (opt))
				body.Add ("\t" + cleanup);

			foreach (var p in method.Parameters.Where (para => para.ShouldGenerateKeepAlive ()))
				body.Add ($"\tglobal::System.GC.KeepAlive ({opt.GetSafeIdentifier (p.Name)});");

			body.Add ("}");
		}

		public static void AddMethodBodyTryBlock (List<string> body, Method method, CodeGenerationOptions opt, string members = "_members")
		{
			AddParameterListCallArgs (body, method.Parameters, opt, false);

			var invokeType = JavaInteropCodeGenerator.GetInvokeType (method.RetVal.CallMethodPrefix);

			var return_var = method.IsVoid ? string.Empty : "var __rm = ";
			var method_type = method.IsStatic ? "StaticMethods" : "InstanceMethods";
			var virt_type = method switch
			{
				{ IsStatic: true } => string.Empty,
				{ IsFinal: true } => "Nonvirtual",
				{ IsVirtual: true, IsAbstract: false } => "Virtual",
				{ IsInterfaceDefaultMethod: true } => "Virtual",
				_ => "Abstract"
			};
			var call_args = method.Parameters.GetCallArgs (opt, invoker: false);
			var this_param = method.IsStatic ? $"__id{call_args}" : $"__id, this{call_args}";

			// Example: var __rm = _members.InstanceMethods.InvokeVirtualObjectMethod (__id, this, __args);
			body.Add ($"\t{return_var}{members}.{method_type}.Invoke{virt_type}{invokeType}Method ({this_param});");

			if (!method.IsVoid) {
				var r = "__rm";
				if (opt.CodeGenerationTarget != CodeGenerationTarget.JavaInterop1 && invokeType == "Object") {
					r += ".Handle";
				}
				body.Add ($"\treturn {method.RetVal.ReturnCast}{method.RetVal.FromNative (opt, r, true, false) + opt.GetNullForgiveness (method.RetVal)};");
			}
		}

		public static void AddParameterListCallArgs (List<string> body, ParameterList parameters, CodeGenerationOptions opt, bool invoker)
		{
			if (parameters.Count == 0)
				return;

			invoker = invoker && opt.EmitLegacyInterfaceInvokers;

			var JValue = invoker ? "JValue" : "JniArgumentValue";

			body.Add ($"\t{JValue}* __args = stackalloc {JValue} [{parameters.Count}];");

			for (var i = 0; i < parameters.Count; ++i) {
				var p = parameters [i];
				body.Add ($"\t__args [{i}] = new {JValue} ({p.GetCall (opt)});");
			}
		}

		public static void AddSupportedOSPlatform (List<AttributeWriter> attributes, ApiVersionsSupport.IApiAvailability member, CodeGenerationOptions opt)
		{
			AddSupportedOSPlatform (attributes, member.ApiAvailableSince, opt);
			AddUnsupportedOSPlatform (attributes, member.ApiRemovedSince, opt);
		}

		public static void AddSupportedOSPlatform (List<AttributeWriter> attributes, int since, CodeGenerationOptions opt)
		{
			// There's no sense in writing say 'android15' because we do not support older APIs,
			// so those APIs will be available in all of our versions.
			if (since > MINIMUM_API_LEVEL && opt.CodeGenerationTarget == Xamarin.Android.Binder.CodeGenerationTarget.XAJavaInterop1)
				attributes.Add (new SupportedOSPlatformAttr (since));
		}

		public static void AddUnsupportedOSPlatform (List<AttributeWriter> attributes, int since, CodeGenerationOptions opt)
		{
			// Here it makes sense to still write 'android15' because it will be missing in later versions like `android35`.
			if (since > 0 && opt.CodeGenerationTarget == CodeGenerationTarget.XAJavaInterop1)
				attributes.Add (new UnsupportedOSPlatformAttr (since));
		}

		public static void AddObsolete (List<AttributeWriter> attributes, string message, CodeGenerationOptions opt, bool forceDeprecate = false, bool isError = false, int? deprecatedSince = null)
		{
			// Bail if we're not obsolete
			if ((!forceDeprecate && !message.HasValue ()) || message == "not deprecated")
				return;

			// Check if we should use [ObsoletedOSPlatform] instead of [Obsolete]
			if (AddObsoletedOSPlatformAttribute (attributes, message, deprecatedSince, opt))
				return;

			attributes.Add (new ObsoleteAttr (message, isError));
		}

		// Returns true if attribute was applied
		static bool AddObsoletedOSPlatformAttribute (List<AttributeWriter> attributes, string message, int? deprecatedSince, CodeGenerationOptions opt)
		{
			if (!opt.UseObsoletedOSPlatformAttributes)
				return false;

			// If it was obsoleted in a version earlier than we support (like 15), use a regular [Obsolete] instead
			if (!deprecatedSince.HasValue || deprecatedSince <= MINIMUM_API_LEVEL)
				return false;

			// This is the default Android message, but it isn't useful so remove it
			if (message == "deprecated")
				message = string.Empty;

			attributes.Add (new ObsoletedOSPlatformAttr (message, deprecatedSince.Value));

			return true;
		}

		public static void AddRestrictToWarning (List<AttributeWriter> attributes, string scope, bool isType, CodeGenerationOptions opt)
		{
			if (!opt.UseRestrictToAttributes)
				return;

			// Empty scope means there is no @RestrictTo annotation
			if (!scope.HasValue ())
				return;

			// We can't add 2 [Obsolete] attributes, we'll let a "real" obsolete message take precedence
			if (attributes.OfType<ObsoleteAttr> ().Any ())
				return;

			// We consider all scopes to be "internal" API we need to warn about
			attributes.Add (new RestrictToAttr (isType));
		}

		public static void WriteMethodInvokerBodyLegacy (CodeWriter writer, Method method, CodeGenerationOptions opt, string contextThis)
		{
			writer.WriteLine ($"if ({method.EscapedIdName} == IntPtr.Zero)");
			writer.WriteLine ($"\t{method.EscapedIdName} = JNIEnv.GetMethodID (class_ref, \"{method.JavaName}\", \"{method.JniSignature}\");");

			foreach (var prep in method.Parameters.GetCallPrep (opt))
				writer.WriteLine (prep);

			WriteParameterListCallArgs (writer, method.Parameters, opt, invoker: true);

			var env_method = $"Call{method.RetVal.CallMethodPrefix}Method";
			var call = $"{method.RetVal.ReturnCast}JNIEnv.{env_method} ({contextThis}, {method.EscapedIdName}{method.Parameters.GetCallArgs (opt, invoker: true)})";

			if (method.IsVoid)
				writer.WriteLine (call + ";");
			else
				writer.WriteLine ($"{(method.Parameters.HasCleanup ? "var __ret = " : "return ")}{method.RetVal.FromNative (opt, call, true) + opt.GetNullForgiveness (method.RetVal)};");

			foreach (var cleanup in method.Parameters.GetCallCleanup (opt))
				writer.WriteLine (cleanup);

			if (!method.IsVoid && method.Parameters.HasCleanup)
				writer.WriteLine ("return __ret;");
		}

		public static void WriteParameterListCallArgs (CodeWriter writer, ParameterList parameters, CodeGenerationOptions opt, bool invoker)
		{
			if (parameters.Count == 0)
				return;

			invoker = invoker && opt.EmitLegacyInterfaceInvokers;

			var JValue = invoker ? "JValue" : "JniArgumentValue";

			writer.WriteLine ($"{JValue}* __args = stackalloc {JValue} [{parameters.Count}];");

			for (var i = 0; i < parameters.Count; ++i) {
				var p = parameters [i];
				writer.WriteLine ($"__args [{i}] = new {JValue} ({p.GetCall (opt)});");
			}
		}

		public static void WriteMethodStringOverloadBody (CodeWriter writer, Method method, CodeGenerationOptions opt, bool haveSelf)
		{
			var call = new StringBuilder ();

			foreach (var p in method.Parameters) {
				var pname = p.Name;
				if (p.Type == "Java.Lang.ICharSequence") {
					pname = p.GetName ("jls_");
					writer.WriteLine ($"var {pname} = {p.Name} == null ? null : new global::Java.Lang.String ({p.Name});");
				} else if (p.Type == "Java.Lang.ICharSequence[]" || p.Type == "params Java.Lang.ICharSequence[]") {
					pname = p.GetName ("jlca_");
					writer.WriteLine ($"var {pname} = {opt.GetStringArrayToCharSequenceArrayMethodName ()} ({p.Name});");
				}

				if (call.Length > 0)
					call.Append (", ");

				call.Append (pname + (p.Type == "Java.Lang.ICharSequence" ? opt.GetNullForgiveness (p) : string.Empty));
			}

			writer.WriteLine ($"{(method.RetVal.IsVoid ? string.Empty : opt.GetTypeReferenceName (method.RetVal) + " __result = ")}{(haveSelf ? "self." : "")}{method.AdjustedName} ({call.ToString ()});");

			switch (method.RetVal.FullName) {
				case "void":
					break;
				case "Java.Lang.ICharSequence[]":
					writer.WriteLine ("var __rsval = CharSequence.ArrayToStringArray (__result);");
					break;
				case "Java.Lang.ICharSequence":
					writer.WriteLine ("var __rsval = __result?.ToString ();");
					break;
				default:
					writer.WriteLine ("var __rsval = __result;");
					break;
			}

			foreach (var p in method.Parameters) {
				if (p.Type == "Java.Lang.ICharSequence")
					writer.WriteLine ($"{p.GetName ("jls_")}?.Dispose ();");
				else if (p.Type == "Java.Lang.ICharSequence[]")
					writer.WriteLine ($"if ({p.GetName ("jlca_")} != null) foreach (var s in {p.GetName ("jlca_")}) s?.Dispose ();");
			}

			if (!method.RetVal.IsVoid)
				writer.WriteLine ($"return __rsval{opt.GetNullForgiveness (method.RetVal)};");
		}

		public static TypeWriter BuildManagedTypeModel (GenBase gen, CodeGenerationOptions opt, CodeGeneratorContext context, GenerationInfo genInfo)
		{
			if (gen is ClassGen klass)
				return new BoundClass (klass, opt, context, genInfo);
			else if (gen is InterfaceGen iface)
				return new BoundInterface (iface, opt, context, genInfo);

			throw new InvalidOperationException ("Unknown GenBase type");
		}

		public static void WarnIfTypeNameMatchesNamespace (TypeWriter type, GenBase gen)
		{
			// We only care about the last part of a namespace
			// eg: android.graphics.drawable -> drawable
			var ns = gen.Namespace?.LastSubset ('.');

			if (!ns.HasValue ())
				return;

			if (string.Equals (ns, type.Name, StringComparison.OrdinalIgnoreCase))
				Report.LogCodedWarning (0, Report.WarningTypeNameMatchesEnclosingNamespace, $"{gen.Namespace}.{type.Name}");
		}
	}
}
