using Microsoft.WindowsAzure.Storage.Table;
using Vabulu.Attributes;

namespace Vabulu.Tables
{
    [Table("Tokens")]
    public class Token : UserData
    {
        public string Name { get; set; }
        public string Value { get; set; }

        [IgnoreProperty]
        [RowKey]
        public string LoginProvider { get; set; }
        public Token() { }
    }
}