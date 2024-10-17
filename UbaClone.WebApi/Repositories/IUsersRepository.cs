namespace UbaClone.WebApi.Repositories
{
    public interface IUsersRepository
    {
        Task<Models.UbaClone?> CreateUserAsync(Models.UbaClone user);
        Task<Models.UbaClone[]> RetrieveAllAsync();
        Task<Models.UbaClone?> RetrieveAsync(Guid id);
        Task<Models.UbaClone?> UpdateUserAsync(Models.UbaClone user);
        Task<bool?> DeleteUserAsync(Guid id);
        Task<int?> GetMaxAccountNo();
        Task<Models.UbaClone?> GetUserByContactAsync(string contact);
        bool VerifyPasswordAsync(Models.UbaClone user,  string Password);
        bool VerifyPinAsync(Models.UbaClone user, string pin);
        Task ChangePasswordAsync(Models.UbaClone user, string newPassword);
        Task ChangePinAsync(Models.UbaClone user, string newPin);
        Task<Models.UbaClone?> GetUserByAccountNo(int accountNumber);

        //Task UpdateUser ();
        Task<bool> SaveAsync(Models.UbaClone user);
    }
}
