
using System.Text.Json.Serialization;

namespace PM.SharedKernel;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IncludeOption
{
    Accounts,
    Holdings,
    Transactions,
    Tags
}
