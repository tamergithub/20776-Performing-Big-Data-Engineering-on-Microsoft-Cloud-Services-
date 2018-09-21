tempXDFFile <- 'VehicleData.xdf'

observations <- rxImport(inData = inputFromUSQL, outFile = tempXDFFile, overwrite = TRUE)

refactoredObservations <- rxFactors(observations, c('CameraID', 'VehicleRegistration'))

cleanData <- rxDataStep(inData = refactoredObservations, outFile = 'cleanData.xdf', overwrite = TRUE,
                        transforms = list(
                          IsSpeeding = Speed > SpeedLimit,
                          IsStolen = !is.na(DateStolen))
                        )

speedingCor <- rxCor(formula = ~IsSpeeding + IsStolen, data = cleanData)

outputToUSQL <- data.frame(refactoredObservations[1,]["CameraID"], speedingCor[1,]["IsStolen"])
colnames(outputToUSQL)[2] <- "Correlation"
