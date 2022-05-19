using Microsoft.EntityFrameworkCore;
using Tarefar.DB;
using Tarefar.DB.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Tarefar.Tests
{
    public class DatatbaseTestInput
    {
        public List<ApplicationUser> UsersToAdd { get; set; }

        /// <summary>
        /// Create InMemoryContext to be used in tests
        /// </summary>
        /// <returns>The context</returns>
        [ExcludeFromCodeCoverage]
        public ApiContext CreateNewInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApiContext>()
                   .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                   .Options;

            var apiContext = new ApiContext(options);
            _AddValues(apiContext);
            return apiContext;
        }

        /// <summary>
        /// Save one by one to allow create "old" relationships
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="input">The values that shold be added</param>
        [ExcludeFromCodeCoverage]
        private void _AddValues(ApiContext context)
        {
            if (UsersToAdd != null)
            {
                foreach (ApplicationUser user in this.UsersToAdd)
                {
                    context.Users.Add(user);
                    context.SaveChanges();
                }
            }
        }
    }
}
