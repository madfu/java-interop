#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Android.Runtime;
using Java.Interop.Tools.JavaCallableWrappers;

#if HAVE_CECIL
using Mono.Cecil;
using Java.Interop.Tools.Cecil;
#endif  // HAVE_CECIL

namespace Java.Interop.Tools.TypeNameMappings
{

#if HAVE_CECIL
	public
#endif
	enum PackageNamingPolicy {
		[Obsolete ("No longer supported. Use PackageNamingPolicy.LowercaseCrc64 instead.", error: true)]
		LowercaseHash = 0,
		Lowercase = 1,
		LowercaseWithAssemblyName = 2,
		[Obsolete ("No longer supported. Use PackageNamingPolicy.LowercaseCrc64 instead.", error: true)]
		LowercaseMD5 = LowercaseHash,
		LowercaseCrc64 = 3,
	}

#if HAVE_CECIL
	public
#endif
	class JniTypeName
	{
		string? type;
		public string Type {
			get => type ?? throw new InvalidOperationException ("`Type` must be set before accessing it!");
			internal set => type = value;
		}
		public bool IsKeyword { get; internal set; }
	}

#if HAVE_CECIL
	public
#endif
	static class JavaNativeTypeManager {
		const string CRC_PREFIX = "crc64";

		public static PackageNamingPolicy PackageNamingPolicy { get; set; } = PackageNamingPolicy.LowercaseCrc64;

		public static string? ApplicationJavaClass { get; set; }

		public static JniTypeName? Parse (string? jniType)
		{
			int _ = 0;
			return ExtractType (jniType, ref _);
		}

		public static IEnumerable<JniTypeName> FromSignature (string signature)
		{
			if (signature.StartsWith ("(", StringComparison.Ordinal)) {
				int e = signature.IndexOf (')');
				signature = signature.Substring (1, e >= 0 ? e-1 : signature.Length-1);
			}
			int i = 0;
			JniTypeName t;
			while ((t = ExtractType (signature, ref i)) != null)
				yield return t;
		}

		[return: NotNullIfNotNull ("signature")]
		public static JniTypeName? ReturnTypeFromSignature (string? signature)
		{
			if (signature == null) {
				return null;
			}
			int idx = signature.LastIndexOf (')') + 1;
			return ExtractType (signature, ref idx);
		}

		// as per: http://java.sun.com/j2se/1.5.0/docs/guide/jni/spec/types.html
		[return: NotNullIfNotNull ("signature")]
		static JniTypeName? ExtractType (string? signature, ref int index)
		{
			if (signature is null || index >= signature.Length)
				return null;
			var i = index++;
			switch (signature [i]) {
				case '[': {
					++i;
					if (i >= signature.Length)
						throw new InvalidOperationException ("Missing array type after '[' at index " + i + " in: " + signature);
					var r = ExtractType (signature, ref index);
					return new JniTypeName { Type = r.Type + "[]", IsKeyword = r.IsKeyword };
				}
				case 'B':
					return new JniTypeName { Type = "byte", IsKeyword = true };
				case 'C':
					return new JniTypeName { Type = "char", IsKeyword = true };
				case 'D':
					return new JniTypeName { Type = "double", IsKeyword = true };
				case 'F':
					return new JniTypeName { Type = "float", IsKeyword = true };
				case 'I':
					return new JniTypeName { Type = "int", IsKeyword = true };
				case 'J':
					return new JniTypeName { Type = "long", IsKeyword = true };
				case 'L': {
					var e = signature.IndexOf (';', index);
					if (e <= 0)
						throw new InvalidOperationException ("Missing reference type after 'L' at index " + i + "in: " + signature);
					var s = index;
					index = e + 1;
					return new JniTypeName {
						Type      = signature.Substring (s, e - s).Replace ("/", ".").Replace ("$", "."),
						IsKeyword = false,
					};
				}
				case 'S':
					return new JniTypeName { Type = "short", IsKeyword = true };
				case 'V':
					return new JniTypeName { Type = "void", IsKeyword = true };
				case 'Z':
					return new JniTypeName { Type = "boolean", IsKeyword = true };
				default:
					throw new InvalidOperationException ("Unknown JNI Type '" + signature [i] + "' within: " + signature);
			}
		}

