using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Models
{
    public class Notification : BaseEntity
    {
        public entity_action Action { get; set; }
        public entity_type Type { get; set; }
        public string Message { get; set; }
        public string ItemIds { get; set; }
        public virtual ICollection<UserNotification> UserNotifications { get; set; }
    }
}
