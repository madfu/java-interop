//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable restore
using System;
using System.Collections.Generic;
using Android.Runtime;
using Java.Interop;

namespace Xamarin.Test {

	// Metadata.xml XPath class reference: path="/api/package[@name='xamarin.test']/class[@name='NotificationCompatBase']"
	[global::Android.Runtime.Register ("xamarin/test/NotificationCompatBase", DoNotGenerateAcw=true)]
	public partial class NotificationCompatBase : global::Java.Lang.Object {
		// Metadata.xml XPath class reference: path="/api/package[@name='xamarin.test']/class[@name='NotificationCompatBase.Action']"
		[global::Android.Runtime.Register ("xamarin/test/NotificationCompatBase$Action", DoNotGenerateAcw=true)]
		public abstract partial class Action : global::Java.Lang.Object {
			// Metadata.xml XPath interface reference: path="/api/package[@name='xamarin.test']/interface[@name='NotificationCompatBase.Action.Factory']"
			[Register ("xamarin/test/NotificationCompatBase$Action$Factory", "", "Xamarin.Test.NotificationCompatBase/Action/IFactoryInvoker")]
			public partial interface IFactory : IJavaObject, IJavaPeerable {
				// Metadata.xml XPath method reference: path="/api/package[@name='xamarin.test']/interface[@name='NotificationCompatBase.Action.Factory']/method[@name='build' and count(parameter)=1 and parameter[1][@type='int']]"
				[Register ("build", "(I)Lxamarin/test/NotificationCompatBase$Action;", "GetBuild_IHandler:Xamarin.Test.NotificationCompatBase+Action+IFactoryInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")]
				global::Xamarin.Test.NotificationCompatBase.Action Build (int p0);

			}

			[global::Android.Runtime.Register ("xamarin/test/NotificationCompatBase$Action$Factory", DoNotGenerateAcw=true)]
			internal partial class IFactoryInvoker : global::Java.Lang.Object, IFactory {
				static IntPtr java_class_ref {
					get { return _members_xamarin_test_NotificationCompatBase_Action_Factory.JniPeerType.PeerReference.Handle; }
				}

