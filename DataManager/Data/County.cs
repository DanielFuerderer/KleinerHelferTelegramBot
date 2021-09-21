namespace Data.Data
{
  public class County : AdministrativeUnit
  {
    public County(string name)
    {
      Name = name;
    }

    public string Name { get; }
  }
}