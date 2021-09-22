using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Data.Data;
using Data.Entities;
using Community = Data.Data.Community;
using County = Data.Data.County;
using Institution = Data.Data.Institution;

namespace Data
{
  public class CommunityRepository : ICommunityRepository
  {
    private readonly Dictionary<CommunityId, Community> _communities = new Dictionary<CommunityId, Community>();

    private readonly List<County> _counties = new List<County>();

    private readonly Dictionary<InstitutionId, Institution> _institutions =
      new Dictionary<InstitutionId, Institution>();

    public CommunityRepository(string fileName)
    {
      FileName = fileName;

      if (!File.Exists(fileName))
      {
        return;
      }

      var serializedData = File.ReadAllText(fileName);

      if (string.IsNullOrEmpty(serializedData))
      {
        return;
      }

      var result = JsonSerializer.Deserialize<CommunityFile>(serializedData);

      foreach (var community in result.Communities)
      {
        _communities.Add(community.Id, Serializer.Deserialize(community));
      }

      _counties.AddRange(result.Counties.Select(c => Serializer.Deserialize(c, _communities)));

      foreach (var institution in result.Institutions)
      {
        _institutions.Add(institution.Id, Serializer.Deserialize(institution, _communities));
      }
    }

    public string FileName { get; }

    public IEnumerable<Community> Communities => _communities.Values;

    public IEnumerable<County> Counties => _counties;

    public IEnumerable<Institution> Institutions => _institutions.Values;

    public void Save(string fileName = null)
    {
      fileName ??= FileName;

      var data = Serializer.Serialize(Counties, Communities, _institutions);

      var serializedData = JsonSerializer.Serialize(data);

      Directory.CreateDirectory(Path.GetDirectoryName(fileName));

      File.WriteAllText(fileName, serializedData);
    }

    public IEnumerable<Institution> GetInstitutions(ZipCode zipCode)
    {
      return Institutions.Where(dcc => Equals(dcc.Community?.ZipCode, zipCode));
    }

    public Institution GetInstitution(InstitutionId institutionId)
    {
      return _institutions[institutionId];
    }

    public bool TryGetCommunity(int zipCode, out Community community)
    {
      if (_communities.ContainsKey(zipCode))
      {
        community = _communities[zipCode];

        return true;
      }

      community = null;

      return false;
    }

    public string GetId(Institution arg)
    {
      return _institutions.FirstOrDefault(i => i.Value == arg).Key;
    }

    public void AddCommunity(Community community, Action<Community>? communityExistsAlready)
    {
      var communityId = new CommunityId(community.ZipCode);

      if (_communities.TryGetValue(communityId, out var existingCommunity))
      {
        communityExistsAlready?.Invoke(existingCommunity);

        return;
      }

      _communities.Add(communityId, community);
    }

    public void AddInstitution(Institution institution)
    {
      _institutions.Add(new InstitutionId(Guid.NewGuid().ToString()), institution);
    }
  }
}