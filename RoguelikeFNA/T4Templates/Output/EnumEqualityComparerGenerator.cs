



using System.Collections.Generic;


namespace Nez
{
	public class StringSplitOptionsComparer : IEqualityComparer<System.StringSplitOptions>
	{
		static public readonly StringSplitOptionsComparer defaultStringSplitOptionsComparer = new StringSplitOptionsComparer();

		public bool Equals( System.StringSplitOptions x, System.StringSplitOptions y )
		{
			return x == y;
		}


		public int GetHashCode( System.StringSplitOptions b )
		{
			return (int)b;
		}
	}



}