﻿using AktBob.Database.Entities;

namespace AktBob.Database.Contracts;
public interface IMessageRepository
{
    /// <summary>
    /// Add a new message
    /// </summary>
    /// <param name="message"></param>
    /// <returns>True if the message was added</returns>
    Task<bool> Add(Message message);
    Task<bool> Delete(int id);
    Task<Message?> GetByDeskproMessageId(int deskproMessageId);
    Task<Message?> Get(int id);
    Task<bool> Update(Message message);
}
