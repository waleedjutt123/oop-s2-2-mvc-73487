using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FoodSafetyTracker.Tests;

public class RoleAuthorizationTests
{
    private static MethodInfo? GetGetMethod(Type type, string name, int parameterCount)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == name && m.GetParameters().Length == parameterCount && m.GetCustomAttribute<HttpPostAttribute>() == null);
    }

    [Fact]
    public void PremisesController_CreateEditDelete_RequireAdminRole()
    {
        var type = typeof(FoodSafetyTracker.Controllers.PremisesController);
        var createGet = GetGetMethod(type, "Create", 0);
        var editGet = GetGetMethod(type, "Edit", 2);
        var deleteGet = GetGetMethod(type, "Delete", 2);
        Assert.NotNull(createGet);
        Assert.NotNull(editGet);
        Assert.NotNull(deleteGet);
        AssertHasAuthorizeRole(createGet, "Admin");
        AssertHasAuthorizeRole(editGet, "Admin");
        AssertHasAuthorizeRole(deleteGet, "Admin");
    }

    [Fact]
    public void InspectionsController_CreateEditDelete_RequireAdminOrInspector()
    {
        var type = typeof(FoodSafetyTracker.Controllers.InspectionsController);
        var createGet = GetGetMethod(type, "Create", 1);
        var editGet = GetGetMethod(type, "Edit", 2);
        var deleteGet = GetGetMethod(type, "Delete", 2);
        Assert.NotNull(createGet);
        Assert.NotNull(editGet);
        Assert.NotNull(deleteGet);
        AssertHasAuthorizeWithRoles(createGet, "Admin", "Inspector");
        AssertHasAuthorizeWithRoles(editGet, "Admin", "Inspector");
        AssertHasAuthorizeWithRoles(deleteGet, "Admin", "Inspector");
    }

    private static void AssertHasAuthorizeRole(MethodInfo method, string expectedRole)
    {
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authorize);
        Assert.NotNull(authorize.Roles);
        Assert.Contains(expectedRole, authorize.Roles.Split(',').Select(r => r.Trim()));
    }

    private static void AssertHasAuthorizeWithRoles(MethodInfo method, string role1, string role2)
    {
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authorize);
        Assert.NotNull(authorize.Roles);
        var roles = authorize.Roles!.Split(',').Select(r => r.Trim()).ToHashSet();
        Assert.Contains(role1, roles);
        Assert.Contains(role2, roles);
    }
}
