using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeaconCCGManagement.DAL;

namespace DeaconCCGManagement.Tests
{
    [TestClass]
    public class TestSeedDatabase
    {
        [TestMethod]
        public void DatabaseSeederTest()
        {
            var seeder = new DatabaseSeeder();
            seeder.SeedDatabase(new CcgDbContext());

            
        }
    }
}
