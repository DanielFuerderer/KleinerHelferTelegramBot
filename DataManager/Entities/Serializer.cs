using System;
using System.Collections.Generic;
using System.Linq;
using Data.Data;

namespace Data.Entities
{
  internal class Serializer
  {
    public static Data.County Deserialize(County county, IDictionary<CommunityId, Data.Community> communityLookup)
    {
      var dataCounty = new Data.County(county.Name);

      foreach (var communityId in county.CommunityIds)
      {
        dataCounty.AddAssociated(communityLookup[communityId]);
      }

      return dataCounty;
    }

    public static Data.Community Deserialize(Community community)
    {
      return new Data.Community(community.Name, community.Id);
    }

    public static Data.Institution Deserialize(Institution dayCareCenter,
      IDictionary<CommunityId, Data.Community> communityLookup)
    {
      return new Data.Institution(dayCareCenter.Name, communityLookup[dayCareCenter.CommunityId]);
    }

    public static CommunityFile Serialize(IEnumerable<Data.County> counties, IEnumerable<Data.Community> communities,
      IEnumerable<KeyValuePair<InstitutionId, Data.Institution>> institutions)
    {
      return new CommunityFile
      {
        Counties = counties.Select(Serialize),
        Communities = communities.Select(Serialize),
        Institutions = institutions.Select(Serialize)
      };
    }

    public static County Serialize(Data.County county)
    {
      return new County
      {
        Name = county.Name,
        CommunityIds = county.Associated.Select(ci => ci.ZipCode.Value)
      };
    }

    public static void Test(CommunityId id) { }

    public static Institution Serialize(KeyValuePair<InstitutionId, Data.Institution> institution)
    {
      return new Institution(institution.Key)
      {
        Name = institution.Value.Name,
        CommunityId = institution.Value.Community.ZipCode.Value
      };
    }

    public static Community Serialize(Data.Community community)
    {
      return new Community(community.ZipCode.Value)
      {
        Name = community.Name
      };
    }

    public static UserInformation Serialize(string userInformationId, Data.UserInformation userInformation,
      Func<Data.Institution, string> referenceCreate)
    {
      return new UserInformation(userInformationId)
      {
        FirstName = userInformation.FirstName,
        ZipCode = userInformation.Community?.ZipCode,
        AssociatedInstitutions = userInformation.AssociatedInstitutions.Select(referenceCreate).ToList()
      };
    }
  }
}