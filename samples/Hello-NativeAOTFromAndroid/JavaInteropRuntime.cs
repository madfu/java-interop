﻿using System.Runtime.InteropServices;
using System.Reflection;
using Java.Interop;

namespace Java.Interop.Samples.NativeAotFromAndroid;

static class JavaInteropRuntime
{
	static JniRuntime? runtime;

	[UnmanagedCallersOnly (EntryPoint="JNI_OnLoad")]
	static int JNI_OnLoad (IntPtr vm, IntPtr reserved)
	{
		try {
			AndroidLog.Print (AndroidLogLevel.Info, "JavaInteropRuntime", "JNI_OnLoad()");
			LogcatTextWriter.Init ();
			return (int) JniVersion.v1_6;
		}
		catch (Exception e) {
			AndroidLog.Print (AndroidLogLevel.Error, "JavaInteropRuntime", $"JNI_OnLoad() failed: {e}");
			return 0;
		}
	}

	[UnmanagedCallersOnly (EntryPoint="JNI_OnUnload")]
	static void JNI_OnUnload (IntPtr vm, IntPtr reserved)
	{
		AndroidLog.Print(AndroidLogLevel.Info, "JavaInteropRuntime", "JNI_OnUnload");
		runtime?.Dispose ();
	}

	// symbol name from `$(IntermediateOutputPath)obj/Release/osx-arm64/h-classes/net_dot_jni_hello_JavaInteropRuntime.h`
	[UnmanagedCallersOnly (EntryPoint="Java_net_dot_jni_nativeaot_JavaInteropRuntime_init")]
	static void init (IntPtr jnienv, IntPtr klass)
	{
		Console.WriteLine ($"C# init()");
		try {
			var options = new JreRuntimeOptions {
				EnvironmentPointer          = jnienv,
				TypeManager                 = new NativeAotTypeManager (),
				UseMarshalMemberBuilder     = false,
				JniGlobalReferenceLogWriter = new LogcatTextWriter (AndroidLogLevel.Debug, "NativeAot:GREF"),
				JniLocalReferenceLogWriter  = new LogcatTextWriter (AndroidLogLevel.Debug, "NativeAot:LREF"),
			};
			runtime = options.CreateJreVM ();

			var jnienvinit = Type.GetType("Android.Runtime.JNIEnvInit, Mono.Android");
			ArgumentNullException.ThrowIfNull(jnienvinit);

			var init = jnienvinit.GetMethod("InitializeNativeAot", BindingFlags.Static | BindingFlags.NonPublic);
			ArgumentNullException.ThrowIfNull(init);

			init.Invoke(null, [ runtime ]);
		}
		catch (Exception e) {
			AndroidLog.Print (AndroidLogLevel.Error, "JavaInteropRuntime", $"JavaInteropRuntime.init: error: {e}");
		}
	}
}
