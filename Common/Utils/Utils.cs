using System;
using System.Text;

namespace Common.Utils
{
	public class Utils
	{
		public static string FromCamelCaseToUnderscoreLowerCase(string input, bool plural = true)
		{
			var nonPlural = System.Text.RegularExpressions.Regex.Replace(input, "(?<=.)([A-Z])", "_$0",
					  System.Text.RegularExpressions.RegexOptions.Compiled).ToLower();
			return new StringBuilder().Append(nonPlural).Append("s").ToString();
        }
	}
}