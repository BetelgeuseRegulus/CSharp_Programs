using DotnetAPI.Models;

namespace DotnetAPI.Data
{
    public class UserRepository : IUserRepository
    {

        DataContextEF _entityFramework;

        public UserRepository(IConfiguration config)
        {
            _entityFramework = new DataContextEF(config);
        }

        public bool SaveChange()
        {
            return _entityFramework.SaveChanges() > 0;
        }

        public void AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entityFramework.Add(entityToAdd);
            }
        }
        public void RemoveEntity<T>(T entityToRemove)
        {
            if (entityToRemove != null)
            {
                _entityFramework.Remove(entityToRemove);
            }
        }

        public bool TryAddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entityFramework.Add(entityToAdd);
                return true;
            }

            return false;
        }

        public bool TryRemoveEntity<T>(T entityToRemove)
        {
            if (entityToRemove != null)
            {
                _entityFramework.Add(entityToRemove);
                return true;
            }

            return false;
        }

        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _entityFramework.Users.ToList<User>();
            return users;
        }

        public User GetSingleUsers(int userId)
        {
            User? user = _entityFramework.Users
                                         .Where(u => u.UserId == userId)
                                         .FirstOrDefault<User>();
            if (user != null)
                return user;

            throw new Exception("Failed to Get User");
        }

        public UserJobInfo GetSingleUserJobInfo(int userId)
        {
            UserJobInfo? userJobInfo = _entityFramework.UserJobInfo
                                                       .Where(u => u.UserId == userId)
                                                       .FirstOrDefault<UserJobInfo>();

            if (userJobInfo != null)
                return userJobInfo;

            throw new Exception("Faild to Get UserJobInfo");
        }
    }
}
