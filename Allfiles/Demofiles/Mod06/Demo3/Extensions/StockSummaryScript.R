stockData <- rxImport(inputFromUSQL, outFile = NULL)
refactoredStockData <- rxFactors(stockData, c('Ticker'))
sortedData <- refactoredStockData[order(refactoredStockData[,3]),]
openingPrices <- aggregate(Price ~ Ticker, sortedData, head)
closingPrices <- aggregate(Price ~ Ticker, sortedData, tail)
lowestPrices <- aggregate(Price ~ Ticker, sortedData, min)
highestPrices <- aggregate(Price ~ Ticker, sortedData, max)

results <- data.frame(openingPrices[1], openingPrices[[2]][,1], closingPrices[[2]][,6], lowestPrices[2], highestPrices[2])
colnames(results) <- c('Ticker', 'OpeningPrice', 'ClosingPrice', 'LowestPrice', 'HighestPrice')
outputToUSQL <- results