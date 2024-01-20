using System.Text.RegularExpressions;

namespace GameEngine.Utility.Parsing
{
	/// <summary>
	/// A class that parses data.
	/// </summary>
	public class Scanner
	{
		/// <summary>
		/// Creates a new parser that takes a stream.
		/// </summary>
		/// <param name="src">The input source.</param>
		/// <param name="delineators">The characters that split inputs.</param>
		/// <param name="ignore_empty">Determines if empty tokens are ignored.</param>
		/// <param name="identify_delineators">If true, then the left/right delineators of each entry will be identified for use.</param>
		public Scanner(Stream src, string[]? delineators = null, StringSplitOptions ignore_empty = StringSplitOptions.RemoveEmptyEntries, bool identify_delineators = false)
		{
			Delineators = delineators;
			IgnoresEmptyTokens = ignore_empty;
			DelineatorsIdentified = identify_delineators;

			using(StreamReader srtemp = new StreamReader(src))
				PrepareParser(srtemp.ReadToEnd());
			
			return;
		}

		/// <summary>
		/// Creates a new parser that takes a string.
		/// </summary>
		/// <param name="src">The input source.</param>
		/// <param name="delineators">The characters that split inputs.</param>
		/// <param name="ignore_empty">Determines if empty tokens are ignored.</param>
		/// <param name="identify_delineators">If true, then the left/right delineators of each entry will be identified for use.</param>
		public Scanner(string src, string[]? delineators = null, StringSplitOptions ignore_empty = StringSplitOptions.RemoveEmptyEntries, bool identify_delineators = false)
		{
			Delineators = delineators;
			IgnoresEmptyTokens = ignore_empty;
			DelineatorsIdentified = identify_delineators;

			PrepareParser(src);
			return;
		}

		/// <summary>
		/// Prepares the parser for parsing.
		/// </summary>
		/// <param name="input">The input string to parse.</param>
		protected void PrepareParser(string input)
		{
			// If we don't need to identify the delineators, this is a lot easier
			if(!DelineatorsIdentified)
			{
				parser = input.Split(Delineators,IgnoresEmptyTokens).Select<string,Func<Type,object>>(s => t => (s as IConvertible).ToType(t,System.Globalization.CultureInfo.InvariantCulture)).GetEnumerator();
				extended_parser = null;
			}
			else
			{
				string dl = "";
				
				foreach(string str in Delineators!)
					dl += str.Replace("[","\\[").Replace("]","\\]"); // If for some unresonable reason \[ is a delineator, for example, this doesn't error, so we're good

				// We split the string preserving the delineators on the right hand side for additional processing
				string[] splits = Regex.Split(input,"(?<=[" + dl + "])");

				// We now need to clean up our splits
				List<SplitString> ret = new List<SplitString>();
				string? last_d = null;

				foreach(string str in splits)
				{
					SplitString next = new SplitString(str,last_d,Delineators);
					last_d = next.RightDelineator;

					if(next.Value != "" || IgnoresEmptyTokens == StringSplitOptions.None)
						ret.Add(next);
				}

				parser = null;
				extended_parser = ret.Select(s => new Tuple<string?,Func<Type,object>,string?>(s.LeftDelineator,t => (s.Value as IConvertible).ToType(t,System.Globalization.CultureInfo.InvariantCulture),s.RightDelineator)).GetEnumerator();
			}
			
			HasNextItem = MoveNext();
			return;
		}

		/// <summary>
		/// Moves to the next item.
		/// </summary>
		/// <returns>Returns true if there is a next item.</returns>
		protected bool MoveNext()
		{
			if(DelineatorsIdentified)
				return extended_parser!.MoveNext();
			
			return parser!.MoveNext();
		}

		/// <summary>
		/// Peeks at the next item in the scanner.
		/// </summary>
		/// <typeparam name="T">The type we are expecting.</typeparam>
		/// <returns>Returns the default value of type T if the next item is not the provided type or if there are no more items.</returns>
		public T? PeekNext<T>()
		{
			T? ret = default(T);

			if(!HasNextItem)
				return ret;

			try
			{ret = DelineatorsIdentified ? (T?)extended_parser!.Current.Item2(typeof(T)) : (T?)parser!.Current(typeof(T));}
			catch
			{}

			return ret;
		}

		/// <summary>
		/// Peeks at the next item in the scanner plus its delineators.
		/// </summary>
		/// <typeparam name="T">The type we are expecting.</typeparam>
		/// <returns>Returns null if the next item is not the provided type or if there are no more items.</returns>
		public Tuple<string?,T,string?>? PeekNextPlus<T>()
		{
			if(!HasNextItem)
				return null;

			string? ret1 = null;
			T ret2;
			string? ret3 = null;

			if(DelineatorsIdentified)
			{
				ret1 = extended_parser!.Current.Item1;
				ret2 = (T)extended_parser!.Current.Item2(typeof(T));
				ret3 = extended_parser!.Current.Item3;
			}
			else
				ret2 = (T)parser!.Current(typeof(T));
			
			return new Tuple<string?,T,string?>(ret1,ret2,ret3);
		}

