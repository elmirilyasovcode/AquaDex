using Xunit;

namespace AquaDex.Tests;


public class RoleProtectionTests
{
    private static readonly string[] ProtectedRoles =
        { "Angler", "VerifiedExpert", "FishingGuide", "ShopOwner", "Admin" };

    private static bool IsDeletionAllowed(string roleName) => !ProtectedRoles.Contains(roleName);

    [Theory]
    [InlineData("Angler")]
    [InlineData("VerifiedExpert")]
    [InlineData("FishingGuide")]
    [InlineData("ShopOwner")]
    [InlineData("Admin")]
    public void CoreRoles_CannotBeDeleted(string coreRoleName)
    {
        var allowed = IsDeletionAllowed(coreRoleName);

        Assert.False(allowed);
    }

    [Fact]
    public void CustomRole_CanBeDeleted()
    {
        var allowed = IsDeletionAllowed("SomeCustomRoleAnAdminCreated");

        Assert.True(allowed);
    }

    [Fact]
    public void RoleNameComparison_IsCaseSensitive()
    {
      
        var allowed = IsDeletionAllowed("admin");

        Assert.True(allowed);
    }
}