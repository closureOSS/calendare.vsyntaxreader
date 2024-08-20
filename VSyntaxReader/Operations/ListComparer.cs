using System.Collections.Generic;
using System.Linq;

namespace Calendare.VSyntaxReader.Operations;

public enum ListItemState
{
    Unknown,
    Both,
    RightOnly,
    LeftOnly,
}

public class ListItem<T> where T : class
{
    public required T Target { get; set; }
    public T? Source { get; set; }
    public required ListItemState Status { get; set; }
}

public class ListComparer<T> where T : class
{
    public List<ListItem<T>> Values { get; set; } = [];

    public ListComparer(IEnumerable<T> left, IEnumerable<T> right, IEqualityComparer<T> fnEqual)
    {
        foreach (var l in left)
        {
            Values.Add(new ListItem<T> { Target = l, Status = ListItemState.LeftOnly });
        }
        foreach (var r in right)
        {
            var hit = Values.FirstOrDefault(x => fnEqual.Equals(x.Target, r));
            if (hit is not null)
            {
                hit.Status = ListItemState.Both;
                hit.Source = r;
            }
            else
            {
                Values.Add(new ListItem<T> { Target = r, Status = ListItemState.RightOnly });
            }
        }
    }
}
