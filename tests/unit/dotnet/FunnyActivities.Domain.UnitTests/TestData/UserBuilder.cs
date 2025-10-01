using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.UnitTests.TestData
{
    public class UserBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _email = "test@example.com";
        private string _passwordHash = "hashedpassword";
        private string _firstName = "John";
        private string _lastName = "Doe";
        private string _profileImageUrl = null;

        public UserBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public UserBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public UserBuilder WithPasswordHash(string passwordHash)
        {
            _passwordHash = passwordHash;
            return this;
        }

        public UserBuilder WithFirstName(string firstName)
        {
            _firstName = firstName;
            return this;
        }

        public UserBuilder WithLastName(string lastName)
        {
            _lastName = lastName;
            return this;
        }

        public UserBuilder WithProfileImageUrl(string profileImageUrl)
        {
            _profileImageUrl = profileImageUrl;
            return this;
        }

        public User Build()
        {
            var user = new User(_id, _email, _passwordHash, _firstName, _lastName);
            if (_profileImageUrl != null)
            {
                user.UpdateProfile(_firstName, _lastName, _profileImageUrl);
            }
            return user;
        }

        public static UserBuilder Create() => new UserBuilder();

        public static User DefaultUser() => Create().Build();

        public static User AdminUser() => Create()
            .WithEmail("admin@example.com")
            .WithFirstName("Admin")
            .WithLastName("User")
            .Build();

        public static User RegularUser() => Create()
            .WithEmail("user@example.com")
            .WithFirstName("Regular")
            .WithLastName("User")
            .Build();
    }
}