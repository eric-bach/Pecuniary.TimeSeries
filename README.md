# Pecuniary.TimeSeries

This project is part of the Pecuniary application.  It contains a single serverless function.

The function reads the `TimeSeries` table.  For each unique symbol, subsequent time series quotes are retrieved from `AlphaVantage` and written back to the `TimeSeries` table.

## Build Status

Pipeline | Build Status
-|-
Build | [![Build status](https://ci.appveyor.com/api/projects/status/rsg5qdd3ml9aarc8?svg=true)](https://ci.appveyor.com/project/eric-bach/pecuniary.timeseries)
Unit Tests | ![AppVeyor tests](https://img.shields.io/appveyor/tests/eric-bach/Pecuniary.TimeSeries)
Code Coverage | [![codecov](https://codecov.io/gh/eric-bach/Pecuniary.TimeSeries/branch/master/graph/badge.svg)](https://codecov.io/gh/eric-bach/Pecuniary.TimeSeries)
Code Quality | [![CodeFactor](https://www.codefactor.io/repository/github/eric-bach/pecuniary.TimeSeries/badge)](https://www.codefactor.io/repository/github/eric-bach/pecuniary.timeseries)
