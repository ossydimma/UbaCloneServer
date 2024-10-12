namespace UbaClone.WebApi.Repositories
{
    public interface IUsersRepository
    {
        Task<Models.UbaClone?> CreateUserAsync(Models.UbaClone user);
        Task<Models.UbaClone[]> RetrieveAllAsync();
        //Task<Models.UbaClone?> RetrieveAsync(int id);
        Task<Models.UbaClone?> UpdateUserAsync(Models.UbaClone user);
        Task<bool?> DeleteUserAsync(int id);
        Task<int?> GetMaxAccountNo();
        Task<Models.UbaClone?> GetUserByContactAsync(string contact);
        Task<bool> VerifyPasswordAsync(string contact,  string oldPassword);
        Task<bool> VerifyPinAsync(string contact, string pin);
        Task ChangePasswordAsync(string contact, string password);
    }
}
