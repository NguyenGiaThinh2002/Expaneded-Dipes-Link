using System.Windows;
using static DipesLink.Datatypes.CommonDataType;

namespace DipesLink.Extensions
{
    public class AuthorizationHelper
    {
        public static bool IsUserInRole(UserRole role)
        {
            if (Application.Current.Properties["UserRole"] != null)
            {
                UserRole currentUserRole = (UserRole)Application.Current.Properties["UserRole"];
                return currentUserRole == role;
            }
            return false;
        }

        public static UserRole GetRole(object stringRole)
        {
            switch (stringRole)
            {
                case "Administrator":
                    return UserRole.Administrator;
                case "Operator":
                    return UserRole.Operator;
                case "User":
                    return UserRole.User;
                case "Guest":
                    return UserRole.Guest;
                default:
                    return UserRole.Guest;
            }
        }

        public static bool IsAdmin()
        {
            return IsUserInRole(UserRole.Administrator);
        }

        public static bool IsOperator()
        {
            return IsUserInRole(UserRole.Operator);
        }

        public static bool IsUser()
        {
            return IsUserInRole(UserRole.User);
        }

        public static bool IsGuest()
        {
            return IsUserInRole(UserRole.Guest);
        }
    }
}