				[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
				[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
				public override global::Java.Interop.JniPeerMembers JniPeerMembers {
					get { return _members_xamarin_test_NotificationCompatBase_Action_Factory; }
				}

				[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
				[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
				protected override IntPtr ThresholdClass {
					get { return _members_xamarin_test_NotificationCompatBase_Action_Factory.JniPeerType.PeerReference.Handle; }
				}

				[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
				[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
				protected override global::System.Type ThresholdType {
					get { return _members_xamarin_test_NotificationCompatBase_Action_Factory.ManagedPeerType; }
				}

				static readonly JniPeerMembers _members_xamarin_test_NotificationCompatBase_Action_Factory = new XAPeerMembers ("xamarin/test/NotificationCompatBase$Action$Factory", typeof (IFactoryInvoker));

				public IFactoryInvoker (IntPtr handle, JniHandleOwnership transfer) : base (handle, transfer)
				{
				}

				static Delegate cb_build_Build_I_Lxamarin_test_NotificationCompatBase_Action_;
#pragma warning disable 0169
				static Delegate GetBuild_IHandler ()
				{
					return cb_build_Build_I_Lxamarin_test_NotificationCompatBase_Action_ ??= new _JniMarshal_PPI_L (n_Build_I);
				}

				[global::System.Diagnostics.DebuggerDisableUserUnhandledExceptions]
				static IntPtr n_Build_I (IntPtr jnienv, IntPtr native__this, int p0)
				{
					if (!global::Java.Interop.JniEnvironment.BeginMarshalMethod (jnienv, out var __envp, out var __r))
						return default;

					try {
						var __this = global::Java.Lang.Object.GetObject<global::Xamarin.Test.NotificationCompatBase.Action.IFactory> (jnienv, native__this, JniHandleOwnership.DoNotTransfer);
						return JNIEnv.ToLocalJniHandle (__this.Build (p0));
					} catch (global::System.Exception __e) {
						__r.OnUserUnhandledException (ref __envp, __e);
						return default;
					} finally {
						global::Java.Interop.JniEnvironment.EndMarshalMethod (ref __envp);
					}
				}
#pragma warning restore 0169

				public unsafe global::Xamarin.Test.NotificationCompatBase.Action Build (int p0)
				{
					const string __id = "build.(I)Lxamarin/test/NotificationCompatBase$Action;";
					try {
						JniArgumentValue* __args = stackalloc JniArgumentValue [1];
						__args [0] = new JniArgumentValue (p0);
						var __rm = _members_xamarin_test_NotificationCompatBase_Action_Factory.InstanceMethods.InvokeAbstractObjectMethod (__id, this, __args);
						return global::Java.Lang.Object.GetObject<global::Xamarin.Test.NotificationCompatBase.Action> (__rm.Handle, JniHandleOwnership.TransferLocalRef);
					} finally {
					}
				}

			}

			static readonly JniPeerMembers _members = new XAPeerMembers ("xamarin/test/NotificationCompatBase$Action", typeof (Action));

			internal static new IntPtr class_ref {
				get { return _members.JniPeerType.PeerReference.Handle; }
			}

			[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
			[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
			public override global::Java.Interop.JniPeerMembers JniPeerMembers {
				get { return _members; }
			}

			[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
			[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
			protected override IntPtr ThresholdClass {
				get { return _members.JniPeerType.PeerReference.Handle; }
			}

			[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
			[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
			protected override global::System.Type ThresholdType {
				get { return _members.ManagedPeerType; }
			}

			protected Action (IntPtr javaReference, JniHandleOwnership transfer) : base (javaReference, transfer)
			{
			}

		}

		[global::Android.Runtime.Register ("xamarin/test/NotificationCompatBase$Action", DoNotGenerateAcw=true)]
		internal partial class ActionInvoker : Action {
			public ActionInvoker (IntPtr handle, JniHandleOwnership transfer) : base (handle, transfer)
			{
			}

			static readonly JniPeerMembers _members = new XAPeerMembers ("xamarin/test/NotificationCompatBase$Action", typeof (ActionInvoker));

			[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
			[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
			public override global::Java.Interop.JniPeerMembers JniPeerMembers {
				get { return _members; }
			}

			[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
			[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
			protected override global::System.Type ThresholdType {
				get { return _members.ManagedPeerType; }
			}

		}

		// Metadata.xml XPath class reference: path="/api/package[@name='xamarin.test']/class[@name='NotificationCompatBase.InstanceInner']"
		[global::Android.Runtime.Register ("xamarin/test/NotificationCompatBase$InstanceInner", DoNotGenerateAcw=true)]
		public abstract partial class InstanceInner : global::Java.Lang.Object {
			static readonly JniPeerMembers _members = new XAPeerMembers ("xamarin/test/NotificationCompatBase$InstanceInner", typeof (InstanceInner));

			internal static new IntPtr class_ref {
				get { return _members.JniPeerType.PeerReference.Handle; }
			}

			[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
			[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
			public override global::Java.Interop.JniPeerMembers JniPeerMembers {
				get { return _members; }
			}

			[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
			[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
			protected override IntPtr ThresholdClass {
				get { return _members.JniPeerType.PeerReference.Handle; }
			}

			[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
			[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
			protected override global::System.Type ThresholdType {
				get { return _members.ManagedPeerType; }
			}

			protected InstanceInner (IntPtr javaReference, JniHandleOwnership transfer) : base (javaReference, transfer)
			{
			}

			// Metadata.xml XPath constructor reference: path="/api/package[@name='xamarin.test']/class[@name='NotificationCompatBase.InstanceInner']/constructor[@name='NotificationCompatBase.InstanceInner' and count(parameter)=1 and parameter[1][@type='xamarin.test.NotificationCompatBase']]"
			[Register (".ctor", "(Lxamarin/test/NotificationCompatBase;)V", "")]
			public unsafe InstanceInner (global::Xamarin.Test.NotificationCompatBase __self) : base (IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
			{
				string __id = "(L" + global::Android.Runtime.JNIEnv.GetJniName (GetType ().DeclaringType) + ";)V";

				if (((global::Java.Lang.Object) this).Handle != IntPtr.Zero)
					return;

				try {
					JniArgumentValue* __args = stackalloc JniArgumentValue [1];
					__args [0] = new JniArgumentValue ((__self == null) ? IntPtr.Zero : ((global::Java.Lang.Object) __self).Handle);
					var __r = _members.InstanceMethods.StartCreateInstance (__id, ((object) this).GetType (), __args);
					SetHandle (__r.Handle, JniHandleOwnership.TransferLocalRef);
					_members.InstanceMethods.FinishCreateInstance (__id, this, __args);
				} finally {
					global::System.GC.KeepAlive (__self);
				}
			}

		}

		[global::Android.Runtime.Register ("xamarin/test/NotificationCompatBase$InstanceInner", DoNotGenerateAcw=true)]
		internal partial class InstanceInnerInvoker : InstanceInner {
			public InstanceInnerInvoker (IntPtr handle, JniHandleOwnership transfer) : base (handle, transfer)
			{
			}

			static readonly JniPeerMembers _members = new XAPeerMembers ("xamarin/test/NotificationCompatBase$InstanceInner", typeof (InstanceInnerInvoker));

			[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
			[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
			public override global::Java.Interop.JniPeerMembers JniPeerMembers {
				get { return _members; }
			}

			[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
			[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
			protected override global::System.Type ThresholdType {
				get { return _members.ManagedPeerType; }
			}

		}

		static readonly JniPeerMembers _members = new XAPeerMembers ("xamarin/test/NotificationCompatBase", typeof (NotificationCompatBase));

		internal static new IntPtr class_ref {
			get { return _members.JniPeerType.PeerReference.Handle; }
		}

		[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
		public override global::Java.Interop.JniPeerMembers JniPeerMembers {
			get { return _members; }
		}

		[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
		protected override IntPtr ThresholdClass {
			get { return _members.JniPeerType.PeerReference.Handle; }
		}

		[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
		protected override global::System.Type ThresholdType {
			get { return _members.ManagedPeerType; }
		}

		protected NotificationCompatBase (IntPtr javaReference, JniHandleOwnership transfer) : base (javaReference, transfer)
		{
		}

	}
}
