using Crypen.Core.Models;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace Crypen.Core.Services;

/// <summary>
/// Service for managing encryption history using SQLite
/// </summary>
public class HistoryService
{
    private readonly string _dbPath;
    
    public HistoryService(string storageDirectory)
    {
        Directory.CreateDirectory(storageDirectory);
        _dbPath = Path.Combine(storageDirectory, "history.db");
        
        InitializeDatabase();
    }
    
    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS EncryptedItems (
                Id TEXT PRIMARY KEY,
                Path TEXT NOT NULL,
                Name TEXT NOT NULL,
                ItemType INTEGER NOT NULL,
                EncryptedAt TEXT NOT NULL,
                IsPasswordStored INTEGER NOT NULL,
                Status TEXT NOT NULL,
                Properties TEXT
            );
        ";
        
        command.ExecuteNonQuery();
    }
    
    /// <summary>
    /// Adds a new encrypted item to the history
    /// </summary>
    /// <param name="item">The encrypted item to add</param>
    public void AddItem(EncryptedItem item)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO EncryptedItems (Id, Path, Name, ItemType, EncryptedAt, IsPasswordStored, Status, Properties)
            VALUES (@Id, @Path, @Name, @ItemType, @EncryptedAt, @IsPasswordStored, @Status, @Properties);
        ";
        
        command.Parameters.AddWithValue("@Id", item.Id.ToString());
        command.Parameters.AddWithValue("@Path", item.Path);
        command.Parameters.AddWithValue("@Name", item.Name);
        command.Parameters.AddWithValue("@ItemType", (int)item.ItemType);
        command.Parameters.AddWithValue("@EncryptedAt", item.EncryptedAt.ToString("o"));
        command.Parameters.AddWithValue("@IsPasswordStored", item.IsPasswordStored ? 1 : 0);
        command.Parameters.AddWithValue("@Status", item.Status);
        command.Parameters.AddWithValue("@Properties", "{}"); // JSON properties for extensibility
        
        command.ExecuteNonQuery();
    }
    
    /// <summary>
    /// Updates an existing encrypted item in the history
    /// </summary>
    /// <param name="item">The encrypted item to update</param>
    public void UpdateItem(EncryptedItem item)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE EncryptedItems
            SET Path = @Path,
                Name = @Name,
                ItemType = @ItemType,
                EncryptedAt = @EncryptedAt,
                IsPasswordStored = @IsPasswordStored,
                Status = @Status
            WHERE Id = @Id;
        ";
        
        command.Parameters.AddWithValue("@Id", item.Id.ToString());
        command.Parameters.AddWithValue("@Path", item.Path);
        command.Parameters.AddWithValue("@Name", item.Name);
        command.Parameters.AddWithValue("@ItemType", (int)item.ItemType);
        command.Parameters.AddWithValue("@EncryptedAt", item.EncryptedAt.ToString("o"));
        command.Parameters.AddWithValue("@IsPasswordStored", item.IsPasswordStored ? 1 : 0);
        command.Parameters.AddWithValue("@Status", item.Status);
        
        command.ExecuteNonQuery();
    }
    
    /// <summary>
    /// Removes an encrypted item from the history
    /// </summary>
    /// <param name="itemId">The ID of the item to remove</param>
    public void RemoveItem(Guid itemId)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM EncryptedItems WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", itemId.ToString());
        
        command.ExecuteNonQuery();
    }
    
    /// <summary>
    /// Gets all encrypted items in the history
    /// </summary>
    /// <returns>A list of encrypted items</returns>
    public List<EncryptedItem> GetItems()
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM EncryptedItems ORDER BY EncryptedAt DESC;";
        
        using var reader = command.ExecuteReader();
        
        var items = new List<EncryptedItem>();
        
        while (reader.Read())
        {
            var item = new EncryptedItem
            {
                Id = Guid.Parse(reader.GetString(0)),
                Path = reader.GetString(1),
                Name = reader.GetString(2),
                ItemType = (EncryptedItemType)reader.GetInt32(3),
                EncryptedAt = DateTime.Parse(reader.GetString(4)),
                IsPasswordStored = reader.GetInt32(5) == 1,
                Status = reader.GetString(6)
            };
            
            items.Add(item);
        }
        
        return items;
    }
    
    /// <summary>
    /// Gets a specific encrypted item by ID
    /// </summary>
    /// <param name="itemId">The ID of the item to get</param>
    /// <returns>The encrypted item, or null if not found</returns>
    public EncryptedItem? GetItem(Guid itemId)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM EncryptedItems WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", itemId.ToString());
        
        using var reader = command.ExecuteReader();
        
        if (reader.Read())
        {
            return new EncryptedItem
            {
                Id = Guid.Parse(reader.GetString(0)),
                Path = reader.GetString(1),
                Name = reader.GetString(2),
                ItemType = (EncryptedItemType)reader.GetInt32(3),
                EncryptedAt = DateTime.Parse(reader.GetString(4)),
                IsPasswordStored = reader.GetInt32(5) == 1,
                Status = reader.GetString(6)
            };
        }
        
        return null;
    }
    
    /// <summary>
    /// Finds an encrypted item by its path
    /// </summary>
    /// <param name="path">The path to search for</param>
    /// <returns>The encrypted item, or null if not found</returns>
    public EncryptedItem? FindItemByPath(string path)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM EncryptedItems WHERE Path = @Path;";
        command.Parameters.AddWithValue("@Path", path);
        
        using var reader = command.ExecuteReader();
        
        if (reader.Read())
        {
            return new EncryptedItem
            {
                Id = Guid.Parse(reader.GetString(0)),
                Path = reader.GetString(1),
                Name = reader.GetString(2),
                ItemType = (EncryptedItemType)reader.GetInt32(3),
                EncryptedAt = DateTime.Parse(reader.GetString(4)),
                IsPasswordStored = reader.GetInt32(5) == 1,
                Status = reader.GetString(6)
            };
        }
        
        return null;
    }
}
