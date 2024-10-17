
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Reflection;
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

    public bool VerifyPasswordAsync(Models.UbaClone user, string password)
    {
        
        if (user is null)
            return false;
        

        return Hasher.VerifyValue(password, user.PasswordHash, user.PasswordSalt);

    }

    public bool VerifyPinAsync(Models.UbaClone user, string pin)
    {

        if (user is null)
            return false;

        return Hasher.VerifyValue(pin, user.PinHash, user.PinSalt);

    }

    public async Task ChangePasswordAsync(Models.UbaClone user, string newPassword)
    {
        if (user is not null)
        {
            Hasher.CreateValueHash(newPassword, out byte[] newPasswordHash, out byte[] newPasswordSalt);

            user.PasswordHash = newPasswordHash;
            user.PasswordSalt = newPasswordSalt;

            _db.Update(user);
            int affect = await _db.SaveChangesAsync();

            if (affect == 1)
            {
                await _distributedCache.SetStringAsync($"user:{user.Contact}", JsonConvert.SerializeObject(user), _cacheEntryOptions);
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

    public async Task ChangePinAsync (Models.UbaClone user, string newPin)
    {
        if (user is not null)
        {
            Hasher.CreateValueHash(newPin, out byte[] pinHash, out byte[] pinSalt);
            user.PinHash = pinHash;
            user.PinSalt = pinSalt;

            _db.Update(user);
            int affected = await _db.SaveChangesAsync();

            if (affected == 1)
            {
                await _distributedCache.SetStringAsync($"user:{user.Contact}", JsonConvert.SerializeObject(user), _cacheEntryOptions);
            }
            else
            {
                throw new Exception("Failed to Update PIN");
            }
        }
        else
        {
            throw new Exception($"Unable to access user");
        }

    }

    public async  Task<Models.UbaClone?> GetUserByAccountNo(int accountNumber)
    {
        Models.UbaClone? user = await _db.ubaClones.FirstOrDefaultAsync( u => u.AccountNumber == accountNumber );
        if (user == null) return null;

        return user;
    }

     public async Task<Models.UbaClone[]> RetrieveAllAsync()
    {
        return await _db.ubaClones.ToArrayAsync();

    }

    public async Task<Models.UbaClone?> RetrieveAsync(Guid id)
    {
        string key = $"user:{id}";
        string? fromCache = await _distributedCache.GetStringAsync(key);

        if (!string.IsNullOrEmpty(fromCache))
            return JsonConvert.DeserializeObject<Models.UbaClone>(fromCache);

        Models.UbaClone? fromDb = await _db.ubaClones.FirstOrDefaultAsync(u => u.UserId == id);

        if (fromDb == null) return fromDb;

        await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(fromDb), _cacheEntryOptions);

        return fromDb;
    }

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

    public async Task<bool> SaveAsync(Models.UbaClone user)
    {
        // Ensure no other entity with the same key is being tracked
        _db.Entry(user).State = EntityState.Detached;

        // Attach the user entity and mark it as modified
        _db.Attach(user);
        _db.Entry(user).State = EntityState.Modified;

        try
        {
            // Save the changes
            int affected = await _db.SaveChangesAsync();

            if (affected > 0)
            {
                await _distributedCache.SetStringAsync($"user:{user.Contact}", JsonConvert.SerializeObject(user), _cacheEntryOptions);
                return true;
            }else
            {
                return false;
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle concurrency conflict
            Console.WriteLine(ex);
            return false;
            //return Conflict("Concurrency conflict detected. The data may have been modified or deleted.");
        }

    }
    public async Task<bool?> DeleteUserAsync(Guid id)
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