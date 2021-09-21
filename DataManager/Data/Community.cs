namespace Data.Data
{
  public class Community
  {
    public Community(string name, ZipCode zipCode)
    {
      Name = name;
      ZipCode = zipCode;
    }

    public string Name { get; }

    public ZipCode ZipCode { get; }
  }
}