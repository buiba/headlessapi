using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Serialization.Internal;
using System.Linq;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Internal
{
	public class ReflectionHelperTest
	{
		public class GetDerivedTypes : ReflectionHelperTest
		{
			[Fact]
			public void It_should_return_all_concrete_derived_types_if_existed()
			{
				var derivedTypes = ReflectionHelper.GetConcreteDerivedTypes(typeof(MyInterface));

				Assert.Contains(derivedTypes, t => t.Name == typeof(MyChildClass).Name);
				Assert.Contains(derivedTypes, t => t.Name == typeof(MyDeeperChildClass).Name);
			}

			[Fact]
			public void It_should_return_empty_derived_types_if_not_existed()
			{
				var derivedTypes = ReflectionHelper.GetConcreteDerivedTypes(typeof(MyNeverInterface));
				Assert.Empty(derivedTypes);
			}
		}

		public class GetParentTypes : ReflectionHelperTest
		{
			[Fact]
			public void It_should_return_all_parent_types_if_existed()
			{
				var parentTypes = ReflectionHelper.GetParentTypes(typeof(MyChildClass));
				Assert.Contains(parentTypes, t => t.Name == typeof(MyBaseClass<>).Name);
				Assert.Contains(parentTypes, t => t.Name == typeof(MyGenericInterface<>).Name);
				Assert.Contains(parentTypes, t => t.Name == typeof(MyInterface).Name);
				Assert.Contains(parentTypes, t => t.Name == typeof(object).Name);
			}

			[Fact]
			public void It_should_return_empty_parent_types_if_not_existed()
			{
				var parentTypes = ReflectionHelper.GetParentTypes(typeof(object));
				Assert.Empty(parentTypes);
			}
		}

		public class GetParentGenericTypes : ReflectionHelperTest
		{
			[Fact]
			public void It_should_return_all_generic_parent_types_if_existed()
			{
				var parentTypes = ReflectionHelper.GetParentGenericTypes(typeof(MyChildClass));
				Assert.Contains(parentTypes, t => t.Name == typeof(MyBaseClass<>).Name);
				Assert.Contains(parentTypes, t => t.Name == typeof(MyGenericInterface<>).Name);
				Assert.Equal(2, parentTypes.Count());
			}

			[Fact]
			public void It_should_return_empty_generic_parent_types_if_not_existed()
			{
				var parentTypes = ReflectionHelper.GetParentGenericTypes(typeof(object));
				Assert.Empty(parentTypes);
			}
		}
	}

	internal interface MyInterface { }
	internal interface MyGenericInterface<T> : MyInterface where T : class { }
	public abstract class MyBaseClass<T> : MyGenericInterface<T> where T : class { }
	public class MyChildClass : MyBaseClass<object> { }
	public class MyDeeperChildClass : MyBaseClass<object> { }

	internal interface MyNeverInterface { }
}
