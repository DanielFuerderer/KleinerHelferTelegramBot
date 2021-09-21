using Data.Entities;

namespace Data.Data
{
  public class InstitutionId : IdBase<string>
  {
    public InstitutionId(string id) : base(id) { }

    public static implicit operator InstitutionId(string id)
    {
      return new InstitutionId(id);
    }

    public static implicit operator string(InstitutionId institutionId)
    {
      return institutionId.Id;
    }
  }
}