		public static string ToCliType (string jniType)
		{
			if (string.IsNullOrEmpty (jniType))
				return jniType;
			string[] parts = jniType.Split ('/');
			for (int i = 0; i < parts.Length; ++i) {
				parts [i] = ToCliTypePart (parts [i]);
			}
			return string.Join (".", parts);
		}

		static string ToCliTypePart (string part)
		{
			if (part.IndexOf ('$') < 0)
				return ToPascalCase (part, 2);
			string[] parts = part.Split ('$');
			for (int i = 0; i < parts.Length; ++i) {
				parts [i] = ToPascalCase (parts [i], 1);
			}
			return string.Join ("+", parts);
		}

		static string ToPascalCase (string value, int minLength)
		{
			return value.Length <= minLength
				? value.ToUpperInvariant ()
				: char.ToUpperInvariant (value [0]) + value.Substring (1);
		}

		// Keep in sync with ToJniName(TypeDefinition)
		public static string ToJniName (Type type)
		{
			return ToJniName (type, ExportParameterKind.Unspecified) ??
				"java/lang/Object";
		}

		static string? ToJniName (Type type, ExportParameterKind exportKind)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (type.IsValueType)
				return GetPrimitiveClass (type);

			if (type == typeof (string))
				return "java/lang/String";

			if (ShouldCheckSpecialExportJniType (type))
				return GetSpecialExportJniType (type.FullName!, exportKind);

			return ToJniName (type, t => t.DeclaringType!, t => t.Name, GetPackageName, t => {
				return ToJniNameFromAttributes (t);
			}, _ => false);
		}

#if !NETSTANDARD2_0
		static readonly Lazy<Type> IJavaPeerableType = new Lazy<Type> (() =>
			Type.GetType ("Java.Interop.IJavaPeerable, Java.Interop", throwOnError: true)!
		);
#endif

		// NOTE: NETSTANDARD2_0 could be running in an MSBuild context where Java.Interop.dll is not available.
		// Trimming warnings are not enabled for netstandard2.0 in this project.
		static bool ShouldCheckSpecialExportJniType (Type type) =>
#if NETSTANDARD2_0
			!type.GetInterfaces ().Any (t => t.FullName == "Java.Interop.IJavaPeerable");
#else
			!IJavaPeerableType.Value.IsAssignableFrom (type);
#endif

		public static string ToJniName (string jniType, int rank)
		{
			if (rank == 0)
				return jniType;

			if (jniType.Length > 1 && jniType [0] != '[')
				jniType = "L" + jniType + ";";
			return new string ('[', rank) + jniType;
		}

		static bool IsPackageNamePreservedForAssembly (string assemblyName)
		{
			return assemblyName == "Mono.Android";
		}

		public static string GetPackageName (Type type)
		{
			string assemblyName = GetAssemblyName (type.Assembly);
			if (IsPackageNamePreservedForAssembly (assemblyName))
				return type.Namespace!.ToLowerInvariant ();
			switch (PackageNamingPolicy) {
			case PackageNamingPolicy.Lowercase:
				return type.Namespace!.ToLowerInvariant ();
			case PackageNamingPolicy.LowercaseWithAssemblyName:
				return "assembly_" + (assemblyName.Replace ('.', '_') + "." + type.Namespace).ToLowerInvariant ();
			case PackageNamingPolicy.LowercaseCrc64:
				return CRC_PREFIX + ToCrc64 (type.Namespace + ":" + assemblyName);
			default:
					throw new NotSupportedException ($"PackageNamingPolicy.{PackageNamingPolicy} is no longer supported.");
			}
		}

		/// <summary>
		/// A more performant equivalent of `Assembly.GetName().Name`
		/// </summary>
		static string GetAssemblyName (Assembly assembly)
		{
			var name = assembly.FullName!;
			int index = name.IndexOf (',');
			if (index != -1) {
				return name.Substring (0, index);
			}
			return name;
		}

		public static int GetArrayInfo (Type type, out Type elementType)
		{
			elementType = type;
			int rank = 0;
			while (type.IsArray) {
				rank++;
				elementType = type = type.GetElementType ()!;
			}
			return rank;
		}

