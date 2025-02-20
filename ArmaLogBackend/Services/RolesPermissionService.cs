namespace ArmaLogBackend.Services;

/// <summary>
/// Example roles/permissions => admin, moderator, viewer
/// </summary>
public enum UserRole { Admin, Moderator, Viewer }

public static class RolesPermissionsService
{
    public static bool CanEditPlayers(UserRole role) => (role == UserRole.Admin || role == UserRole.Moderator);
    public static bool CanViewPlayers(UserRole role) => true;
    public static bool CanManageLicenses(UserRole role) => (role == UserRole.Admin);
}
