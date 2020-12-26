using Piranha.AttributeBuilder;
using Piranha.Models;

namespace KnApp.Models
{
    [PostType(Title = "Standard post")]
    public class StandardPost  : Post<StandardPost>
    {
    }
}