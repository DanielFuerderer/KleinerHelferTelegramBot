using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Data.Entities;
using Community = Data.Data.Community;
using Institution = Data.Data.Institution;
using UserInformation = Data.Data.UserInformation;

namespace Data
{
  public class UserRepository : IUserRepository
  {
    private readonly ICommunityRepository _communityRepository;

    private readonly Dictionary<string, UserInformation> _userInformation = new Dictionary<string, UserInformation>();

    public UserRepository(string fileName, ICommunityRepository communityRepository)
    {
      _communityRepository = communityRepository;
      FileName = fileName;

      if (!TryLoad(fileName, out var fileContent) || string.IsNullOrEmpty(fileContent))
      {
        return;
      }

      var data = Deserialize(fileContent);

      foreach (var userInformation in data.UsersInformation)
      {
        communityRepository.TryGetCommunity(userInformation.ZipCode, out var community);

        var associatedInstitutions = userInformation.AssociatedInstitutions
          .Select(ai => communityRepository.GetInstitution(ai)).ToList();

        _userInformation[userInformation.Id] = new UserInformation(userInformation.Id, userInformation.FirstName,
          community, associatedInstitutions);
      }
    }

    public string FileName { get; }

    public IEnumerable<UserInformation> UsersInformation => _userInformation.Values;

    public UserInformation this[string userId] => _userInformation[userId];

    public bool Exists(string userId)
    {
      return _userInformation.ContainsKey(userId);
    }

    public UserInformation Get(string userId)
    {
      return _userInformation[userId];
    }

    public void AddUser(UserInformation userInformation)
    {
      _userInformation.Add(userInformation.Id, userInformation);
    }

    public IEnumerable<UserInformation> GetUsersFrom(Community community)
    {
      return _userInformation
        .Where(ui => Equals(ui.Value.Community?.ZipCode, community.ZipCode))
        .Select(kvp => kvp.Value);
    }

    public IEnumerable<UserInformation> GetAssignedUsers()
    {
      return _userInformation
        .Where(u => u.Value.Community != null)
        .Select(u => u.Value);
    }

    public IEnumerable<UserInformation> GetUsersFrom(Institution institution)
    {
      return _userInformation.Values.Where(ui => ui.AssociatedInstitutions.Contains(institution));
    }

    public void Save()
    {
      Save(FileName);
    }

    public void Update(UserInformation newUser)
    {
      _userInformation[newUser.Id] = newUser;
    }

    public UserInformationUpdater StartUpdate(UserInformation userInformation)
    {
      return new UserInformationUpdater(userInformation, this);
    }

    private static bool TryLoad(string fileName, out string data)
    {
      if (File.Exists(fileName))
      {
        data = File.ReadAllText(fileName);

        return true;
      }

      data = null;

      return false;
    }

    private static UserFile Deserialize(string data)
    {
      return JsonSerializer.Deserialize<UserFile>(data);
    }

    public void Save(string fileName)
    {
      var data = JsonSerializer.Serialize(new UserFile
      {
        UsersInformation = _userInformation
          .Select(ui => Serializer.Serialize(ui.Key, ui.Value, _communityRepository.GetId)).ToList()
      });

      Directory.CreateDirectory(Path.GetDirectoryName(fileName));

      File.WriteAllText(fileName, data);
    }
  }
}