		static string? GetPrimitiveClass (Type type)
		{
			if (type.IsEnum)
				return GetPrimitiveClass (Enum.GetUnderlyingType (type));
			if (type == typeof (byte))
				return "B";
			if (type == typeof (char))
				return "C";
			if (type == typeof (double))
				return "D";
			if (type == typeof (float))
				return "F";
			if (type == typeof (int))
				return "I";
			if (type == typeof (uint))
				return "I";
			if (type == typeof (long))
				return "J";
			if (type == typeof (ulong))
				return "J";
			if (type == typeof (short))
				return "S";
			if (type == typeof (ushort))
				return "S";
			if (type == typeof (bool))
				return "Z";
			return null;
		}

		static string? GetSpecialExportJniType (string typeName, ExportParameterKind exportKind)
		{
			switch (exportKind) {
			case ExportParameterKind.InputStream:
				if (typeName != "System.IO.Stream")
					throw new ArgumentException ("ExportParameterKind.InputStream is valid only for System.IO.Stream parameter type");
				return "java/io/InputStream";
			case ExportParameterKind.OutputStream:
				if (typeName != "System.IO.Stream")
					throw new ArgumentException ("ExportParameterKind.OutputStream is valid only for System.IO.Stream parameter type");
				return "java/io/OutputStream";
			case ExportParameterKind.XmlPullParser:
				if (typeName != "System.Xml.XmlReader")
					throw new ArgumentException ("ExportParameterKind.XmlPullParser is valid only for System.Xml.XmlReader parameter type");
				return "org/xmlpull/v1/XmlPullParser";
			case ExportParameterKind.XmlResourceParser:
				if (typeName != "System.Xml.XmlReader")
					throw new ArgumentException ("ExportParameterKind.XmlResourceParser is valid only for System.Xml.XmlReader parameter type");
				return "android/content/res/XmlResourceParser";
			}
			// FIXME: this *must* error out here, instead of returning null.
			// Either Droidinator must be fixed to not reach here, or a global flag that skips this error check must be added.
			return null;
		}

		// Keep in sync with ToJniNameFromAttributes(TypeDefinition)
		public static string? ToJniNameFromAttributes (Type type)
		{
			var aa = (IJniNameProviderAttribute []) type.GetCustomAttributes (typeof (IJniNameProviderAttribute), inherit: false);
			return aa.Length > 0 && !string.IsNullOrEmpty (aa [0].Name) ? aa [0].Name.Replace ('.', '/') : null;
		}

		/*
		 * Semantics: return `null` on "failure", DO NOT throw an exception.
		 *
		 * Why? tools/msbuild/Generator/JavaTypeInfo.cs!AddConstructors() attempts
		 * to generate (non-[Export]) constructors, and to determine whether or
		 * not the constructor CAN be declared it calls
		 * JniType.GetJniSignature(MethodDefinition). If GetJniSignature() returns
		 * null, it can't be exported, and the method is skipped.
		 *
		 * Callers of GetJniSignature() MUST check for `null` and behave
		 * appropriately.
		 */
		static string? GetJniSignature<T,P> (IEnumerable<P> parameters, Func<P,T> getParameterType, Func<P,ExportParameterKind> getExportKind, T returnType, ExportParameterKind returnExportKind, Func<T,ExportParameterKind,string?> getJniTypeName, bool isConstructor)
		{
			StringBuilder sb = new StringBuilder ().Append ("(");
			foreach (P p in parameters) {
				var jniType = getJniTypeName (getParameterType (p), getExportKind (p));
				if (jniType == null)
					return null;
				sb.Append (jniType);
			}
			sb.Append (')');
			if (isConstructor)
				sb.Append ("V");
			else {
				var jniType = getJniTypeName (returnType, returnExportKind);
				if (jniType == null)
					return null;
				sb.Append (jniType);
			}
			return sb.ToString ();
		}

