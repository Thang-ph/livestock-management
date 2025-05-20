using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class UserNotification : BaseEntity
    {
        public string UserId { get; set; }
        public string NotificationId { get; set; }
        public bool IsRead { get; set; }
        public virtual Notification Notification { get; set; }
    }
}
