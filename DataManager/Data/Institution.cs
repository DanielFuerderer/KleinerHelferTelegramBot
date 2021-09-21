namespace Data.Data
{
  public class Institution
  {
    public Institution(string name, Community community)
    {
      Name = name;
      Community = community;
    }

    public string Name { get; }

    public Community Community { get; }
  }
}