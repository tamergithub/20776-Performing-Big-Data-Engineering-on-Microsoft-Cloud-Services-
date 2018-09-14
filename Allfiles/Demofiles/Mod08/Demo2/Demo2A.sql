SELECT Ticker, MAX(Price) AS HighestPrice
FROM dbo.StockPrice
GROUP BY Ticker
ORDER BY HighestPrice DESC
GO