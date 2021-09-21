using Data.Entities;

namespace Data.Data
{
  internal class CommunityId : IdBase<int>
  {
    public CommunityId(ZipCode zipCode) : base(zipCode) { }

    public static implicit operator CommunityId(int zipCode)
    {
      return new CommunityId(zipCode);
    }

    public static implicit operator int(CommunityId communityId)
    {
      return communityId.Id;
    }
  }
}