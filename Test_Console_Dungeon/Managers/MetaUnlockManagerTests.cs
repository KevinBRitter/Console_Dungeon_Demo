using System;
using System.IO;
using Console_Dungeon.Managers;
using Xunit;

namespace Console_Dungeon.Tests.Managers
{
    public class MetaUnlockManagerTests
    {
        [Fact]
        public void Unlock_PersistsAndReportsUnlocked()
        {
            // Use a temp file so tests don't touch actual Data/MetaUnlocks.json
            string temp = Path.Combine(Path.GetTempPath(), $"meta_{Guid.NewGuid():N}.json");
            try
            {
                MetaUnlockManager.Load(temp);
                string id = "test_meta_item";
                MetaUnlockManager.Unlock(id);
                Assert.True(MetaUnlockManager.IsUnlocked(id));

                // Reload from file to ensure persistence
                MetaUnlockManager.Load(temp);
                Assert.True(MetaUnlockManager.IsUnlocked(id));
            }
            finally
            {
                if (File.Exists(temp)) File.Delete(temp);
            }
        }
    }
}
