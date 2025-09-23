using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Utility.ExtensionMethods.SerializationExtensions
{
	/// <summary>
	/// Creates JSON converters with generic parameters.
	/// </summary>
	public abstract class JsonBaseConverterFactory : JsonConverterFactory
	{
		/// <summary>
		/// Creates the factory.
		/// </summary>
		/// <param name="args_func">A function that builds the arguments to pass to a converter constructor.</param>
		/// <param name="t">The raw unbound generic type (e.g. Queue&lt;&gt;).</param>
		/// <param name="c">
		///	The raw unbound generic type of the converter for <paramref name="t"/> (e.g. QueueConverter&lt;&gt;).
		///	<para/>
		///	This type must extend JsonConverter&lt;<paramref name="t"/>&gt; with an appropriate generic parameter(s) (e.g. QueueConverter&lt;E&gt; : JsonConverter&lt;Queue&lt;E&gt;&gt;).
		/// </param>
		protected JsonBaseConverterFactory(JsonArgumentBuilder args_func, Type t, Type c)
		{
			T = t;
			
			C = c;
			CArgs = C.GetGenericArguments();

			Arguments = args_func;
			return;
		}

		public override bool CanConvert(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == T;

		public override JsonConverter? CreateConverter(Type t, JsonSerializerOptions ops)
		{
			Type[] t_args = t.GetGenericArguments();

			if(CArgs.Length != t_args.Length)
				throw new JsonException();

			return (JsonConverter?)Activator.CreateInstance(C.MakeGenericType(t_args),BindingFlags.Instance | BindingFlags.Public,null,Arguments(t,ops),null);
		}

		/// <summary>
		/// This generates constructor arguments for new converters.
		/// </summary>
		protected JsonArgumentBuilder Arguments
		{get;}

		/// <summary>
		/// This is the raw unbound generic type of what will be converted (e.g. Queue&lt;&gt;).
		/// </summary>
		protected Type T
		{get;}

		/// <summary>
		/// This is the raw unbound generic type of the converter to construct (e.g. QueueConverter&lt;&gt;).
		/// <para/>
		/// This type must extend JsonConverter&lt;<see cref="T"/>&gt; with an appropriate generic parameter(s) (e.g. QueueConverter&lt;E&gt; : JsonConverter&lt;Queue&lt;E&gt;&gt;).
		/// </summary>
		protected Type C
		{get;}

		/// <summary>
		/// This is the list of generic arguments for <see cref="C"/>.
		/// </summary>
		protected Type[] CArgs
		{get;}
	}

	/// <summary>
	/// Represents a function that builds arguments to pass to a converter constructor within a JsonBaseConverterFactory.
	/// </summary>
	/// <param name="t">The type for which a converter is being constructed.</param>
	/// <param name="ops">The serializer options at construction time.</param>
	/// <returns>Returns the argument list. This should be an empty list if no arguments are required.</returns>
	public delegate object?[] JsonArgumentBuilder(Type t, JsonSerializerOptions ops);
}
