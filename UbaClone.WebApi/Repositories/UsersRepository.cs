using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Text.Json.Serialization;
using UbaClone.WebApi.Data;

namespace UbaClone.WebApi.Repositories;

public class UsersRepository(IDistributedCache distributedCache, DataContext db) : IUsersRepository
{
    private readonly DataContext _db = db;
    private readonly IDistributedCache _distributedCache = distributedCache;
    private readonly DistributedCacheEntryOptions _cacheEntryOptions = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)) // Expire after 10 Mins
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

    public async Task<Models.UbaClone?> GetUserByContactAsync(string contact)
    {
        string key = $"user:{contact}";
        string? fromCache = await _distributedCache.GetStringAsync(key);

        if (!string.IsNullOrEmpty(fromCache))
            return JsonConvert.DeserializeObject<Models.UbaClone>(fromCache);

        Models.UbaClone? fromDb = await _db.ubaClones.FirstOrDefaultAsync(u => u.Contact == contact);

        if (fromDb == null) return fromDb;

        await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(fromDb), _cacheEntryOptions);

        return fromDb;
    }

    public  async Task<bool> VerifyPasswordAsync(string contact, string oldPassword)
    {
        
        Models.UbaClone? user = await GetUserByContactAsync(contact);
        if (user is null)
            return false;

        return Hasher.VerifyValue(oldPassword, user.PasswordHash, user.PasswordSalt);

    }

    public async Task<bool> VerifyPinAsync(string contact, string pin)
    {

        Models.UbaClone? user = await GetUserByContactAsync(contact);
        if (user is null)
            return false;

        return Hasher.VerifyValue(pin, user.PinHash, user.PinSalt);

    }

    public async Task ChangePasswordAsync(string contact, string newPassword)
    {
        Models.UbaClone? user = await GetUserByContactAsync(contact);
        if (user is not null)
        {
            Hasher.CreateValueHash(newPassword, out byte[] newPasswordHash, out byte[] newPasswordSalt);

            user.PasswordHash = newPasswordHash;
            user.PasswordSalt = newPasswordSalt;

            _db.Update(user);
            int affect = await _db.SaveChangesAsync();

            if (affect == 1)
            {
                string key = $"user:{user.Contact}";
                await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(user), _cacheEntryOptions);
            }
            else
            {
                throw new Exception("Failed to update password.");
            }

        }else
        {
            // Handle case where user is not found
            throw new Exception("User not found.");
        }
            
    }

     public async Task<Models.UbaClone[]> RetrieveAllAsync()
    {
        return await _db.ubaClones.ToArrayAsync();

    }

    //public async Task<Models.UbaClone?> RetrieveAsync(int id)
    //{
    //    string key = $"user:{id}";
    //    string? fromCache = await _distributedCache.GetStringAsync(key);

    //    if (!string.IsNullOrEmpty(fromCache))
    //        return JsonConvert.DeserializeObject<Models.UbaClone>(fromCache);

    //   Models.UbaClone? fromDb = await _db.ubaClones.FirstOrDefaultAsync(u => u.Id == id);
       
    //    if(fromDb == null) return fromDb;

    //    await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(fromDb), _cacheEntryOptions);

    //    return fromDb;
    //}

    public async Task<Models.UbaClone?> CreateUserAsync(Models.UbaClone user)
    {
        string key = $"user:{user.Contact}";

        await _db.ubaClones.AddAsync(user);
        int affect = await _db.SaveChangesAsync();

        if (affect == 1)
        {
           await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(user), _cacheEntryOptions);
           return user;
        }

        return null;
    }

    public async Task<Models.UbaClone?> UpdateUserAsync(Models.UbaClone user)
    {
        string key = $"user:{user.Contact}";

        _db.Update(user);
        int affect = await _db.SaveChangesAsync();

        if (affect == 1)
        {
            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(user), _cacheEntryOptions);
            return user;
        }
        return null;
    }

    public async Task<bool?> DeleteUserAsync(int id)
    {
        string key = $"user:{id}";

        Models.UbaClone? user = await _db.ubaClones.FindAsync(id);
        if (user is null) return null;

        _db.ubaClones.Remove(user);
        int affect = await _db.SaveChangesAsync();

        if (affect == 1)
        {
            _distributedCache.Remove(key);
            return true;
        }
        return null;
    }

    public async Task<int?> GetMaxAccountNo()
    {
        return await _db.ubaClones.MaxAsync(c => (int?)c.AccountNumber);
    }
    
}