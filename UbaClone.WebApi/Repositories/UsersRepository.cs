﻿
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Globalization;
using System.Reflection;
using UbaClone.WebApi.Data;
using UbaClone.WebApi.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        Models.UbaClone? user = null;
        string key = $"UBACLONE-user:{contact}";

        try
        {

            string? fromCache = await _distributedCache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(fromCache))
            {
                user = JsonConvert.DeserializeObject<Models.UbaClone>(fromCache);
                if (user != null)
                {
                    Console.WriteLine("Fetched user from Redis cache");
                    return user;
                }
            }
                
        }
        catch (Exception ex)
        {
            Console.WriteLine("Redis unavailable: " + ex.Message);
        }
        

        user = await _db.Users
            .Include(u => u.TransactionHistory)
            .FirstOrDefaultAsync(u => u.Contact == contact);

        if (user == null) return user;

        try
        {
            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(user), _cacheEntryOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to cache user in Redis: " + ex.Message);
        }


        return user;
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
                try
                {
                    await _distributedCache.SetStringAsync($"UBACLONE-user:{user.Contact}", JsonConvert.SerializeObject(user), _cacheEntryOptions);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to cache user in Redis: " + ex.Message);
                    
                }
                
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
                try
                {
                    await _distributedCache.SetStringAsync($"UBACLONE-user:{user.Contact}", JsonConvert.SerializeObject(user), _cacheEntryOptions);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to cache user in Redis: " + ex.Message);
                    
                }
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
        Models.UbaClone? user = await _db.Users.FirstOrDefaultAsync( u => u.AccountNumber == accountNumber );
        if (user == null) return null;

        return user;
    }

     public async Task<Models.UbaClone[]> RetrieveAllAsync()
    {
        return await _db.Users.ToArrayAsync();

    }

    public async Task<Models.UbaClone?> RetrieveAsync(Guid id)
    {
        string key = $"UBACLONE-user:{id}";
        Models.UbaClone? user = null;
        try
        {
            string? fromCache = await _distributedCache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(fromCache))
            {
                user = JsonConvert.DeserializeObject<Models.UbaClone>(fromCache);
                if (user != null)
                {
                    return user;
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("Redis unavailable: " + ex.Message);
        }
        

        user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null) return user;

        try
        {
            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(user), _cacheEntryOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to cache user in Redis: " + ex.Message);
        }

        return user;
    }

    public async Task<Models.UbaClone?> CreateUserAsync(Models.UbaClone user)
    {
        string key = $"UBACLONE-user:{user.Contact}";

        await _db.Users.AddAsync(user);
        int affect = await _db.SaveChangesAsync();

        if (affect == 1)
        {
            try
            {
                await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(user), _cacheEntryOptions);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to cache user in Redis: " + ex.Message);
            }

           return user;
        }

        return null;
    }

    public async Task<Models.UbaClone?> UpdateUserAsync(Models.UbaClone user)
    {
        string key = $"UBACLONE-user:{user.Contact}";

        _db.Update(user);
        int affect = await _db.SaveChangesAsync();

        if (affect == 1)
        {
            try
            {
                await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(user), _cacheEntryOptions);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to cache user in Redis: " + ex.Message);
            }
            
           return user;
        }
        return null;
    }

    public async Task SaveAsync()
    {
        // Save changes to the context
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Models.UbaClone user)
    {
        // Update the entity state to modified
        _db.Entry(user).State = EntityState.Modified;
        int affected = await _db.SaveChangesAsync();

        if (affected > 0)
        {
            try
            {
                await _distributedCache.SetStringAsync($"UBACLONE-user:{user.Contact}", JsonConvert.SerializeObject(user), _cacheEntryOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to cache user in Redis: " + ex.Message);
            }
        }

    }

    public List<HistoryDTO> GetTransactionHistories(Models.UbaClone user)
    {
        var histories = _db.TransactionHistories
            .Where(t => t.UbaCloneUserId == user.UserId)
            .AsEnumerable()
            .OrderByDescending(t => DateTime.ParseExact(t.Date + " " + t.Time, "ddd MMM dd yyyy h:mm tt", CultureInfo.InvariantCulture))
            .Select(t => new HistoryDTO
            {
            Name = t.Name,
            Number = t.Number,
            Date = t.Date,
            Time = t.Time,
            Amount = t.Amount,
            Narrator = t.Narrator,
            TypeOfTranscation = t.TypeOfTranscation
            }).ToList();
        if (histories is null || !histories.Any())
            return [];

        return histories;

    }

    public async Task<bool?> DeleteUserAsync(Guid id)
    {
        string key = $"UBACLONE-user:{id}";

        Models.UbaClone? user = await _db.Users.FindAsync(id);
        if (user is null) return null;

        _db.Users.Remove(user);
        int affect = await _db.SaveChangesAsync();

        if (affect == 1)
        {
            try
            {
                _distributedCache.Remove(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to cache user in Redis: " + ex.Message);
            }
            return true;
        }
        return null;
    }

    public async Task<int?> GetMaxAccountNo()
    {
        return await _db.Users.MaxAsync(c => (int?)c.AccountNumber);
    }
    
}