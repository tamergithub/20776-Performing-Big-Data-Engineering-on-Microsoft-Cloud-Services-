    DELETE FROM dbo.VehicleSpeed
	GO
	
	CREATE DATABASE SCOPED CREDENTIAL SpeedDataCredentials
    WITH IDENTITY = '<storage account name>',
    SECRET = '<storage account key>';
    GO

    CREATE EXTERNAL DATA SOURCE SpeedDataSource
    WITH (
        TYPE = HADOOP,     
        LOCATION = 'wasbs://capturedspeeds@<storage account name>.blob.core.windows.net',
        CREDENTIAL = SpeedDataCredentials
    )
    GO

    CREATE EXTERNAL TABLE ExternalSpeedData (
        CameraID VARCHAR(10) NOT NULL,
        SpeedLimit INT NOT NULL,
        Speed INT NOT NULL,
        VehicleRegistration VARCHAR(7) NOT NULL,
        WhenDate VARCHAR(20) NOT NULL,
        WhenMonth INT NOT NULL
    )
    WITH (
        LOCATION='SpeedData.csv',
        DATA_SOURCE = SpeedDataSource,
        FILE_FORMAT = CommaSeparatedFileFormat,
		REJECT_TYPE = percentage,
		REJECT_VALUE = 2,
		REJECT_SAMPLE_VALUE = 1000
    )
    GO

	INSERT INTO dbo.VehicleSpeed(CameraID, SpeedLimit, Speed, VehicleRegistration, WhenDate, WhenMonth)
	SELECT CameraID, SpeedLimit, Speed, VehicleRegistration, CONVERT(DATETIME, WhenDate, 103) AS WhenDate, WhenMonth
	FROM ExternalSpeedData
	GO


