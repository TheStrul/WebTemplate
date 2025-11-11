namespace WebTemplate.UnitTests.Builders
{
    using WebTemplate.Core.Entities;

    /// <summary>
    /// Builder for creating RefreshToken test data
    /// </summary>
    public class RefreshTokenBuilder
    {
        private int _id = 1;
        private string _token = Guid.NewGuid().ToString();
        private string _userId = Guid.NewGuid().ToString();
        private DateTime _expiryDate = DateTime.UtcNow.AddDays(7);
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime? _revokedAt = null;
        private string? _deviceId = "test-device-id";
        private string? _deviceName = "Test Device";

        public RefreshTokenBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public RefreshTokenBuilder WithToken(string token)
        {
            _token = token;
            return this;
        }

        public RefreshTokenBuilder WithUserId(string userId)
        {
            _userId = userId;
            return this;
        }

        public RefreshTokenBuilder WithExpiryDate(DateTime expiryDate)
        {
            _expiryDate = expiryDate;
            return this;
        }

        public RefreshTokenBuilder AsExpired()
        {
            _expiryDate = DateTime.UtcNow.AddDays(-1);
            return this;
        }

        public RefreshTokenBuilder AsRevoked()
        {
            _revokedAt = DateTime.UtcNow;
            return this;
        }

        public RefreshTokenBuilder WithDeviceId(string? deviceId)
        {
            _deviceId = deviceId;
            return this;
        }

        public RefreshTokenBuilder WithDeviceName(string? deviceName)
        {
            _deviceName = deviceName;
            return this;
        }

        public RefreshToken Build() => new()
        {
            Id = _id,
            Token = _token,
            UserId = _userId,
            ExpiryDate = _expiryDate,
            CreatedAt = _createdAt,
            RevokedAt = _revokedAt,
            DeviceId = _deviceId,
            DeviceName = _deviceName
        };
    }
}
