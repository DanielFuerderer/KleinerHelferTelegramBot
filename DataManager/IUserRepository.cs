using System.Collections.Generic;
using System.Linq;
using Data.Data;

namespace Data
{
  public interface IUserRepository
  {
    IEnumerable<UserInformation> UsersInformation { get; }

    UserInformation this[string userId] { get; }

    bool Exists(string userId);

    UserInformation Get(string userId);

    void AddUser(UserInformation userInformation);

    IEnumerable<UserInformation> GetUsersFrom(Community community);

    IEnumerable<UserInformation> GetUsersFrom(Institution institution);

    void Save();

    void Update(UserInformation userInformation);

    UserInformationUpdater StartUpdate(UserInformation userInformation);
  }

  public class UserInformationUpdater
  {
    private readonly List<Institution> _associatedInstitutions;
    private readonly string _firstName;
    private readonly UserInformation _userInformation;
    private readonly UserRepository _userRepository;
    private Community _community;

    public UserInformationUpdater(UserInformation userInformation, UserRepository userRepository)
    {
      _userInformation = userInformation;
      _userRepository = userRepository;
      _community = userInformation.Community;
      _firstName = userInformation.FirstName;
      _associatedInstitutions = userInformation.AssociatedInstitutions.ToList();
    }

    public UserInformationUpdater AddInstitution(Institution institution)
    {
      _associatedInstitutions.Add(institution);

      return this;
    }

    public UserInformationUpdater RemoveInstitution(Institution institution)
    {
      _associatedInstitutions.Remove(institution);

      return this;
    }

    public void Commit()
    {
      _userRepository.Update(new UserInformation(_userInformation.Id, _firstName, _community, _associatedInstitutions));
    }

    public UserInformationUpdater SetCommunity(Community community)
    {
      _community = community;

      return this;
    }
  }
}