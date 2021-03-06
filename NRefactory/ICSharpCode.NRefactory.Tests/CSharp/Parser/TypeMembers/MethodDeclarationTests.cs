﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;

namespace ICSharpCode.NRefactory.CSharp.Parser.TypeMembers
{
	[TestFixture]
	public class MethodDeclarationTests
	{
		[Test]
		public void SimpleMethodDeclarationTest()
		{
			MethodDeclaration md = ParseUtilCSharp.ParseTypeMember<MethodDeclaration>("void MyMethod() {} ");
			Assert.AreEqual("void", ((PrimitiveType)md.ReturnType).Keyword);
			Assert.AreEqual(0, md.Parameters.Count());
			Assert.IsFalse(md.IsExtensionMethod);
		}
		
		[Test]
		public void AbstractMethodDeclarationTest()
		{
			MethodDeclaration md = ParseUtilCSharp.ParseTypeMember<MethodDeclaration>("abstract void MyMethod();");
			Assert.AreEqual("void", ((PrimitiveType)md.ReturnType).Keyword);
			Assert.AreEqual(0, md.Parameters.Count());
			Assert.IsFalse(md.IsExtensionMethod);
			Assert.IsTrue(md.Body.IsNull);
			Assert.AreEqual(Modifiers.Abstract, md.Modifiers);
		}
		
		[Test]
		public void DefiningPartialMethodDeclarationTest()
		{
			MethodDeclaration md = ParseUtilCSharp.ParseTypeMember<MethodDeclaration>("partial void MyMethod();");
			Assert.AreEqual("void", ((PrimitiveType)md.ReturnType).Keyword);
			Assert.AreEqual(0, md.Parameters.Count());
			Assert.IsFalse(md.IsExtensionMethod);
			Assert.IsTrue(md.Body.IsNull);
			Assert.AreEqual(Modifiers.Partial, md.Modifiers);
		}
		
		[Test]
		public void ImplementingPartialMethodDeclarationTest()
		{
			MethodDeclaration md = ParseUtilCSharp.ParseTypeMember<MethodDeclaration>("partial void MyMethod() { }");
			Assert.AreEqual("void", ((PrimitiveType)md.ReturnType).Keyword);
			Assert.AreEqual(0, md.Parameters.Count());
			Assert.IsFalse(md.IsExtensionMethod);
			Assert.IsFalse(md.Body.IsNull);
			Assert.AreEqual(Modifiers.Partial, md.Modifiers);
		}
		
		[Test]
		public void SimpleMethodRegionTest()
		{
			const string program = @"
		void MyMethod()
		{
			OtherMethod();
		}
";
			MethodDeclaration md = ParseUtilCSharp.ParseTypeMember<MethodDeclaration>(program);
			Assert.AreEqual(2, md.StartLocation.Line, "StartLocation.Y");
			Assert.AreEqual(5, md.EndLocation.Line, "EndLocation.Y");
			Assert.AreEqual(3, md.StartLocation.Column, "StartLocation.X");
			Assert.AreEqual(4, md.EndLocation.Column, "EndLocation.X");
		}
		
		[Test]
		public void MethodWithModifiersRegionTest()
		{
			const string program = @"
		public static void MyMethod()
		{
			OtherMethod();
		}
";
			MethodDeclaration md = ParseUtilCSharp.ParseTypeMember<MethodDeclaration>(program);
			Assert.AreEqual(2, md.StartLocation.Line, "StartLocation.Y");
			Assert.AreEqual(5, md.EndLocation.Line, "EndLocation.Y");
			Assert.AreEqual(3, md.StartLocation.Column, "StartLocation.X");
			Assert.AreEqual(4, md.EndLocation.Column, "EndLocation.X");
		}
		
		[Test]
		public void MethodWithUnnamedParameterDeclarationTest()
		{
			MethodDeclaration md = ParseUtilCSharp.ParseTypeMember<MethodDeclaration>("void MyMethod(int) {} ", true);
			Assert.AreEqual("void", md.ReturnType.ToString ());
			Assert.AreEqual(1, md.Parameters.Count());
			Assert.AreEqual("int", ((PrimitiveType)md.Parameters.Single().Type).Keyword);
		}
		
