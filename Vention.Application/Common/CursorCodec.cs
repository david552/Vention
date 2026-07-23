using System.Globalization;
using System.Text;
namespace Vention.Application.Common
{
    public static class CursorCodec
    {
        public static string Encode(DateTimeOffset sortValue, long sequence)
        {
            var raw = $"{sortValue:O}|{sequence}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        }
        public static (DateTimeOffset SortValue, long Sequence) Decode(string cursor)
        {
            try
            {
                var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
                var sep = raw.LastIndexOf('|');
                if (sep < 0) throw new FormatException();
                var sortValue = DateTimeOffset.Parse(
                    raw[..sep], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                var sequence = long.Parse(raw[(sep + 1)..], CultureInfo.InvariantCulture);
                return (sortValue, sequence);
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException or OverflowException)
            {
                throw new ArgumentException("Invalid cursor.", nameof(cursor), ex);
            }
        }
        public static int NormalizePageSize(int? pageSize, int defaultSize = 50, int maxSize = 100)
            => Math.Clamp(pageSize ?? defaultSize, 1, maxSize);
    }
}