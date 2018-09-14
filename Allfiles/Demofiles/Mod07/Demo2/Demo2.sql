-- Display the list of databases that comprise the distributions for the SQL Data Warehouse
SELECT *
FROM sys.pdw_nodes_pdw_physical_databases
GO

-- Create a table with the ROUND ROBIN distribution
CREATE TABLE dbo.CustomerPortfolio
(
    CustomerID int NOT NULL,
    Ticker char(4) NOT NULL,
    VolumeBought int NOT NULL,
    WhenBought datetime NOT NULL
)
WITH
(
    DISTRIBUTION = ROUND_ROBIN
)
GO

-- Create a table distributed by hashing the Ticker column
CREATE TABLE dbo.StockPriceMovement
(
    Ticker char(4) NOT NULL,
    Price int NOT NULL,
    PriceChangedWhen datetime NOT NULL
)
WITH
(
    DISTRIBUTION = HASH(Ticker)
)
GO

-- Create a table that is replicated across every database in the warehouse
CREATE TABLE dbo.Stocks
(
    Ticker char(4) NOT NULL,
    Name varchar(200) NOT NULL
)
WITH
(
    DISTRIBUTION = REPLICATE
)
GO

-- Display the distribution policy for every table in the data warehouse
SELECT T.name ,p.distribution_policy_desc
  FROM sys.tables AS T
  JOIN sys.pdw_table_distribution_properties AS P
  ON T.object_id = P.object_id
GO

-- Create a non-indexed table
CREATE TABLE dbo.TempTab
(
    Column1 int NOT NULL,
    Column2 varchar(50) NOT NULL
)
WITH (HEAP)
GO

-- Display the indexes for every table in the data warehouse
SELECT T.name, I.name, I.type_desc, T.object_id
  FROM sys.tables AS T
  JOIN sys.indexes AS I
  ON T.object_id = I.object_id
GO

-- Partition the dbo.StockPriceMovement table by the day in the PriceChangedWhen column
DROP TABLE dbo.StockPriceMovement
GO

CREATE TABLE dbo.StockPriceMovement
(
    Ticker char(4) NOT NULL,
    Price int NOT NULL,
    PriceChangedWhen datetime NOT NULL,
    PriceChangedMonth int NOT NULL
)
WITH
(   
    CLUSTERED COLUMNSTORE INDEX,
    DISTRIBUTION = HASH(Ticker),
    PARTITION (PriceChangedMonth RANGE FOR VALUES (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12))
)
GO

-- Display the partitions for the StockMovements table.
-- Each distribution has the same partitions for this table
SELECT T.name, P.partition_number, P.rows
FROM sys.tables AS T
JOIN sys.partitions AS P
ON T.object_id = P.object_id
AND T.name = 'StockPriceMovement'
GO
