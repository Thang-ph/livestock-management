using System;

namespace LivestockManagementSystemAPI.Extensions;

public class TokenSessionService
{
    // Dictionary lưu trữ token với key là sessionId
    private readonly Dictionary<string, TokenSessionData> _sessions = new();

    // Thời gian hết hạn mặc định là 5 phút
    private readonly TimeSpan _expiration = TimeSpan.FromMinutes(5);

    public class TokenSessionData
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    // Tạo session mới và trả về sessionId
    public string CreateSession(string token)
    {
        // Tạo sessionId ngẫu nhiên
        var sessionId = Guid.NewGuid().ToString("N");

        // Lưu token với thời gian hết hạn
        _sessions[sessionId] = new TokenSessionData
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.Add(_expiration)
        };

        // Dọn dẹp các session cũ
        CleanupExpiredSessions();

        return sessionId;
    }

    // Lấy token từ sessionId và xóa session đó
    public string GetAndRemoveToken(string sessionId)
    {
        // Kiểm tra session tồn tại và chưa hết hạn
        if (_sessions.TryGetValue(sessionId, out var data) && data.ExpiresAt > DateTime.UtcNow)
        {
            // Xóa session
            _sessions.Remove(sessionId);
            return data.Token;
        }

        return null;
    }

    // Xóa các session đã hết hạn
    private void CleanupExpiredSessions()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _sessions.Where(kvp => kvp.Value.ExpiresAt < now)
                                 .Select(kvp => kvp.Key)
                                 .ToList();

        foreach (var key in expiredKeys)
        {
            _sessions.Remove(key);
        }
    }
}
