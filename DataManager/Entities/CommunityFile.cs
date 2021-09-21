using System.Collections.Generic;

namespace Data.Entities
{
  internal class CommunityFile
  {
    public IEnumerable<County> Counties { get; set; }

    public IEnumerable<Community> Communities { get; set; }

    public IEnumerable<Institution> Institutions { get; set; }
  }
}