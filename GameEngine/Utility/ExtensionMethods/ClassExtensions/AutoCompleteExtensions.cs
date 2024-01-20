using System;

namespace GameEngine.Utility.ExtensionMethods.ClassExtensions
{
	/// <summary>
	/// The string extension to perform auto completion.
	/// </summary>
	public static class AutoCompleteExtensions
	{
		/// <summary>
		/// Returns the string which is the longest common prefix of all possibilities for which this string is a prefix.
		/// </summary>
		/// <param name="str">The prefix string.</param>
		/// <param name="possibilities">The list of possible strings to auto complete to.</param>
		/// <returns>
		///	Returns the longest common prefix w of each element of <paramref name="possibilities"/> for which <paramref name="str"/> is a prefix.
		/// </returns>
		public static string AutoComplete(this string str, IEnumerable<string> possibilities)
		{
			List<string> valids = new List<string>();
			int shortest_length = int.MaxValue;

			// Check if we have a prefix for any commands
			foreach(string s in possibilities)
				if(s.StartsWith(str))
				{
					valids.Add(s);
					shortest_length = Math.Min(shortest_length,s.Length);
				}
			
			// We know that starting string is a prefix of everything, so start there
			string lcp = str;

			// If there's at least one possibility, find out what the longest common prefix is of them all
			if(valids.Count > 0)
			{
				for(int i = lcp.Length;i < shortest_length;i++)
				{
					char next = valids[0][i]; // There is at least one possibility, so we're safe to do this
					bool invalid = false;

					foreach(string s in valids)
						if(s[i] != next)
							invalid = true;

					// If this character is not a prefix of every possibility, we're done
					if(invalid)
						break;

					// Otherwise, go ahead and append the string with this extended prefix
					lcp += next;
				}
			}

			return lcp;
		}
	}
}
