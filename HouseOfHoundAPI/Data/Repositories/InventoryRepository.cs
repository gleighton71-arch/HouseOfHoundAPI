using HouseOfHound.Api.Models;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace HouseOfHound.Api.Repositories
{
    public class InventoryRepository
    {
        public List<StockItem> GetStockItems(string searchTerm = null, bool activeOnly = true)
        {
            var items = new List<StockItem>();

            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    SELECT
                        Id,
                        Code,
                        Description,
                        QuantityInStock,
                        MinimumStockHolding,
                        CostPrice,
                        SalePrice,
                        IsActive,
                        CreatedDateUTC,
                        UpdatedDateUTC
                    FROM dbo.StockItem
                    WHERE
                        (@ActiveOnly = 0 OR IsActive = 1)
                        AND
                        (
                            @SearchTerm IS NULL
                            OR Code LIKE '%' + @SearchTerm + '%'
                            OR Description LIKE '%' + @SearchTerm + '%'
                        )
                    ORDER BY Code;", conn))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? (object)DBNull.Value : searchTerm);
                    cmd.Parameters.AddWithValue("@ActiveOnly", activeOnly);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(MapStockItem(reader));
                        }
                    }
                }
            }

            return items;
        }

        public StockItem GetStockItemById(int id)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    SELECT
                        Id,
                        Code,
                        Description,
                        QuantityInStock,
                        MinimumStockHolding,
                        CostPrice,
                        SalePrice,
                        IsActive,
                        CreatedDateUTC,
                        UpdatedDateUTC
                    FROM dbo.StockItem
                    WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapStockItem(reader);
                        }
                    }
                }
            }

            return null;
        }

        public StockItem CreateStockItem(StockItem item)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.StockItem
                    (
                        Code,
                        Description,
                        QuantityInStock,
                        MinimumStockHolding,
                        CostPrice,
                        SalePrice,
                        IsActive,
                        CreatedDateUTC
                    )
                    OUTPUT
                        INSERTED.Id,
                        INSERTED.Code,
                        INSERTED.Description,
                        INSERTED.QuantityInStock,
                        INSERTED.MinimumStockHolding,
                        INSERTED.CostPrice,
                        INSERTED.SalePrice,
                        INSERTED.IsActive,
                        INSERTED.CreatedDateUTC,
                        INSERTED.UpdatedDateUTC
                    VALUES
                    (
                        @Code,
                        @Description,
                        @QuantityInStock,
                        @MinimumStockHolding,
                        @CostPrice,
                        @SalePrice,
                        1,
                        SYSUTCDATETIME()
                    );", conn))
                {
                    cmd.Parameters.AddWithValue("@Code", item.Code);
                    cmd.Parameters.AddWithValue("@Description", item.Description);
                    cmd.Parameters.AddWithValue("@QuantityInStock", item.QuantityInStock);
                    cmd.Parameters.AddWithValue("@MinimumStockHolding", item.MinimumStockHolding);
                    cmd.Parameters.AddWithValue("@CostPrice", item.CostPrice);
                    cmd.Parameters.AddWithValue("@SalePrice", item.SalePrice);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapStockItem(reader);
                        }
                    }
                }
            }

            throw new Exception("Failed to create stock item.");
        }

        public bool UpdateStockItem(StockItem item)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.StockItem
                    SET
                        Code = @Code,
                        Description = @Description,
                        QuantityInStock = @QuantityInStock,
                        MinimumStockHolding = @MinimumStockHolding,
                        CostPrice = @CostPrice,
                        SalePrice = @SalePrice,
                        IsActive = @IsActive,
                        UpdatedDateUTC = SYSUTCDATETIME()
                    WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", item.Id);
                    cmd.Parameters.AddWithValue("@Code", item.Code);
                    cmd.Parameters.AddWithValue("@Description", item.Description);
                    cmd.Parameters.AddWithValue("@QuantityInStock", item.QuantityInStock);
                    cmd.Parameters.AddWithValue("@MinimumStockHolding", item.MinimumStockHolding);
                    cmd.Parameters.AddWithValue("@CostPrice", item.CostPrice);
                    cmd.Parameters.AddWithValue("@SalePrice", item.SalePrice);
                    cmd.Parameters.AddWithValue("@IsActive", item.IsActive);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool SoftDeleteStockItem(int id)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.StockItem
                    SET
                        IsActive = 0,
                        UpdatedDateUTC = SYSUTCDATETIME()
                    WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool AdjustStock(int stockItemId, int quantityChange, string note)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(@"
                            UPDATE dbo.StockItem
                            SET
                                QuantityInStock = QuantityInStock + @QuantityChange,
                                UpdatedDateUTC = SYSUTCDATETIME()
                            WHERE Id = @StockItemId;", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@StockItemId", stockItemId);
                            cmd.Parameters.AddWithValue("@QuantityChange", quantityChange);

                            if (cmd.ExecuteNonQuery() == 0)
                            {
                                tran.Rollback();
                                return false;
                            }
                        }

                        using (var cmd = new SqlCommand(@"
                            INSERT INTO dbo.StockMovement
                            (
                                StockItemId,
                                MovementType,
                                QuantityChange,
                                ReferenceType,
                                ReferenceId,
                                Note,
                                CreatedDateUTC
                            )
                            VALUES
                            (
                                @StockItemId,
                                'Adjustment',
                                @QuantityChange,
                                'Manual',
                                NULL,
                                @Note,
                                SYSUTCDATETIME()
                            );", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@StockItemId", stockItemId);
                            cmd.Parameters.AddWithValue("@QuantityChange", quantityChange);
                            cmd.Parameters.AddWithValue("@Note", string.IsNullOrWhiteSpace(note) ? (object)DBNull.Value : note);

                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                        return true;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public Sale CreateSale(CreateSaleRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.Lines == null || request.Lines.Count == 0)
                throw new ArgumentException("Sale must contain at least one line.");

            using (var conn = HohManager.GetOpenConnection())
            {
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var saleLinesToCreate = new List<SaleLine>();
                        decimal totalAmount = 0;

                        foreach (var line in request.Lines)
                        {
                            if (line.Quantity <= 0)
                                throw new ArgumentException("Quantity must be greater than zero.");

                            var stockItem = GetStockItemById(line.StockItemId, conn, tran);

                            if (stockItem == null)
                                throw new ArgumentException("Stock item not found: " + line.StockItemId);

                            if (!stockItem.IsActive)
                                throw new ArgumentException("Stock item is inactive: " + stockItem.Code);

                            if (stockItem.QuantityInStock < line.Quantity)
                                throw new ArgumentException("Not enough stock for " + stockItem.Code + ".");

                            var saleLine = new SaleLine
                            {
                                StockItemId = stockItem.Id,
                                StockCode = stockItem.Code,
                                StockDescription = stockItem.Description,
                                Quantity = line.Quantity,
                                UnitPrice = stockItem.SalePrice,
                                UnitCost = stockItem.CostPrice,
                                LineTotal = line.Quantity * stockItem.SalePrice
                            };

                            saleLinesToCreate.Add(saleLine);
                            totalAmount += saleLine.LineTotal;
                        }

                        int saleId;

                        using (var cmd = new SqlCommand(@"
                            INSERT INTO dbo.Sale
                            (
                                CustomerName,
                                CustomerId,
                                DogId,
                                SaleDateUTC,
                                PaymentMethod,
                                TotalAmount,
                                ReceiptPdfPath,
                                CreatedDateUTC
                            )
                            VALUES
                            (
                                @CustomerName,
                                @CustomerId,
                                @DogId,
                                SYSUTCDATETIME(),
                                @PaymentMethod,
                                @TotalAmount,
                                @ReceiptPdfPath,
                                SYSUTCDATETIME()
                            );

                            SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@CustomerName", string.IsNullOrWhiteSpace(request.CustomerName) ? (object)DBNull.Value : request.CustomerName);
                            cmd.Parameters.AddWithValue("@CustomerId", request.CustomerId.HasValue ? (object)request.CustomerId.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@DogId", request.DogId.HasValue ? (object)request.DogId.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@PaymentMethod", request.PaymentMethod);
                            cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                            cmd.Parameters.AddWithValue("@ReceiptPdfPath", string.IsNullOrWhiteSpace(request.ReceiptPdfPath) ? (object)DBNull.Value : request.ReceiptPdfPath);

                            saleId = (int)cmd.ExecuteScalar();
                        }

                        foreach (var line in saleLinesToCreate)
                        {
                            using (var cmd = new SqlCommand(@"
                                INSERT INTO dbo.SaleLine
                                (
                                    SaleId,
                                    StockItemId,
                                    Quantity,
                                    UnitPrice,
                                    UnitCost
                                )
                                VALUES
                                (
                                    @SaleId,
                                    @StockItemId,
                                    @Quantity,
                                    @UnitPrice,
                                    @UnitCost
                                );", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@SaleId", saleId);
                                cmd.Parameters.AddWithValue("@StockItemId", line.StockItemId);
                                cmd.Parameters.AddWithValue("@Quantity", line.Quantity);
                                cmd.Parameters.AddWithValue("@UnitPrice", line.UnitPrice);
                                cmd.Parameters.AddWithValue("@UnitCost", line.UnitCost.HasValue ? (object)line.UnitCost.Value : DBNull.Value);

                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SqlCommand(@"
                                UPDATE dbo.StockItem
                                SET
                                    QuantityInStock = QuantityInStock - @Quantity,
                                    UpdatedDateUTC = SYSUTCDATETIME()
                                WHERE Id = @StockItemId;", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@StockItemId", line.StockItemId);
                                cmd.Parameters.AddWithValue("@Quantity", line.Quantity);

                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SqlCommand(@"
                                INSERT INTO dbo.StockMovement
                                (
                                    StockItemId,
                                    MovementType,
                                    QuantityChange,
                                    ReferenceType,
                                    ReferenceId,
                                    Note,
                                    CreatedDateUTC
                                )
                                VALUES
                                (
                                    @StockItemId,
                                    'Sale',
                                    @QuantityChange,
                                    'Sale',
                                    @SaleId,
                                    @Note,
                                    SYSUTCDATETIME()
                                );", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@StockItemId", line.StockItemId);
                                cmd.Parameters.AddWithValue("@QuantityChange", -line.Quantity);
                                cmd.Parameters.AddWithValue("@SaleId", saleId);
                                cmd.Parameters.AddWithValue("@Note", "Stock sold");

                                cmd.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();

                        return GetSaleById(saleId);
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<Sale> GetSalesHistory(int maxResults = 100, int? dogId = null)
        {
            var sales = new List<Sale>();

            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    SELECT TOP (@MaxResults)
                        Id,
                        CustomerName,
                        CustomerId,
                        DogId,
                        SaleDateUTC,
                        PaymentMethod,
                        TotalAmount,
                        ReceiptPdfPath,
                        CreatedDateUTC
                    FROM dbo.Sale
                    WHERE @DogId IS NULL OR DogId = @DogId
                    ORDER BY SaleDateUTC DESC;", conn))
                {
                    cmd.Parameters.AddWithValue("@MaxResults", maxResults);
                    cmd.Parameters.AddWithValue("@DogId", dogId.HasValue ? (object)dogId.Value : DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sales.Add(MapSale(reader));
                        }
                    }
                }

                foreach (var sale in sales)
                {
                    sale.Lines = GetSaleLines(sale.Id, conn);
                }
            }

            return sales;
        }

        public Sale GetSaleById(int saleId)
        {
            Sale sale = null;

            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    SELECT
                        Id,
                        CustomerName,
                        CustomerId,
                        DogId,
                        SaleDateUTC,
                        PaymentMethod,
                        TotalAmount,
                        ReceiptPdfPath,
                        CreatedDateUTC
                    FROM dbo.Sale
                    WHERE Id = @SaleId;", conn))
                {
                    cmd.Parameters.AddWithValue("@SaleId", saleId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            sale = MapSale(reader);
                        }
                    }
                }

                if (sale != null)
                {
                    sale.Lines = GetSaleLines(sale.Id, conn);
                }
            }

            return sale;
        }

        public List<StockMovement> GetStockMovements(int stockItemId)
        {
            var movements = new List<StockMovement>();

            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    SELECT
                        sm.Id,
                        sm.StockItemId,
                        si.Code AS StockCode,
                        si.Description AS StockDescription,
                        sm.MovementType,
                        sm.QuantityChange,
                        sm.ReferenceType,
                        sm.ReferenceId,
                        sm.Note,
                        sm.CreatedDateUTC
                    FROM dbo.StockMovement sm
                    INNER JOIN dbo.StockItem si
                        ON si.Id = sm.StockItemId
                    WHERE sm.StockItemId = @StockItemId
                    ORDER BY sm.CreatedDateUTC DESC;", conn))
                {
                    cmd.Parameters.AddWithValue("@StockItemId", stockItemId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            movements.Add(MapStockMovement(reader));
                        }
                    }
                }
            }

            return movements;
        }

        private StockItem GetStockItemById(int id, SqlConnection conn, SqlTransaction tran)
        {
            using (var cmd = new SqlCommand(@"
                SELECT
                    Id,
                    Code,
                    Description,
                    QuantityInStock,
                    MinimumStockHolding,
                    CostPrice,
                    SalePrice,
                    IsActive,
                    CreatedDateUTC,
                    UpdatedDateUTC
                FROM dbo.StockItem
                WHERE Id = @Id;", conn, tran))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapStockItem(reader);
                    }
                }
            }

            return null;
        }

        private List<SaleLine> GetSaleLines(int saleId, SqlConnection conn)
        {
            var lines = new List<SaleLine>();

            using (var cmd = new SqlCommand(@"
                SELECT
                    sl.Id,
                    sl.SaleId,
                    sl.StockItemId,
                    si.Code AS StockCode,
                    si.Description AS StockDescription,
                    sl.Quantity,
                    sl.UnitPrice,
                    sl.UnitCost,
                    sl.LineTotal
                FROM dbo.SaleLine sl
                INNER JOIN dbo.StockItem si
                    ON si.Id = sl.StockItemId
                WHERE sl.SaleId = @SaleId
                ORDER BY sl.Id;", conn))
            {
                cmd.Parameters.AddWithValue("@SaleId", saleId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lines.Add(MapSaleLine(reader));
                    }
                }
            }

            return lines;
        }

        private StockItem MapStockItem(SqlDataReader reader)
        {
            return new StockItem
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Code = reader.GetString(reader.GetOrdinal("Code")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                QuantityInStock = reader.GetInt32(reader.GetOrdinal("QuantityInStock")),
                MinimumStockHolding = reader.GetInt32(reader.GetOrdinal("MinimumStockHolding")),
                CostPrice = reader.GetDecimal(reader.GetOrdinal("CostPrice")),
                SalePrice = reader.GetDecimal(reader.GetOrdinal("SalePrice")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedDateUTC = reader.GetDateTime(reader.GetOrdinal("CreatedDateUTC")),
                UpdatedDateUTC = reader.IsDBNull(reader.GetOrdinal("UpdatedDateUTC"))
                    ? (DateTime?)null
                    : reader.GetDateTime(reader.GetOrdinal("UpdatedDateUTC"))
            };
        }

        private Sale MapSale(SqlDataReader reader)
        {
            return new Sale
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("CustomerName")),
                CustomerId = reader.IsDBNull(reader.GetOrdinal("CustomerId"))
                    ? (int?)null
                    : reader.GetInt32(reader.GetOrdinal("CustomerId")),
                DogId = reader.IsDBNull(reader.GetOrdinal("DogId"))
                    ? (int?)null
                    : reader.GetInt32(reader.GetOrdinal("DogId")),
                SaleDateUTC = reader.GetDateTime(reader.GetOrdinal("SaleDateUTC")),
                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                ReceiptPdfPath = reader.IsDBNull(reader.GetOrdinal("ReceiptPdfPath"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ReceiptPdfPath")),
                CreatedDateUTC = reader.GetDateTime(reader.GetOrdinal("CreatedDateUTC"))
            };
        }

        private SaleLine MapSaleLine(SqlDataReader reader)
        {
            return new SaleLine
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                SaleId = reader.GetInt32(reader.GetOrdinal("SaleId")),
                StockItemId = reader.GetInt32(reader.GetOrdinal("StockItemId")),
                StockCode = reader.GetString(reader.GetOrdinal("StockCode")),
                StockDescription = reader.GetString(reader.GetOrdinal("StockDescription")),
                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                UnitCost = reader.IsDBNull(reader.GetOrdinal("UnitCost"))
                    ? (decimal?)null
                    : reader.GetDecimal(reader.GetOrdinal("UnitCost")),
                LineTotal = reader.GetDecimal(reader.GetOrdinal("LineTotal"))
            };
        }

        private StockMovement MapStockMovement(SqlDataReader reader)
        {
            return new StockMovement
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                StockItemId = reader.GetInt32(reader.GetOrdinal("StockItemId")),
                StockCode = reader.GetString(reader.GetOrdinal("StockCode")),
                StockDescription = reader.GetString(reader.GetOrdinal("StockDescription")),
                MovementType = reader.GetString(reader.GetOrdinal("MovementType")),
                QuantityChange = reader.GetInt32(reader.GetOrdinal("QuantityChange")),
                ReferenceType = reader.IsDBNull(reader.GetOrdinal("ReferenceType"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ReferenceType")),
                ReferenceId = reader.IsDBNull(reader.GetOrdinal("ReferenceId"))
                    ? (int?)null
                    : reader.GetInt32(reader.GetOrdinal("ReferenceId")),
                Note = reader.IsDBNull(reader.GetOrdinal("Note"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Note")),
                CreatedDateUTC = reader.GetDateTime(reader.GetOrdinal("CreatedDateUTC"))
            };
        }
    }
}
