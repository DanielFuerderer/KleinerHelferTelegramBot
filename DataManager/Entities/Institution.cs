namespace Data.Entities
{
  internal class Institution : IdBase<string>
  {
    public Institution(string id) : base(id) { }

    protected Institution() { }

    public string Name { get; set; }

    public int CommunityId { get; set; }
  }
}