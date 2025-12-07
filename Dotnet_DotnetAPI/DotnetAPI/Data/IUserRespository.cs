using DotnetAPI.Models;

namespace DotnetAPI.Data
{
    public interface IUserRepository
    {
        public bool SaveChange();

        public void AddEntity<T>(T entityToAdd);
        public void RemoveEntity<T>(T entityToRemove);

        public bool TryAddEntity<T>(T entityToAdd);
        public bool TryRemoveEntity<T>(T entityToRemove);
        public IEnumerable<User> GetUsers();

        public User GetSingleUsers(int userId);
        public UserJobInfo GetSingleUserJobInfo(int userId);
    }
}