		static string? GetJniTypeName<TR,TD> (TR typeRef, ExportParameterKind exportKind, Func<TR,TD> resolve, Func<TR,KeyValuePair<int,TR>> getArrayInfo, Func<TD,string> getFullName, Func<TD,ExportParameterKind,string?> toJniName)
		{
			TD ptype = resolve (typeRef);
			var p = getArrayInfo (typeRef);
			int rank = p.Key;
			TR etype = p.Value;
			ptype = resolve (etype);
			if (ptype == null) {
				// Likely caused by generic parameters, which we probably can't bind anyway.
				return null;
			}
			if (getFullName (ptype) == "System.Void")
				return "V";
			if (getFullName (ptype) == "System.IntPtr")
				// Probably a (IntPtr, JniHandleOwnership) parameter; skip
				return null;

			var pJniName = toJniName (ptype, exportKind);
			if (pJniName == null) {
				return null;
			}
			return (rank == 0 && pJniName.Length > 1 && pJniName[0] != '[')
				? "L" + pJniName + ";"
				: ToJniName (pJniName, rank);
		}

		static ExportParameterKind GetExportKind (System.Reflection.ICustomAttributeProvider p)
		{
			foreach (ExportParameterAttribute a in p.GetCustomAttributes (typeof (ExportParameterAttribute), false))
				return a.Kind;
			return ExportParameterKind.Unspecified;
		}

		public static string? GetJniSignature (MethodInfo method)
		{
			return GetJniSignature<Type,ParameterInfo> (method.GetParameters (),
				p => p.ParameterType,
				p => GetExportKind (p),
				method.ReturnType,
				GetExportKind (method.ReturnParameter),
				(t, k) => GetJniTypeName (t, k),
				method.IsConstructor);
		}

		public static string? GetJniTypeName (Type typeRef)
		{
			return GetJniTypeName (typeRef, ExportParameterKind.Unspecified);
		}

		internal static string? GetJniTypeName (Type typeRef, ExportParameterKind exportKind)
		{
			return GetJniTypeName<Type,Type> (typeRef, exportKind, t => t, t => {
				Type etype;
				int rank = GetArrayInfo (t, out etype);
				return new KeyValuePair<int,Type> (rank, etype);
			}, t => t.FullName!, (t, k) => ToJniNameWhichShouldReplaceExistingToJniName (t, k));
		}

		static string? ToJniNameWhichShouldReplaceExistingToJniName (Type type, ExportParameterKind exportKind)
		{
			// we need some method that exactly does the same as ToJniName(TypeDefinition)
			var ret = ToJniNameFromAttributes (type);
			return ret ?? ToJniName (type, exportKind);
		}

#if HAVE_CECIL

		internal static ExportParameterKind GetExportKind (Mono.Cecil.ICustomAttributeProvider p)
		{
			foreach (CustomAttribute a in p.GetCustomAttributes (typeof (ExportParameterAttribute)))
				return ToExportParameterAttribute (a).Kind;
			return ExportParameterKind.Unspecified;
		}

		internal static ExportParameterAttribute ToExportParameterAttribute (CustomAttribute attr)
		{
			return new ExportParameterAttribute ((ExportParameterKind)attr.ConstructorArguments [0].Value);
		}

		[Obsolete ("Use the TypeDefinitionCache overload for better performance.", error: true)]
		public static bool IsApplication (TypeDefinition type) => throw new NotSupportedException ();

		public static bool IsApplication (TypeDefinition type, TypeDefinitionCache cache) =>
			IsApplication (type, (IMetadataResolver) cache);

		public static bool IsApplication (TypeDefinition type, IMetadataResolver resolver)
		{
			return type.GetBaseTypes (resolver).Any (b => b.FullName == "Android.App.Application");
		}

		[Obsolete ("Use the TypeDefinitionCache overload for better performance.", error: true)]
		public static bool IsInstrumentation (TypeDefinition type) => throw new NotSupportedException ();

		public static bool IsInstrumentation (TypeDefinition type, TypeDefinitionCache cache) =>
			IsInstrumentation (type, (IMetadataResolver) cache);

		public static bool IsInstrumentation (TypeDefinition type, IMetadataResolver resolver)
		{
			return type.GetBaseTypes (resolver).Any (b => b.FullName == "Android.App.Instrumentation");
		}

		// moved from JavaTypeInfo
		[Obsolete ("Use the TypeDefinitionCache overload for better performance.", error: true)]
		public static string? GetJniSignature (MethodDefinition method) => throw new NotSupportedException ();

		public static string? GetJniSignature (MethodDefinition method, TypeDefinitionCache cache) =>
			GetJniSignature (method, (IMetadataResolver) cache);

