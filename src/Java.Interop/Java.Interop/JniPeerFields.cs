﻿namespace Java.Interop {

	partial class JniPeerInstanceFields {

		public bool GetBooleanValue (
			string encodedMember,
			IJavaObject self)
		{
			JniPeerMembers.AssertSelf (self);

			return GetFieldID (encodedMember)
				.GetBooleanValue (self.PeerReference);
		}

		public void SetValue (string encodedMember, IJavaObject self, bool value)
		{
			JniPeerMembers.AssertSelf (self);

			GetFieldID (encodedMember)
				.SetValue (self.PeerReference, value);
		}

		public sbyte GetByteValue (
			string encodedMember,
			IJavaObject self)
		{
			JniPeerMembers.AssertSelf (self);

			return GetFieldID (encodedMember)
				.GetByteValue (self.PeerReference);
		}

		public void SetValue (string encodedMember, IJavaObject self, sbyte value)
		{
			JniPeerMembers.AssertSelf (self);

			GetFieldID (encodedMember)
				.SetValue (self.PeerReference, value);
		}

		public char GetCharValue (
			string encodedMember,
			IJavaObject self)
		{
			JniPeerMembers.AssertSelf (self);

			return GetFieldID (encodedMember)
				.GetCharValue (self.PeerReference);
		}

		public void SetValue (string encodedMember, IJavaObject self, char value)
		{
			JniPeerMembers.AssertSelf (self);

			GetFieldID (encodedMember)
				.SetValue (self.PeerReference, value);
		}

		public short GetInt16Value (
			string encodedMember,
			IJavaObject self)
		{
			JniPeerMembers.AssertSelf (self);

			return GetFieldID (encodedMember)
				.GetInt16Value (self.PeerReference);
		}

		public void SetValue (string encodedMember, IJavaObject self, short value)
		{
			JniPeerMembers.AssertSelf (self);

			GetFieldID (encodedMember)
				.SetValue (self.PeerReference, value);
		}

		public int GetInt32Value (
			string encodedMember,
			IJavaObject self)
		{
			JniPeerMembers.AssertSelf (self);

			return GetFieldID (encodedMember)
				.GetInt32Value (self.PeerReference);
		}

		public void SetValue (string encodedMember, IJavaObject self, int value)
		{
			JniPeerMembers.AssertSelf (self);

			GetFieldID (encodedMember)
				.SetValue (self.PeerReference, value);
		}

		public long GetInt64Value (
			string encodedMember,
			IJavaObject self)
		{
			JniPeerMembers.AssertSelf (self);

			return GetFieldID (encodedMember)
				.GetInt64Value (self.PeerReference);
		}

		public void SetValue (string encodedMember, IJavaObject self, long value)
		{
			JniPeerMembers.AssertSelf (self);

			GetFieldID (encodedMember)
				.SetValue (self.PeerReference, value);
		}

		public float GetSingleValue (
			string encodedMember,
			IJavaObject self)
		{
			JniPeerMembers.AssertSelf (self);

			return GetFieldID (encodedMember)
				.GetSingleValue (self.PeerReference);
		}

		public void SetValue (string encodedMember, IJavaObject self, float value)
		{
			JniPeerMembers.AssertSelf (self);

			GetFieldID (encodedMember)
				.SetValue (self.PeerReference, value);
		}

		public double GetDoubleValue (
			string encodedMember,
			IJavaObject self)
		{
			JniPeerMembers.AssertSelf (self);

			return GetFieldID (encodedMember)
				.GetDoubleValue (self.PeerReference);
		}

		public void SetValue (string encodedMember, IJavaObject self, double value)
		{
			JniPeerMembers.AssertSelf (self);

			GetFieldID (encodedMember)
				.SetValue (self.PeerReference, value);
		}

		public JniObjectReference GetObjectValue (
			string encodedMember,
			IJavaObject self)
		{
			JniPeerMembers.AssertSelf (self);

			return GetFieldID (encodedMember)
				.GetObjectValue (self.PeerReference);
		}

		public void SetValue (string encodedMember, IJavaObject self, JniObjectReference value)
		{
			JniPeerMembers.AssertSelf (self);

			GetFieldID (encodedMember)
				.SetValue (self.PeerReference, value);
		}
	}

	partial class JniPeerStaticFields {

		public bool GetBooleanValue (string encodedMember)
		{
			return GetFieldID (encodedMember)
				.GetBooleanValue (Members.JniPeerType.PeerReference);
		}

		public void SetValue (string encodedMember, bool value)
		{
			GetFieldID (encodedMember)
				.SetValue (Members.JniPeerType.PeerReference, value);
		}

		public sbyte GetByteValue (string encodedMember)
		{
			return GetFieldID (encodedMember)
				.GetByteValue (Members.JniPeerType.PeerReference);
		}

		public void SetValue (string encodedMember, sbyte value)
		{
			GetFieldID (encodedMember)
				.SetValue (Members.JniPeerType.PeerReference, value);
		}

		public char GetCharValue (string encodedMember)
		{
			return GetFieldID (encodedMember)
				.GetCharValue (Members.JniPeerType.PeerReference);
		}

		public void SetValue (string encodedMember, char value)
		{
			GetFieldID (encodedMember)
				.SetValue (Members.JniPeerType.PeerReference, value);
		}

		public short GetInt16Value (string encodedMember)
		{
			return GetFieldID (encodedMember)
				.GetInt16Value (Members.JniPeerType.PeerReference);
		}

		public void SetValue (string encodedMember, short value)
		{
			GetFieldID (encodedMember)
				.SetValue (Members.JniPeerType.PeerReference, value);
		}

		public int GetInt32Value (string encodedMember)
		{
			return GetFieldID (encodedMember)
				.GetInt32Value (Members.JniPeerType.PeerReference);
		}

		public void SetValue (string encodedMember, int value)
		{
			GetFieldID (encodedMember)
				.SetValue (Members.JniPeerType.PeerReference, value);
		}

		public long GetInt64Value (string encodedMember)
		{
			return GetFieldID (encodedMember)
				.GetInt64Value (Members.JniPeerType.PeerReference);
		}

		public void SetValue (string encodedMember, long value)
		{
			GetFieldID (encodedMember)
				.SetValue (Members.JniPeerType.PeerReference, value);
		}

		public float GetSingleValue (string encodedMember)
		{
			return GetFieldID (encodedMember)
				.GetSingleValue (Members.JniPeerType.PeerReference);
		}

		public void SetValue (string encodedMember, float value)
		{
			GetFieldID (encodedMember)
				.SetValue (Members.JniPeerType.PeerReference, value);
		}

		public double GetDoubleValue (string encodedMember)
		{
			return GetFieldID (encodedMember)
				.GetDoubleValue (Members.JniPeerType.PeerReference);
		}

		public void SetValue (string encodedMember, double value)
		{
			GetFieldID (encodedMember)
				.SetValue (Members.JniPeerType.PeerReference, value);
		}

		public JniObjectReference GetObjectValue (string encodedMember)
		{
			return GetFieldID (encodedMember)
				.GetObjectValue (Members.JniPeerType.PeerReference);
		}

		public void SetValue (string encodedMember, JniObjectReference value)
		{
			GetFieldID (encodedMember)
				.SetValue (Members.JniPeerType.PeerReference, value);
		}
	}
}
