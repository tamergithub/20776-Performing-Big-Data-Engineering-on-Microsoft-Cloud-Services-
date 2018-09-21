CREATE MASTER KEY ENCRYPTION BY PASSWORD='Pa55w.rd'
GO

CREATE DATABASE SCOPED CREDENTIAL CredentialsToBlobStorage
WITH IDENTITY = '<storage account name>',
SECRET = '<storage key>';
GO

CREATE EXTERNAL DATA SOURCE LocationDataSource
WITH (
    TYPE = HADOOP,     
    LOCATION = 'wasbs://locationdata@<storage account name>.blob.core.windows.net',
    CREDENTIAL = CredentialsToBlobStorage
)
GO

CREATE EXTERNAL FILE FORMAT CommaSeparatedFileFormat
WITH (
    FORMAT_TYPE = DelimitedText,
    FORMAT_OPTIONS (FIELD_TERMINATOR =',')
)
GO

CREATE EXTERNAL TABLE ExternalLocationData (
    CameraID VARCHAR(10) NOT NULL,
    GPSLocationX FLOAT NOT NULL,
    GPSLocationY FLOAT NOT NULL)
WITH (
    LOCATION='CameraData.csv',
    DATA_SOURCE = LocationDataSource,
    FILE_FORMAT = CommaSeparatedFileFormat,
    REJECT_TYPE = VALUE,
    REJECT_VALUE = 0
)
GO

SELECT *
FROM ExternalLocationData
GO

DROP TABLE CameraLocation
GO

CREATE TABLE CameraLocation
WITH
(
  HEAP,
  DISTRIBUTION = REPLICATE
)
AS SELECT CameraID, GPSLocationX, GPSLocationY
FROM ExternalLocationData
GO

SELECT *
FROM CameraLocation
GO