		public static string? GetJniSignature (MethodDefinition method, IMetadataResolver resolver)
		{
			return GetJniSignature<TypeReference,ParameterDefinition> (
				method.Parameters,
				p => p.ParameterType,
				p => GetExportKind (p),
				method.ReturnType,
				GetExportKind (method.MethodReturnType),
				(t, k) => GetJniTypeName (t, k, resolver),
				method.IsConstructor);
		}

		// moved from JavaTypeInfo
		[Obsolete ("Use the TypeDefinitionCache overload for better performance.", error: true)]
		public static string? GetJniTypeName (TypeReference typeRef) => throw new NotSupportedException ();

		public static string? GetJniTypeName (TypeReference typeRef, TypeDefinitionCache cache) =>
			GetJniTypeName (typeRef, (IMetadataResolver) cache);

		public static string? GetJniTypeName (TypeReference typeRef, IMetadataResolver resolver)
		{
			return GetJniTypeName (typeRef, ExportParameterKind.Unspecified, resolver);
		}

		internal static string? GetJniTypeName (TypeReference typeRef, ExportParameterKind exportKind, IMetadataResolver cache)
		{
			return GetJniTypeName<TypeReference, TypeDefinition> (typeRef, exportKind, t => cache.Resolve (t), t => {
				TypeReference etype;
				int rank = GetArrayInfo (typeRef, out etype);
				return new KeyValuePair<int,TypeReference> (rank,etype);
				}, t => t.FullName, (t, k) => ToJniName (t, k, cache));
		}

		[Obsolete ("Use the TypeDefinitionCache overload for better performance.", error: true)]
		public static string ToCompatJniName (TypeDefinition type) => throw new NotSupportedException ();

		public static string ToCompatJniName (TypeDefinition type, TypeDefinitionCache cache) =>
			ToCompatJniName (type, (IMetadataResolver) cache);

		public static string ToCompatJniName (TypeDefinition type, IMetadataResolver resolver)
		{
			return ToJniName (
					type:               type,
					decl:               t => t.DeclaringType,
					name:               t => t.Name,
					ns:                 ToCompatPackageName,
					overrideName:       t => ToJniNameFromAttributes (t, resolver),
					shouldUpdateName:   t => IsNonStaticInnerClass (t as TypeDefinition, resolver)
			);
		}

		static string ToCompatPackageName (TypeDefinition type)
		{
			return type.Namespace;
		}

		// Keep in sync with ToJniNameFromAttributes(Type) and ToJniName(Type)
		[Obsolete ("Use the TypeDefinitionCache overload for better performance.", error: true)]
		public static string ToJniName (TypeDefinition type) => throw new NotSupportedException ();

		public static string ToJniName (TypeDefinition type, TypeDefinitionCache cache) =>
			ToJniName (type, (IMetadataResolver) cache);

		public static string ToJniName (TypeDefinition type, IMetadataResolver resolver)
		{
			var x = ToJniName (type, ExportParameterKind.Unspecified, resolver) ??
				"java/lang/Object";
			return x;
		}

		static string? ToJniName (TypeDefinition type, ExportParameterKind exportKind, IMetadataResolver cache)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (type.IsValueType)
				return GetPrimitiveClass (type, cache);

			if (type.FullName == "System.String")
				return "java/lang/String";

			if (!type.ImplementsInterface ("Android.Runtime.IJavaObject", cache) && 
					!type.ImplementsInterface ("Java.Interop.IJavaPeerable", cache)) {
				return GetSpecialExportJniType (type.FullName, exportKind);
			}

