using System;

namespace Java.Interop
{
	public abstract class JniFieldID
	{
		public      IntPtr      ID      {get; private set;}

		internal    bool        IsValid {
			get {return ID != IntPtr.Zero;}
		}

		internal JniFieldID (IntPtr fieldID)
		{
			ID  = fieldID;
		}

		public override string ToString ()
		{
			return string.Format ("{0}(0x{1})", GetType ().FullName, ID.ToString ("x"));
		}
	}
}

