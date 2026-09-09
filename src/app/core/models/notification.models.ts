export type NotificationChannel = 'Email' | 'InApp';
export type NotificationStatus = 'Pending' | 'Sent' | 'Failed';

export interface Notification {
  id: string;
  channel: NotificationChannel;
  subject: string;
  recipient: string;
  status: NotificationStatus;
  templateKey?: string;
  createdAt: string;
  sentAt?: string;
}