using Data.Data;

namespace Data.Entities
{
  internal class Community : IdBase<int>
  {
    protected Community() { }

    public Community(ZipCode zipCode) : base(zipCode) { }

    public string Name { get; set; }
  }
}