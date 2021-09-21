using System.Collections.Generic;
using System.Linq;
using Data.Entities;

namespace Data.Data
{
  public class UserInformation : IdBase<string>
  {
    private readonly List<Institution> _associatedInstitutions;

    public UserInformation(string id, string firstName, Community community = null,
      IEnumerable<Institution> associatedInstitutions = null) : base(id)
    {
      FirstName = firstName;
      Community = community;
      _associatedInstitutions = associatedInstitutions?.ToList() ?? new List<Institution>();
    }

    public string FirstName { get; }

    public Community Community { get; }

    public IReadOnlyList<Institution> AssociatedInstitutions => _associatedInstitutions;
  }
}