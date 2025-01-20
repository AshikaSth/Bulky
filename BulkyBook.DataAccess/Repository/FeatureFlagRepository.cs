using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class FeatureFlagRepository : Repository<FeatureFlag>, IFeatureFlagRepository
    {
        private readonly ApplicationDbContext _fdb;
        private readonly IFeatureManager _featureManager;

        public FeatureFlagRepository(ApplicationDbContext fdb, IFeatureManager featureManager) : base(fdb)
        {
            _fdb = fdb;
            _featureManager = featureManager;
        }


        public async Task<bool> GetFeatureFlagStatusAsync(string featureName)
        {
            // Check in appsettings.json first using IFeatureManager
            bool isEnabledInConfig = await _featureManager.IsEnabledAsync(featureName);
            if (isEnabledInConfig)
            {
                return true;
            }

            // If not found or disabled in appsettings.json, check the database
            var featureFlag = await _fdb.FeatureFlags.FirstOrDefaultAsync(f => f.Name == featureName);
            return featureFlag?.IsEnabled ?? false; // Default to false if not found
        }

        public IEnumerable<FeatureFlag> GetAll()
        {
            return _fdb.FeatureFlags.ToList();
        }


    }
}
