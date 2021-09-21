using System;
using System.Collections.Generic;
using Data.Data;

namespace Data
{
  public interface ICommunityRepository
  {
    IEnumerable<Community> Communities { get; }
    IEnumerable<County> Counties { get; }
    IEnumerable<Institution> Institutions { get; }
    void Save(string fileName = null);
    IEnumerable<Institution> GetInstitutions(ZipCode zipCode);
    Institution GetInstitution(InstitutionId institutionId);
    bool TryGetCommunity(int zipCode, out Community community);
    string GetId(Institution arg);
    void AddCommunity(Community community, Action<Community> communityExistsAlready);
    void AddInstitution(Institution institution);
  }
}