using Dapper;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Security.Claims;

namespace Organo.Auth.Data
{
    public class IdentityUserRepository : UserStoreBase<IdentityUser, string, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>>
    {
        private IList<IdentityUserClaim<string>> UserClaims { get; set; }
        private IList<IdentityUserRole<string>> UserRoles { get; set; } //
        private IList<IdentityUserLogin<string>> UserLogins { get; set; }//
        private IList<IdentityUserToken<string>> UserTokens { get; set; }//

        public static DbConnection GetOpenConnection()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var connection = new SqlConnection(builder.Build().GetConnectionString("OrganoAuth"));
            connection.Open();
            return connection;
        }

        public IdentityUserRepository(IdentityErrorDescriber describer) : base(describer) { }

        public override IQueryable<IdentityUser> Users => throw new NotImplementedException();

        protected override async Task<IdentityUser> FindUserAsync(string userId, CancellationToken cancellationToken)
        {
            return await FindByIdAsync(userId, cancellationToken);
        }

        public override async Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                var p = new DynamicParameters();
                p.Add("@id", userId);
                var user = await connection.QueryFirstOrDefaultAsync<IdentityUser>("aspnetuser_select_id", p, commandType: CommandType.StoredProcedure);
                return user;
            }
        }

        public override async Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                var p = new DynamicParameters();
                p.Add("@NormalizedUserName", normalizedUserName);
                var user = await connection.QueryFirstOrDefaultAsync<IdentityUser>("aspnetuser_select", p, commandType: CommandType.StoredProcedure);
                return user;
            }
        }

        public override async Task<IdentityUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                var p = new DynamicParameters();
                p.Add("@email", normalizedEmail);
                var user = await connection.QueryFirstOrDefaultAsync<IdentityUser>("aspnetuser_select_email", p, commandType: CommandType.StoredProcedure);
                return user;
            }
        }

        public override async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                var p = new DynamicParameters();
                p.Add("@id", user.Id);
                p.Add("@username", user.UserName);
                p.Add("@passwordHash", user.PasswordHash);
                p.Add("@email", user.Email);
                await connection.ExecuteAsync("aspnetuser_create", p, commandType: CommandType.StoredProcedure);
            }

            return IdentityResult.Success;
        }

        /*public override async Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                var p = new DynamicParameters();
                p.Add("@id", user.Id);
                p.Add("@username", user.UserName);
                p.Add("@normalizedUserName", user.NormalizedUserName);
                p.Add("@passwordHash", user.PasswordHash);
                p.Add("@email", user.Email);
                p.Add("@normalizedEmail", user.NormalizedEmail);
                var updated = (await connection.ExecuteAsync("AspNetUsers_updateUser", p, commandType: CommandType.StoredProcedure));
                return updated == 1 ? IdentityResult.Success : IdentityResult.Failed(new IdentityError
                {
                    Code = string.Empty,
                    Description = $"User '{user.UserName}' could not be deleted."
                });
            }
        }*/

        public override async Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                var updated = await UpdateAsyncTeste(user, UserClaims, UserRoles, UserLogins, UserTokens);
                return updated ? IdentityResult.Success : IdentityResult.Failed(new IdentityError
                {
                    Code = string.Empty,
                    Description = $"User '{user.UserName}' could not be deleted."
                });
            }
        }

        private async Task<bool> UpdateAsyncTeste(IdentityUser user, IList<IdentityUserClaim<string>> claims, IList<IdentityUserRole<string>> roles, IList<IdentityUserLogin<string>> logins, IList<IdentityUserToken<string>> tokens)
        {
            using (var connection = GetOpenConnection())
            {
                const string updateUserSql = @"
                UPDATE [dbo].[AspNetUsers]
                SET [UserName] = @UserName, 
                    [NormalizedUserName] = @NormalizedUserName, 
                    [Email] = @Email, 
                    [NormalizedEmail] = @NormalizedEmail, 
                    [EmailConfirmed] = @EmailConfirmed, 
                    [PasswordHash] = @PasswordHash, 
                    [SecurityStamp] = @SecurityStamp, 
                    [ConcurrencyStamp] = @ConcurrencyStamp, 
                    [PhoneNumber] = @PhoneNumber, 
                    [PhoneNumberConfirmed] = @PhoneNumberConfirmed, 
                    [TwoFactorEnabled] = @TwoFactorEnabled, 
                    [LockoutEnd] = @LockoutEnd, 
                    [LockoutEnabled] = @LockoutEnabled, 
                    [AccessFailedCount] = @AccessFailedCount
                WHERE [Id] = @Id;
            ";
                using (var transaction = connection.BeginTransaction())
                {
                    await connection.ExecuteAsync(updateUserSql, new
                    {
                        user.UserName,
                        user.NormalizedUserName,
                        user.Email,
                        user.NormalizedEmail,
                        user.EmailConfirmed,
                        user.PasswordHash,
                        user.SecurityStamp,
                        user.ConcurrencyStamp,
                        user.PhoneNumber,
                        user.PhoneNumberConfirmed,
                        user.TwoFactorEnabled,
                        user.LockoutEnd,
                        user.LockoutEnabled,
                        user.AccessFailedCount,
                        user.Id
                    }, transaction);
                    if (claims?.Count() > 0)
                    {
                        const string deleteClaimsSql = @"
                        DELETE 
                        FROM [dbo].[AspNetUserClaims]
                        WHERE [UserId] = @UserId;
                    ";
                        await connection.ExecuteAsync(deleteClaimsSql, new { UserId = user.Id }, transaction);
                        const string insertClaimsSql = @"
                        INSERT INTO [dbo].[AspNetUserClaims] (UserId, ClaimType, ClaimValue)
                        VALUES (@UserId, @ClaimType, @ClaimValue);
                    ";
                        await connection.ExecuteAsync(insertClaimsSql, claims.Select(x => new
                        {
                            UserId = user.Id,
                            x.ClaimType,
                            x.ClaimValue
                        }), transaction);
                    }
                    if (roles?.Count() > 0)
                    {
                        const string deleteRolesSql = @"
                        DELETE
                        FROM [dbo].[AspNetUserRoles]
                        WHERE [UserId] = @UserId;
                    ";
                        await connection.ExecuteAsync(deleteRolesSql, new { UserId = user.Id }, transaction);
                        const string insertRolesSql = @"
                        INSERT INTO [dbo].[AspNetUserRoles] (UserId, RoleId)
                        VALUES (@UserId, @RoleId);
                    ";
                        await connection.ExecuteAsync(insertRolesSql, roles.Select(x => new
                        {
                            UserId = user.Id,
                            x.RoleId
                        }), transaction);
                    }
                    if (logins?.Count() > 0)
                    {
                        const string deleteLoginsSql = @"
                        DELETE
                        FROM [dbo].[AspNetUserLogins]
                        WHERE [UserId] = @UserId;
                    ";
                        await connection.ExecuteAsync(deleteLoginsSql, new { UserId = user.Id }, transaction);
                        const string insertLoginsSql = @"
                        INSERT INTO [dbo].[AspNetUserLogins] (LoginProvider, ProviderKey, ProviderDisplayName, UserId)
                        VALUES (@LoginProvider, @ProviderKey, @ProviderDisplayName, @UserId);
                    ";
                        await connection.ExecuteAsync(insertLoginsSql, logins.Select(x => new
                        {
                            x.LoginProvider,
                            x.ProviderKey,
                            x.ProviderDisplayName,
                            UserId = user.Id
                        }), transaction);
                    }
                    if (tokens?.Count() > 0)
                    {
                        const string deleteTokensSql = @"
                        DELETE
                        FROM [dbo].[AspNetUserTokens]
                        WHERE [UserId] = @UserId;
                    ";
                        await connection.ExecuteAsync(deleteTokensSql, new { UserId = user.Id }, transaction);
                        const string insertTokensSql = @"
                        INSERT INTO [dbo].[AspNetUserTokens] (UserId, LoginProvider, Name, Value)
                        VALUES (@UserId, @LoginProvider, @Name, @Value);
                    ";
                        await connection.ExecuteAsync(insertTokensSql, tokens.Select(x => new
                        {
                            x.UserId,
                            x.LoginProvider,
                            x.Name,
                            x.Value
                        }), transaction);
                    }
                    try
                    {
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                return true;
            }
        }

        public override async Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                var p = new DynamicParameters();
                p.Add("@id", user.Id);
                var deleted = await connection.ExecuteAsync("aspnetuser_del", p, commandType: CommandType.StoredProcedure);

                return deleted != 0 ? IdentityResult.Success : IdentityResult.Failed(new IdentityError
                {
                    Code = string.Empty,
                    Description = $"User '{user.UserName}' could not be deleted."
                });
            }
        }

        public override async Task<IList<Claim>> GetClaimsAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                var p = new DynamicParameters();
                p.Add("@userId", user.Id);
                var userClaims = (await connection.QueryAsync<IdentityUserClaim<string>>("AspNetUserClaims_selectByUserId", p, commandType: CommandType.StoredProcedure)).ToList();
                return userClaims.Select(x => new Claim(x.ClaimType, x.ClaimValue)).ToList();
            }
        }

        public override async Task AddClaimsAsync(IdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                var p = new DynamicParameters();
                p.Add("@userId", user.Id);
                UserClaims ??= (await connection.QueryAsync<IdentityUserClaim<string>>("AspNetUserClaims_selectByUserId", p, commandType: CommandType.StoredProcedure)).ToList();
                //var userClaims = await GetClaimsAsync(user);

                foreach (var claim in claims)
                {
                   // if (!ClaimExist(claim))
                        UserClaims.Add(CreateUserClaim(user, claim));
                }
            }
        }

        private bool ClaimExist(Claim claim)
        {
            foreach (var claimClaim in UserClaims)
            {
                if (claim.Type == claimClaim.ClaimType && claim.Value == claimClaim.ClaimValue)
                    return true;
            }

            return false;
        }

        public override async Task<IList<IdentityUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                var p = new DynamicParameters();
                p.Add("@claimType", claim.Type);
                p.Add("@claimValue", claim.Value);
                var users = (await connection.QueryAsync<IdentityUser>("AspNetUser_selectUsersByClaim", p, commandType: CommandType.StoredProcedure)).ToList();
                return users;
            }
        }

        public override async Task RemoveClaimsAsync(IdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                var p = new DynamicParameters();
                p.Add("@userId", user.Id);
                UserClaims = (await connection.QueryAsync<IdentityUserClaim<string>>("AspNetUserClaims_selectByUserId", p, commandType: CommandType.StoredProcedure)).ToList();

                foreach (var claim in claims)
                {
                    var matchedClaims = UserClaims.Where(x => x.UserId.Equals(user.Id) && x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
                    foreach (var matchedClaim in matchedClaims)
                    {
                        UserClaims.Remove(matchedClaim);
                    }
                }
            }
        }

        public override async Task ReplaceClaimAsync(IdentityUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                var p = new DynamicParameters();
                p.Add("@userId", user.Id);
                UserClaims = (await connection.QueryAsync<IdentityUserClaim<string>>("AspNetUserClaims_selectByUserId", p, commandType: CommandType.StoredProcedure)).ToList();
                var matchedClaims = UserClaims.Where(x => x.UserId.Equals(user.Id) && x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
                foreach (var matchedClaim in matchedClaims)
                {
                    matchedClaim.ClaimValue = newClaim.Value;
                    matchedClaim.ClaimType = newClaim.Type;
                }
            }
        }

        // -------------------------------------------------------------

        public override Task AddLoginAsync(IdentityUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveLoginAsync(IdentityUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override Task AddUserTokenAsync(IdentityUserToken<string> token)
        {
            throw new NotImplementedException();
        }

        protected override Task<IdentityUserToken<string>> FindTokenAsync(IdentityUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task<IdentityUserLogin<string>> FindUserLoginAsync(string userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task<IdentityUserLogin<string>> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task RemoveUserTokenAsync(IdentityUserToken<string> token)
        {
            throw new NotImplementedException();
        }
    }
}