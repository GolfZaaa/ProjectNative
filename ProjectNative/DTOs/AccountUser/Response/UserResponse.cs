using ProjectNative.Models;
using ProjectNative.SettingUrl;

namespace ProjectNative.DTOs.AccountUser.Response
{
    public class UserResponse
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ProfileImageUrl { get; set; }


        public static UserResponse FromUser(ApplicationUser user)
        {
            var profileImageUrl = !string.IsNullOrEmpty(user.ProfileImage)
                ? $"{ApplicationUrl.Url}/orderImage/{user.ProfileImage}"
                : null;

            return new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                ProfileImageUrl = profileImageUrl,
            };
        }
    }
}