		[Test]
		public void GenericVoidMethodDeclarationTest()
		{
			ParseUtilCSharp.AssertTypeMember(
				"void MyMethod<T>(T a) {} ",
				new MethodDeclaration {
					ReturnType = new PrimitiveType("void"),
					Name = "MyMethod",
					TypeParameters = { new TypeParameterDeclaration { Name = "T" } },
					Parameters = { new ParameterDeclaration(new SimpleType("T"), "a") },
					Body = new BlockStatement()
				});
		}
		
		[Test]
		public void GenericMethodDeclarationTest()
		{
			ParseUtilCSharp.AssertTypeMember(
				"T MyMethod<T>(T a) {} ",
				new MethodDeclaration {
					ReturnType = new SimpleType("T"),
					Name = "MyMethod",
					TypeParameters = { new TypeParameterDeclaration { Name = "T" } },
					Parameters = { new ParameterDeclaration(new SimpleType("T"), "a") },
					Body = new BlockStatement()
				});
		}
		
		[Test]
		public void GenericMethodDeclarationWithConstraintTest()
		{
			ParseUtilCSharp.AssertTypeMember(
				"T MyMethod<T>(T a) where T : ISomeInterface {} ",
				new MethodDeclaration {
					ReturnType = new SimpleType("T"),
					Name = "MyMethod",
					TypeParameters = { new TypeParameterDeclaration { Name = "T" } },
					Parameters = { new ParameterDeclaration(new SimpleType("T"), "a") },
					Constraints = {
						new Constraint {
							TypeParameter = "T",
							BaseTypes = { new SimpleType("ISomeInterface") }
						}
					},
					Body = new BlockStatement()
				});
		}
		
		[Test]
		public void GenericMethodInInterface()
		{
			ParseUtilCSharp.AssertGlobal(
				@"interface MyInterface {
	T MyMethod<T>(T a) where T : ISomeInterface;
}
",
				new TypeDeclaration {
					ClassType = ClassType.Interface,
					Members = {
						new MethodDeclaration {
							ReturnType = new SimpleType("T"),
							Name = "MyMethod",
							TypeParameters = { new TypeParameterDeclaration { Name = "T" } },
							Parameters = { new ParameterDeclaration(new SimpleType("T"), "a") },
							Constraints = {
								new Constraint {
									TypeParameter = "T",
									BaseTypes = { new SimpleType("ISomeInterface") }
								}
							}
						}}});
		}
		
