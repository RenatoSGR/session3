using System;
using System.Threading.Channels;
using ContosoUniversity.Models;
using Microsoft.Extensions.Configuration;

namespace ContosoUniversity.Services
{
    public class NotificationService : INotificationService
    {
        private readonly Channel<Notification> _channel;

        public NotificationService(IConfiguration configuration)
        {
            var capacity = configuration.GetValue<int>("NotificationQueue:Capacity", 100);
            _channel = Channel.CreateBounded<Notification>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            });
        }

        public void SendNotification(string entityType, string entityId, EntityOperation operation, string userName = null)
        {
            SendNotification(entityType, entityId, null, operation, userName);
        }

        public void SendNotification(string entityType, string entityId, string entityDisplayName, EntityOperation operation, string userName = null)
        {
            try
            {
                var notification = new Notification
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Operation = operation.ToString(),
                    Message = GenerateMessage(entityType, entityId, entityDisplayName, operation),
                    CreatedAt = DateTime.Now,
                    CreatedBy = userName ?? "System",
                    IsRead = false
                };

                _channel.Writer.TryWrite(notification);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to send notification: {ex.Message}");
            }
        }

        public Notification ReceiveNotification()
        {
            if (_channel.Reader.TryRead(out var notification))
            {
                return notification;
            }
            return null;
        }

        public void MarkAsRead(int notificationId)
        {
            // In-memory channel does not persist; a database-backed implementation
            // would update the IsRead flag here.
        }

        private static string GenerateMessage(string entityType, string entityId, string entityDisplayName, EntityOperation operation)
        {
            var displayText = !string.IsNullOrWhiteSpace(entityDisplayName)
                ? $"{entityType} '{entityDisplayName}'"
                : $"{entityType} (ID: {entityId})";

            switch (operation)
            {
                case EntityOperation.CREATE: return $"New {displayText} has been created";
                case EntityOperation.UPDATE: return $"{displayText} has been updated";
                case EntityOperation.DELETE: return $"{displayText} has been deleted";
                default: return $"{displayText} operation: {operation}";
            }
        }
    }
}
