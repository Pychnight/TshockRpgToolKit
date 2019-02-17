using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using TShockAPI;

namespace Corruption.PluginSupport
{
	public static class OutputExtensions
	{
		public static IEnumerable<T> GetPage<T>(this IEnumerable<T> items, int page, int itemsPerPage)
		{
			if( items == null )
				throw new ArgumentNullException(nameof(items));

			if( page < 0 )
				throw new ArgumentOutOfRangeException(nameof(page));

			if( itemsPerPage < 1 )
				throw new ArgumentOutOfRangeException(nameof(itemsPerPage));
			
			int itemsToSkip = page * itemsPerPage;

			var skipped = items.Skip(itemsToSkip);
			var taken = skipped.Take(itemsPerPage);

			return taken;
		}

		public static int PageCount<T>(this IEnumerable<T> items, int itemsPerPage)
		{
			if( items == null )
				throw new ArgumentNullException(nameof(items));
							
			if( itemsPerPage < 1 )
				throw new ArgumentOutOfRangeException(nameof(itemsPerPage));

			var itemCount = items.Count();
			var pageCount = itemCount / itemsPerPage;
			pageCount += itemCount % itemsPerPage > 0 ? 1 : 0;

			return pageCount;
		}
	}
}