		[Test]
		public void GenericVoidMethodInInterface()
		{
			ParseUtilCSharp.AssertGlobal(
				@"interface MyInterface {
	void MyMethod<T>(T a) where T : ISomeInterface;
}
",
				new TypeDeclaration {
					ClassType = ClassType.Interface,
					Members = {
						new MethodDeclaration {
							ReturnType = new PrimitiveType("void"),
							Name = "MyMethod",
							TypeParameters = { new TypeParameterDeclaration { Name = "T" } },
							Parameters = { new ParameterDeclaration(new SimpleType("T"), "a") },
							Constraints = {
								new Constraint {
									TypeParameter = "T",
									BaseTypes = { new SimpleType("ISomeInterface") }
								}
							}
						}}});
		}
		
		[Test]
		public void ShadowingMethodInInterface()
		{
			ParseUtilCSharp.AssertGlobal(
				@"interface MyInterface : IDisposable {
	new void Dispose();
}
",
				new TypeDeclaration {
					ClassType = ClassType.Interface,
					Name = "MyInterface",
					BaseTypes = { new SimpleType("IDisposable") },
					Members = {
						new MethodDeclaration {
							Modifiers = Modifiers.New,
							ReturnType = new PrimitiveType("void"),
							Name = "Dispose"
						}}});
		}
		
		[Test]
		public void MethodImplementingInterfaceTest()
		{
			ParseUtilCSharp.AssertTypeMember(
				"int MyInterface.MyMethod() {} ",
				new MethodDeclaration {
					ReturnType = new PrimitiveType("int"),
					PrivateImplementationType = new SimpleType("MyInterface"),
					Name = "MyMethod",
					Body = new BlockStatement()
				});
		}
		
		[Test]
		public void MethodImplementingGenericInterfaceTest()
		{
			ParseUtilCSharp.AssertTypeMember(
				"int MyInterface<string>.MyMethod() {} ",
				new MethodDeclaration {
					ReturnType = new PrimitiveType("int"),
					PrivateImplementationType = new SimpleType("MyInterface") { TypeArguments = { new PrimitiveType("string") } },
					Name = "MyMethod",
					Body = new BlockStatement()
				});
		}
		
		[Test]
		public void VoidMethodImplementingInterfaceTest()
		{
			ParseUtilCSharp.AssertTypeMember (
				"void MyInterface.MyMethod() {} ",
				new MethodDeclaration {
					ReturnType = new PrimitiveType("void"),
					PrivateImplementationType = new SimpleType("MyInterface"),
					Name = "MyMethod",
					Body = new BlockStatement()
				});
		}
		
		[Test]
		public void VoidMethodImplementingGenericInterfaceTest()
		{
			ParseUtilCSharp.AssertTypeMember (
				"void MyInterface<string>.MyMethod() {} ",
				new MethodDeclaration {
					ReturnType = new PrimitiveType("void"),
					PrivateImplementationType = new SimpleType("MyInterface") { TypeArguments = { new PrimitiveType("string") } },
					Name = "MyMethod",
					Body = new BlockStatement()
				});
		}
		
		[Test]
		public void IncompleteConstraintsTest()
		{
			MethodDeclaration md = ParseUtilCSharp.ParseTypeMember<MethodDeclaration>(
				"void a<T>() where T { }", true // expect errors
			);
			Assert.AreEqual("a", md.Name);
			Assert.AreEqual(1, md.TypeParameters.Count);
			Assert.AreEqual("T", md.TypeParameters.Single().Name);
			Assert.AreEqual(0, md.Constraints.Count());
		}
		
		[Test]
		public void ExtensionMethodTest()
		{
			MethodDeclaration md = ParseUtilCSharp.ParseTypeMember<MethodDeclaration>(
				"public static int ToInt32(this string s) { return int.Parse(s); }"
			);
			Assert.AreEqual("ToInt32", md.Name);
			Assert.AreEqual("s", md.Parameters.First().Name);
			Assert.AreEqual(ParameterModifier.This, md.Parameters.First().ParameterModifier);
			Assert.AreEqual("string", ((PrimitiveType)md.Parameters.First().Type).Keyword);
			Assert.IsTrue(md.IsExtensionMethod);
		}
		
		[Test]
		public void VoidExtensionMethodTest()
		{
			MethodDeclaration md = ParseUtilCSharp.ParseTypeMember<MethodDeclaration>(
				"public static void Print(this string s) { Console.WriteLine(s); }"
			);
			Assert.AreEqual("Print", md.Name);
			Assert.AreEqual("s", md.Parameters.First().Name);
			Assert.AreEqual(ParameterModifier.This, md.Parameters.First().ParameterModifier);
			Assert.AreEqual("string", ((PrimitiveType)md.Parameters.First().Type).Keyword);
			Assert.IsTrue(md.IsExtensionMethod);
		}
		
		[Test]
		public void MethodWithEmptyAssignmentErrorInBody()
		{
			MethodDeclaration md = ParseUtilCSharp.ParseTypeMember<MethodDeclaration>(
				"void A ()\n" +
				"{\n" +
				"int a = 3;\n" +
				" = 4;\n" +
				"}", true // expect errors
			);
			Assert.AreEqual("A", md.Name);
			Assert.AreEqual(new AstLocation(2, 1), md.Body.StartLocation);
			Assert.AreEqual(new AstLocation(5, 2), md.Body.EndLocation);
		}
		
		[Test]
		public void OptionalParameterTest()
		{
			ParseUtilCSharp.AssertTypeMember(
				"public void Foo(string bar = null, int baz = 0) { }",
				new MethodDeclaration {
					Modifiers = Modifiers.Public,
					ReturnType = new PrimitiveType("void"),
					Name = "Foo",
					Body = new BlockStatement(),
					Parameters = {
						new ParameterDeclaration {
							Type = new PrimitiveType("string"),
							Name = "bar",
							DefaultExpression = new NullReferenceExpression()
						},
						new ParameterDeclaration {
							Type = new PrimitiveType("int"),
							Name = "baz",
							DefaultExpression = new PrimitiveExpression(0)
						}
					}});
		}
	}
}
