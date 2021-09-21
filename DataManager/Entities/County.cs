using System.Collections.Generic;

namespace Data.Entities
{
  internal class County
  {
    public string Name { get; set; }

    public IEnumerable<int> CommunityIds { get; set; }
  }
}