		/// <summary>
		/// Gets the next item in the scanner.
		/// </summary>
		/// <typeparam name="T">The type we are expecting.</typeparam>
		/// <returns>Returns the default value of type T if the next item is not the provided type or if there are no more items.</returns>
		public T? Next<T>()
		{
			T? ret = default(T);

			if(!HasNextItem)
				return ret;

			ret = DelineatorsIdentified ? (T)extended_parser!.Current.Item2(typeof(T)) : (T)parser!.Current(typeof(T));
			HasNextItem = MoveNext();

			return ret;
		}

		/// <summary>
		/// Gets the next item in the scanner plus its delineators.
		/// </summary>
		/// <typeparam name="T">The type we are expecting.</typeparam>
		/// <returns>Returns null if the next item is not the provided type or if there are no more items.</returns>
		public Tuple<string?,T,string?>? NextPlus<T>()
		{
			if(!HasNextItem)
				return null;

			string? ret1 = null;
			T ret2;
			string? ret3 = null;

			if(DelineatorsIdentified)
			{
				ret1 = extended_parser!.Current.Item1;
				ret2 = (T)extended_parser!.Current.Item2(typeof(T));
				ret3 = extended_parser!.Current.Item3;
			}
			else
				ret2 = (T)parser!.Current(typeof(T));

			HasNextItem = MoveNext();
			return new Tuple<string?,T,string?>(ret1,ret2,ret3);
		}

		/// <summary>
		/// Determines if the scanner's next item (if any) is of the given type.
		/// </summary>
		/// <typeparam name="T">The type to check for.</typeparam>
		/// <returns>Returns true if the next item is of the given type and false otherwise.</returns>
		public bool HasNext<T>()
		{
			if(!HasNextItem)
				return false;

			try
			{
				if(DelineatorsIdentified)
					extended_parser!.Current.Item2(typeof(T));
				else
					parser!.Current(typeof(T));
			}
			catch
			{return false;}

			return true;
		}

		/// <summary>
		/// If true then there are more inputs in the scanner.
		/// </summary>
		public bool HasNextItem
		{get; protected set;}

		/// <summary>
		/// The characters that split input.
		/// </summary>
		/// <remarks>When null is assigned to this field, it default to DefaultDelineators instead. As such, it is not possible to read null from this field..</remarks>
		public string[]? Delineators
		{
			get => _d;

			protected set
			{
				if(value == null)
					_d = DefaultDelineators;
				else
				{
					int c = 0;

					// Count the number of non null entries
					foreach(string? str in value)
						if(str is not null)
							c++;

					// We'll only keep non null entries
					_d = new string[c];

					// Copy non null entries into _d
					for(int i = 0,j = 0;i < value.Length;i++)
						_d[j++] = value[i];
				}

				return;
			}
		}

		protected string[]? _d;

		/// <summary>
		/// If true then empty tokens are ignored in this scanner.
		/// </summary>
		public StringSplitOptions IgnoresEmptyTokens
		{get; protected set;}

		/// <summary>
		/// If true, then the delineators of each entry are available.
		/// </summary>
		public bool DelineatorsIdentified
		{get; protected set;}

		/// <summary>
		/// The parser for this scanner.
		/// </summary>
		protected IEnumerator<Func<Type,object>>? parser;

		/// <summary>
		/// The extended parser for this canner.
		/// </summary>
		protected IEnumerator<Tuple<string?,Func<Type,object>,string?>>? extended_parser;

		/// <summary>
		/// The default delineators if none are given.
		/// </summary>
		protected readonly static string[] DefaultDelineators = new string[] {" ","\t","\n","\r"};

		/// <summary>
		/// Represents a string split from a larger string.
		/// </summary>
		protected class SplitString
		{
			/// <summary>
			/// Constructs a new split string.
			/// </summary>
			/// <param name="str">The string that was split.</param>
			/// <param name="ld">The left delineator.</param>
			/// <param name="rd">The right delineator.</param>
			public SplitString(string str, string? ld, string? rd)
			{
				Value = str;

				LeftDelineator = ld;
				RightDelineator = rd;

				return;
			}

			/// <summary>
			/// Constructs a new split string.
			/// </summary>
			/// <param name="str">The string plus its right delineator (if it exists).</param>
			/// <param name="ld">The left delineator.</param>
			/// <param name="delineators">The possible delineators.</param>
			public SplitString(string str, string? ld, string[] delineators)
			{
				LeftDelineator = ld;
				Value = str;
				RightDelineator = null;

				foreach(string d in delineators)
					if(str.EndsWith(d))
					{
						Value = str.Substring(0,str.Length - d.Length);
						RightDelineator = d;

						break;
					}

				return;
			}

			/// <summary>
			/// The string.
			/// </summary>
			public string Value
			{get; protected set;}

			/// <summary>
			/// The left delineator (or null if none exists).
			/// </summary>
			public string? LeftDelineator
			{get; protected set;}

			/// <summary>
			/// The right delineator (or null if none exists).
			/// </summary>
			public string? RightDelineator
			{get; protected set;}
		}
	}
}