
using System.Text.RegularExpressions;

namespace Bookkeeping.Common
{
    public class Constants
    {
        public const string DbsidClaimType = "dbsid";
        public const string ClientIdClaimType = "clientId";

        public const int MinPasswordLength = 4;
        public const int InnLengthLegal = 12;
        public const int InnLengthIndividual= 10;

        public readonly static Regex InnRegex = new Regex(@"^[0-9]+$", RegexOptions.Compiled);
    }
}
