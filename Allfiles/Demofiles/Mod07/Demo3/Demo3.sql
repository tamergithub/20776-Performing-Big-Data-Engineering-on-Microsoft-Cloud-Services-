-- Create a master encryption key
CREATE MASTER KEY ENCRYPTION BY PASSWORD='Pa55w.rd'
GO

-- Create a database scoped credential for accessing the storage account
CREATE DATABASE SCOPED CREDENTIAL CredentialsToBlobStorage
WITH IDENTITY = '<storage account name>',
SECRET = '<storage key>'
GO

-- Create an external data source that references the blob storage account
CREATE EXTERNAL DATA SOURCE BlobStockData
WITH (
    TYPE = HADOOP,     
    LOCATION = 'wasbs://stocknames@<storage account name>.blob.core.windows.net',
    CREDENTIAL = CredentialsToBlobStorage
)
GO

-- Specify the file format (CSV) of the files in blob storage
CREATE EXTERNAL FILE FORMAT CommaSeparatedFileFormat
WITH (
    FORMAT_TYPE = DelimitedText,
    FORMAT_OPTIONS (FIELD_TERMINATOR =',')
)
GO

-- Create the external StockNames table. The CSV data has two columns; Ticker (char 4), and Name (varchar 200)
CREATE EXTERNAL TABLE dbo.StockNames (
    Ticker char(4) NOT NULL, 
    Name varchar(200) NOT NULL)
WITH (
        LOCATION='StockNames.csv',
        DATA_SOURCE = BlobStockData,
        FILE_FORMAT = CommaSeparatedFileFormat
    )
GO

-- Read the data from blob storage
SELECT *
FROM dbo.StockNames
GO

-- Drop and recreate the Stocks table and copy the data from blob storage
DROP TABLE dbo.Stocks
GO

CREATE TABLE dbo.Stocks
WITH
(
    DISTRIBUTION = REPLICATE
)
AS SELECT Ticker, Name
FROM StockNames
GO

-- Query the Stocks table
SELECT * 
FROM dbo.Stocks
GO

/* Before continuing, create an AAD application to authenticate requests to ADLA */

-- Create a credential for ADLS. Specify the identity information from the AAD application
CREATE DATABASE SCOPED CREDENTIAL ADLCredential
WITH
    IDENTITY = '<application ID>@https://login.windows.net/<directory ID>/oauth2/token',
    SECRET = '<key>'
GO

-- Create an external data source that references the ADLS account using this credential
CREATE EXTERNAL DATA SOURCE ADLSource
WITH (
    TYPE = HADOOP,
    LOCATION = 'adl://<ADLS Account Name>.azuredatalakestore.net',
    CREDENTIAL = ADLCredential
)
GO

-- Define the format of the data in ADLS
CREATE EXTERNAL FILE FORMAT CsvTextFileFormat
WITH (
    FORMAT_TYPE = DelimitedText,
    FORMAT_OPTIONS (
        FIELD_TERMINATOR = ',',
        STRING_DELIMITER = '"',
        DATE_FORMAT = 'yyyy-MM-dd',
        USE_TYPE_DEFAULT = FALSE
    ));
GO

-- Create an external table for stock prices
CREATE EXTERNAL TABLE ADLStockPrices (
    Ticker char(4) NOT NULL, 
    Price int NOT NULL,
    QuoteTime varchar(50) )
WITH (
        LOCATION='/Stocks/',
        DATA_SOURCE = ADLSource,
        FILE_FORMAT = CsvTextFileFormat
    );
GO

-- Read the data from ADLS
SELECT *
FROM ADLStockPrices
GO

-- Drop and recreate the StockPriceMovement table in the data warehouse
DROP TABLE StockPriceMovement
GO

CREATE TABLE StockPriceMovement
WITH
(
    DISTRIBUTION = HASH(Ticker)
)
AS SELECT Ticker, Price, CONVERT(DateTime, SUBSTRING(QuoteTime, 1, 23), 126) AS PriceChangedWhen -- ISO8601 date/time format
FROM ADLStockPrices
GO

-- Display the data in the StockPriceMovement table in the data warehouse
SELECT *
FROM StockPriceMovement
GO