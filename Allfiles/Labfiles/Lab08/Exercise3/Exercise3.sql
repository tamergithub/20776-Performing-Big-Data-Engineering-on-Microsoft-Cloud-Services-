-- Find all vehicles that have not been caught speeding
-- This query causes data movement between nodes
SELECT V.VehicleRegistration
FROM dbo.VehicleOwner V
WHERE V.VehicleRegistration NOT IN
(
  SELECT S.VehicleRegistration
  FROM dbo.VehicleSpeed S
  WHERE S.Speed > S.SpeedLimit
)
ORDER BY V.VehicleRegistration
GO

-- What is the distribution policy of the VehicleOwner table?
SELECT T.name, P.distribution_policy_desc
FROM sys.pdw_table_distribution_properties P
JOIN sys.tables T
ON P.object_id = T.object_id
GO

-- How big is the VehicleOwner table?
-- (All space values displayed are in KB)
DBCC PDW_SHOWSPACEUSED('VehicleOwner')
GO

-- Try replicating the vehicle owner data so that it is available on each node
CREATE TABLE VehicleOwner2
(
  VehicleRegistration VARCHAR(7) NOT NULL,
  Title VARCHAR(30) NOT NULL,
  Forename VARCHAR(30) NOT NULL,
  Surname VARCHAR(30) NOT NULL,
  AddressLine1 VARCHAR(50) NOT NULL,
  AddressLine2 VARCHAR(50) NOT NULL,
  AddressLine3 VARCHAR(50) NOT NULL,
  AddressLine4 VARCHAR(50) NOT NULL
)
WITH
(
  DISTRIBUTION = REPLICATE
)
GO

INSERT INTO VehicleOwner2
SELECT *
FROM VehicleOwner
GO

-- Same query as before, but using the replicated table
-- Still results in data movement because VehicleSpeed data is distributed
SELECT V.VehicleRegistration
FROM dbo.VehicleOwner2 V
WHERE V.VehicleRegistration NOT IN
(
  SELECT S.VehicleRegistration
  FROM dbo.VehicleSpeed S
  WHERE S.Speed > S.SpeedLimit
)
ORDER BY V.VehicleRegistration
GO

-- Try changing the distribution strategy of VehicleSpeed so that the data is distributed according to VehicleRegistration
CREATE TABLE VehicleSpeed2
(
  CameraID VARCHAR(10) NOT NULL,
  SpeedLimit INT NOT NULL,
  Speed INT NOT NULL,
  VehicleRegistration VARCHAR(7) NOT NULL,
  WhenDate DATETIME NOT NULL,
  WhenMonth INT NOT NULL
)
WITH
(
  HEAP,
  DISTRIBUTION = HASH(VehicleRegistration),
  PARTITION (WhenMonth RANGE FOR VALUES(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12))
)
GO

INSERT INTO VehicleSpeed2
SELECT *
FROM VehicleSpeed
GO

-- Same query as before, using the replicated VehicleOwner table and reorganized VehicleSpeed table
-- The query still causes data movement, because the VehicleSpeed data is an unindexed heap
SELECT V.VehicleRegistration
FROM dbo.VehicleOwner2 V
WHERE V.VehicleRegistration NOT IN
(
  SELECT S.VehicleRegistration
  FROM dbo.VehicleSpeed2 S
  WHERE S.Speed > S.SpeedLimit
)
ORDER BY V.VehicleRegistration
GO

-- Recreate the VehicleSpeed table with a columnstore index
CREATE TABLE VehicleSpeed3
(
  CameraID VARCHAR(10) NOT NULL,
  SpeedLimit INT NOT NULL,
  Speed INT NOT NULL,
  VehicleRegistration VARCHAR(7) NOT NULL,
  WhenDate DATETIME NOT NULL,
  WhenMonth INT NOT NULL
)
WITH
(
  DISTRIBUTION = HASH(VehicleRegistration),
  PARTITION (WhenMonth RANGE FOR VALUES(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12))
)
GO

INSERT INTO VehicleSpeed3
SELECT *
FROM VehicleSpeed
GO

-- Repeat the query using the new VehicleSpeed table
SELECT V.VehicleRegistration
FROM dbo.VehicleOwner2 V
WHERE V.VehicleRegistration NOT IN
(
  SELECT S.VehicleRegistration
  FROM dbo.VehicleSpeed3 S
  WHERE S.Speed > S.SpeedLimit
)
ORDER BY V.VehicleRegistration
GO

-- Change the distribution strategy of the VehicleOwner table to match that of the VehicleSpeed table
CREATE TABLE VehicleOwner3
(
  VehicleRegistration VARCHAR(7) NOT NULL,
  Title VARCHAR(30) NOT NULL,
  Forename VARCHAR(30) NOT NULL,
  Surname VARCHAR(30) NOT NULL,
  AddressLine1 VARCHAR(50) NOT NULL,
  AddressLine2 VARCHAR(50) NOT NULL,
  AddressLine3 VARCHAR(50) NOT NULL,
  AddressLine4 VARCHAR(50) NOT NULL
)
WITH
(
  DISTRIBUTION = HASH(VehicleRegistration)
)
GO

INSERT INTO VehicleOwner3
SELECT *
FROM VehicleOwner
GO

-- This version of the query avoids data movement
SELECT V.VehicleRegistration
FROM dbo.VehicleOwner3 V
WHERE V.VehicleRegistration NOT IN
(
  SELECT S.VehicleRegistration
  FROM dbo.VehicleSpeed3 S
  WHERE S.Speed > S.SpeedLimit
)
ORDER BY V.VehicleRegistration
GO



