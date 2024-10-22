﻿using NUnit.Framework;
using System;

namespace generatortests
{
	[TestFixture]
	public class Java_Lang_Enum : BaseGeneratorTest
	{
		protected override bool TryJavaInterop1 => true;

		[Test]
		public void Generated_OK ()
		{
			RunAllTargets (
					outputRelativePath:     "java.lang.Enum",
					apiDescriptionFile:     "expected.ji/java.lang.Enum/Java.Lang.Enum.xml",
					expectedRelativePath:   "java.lang.Enum");
		}
	}
}

