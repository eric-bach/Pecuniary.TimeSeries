# Pecuniary.TimeSeries

This project is part of the Pecuniary application.  It contains a single serverless function.

The function reads the `TimeSeries` table.  For each unique symbol, subsequent time series quotes are retrieved from `AlphaVantage` and written back to the `TimeSeries` table.