			return ToJniName (
					type:               type,
					decl:               t => t.DeclaringType,
					name:               t => t.Name,
					ns:                 t => GetPackageName (t, cache),
					overrideName:       t => ToJniNameFromAttributes (t, cache),
					shouldUpdateName:   t => IsNonStaticInnerClass (t as TypeDefinition, cache)
			);
		}

		static string? ToJniNameFromAttributes (TypeDefinition type, IMetadataResolver resolver)
		{
			return ToJniNameFromAttributesForAndroid (type, resolver) ??
				ToJniNameFromAttributesForInterop (type, resolver);
		}

		static string? ToJniNameFromAttributesForInterop (TypeDefinition type, IMetadataResolver resolver)
		{
			var attr = type.CustomAttributes.FirstOrDefault (a =>
				resolver.Resolve (a.AttributeType)
				.FullName == "Java.Interop.JniTypeSignatureAttribute");
			if (attr == null) {
				return null;
			}
			var carg    = attr.ConstructorArguments.FirstOrDefault ();
			if (carg.Type == null || carg.Type.FullName != "System.String")
				return null;
			var jniType     = (string) carg.Value;
			var isKeyProp   = attr.Properties.FirstOrDefault (p => p.Name == "IsKeyword");
			var isKeyword   = isKeyProp.Name != null && ((bool) isKeyProp.Argument.Value) == true;
			var arrRankProp = attr.Properties.FirstOrDefault (p => p.Name == "ArrayRank");
			var arrayRank   = arrRankProp.Name != null && arrRankProp.Argument.Value is int rank ? rank : 0;
			jniType = arrayRank == 0
				? jniType
				: new string ('[', arrayRank) + (isKeyword ? jniType : "L" + jniType + ";");
			return jniType;
		}

		static string? ToJniNameFromAttributesForAndroid (TypeDefinition type, IMetadataResolver resolver)
		{
			if (!type.HasCustomAttributes)
				return null;
			foreach (var attr in type.CustomAttributes) {
				if (!IsIJniNameProviderAttribute (attr, resolver))
					continue;
				var ap = attr.HasProperties ? attr.Properties.FirstOrDefault (p => p.Name == "Name") : default;
				string? name = null;
				if (ap.Name == null) {
					var ca = attr.ConstructorArguments.FirstOrDefault ();
					if (ca.Type == null || ca.Type.FullName != "System.String")
						continue;
					name = (string) ca.Value;
				} else
					name = (string) ap.Argument.Value;
				if (!string.IsNullOrEmpty (name))
					return name.Replace ('.', '/');
			}
			return null;
		}

		static readonly HashSet<string> KnownIJniNameProviders = new HashSet<string> (StringComparer.Ordinal) {
			"Android.App.ActivityAttribute",
			"Android.App.ApplicationAttribute",
			"Android.App.InstrumentationAttribute",
			"Android.App.ServiceAttribute",
			"Android.Content.BroadcastReceiverAttribute",
			"Android.Content.ContentProviderAttribute",
			"Android.Runtime.RegisterAttribute",
		};

		static bool IsIJniNameProviderAttribute (CustomAttribute attr, IMetadataResolver resolver)
		{
			// Fast path for a list of known IJniNameProviderAttribute implementations
			if (KnownIJniNameProviders.Contains (attr.AttributeType.FullName))
				return true;

			// Slow path resolves the type, looking for IJniNameProviderAttribute
			var attributeType = resolver.Resolve (attr.AttributeType);
			if (!attributeType.HasInterfaces)
				return false;
			return attributeType.Interfaces.Any (it => it.InterfaceType.FullName == typeof (IJniNameProviderAttribute).FullName);
		}

		public static int GetArrayInfo (Mono.Cecil.TypeReference type, out Mono.Cecil.TypeReference elementType)
		{
			elementType = type;
			int rank = 0;
			while (type.IsArray) {
				rank++;
				elementType = type = type.GetElementType ();
			}
			return rank;
		}

		static string? GetPrimitiveClass (Mono.Cecil.TypeDefinition type, IMetadataResolver cache)
		{
			if (type.IsEnum)
				return GetPrimitiveClass (cache.Resolve (type.Fields.First (f => f.IsSpecialName).FieldType), cache);
			if (type.FullName == "System.Byte")
				return "B";
			if (type.FullName == "System.Char")
				return "C";
			if (type.FullName == "System.Double")
				return "D";
			if (type.FullName == "System.Single")
				return "F";
			if (type.FullName == "System.Int32")
				return "I";
			if (type.FullName == "System.Int64")
				return "J";
			if (type.FullName == "System.Int16")
				return "S";
			if (type.FullName == "System.Boolean")
				return "Z";
			return null;
		}

		[Obsolete ("Use the TypeDefinitionCache overload for better performance.", error: true)]
		public static string GetPackageName (TypeDefinition type) => throw new NotSupportedException ();

		public static string GetPackageName (TypeDefinition type, TypeDefinitionCache cache) =>
			GetPackageName (type, (IMetadataResolver) cache);

		public static string GetPackageName (TypeDefinition type, IMetadataResolver resolver)
		{
			if (IsPackageNamePreservedForAssembly (type.GetPartialAssemblyName (resolver)))
				return type.Namespace.ToLowerInvariant ();
			switch (PackageNamingPolicy) {
			case PackageNamingPolicy.Lowercase:
				return type.Namespace.ToLowerInvariant ();
			case PackageNamingPolicy.LowercaseWithAssemblyName:
				return "assembly_" + (type.GetPartialAssemblyName (resolver).Replace ('.', '_') + "." + type.Namespace).ToLowerInvariant ();
			case PackageNamingPolicy.LowercaseCrc64:
				return CRC_PREFIX + ToCrc64 (type.Namespace + ":" + type.GetPartialAssemblyName (resolver));
			default:
					throw new NotSupportedException ($"PackageNamingPolicy.{PackageNamingPolicy} is no longer supported.");
			}
		}
