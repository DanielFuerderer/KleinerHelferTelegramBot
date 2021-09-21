using System.Collections.Generic;

namespace Data.Data
{
  public class AdministrativeUnit
  {
    private readonly List<Community> _associated = new List<Community>();

    public IEnumerable<Community> Associated => _associated;

    public void AddAssociated(Community community)
    {
      _associated.Add(community);
    }
  }
}