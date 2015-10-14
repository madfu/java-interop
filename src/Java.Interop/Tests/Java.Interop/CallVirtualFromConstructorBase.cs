﻿using System;

using Java.Interop;
using Java.Interop.GenericMarshaler;

namespace Java.InteropTests {

	[JniTypeInfo (CallVirtualFromConstructorBase.JniTypeName)]
	public class CallVirtualFromConstructorBase : JavaObject {

		internal    const   string          JniTypeName = "com/xamarin/interop/CallVirtualFromConstructorBase";
		readonly    static  JniPeerMembers  _members    = new JniPeerMembers (JniTypeName, typeof (CallVirtualFromConstructorBase));

		public override JniPeerMembers JniPeerMembers {
			get {return _members;}
		}

		public unsafe CallVirtualFromConstructorBase (int value)
			: base (ref *InvalidJniObjectReference, JniHandleOwnership.Invalid)
		{
			var peer    = JniPeerMembers.InstanceMethods.StartGenericCreateInstance ("(I)V", GetType (), value);
			using (SetPeerReference (
						ref peer,
						JniHandleOwnership.Transfer)) {
				JniPeerMembers.InstanceMethods.FinishGenericCreateInstance ("(I)V", this, value);
			}
		}

		public virtual void CalledFromConstructor (int value)
		{
			_members.InstanceMethods.CallGenericVoidMethod ("calledFromConstructor\u0000(I)V", this, value);
		}
	}
}