#endif

		static string ToJniName<T> (T type, Func<T, T> decl, Func<T, string> name, Func<T, string> ns, Func<T, string?> overrideName, Func<T,bool> shouldUpdateName)
			where T : class
		{
			var nameParts   = new List<string> ();
			var typeName    = (string?) null;
			var nsType      = type;

			for (var declType = type ; declType != null; declType = decl (declType)) {
				nsType      = declType;
				typeName    = overrideName (declType);
				if (typeName != null) {
					break;
				}
				var n   = name (declType).Replace ('`', '_');
				if (shouldUpdateName (declType)) {
					n = "$" + name (decl (declType)) + "_" + n;
				}
				nameParts.Add (n);
			}

			if (nameParts.Count == 0 && typeName != null)
				return typeName;

			nameParts.Reverse ();

			var nestedSuffix    = string.Join ("_", nameParts.ToArray ()).Replace ("_$", "$");
			if (typeName != null)
				return (typeName + "_" + nestedSuffix).Replace ("_$", "$");;

			// Results in namespace/parts/OuterType_InnerType
			// We do this to simplify monodroid type generation
			typeName = nestedSuffix;
			var _ns = ToLowerCase (ns (nsType));
			return string.IsNullOrEmpty (_ns)
				? typeName
				: _ns.Replace ('.', '/') + "/" + typeName;
		}

#if HAVE_CECIL
		internal static bool IsNonStaticInnerClass (TypeDefinition? type, IMetadataResolver cache)
		{
			if (type == null)
				return false;
			if (!type.IsNested)
				return false;

			if (!type.DeclaringType.HasJavaPeer (cache))
				return false;

			foreach (var baseType in type.GetBaseTypes (cache)) {
				if (baseType == null)
					continue;
				if (!HasTypeRegistrationAttribute (baseType))
					continue;

				foreach (var method in baseType.Methods) {
					if (!method.IsConstructor || method.IsStatic)
						continue;
					if (method.Parameters.Any (p => p.Name == "__self"))
						return true;
				}

				// Stop at the first base type with [Register]
				break;
			}

			return false;
		}

		static bool HasTypeRegistrationAttribute (TypeDefinition type)
		{
			if (!type.HasCustomAttributes)
				return false;
			return type.AnyCustomAttributes (typeof (RegisterAttribute)) ||
				type.AnyCustomAttributes ("Java.Interop.JniTypeSignatureAttribute");
		}
#endif  // HAVE_CECIL

		static string ToCrc64 (string value)
		{
			var data = Encoding.UTF8.GetBytes (value);
			var hash = Crc64Helper.Compute (data);
			var buf  = new StringBuilder (hash.Length * 2);
			foreach (var b in hash)
				buf.AppendFormat ("{0:x2}", b);
			return buf.ToString ();
		}

		static string ToLowerCase (string value)
		{
			if (string.IsNullOrEmpty (value))
				return value;
			string[] parts = value.Split ('.');
			for (int i = 0; i < parts.Length; ++i) {
				parts [i] = parts [i].ToLowerInvariant ();
			}
			return string.Join (".", parts);
		}
	}
}


