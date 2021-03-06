USE VehicleInfo
GO

-- Create VehicleOwner table
CREATE TABLE VehicleOwner
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
GO

-- Populate the table using BCP

-- Verify the contents of the table
SELECT TOP(1000) *
FROM VehicleOwner
GO