using System.Diagnostics;
using TShockAPI;

namespace NpcShops
{
    /// <summary>
    ///     Provides extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     Gets the escaped name for the specified player, suitable for use in commands.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <returns>The escaped name.</returns>
        public static string GetEscapedName(this TSPlayer player)
        {
            Debug.Assert(player != null, "Player must not be null.");

            // First, we need to replace all instances of \\ with \\\\. This is because otherwise, the TShock command
            // system would treat the \\ as an escaped \. Then we need to replace \" with \\" and \(space) with
            // \\(space). Then we escape quotes.
            var name = player.Name.Replace(@"\\", @"\\\\");
            name = name.Replace(@"\""", @"\\""");
            name = name.Replace(@"\ ", @"\\ ");
            name = name.Replace(@"""", @"\""");
            return name;
        }
    }
}
