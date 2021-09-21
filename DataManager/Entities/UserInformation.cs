using System.Collections.Generic;

namespace Data.Entities
{
  internal class UserInformation : IdBase<string>
  {
    public UserInformation(string id) : base(id) { }

    protected UserInformation() { }

    public int ZipCode { get; set; }

    public List<string> AssociatedInstitutions { get; set; }

    public string FirstName { get; set; }

    public bool IsAdmin { get; set; }
  }
}