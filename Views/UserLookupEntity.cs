using Vabulu.Attributes;
using Vabulu.Tables;

namespace Vabulu.Views
{
    [View("Users")]
    public class UserLookupEntity : UserData
    {
        public string UserName { get; set; }
    }
}