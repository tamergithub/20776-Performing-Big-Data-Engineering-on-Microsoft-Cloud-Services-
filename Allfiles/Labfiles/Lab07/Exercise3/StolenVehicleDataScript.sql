CREATE DATABASE SCOPED CREDENTIAL ADLCredential
WITH
    IDENTITY = '<application ID>@https://login.windows.net/<directory ID>/oauth2/token',
    SECRET = '<key>'
GO

CREATE EXTERNAL DATA SOURCE StolenVehicleDataSource
WITH (
    TYPE = HADOOP,
    LOCATION = 'adl://<Data Lake Store name>.azuredatalakestore.net',
    CREDENTIAL = ADLCredential
)
GO

CREATE EXTERNAL FILE FORMAT DelimitedCsvTextFileFormat
WITH (
    FORMAT_TYPE = DelimitedText,
    FORMAT_OPTIONS (
        FIELD_TERMINATOR = ',',
        STRING_DELIMITER = '"'
    ));
GO

CREATE EXTERNAL TABLE ExternalStolenVehicleData (
  VehicleRegistration VARCHAR(7) NOT NULL,
  DateStolen VARCHAR(25) NOT NULL,
  DateRecovered VARCHAR(25) NULL )
WITH (
        LOCATION='/Stolen/',
        DATA_SOURCE = StolenVehicleDataSource,
        FILE_FORMAT = DelimitedCsvTextFileFormat,
		REJECT_TYPE = PERCENTAGE,
		REJECT_VALUE = 5,
		REJECT_SAMPLE_VALUE = 1000
    );
GO

SELECT TOP(1000) *
FROM ExternalStolenVehicleData
GO

INSERT INTO StolenVehicle(VehicleRegistration, DateStolen, DateRecovered, YearStolen)
SELECT VehicleRegistration, CONVERT(DATETIME, DateStolen), CONVERT(DATETIME, DateRecovered), DATEPART(yyyy, CONVERT(DATETIME, DateStolen))
FROM ExternalStolenVehicleData
GO

SELECT TOP(1000) *
FROM StolenVehicle
GO

