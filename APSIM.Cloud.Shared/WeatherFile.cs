﻿// -----------------------------------------------------------------------
// <copyright file="CreateWeatherFile.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace APSIM.Cloud.Shared
{
    using System;
    using System.Data;
    using System.IO;
    using System.Net;
    using System.Collections.Generic;
    using APSIM.Shared.Utilities;
    using System.Xml.Serialization;


    /// <summary>
    /// Creates a custom built weather file
    /// </summary>
    public class WeatherFile
    {
        /// <summary>
        /// A list of fields to ignore when overlaying data.
        /// </summary>
        private static string[] fieldsToOverlay = new string[] { "radn", "maxt", "mint", "rain" };

        /// <summary>The weatherfiles that have been written</summary>
        private List<string> weatherfilesWritten = new List<string>();

        /// <summary>Gets the names of all files created.</summary>
        [XmlIgnore]
        public string[] FilesCreated { get; private set; }

        /// <summary>Gets the last SILO date found. Returns DateTime.MinValue if no data.</summary>
        [XmlIgnore]
        public DateTime LastSILODateFound { get; private set; }

        /// <summary>Gets the first SILO date found. Returns DateTime.MinValue if no data.</summary>
        [XmlIgnore]
        public DateTime FirstSILODateFound { get; private set; }

        /// <summary>
        /// Create a met file that is the same std layout as the apsim std silo files
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="stationNumber"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="observedData"></param>
        public void CreateSimplePeriod(string fileName, int stationNumber,
                                    DateTime startDate,
                                    DateTime endDate,
                                    DataTable observedData)
        {
            MemoryStream siloStream = ExtractMetStreamFromSILO(stationNumber, startDate, endDate);
            if (siloStream != null)
            {
                // Convert the memory stream to a data table.
                siloStream.Seek(0, SeekOrigin.Begin);
                ApsimTextFile inputFile = new ApsimTextFile();
                inputFile.Open(siloStream);

                if (inputFile != null)
                {
                    DataTable weatherData = inputFile.ToTable();
                    if (weatherData.Rows.Count == 0)
                    {
                        FirstSILODateFound = DateTime.MinValue;
                        LastSILODateFound = DateTime.MinValue;
                    }
                    else
                    {
                        FirstSILODateFound = DataTableUtilities.GetDateFromRow(weatherData.Rows[0]);
                        LastSILODateFound = DataTableUtilities.GetDateFromRow(weatherData.Rows[weatherData.Rows.Count - 1]);
                    }

                    // Add a codes column to weatherdata
                    AddCodesColumn(weatherData, '-');

                    if (observedData != null)
                    {
                        AddCodesColumn(observedData, 'O');
                        OverlayData(observedData, weatherData);
                    }

                    //write the raw silo file stream
                    FileStream writer = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                    siloStream.Seek(0, SeekOrigin.Begin);
                    siloStream.WriteTo(writer);
                    writer.Close();

                    inputFile.Close();

                    FilesCreated = new string[1] { fileName };
                }
                else
                    FilesCreated = new string[0];
               
            }
        }

        /// <summary>Creates a one season weather file.</summary>
        /// <param name="fileName">Name of the file to create</param>
        /// <param name="stationNumber">The SILO station number to use.</param>
        /// <param name="startDate">The start date of the weather file.</param>
        /// <param name="observedData">The observed data to use. Can be null</param>
        public void CreateOneSeason(string fileName, int stationNumber,
                                    DateTime startDate,
                                    DateTime endDate,
                                    DataTable observedData)
        {
            Data weatherFile = ExtractDataFromSILO(stationNumber, startDate, endDate);
            if (weatherFile != null)
            {
                DataTable weatherData = weatherFile.DailyData;
                if (weatherData.Rows.Count == 0)
                    LastSILODateFound = DateTime.MinValue;
                else
                    LastSILODateFound = DataTableUtilities.GetDateFromRow(weatherData.Rows[weatherData.Rows.Count - 1]);

                // Add a codes column to weatherdata
                AddCodesColumn(weatherData, '-');

                if (observedData != null)
                {
                    AddCodesColumn(observedData, 'O');
                    OverlayData(observedData, weatherData);
                }

                double latitude = Convert.ToDouble(weatherFile.Latitude);
                double longitude = Convert.ToDouble(weatherFile.Longitude);
                double tav = Convert.ToDouble(weatherFile.TAV);
                double amp = Convert.ToDouble(weatherFile.AMP);
                WriteWeatherFile(weatherData, fileName, latitude, longitude, tav, amp);

                FilesCreated = new string[1] { fileName };
            }
            else
                FilesCreated = new string[0];
        }

        /// <summary>Creates a long term weather file.</summary>
        /// <param name="fileName">Name of the file to create.</param>
        /// <param name="stationNumber">The SILO station number to use.</param>
        /// <param name="startDate">The start date of the weather file.</param>
        /// <param name="nowDate">The end date for using observed data.</param>
        /// <param name="observedData">The observed data to use. Can be null.</param>
        /// <param name="numYears">Number of years for create weather file for.</param>
        public void CreateLongTerm(string fileName, int stationNumber,
                                   DateTime startDate,
                                   DateTime endDate,
                                   DateTime nowDate,
                                   DataTable observedData,
                                   int numYears)
        {

            if (!AlreadyWritten(fileName))
            {
                // Get the longterm (numYears) SILO weather data.
                Data  weatherFileData = ExtractDataFromSILO(stationNumber, 
                                                                   startDate.AddYears(-numYears), 
                                                                   DateTime.Now);
                DataTable weatherData = weatherFileData.DailyData;
                LastSILODateFound = weatherFileData.LastDate;

                // Duplicate the maxt and mint columns to origmaxt and origmint columns. 
                // This is so that we have both the patched and unpatched data.
                weatherData.Columns.Add("origmaxt", typeof(double));
                weatherData.Columns.Add("origmint", typeof(double));
                foreach (DataRow row in weatherData.Rows)
                {
                    row["origmaxt"] = Convert.ToDouble(row["maxt"]);
                    row["origmint"] = Convert.ToDouble(row["mint"]);
                    row["codes"] = row["codes"].ToString() + "--";
                }

                // Move the codes column to the end.
                weatherData.Columns["codes"].SetOrdinal(weatherData.Columns.Count-1);

                // Make sure the observed data has a codes column.
                if (observedData != null)
                    AddCodesColumn(observedData, 'O');

                string workingFolder = Path.GetDirectoryName(fileName);
                WriteDecileFile(weatherData, startDate, Path.Combine(workingFolder, "Decile.out"));


                // Need to create a patch data table from the observed data and the SILO data 
                // between the 'startDate' and the 'now' date.
                DataTable patchData = CreatePatchFile(weatherData, observedData, startDate, nowDate);

                List<string> fileNamesCreated = new List<string>();

                int numberOfDays = (endDate - startDate).Days;
                for (int year = startDate.Year - numYears; year < startDate.Year; year++)
                {
                    DateTime startDateForYear = new DateTime(year, startDate.Month, startDate.Day);
                    DateTime endDateForYear = startDateForYear.AddDays(numberOfDays);
                    DataView yearlyDataView = new DataView(weatherData);
                    yearlyDataView.RowFilter = string.Format("Date >= #{0:yyyy-MM-dd}# and Date <= #{1:yyyy-MM-dd}#",
                                                                startDateForYear, endDateForYear);
                    DataTable yearlyData = yearlyDataView.ToTable();

                    // Change the dates in yearlyData to the start date.
                    DateTime rowDate = startDate;
                    foreach (DataRow row in yearlyData.Rows)
                    {
                        row["Date"] = rowDate;
                        rowDate = rowDate.AddDays(1);
                    }

                    OverlayData(patchData, yearlyData);
                    string weatherFileName = Path.Combine(workingFolder, Path.GetFileNameWithoutExtension(fileName) + year.ToString() + ".met");
                    WriteWeatherFile(yearlyData, weatherFileName,
                                     weatherFileData.Latitude, weatherFileData.Longitude,
                                     weatherFileData.TAV, weatherFileData.AMP);
                    fileNamesCreated.Add(Path.GetFileName(weatherFileName));
                }

                FilesCreated = fileNamesCreated.ToArray();
            }
        }

        /// <summary>Creates a single long term, patched, weather file.</summary>
        /// <param name="fileName">Name of the file to create.</param>
        /// <param name="stationNumber">The SILO station number to use.</param>
        /// <param name="observedData">The observed data to use. Can be null.</param>
        public static void PatchWeatherDataAllYears(DataTable weatherData,
                                                    DataTable observedData,
                                                    DateTime startDate,
                                                    DateTime endDate)
        {
            // Need to create a patch data table from the observed data and the SILO data 
            if (observedData != null)
                AddCodesColumn(observedData, 'O');

            DataTable patchData = CreatePatchFile(weatherData, observedData, startDate, endDate);

            // Loop through all years in the long term weather data and overlay the patch data onto
            // each year of the weather data
            if (patchData.Rows.Count > 0)
            {
                int firstYear = DataTableUtilities.GetDateFromRow(weatherData.Rows[0]).Year;
                int lastYear = DataTableUtilities.GetDateFromRow(weatherData.Rows[weatherData.Rows.Count - 1]).Year;
                for (int year = firstYear; year <= lastYear; year++)
                {
                    // Before overlaying the patch data we need to change the year because the
                    // OverlayData method uses date matching.
                    SetYearInDateColumn(patchData, year);

                    // Now overlay the patch data.
                    OverlayData(patchData, weatherData);
                }
            }
        }

        /// <summary>Sets the year in date column.</summary>
        /// <remarks>
        ///     The patch data can go over a year i.e. starts in 2014 and goes into 2015.
        ///     This method doesn't want to set all years to the one specified, rather
        ///     it needs to work out what value needs to be subtracted from all years 
        ///     such that the first year of patch data is the same as the year specified.
        /// </remarks>
        /// <param name="patchData">The patch data.</param>
        /// <param name="year">The year to set the date to.</param>
        private static void SetYearInDateColumn(DataTable patchData, int year)
        {
            int firstYear = DataTableUtilities.GetDateFromRow(patchData.Rows[0]).Year;
            int offset = year - firstYear;

            DateTime[] dates = DataTableUtilities.GetColumnAsDates(patchData, "Date");
            for (int i = 0; i < dates.Length; i++)
                dates[i] = new DateTime(dates[i].Year + offset, dates[i].Month, dates[i].Day);
            DataTableUtilities.AddColumnOfObjects(patchData, "Date", dates);
        }

        /// <summary>
        /// Extracts weather data from silo for the specified station number, between the 
        /// specified dates. The private variables: latitude, longitude, tav, amp and 
        /// LastSILODateFound will be set to the values from the SILO file.
        /// </summary>
        /// <param name="stationNumber">The station number.</param>
        /// <param name="observedData">The observed data.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>The data returned will have year, month, day, date and code columns.</returns>
        public static Data ExtractDataFromSILO(int stationNumber, DateTime startDate, DateTime endDate)
        {
            ApsimTextFile weatherFile = ExtractMetFromSILO(stationNumber, startDate, endDate);

            // Add a codes and date column to weatherdata
            DataTable weatherData = weatherFile.ToTable();
            AddCodesColumn(weatherData, '-');
            AddDateToTable(weatherData);

            // Return the info object.
            return new Data(weatherData,
                latitude: Convert.ToDouble(weatherFile.Constant("Latitude").Value),
                longitude: Convert.ToDouble(weatherFile.Constant("Longitude").Value),
                tav: Convert.ToDouble(weatherFile.Constant("tav").Value),
                amp: Convert.ToDouble(weatherFile.Constant("amp").Value));
        }

        /// <summary>Creates a monthly decile weather file</summary>
        /// <param name="weatherData">The weather data.</param>
        /// <param name="startDate">First date for decile table.</param>
        /// <param name="fileName">The file name to write.</param>
        /// <results>Montly decile data.</results>
        public static void WriteDecileFile(DataTable weatherData, DateTime startDate, string fileName)
        {
            DataTable decileRain = CreateDecileWeather(weatherData, startDate);
            StreamWriter decileWriter = new StreamWriter(fileName);

            decileWriter.WriteLine("      Date RainDecile1 RainDecile5 RainDecile9");
            decileWriter.WriteLine("(dd/mm/yyyy)      (mm)        (mm)        (mm)");
            foreach (DataRow row in decileRain.Rows)
                decileWriter.WriteLine("{0:dd/MM/yyyy}{1,12:F1}{2,12:F1}{3,12:F1}",
                                       new object[] {row["Date"], row["RainDecile1"],
                                                     row["RainDecile5"], row["RainDecile9"]});

            decileWriter.Close();
        }

        /// <summary>
        /// Calculate the rainfall deciles for each decile for each month
        /// </summary>
        /// <param name="stationNumber"></param>
        /// <param name="startDate">Start date for the calculations</param>
        /// <param name="endDate"></param>
        /// <returns>The deciles array</returns>
        public double[,] CalculateRainDeciles(int stationNumber, DateTime startDate, DateTime endDate)
        {
            Data weatherFileData = ExtractDataFromSILO(stationNumber, startDate, endDate);
            DataTable weatherData = weatherFileData.DailyData;
                        
            if (weatherData != null)
            {
                return CreatePercentileWeather(weatherData, startDate);
            }
            return null;
        }

        /// <summary>
        /// Create the array of monthly deciles for each month from the startDate.
        /// </summary>
        /// <param name="weatherData">The raw daily weather data</param>
        /// <param name="startDate">The starting date. The month is the start of the season.</param>
        /// <returns>Array of monthly deciles (from 10 - 100)</returns>
        private double[,] CreatePercentileWeather(DataTable weatherData, DateTime startDate)
        {
            DateTime firstDate = DataTableUtilities.GetDateFromRow(weatherData.Rows[0]);
            DataView weatherView = new DataView(weatherData);
            weatherView.RowFilter = string.Format("Date >= #{0:yyyy-MM-dd}#", new DateTime(firstDate.Year, startDate.Month, startDate.Day));

            // Create an array of lists, 1 for each month.
            List<double>[] sumsForEachMonth = new List<double>[12];
            for (int i = 0; i < 12; i++)
                sumsForEachMonth[i] = new List<double>();

            int currentMonth = startDate.Month;
            double sum = 0.0;
            double value;
            foreach (DataRowView row in weatherView)
            {
                // Get the date and rain for the row.
                DateTime rowDate = DataTableUtilities.GetDateFromRow(row.Row);
                value = Convert.ToDouble(row["rain"]);              // get rain value
                if (currentMonth != rowDate.Month)                  // if new month then
                {
                    sumsForEachMonth[currentMonth - 1].Add(sum);    // store the month sum
                    if (rowDate.Month == startDate.Month)           // if back at the start of yearly period
                        sum = value;
                    currentMonth = rowDate.Month;
                }
                else
                {
                    sum += value;                                   //accumulate
                }
            }

            double[,] monthlyDeciles = new double[12,10];

            DateTime decileDate = new DateTime(startDate.Year, startDate.Month, 1); ;
            for (int i = 0; i < 12; i++)
            {
                double[] sums = new double[sumsForEachMonth[i].Count];
                Array.Copy(sumsForEachMonth[i].ToArray(), sums, sumsForEachMonth[i].Count);
                Array.Sort(sums);

                for (int dec = 1; dec <= 10; dec++)
                {
                    monthlyDeciles[i, dec - 1] = MathUtilities.Percentile(sums, dec * 0.1);
                }
            }
            return monthlyDeciles;
        }

        /// <summary>Creates a monthly decile weather DataTable</summary>
        /// <param name="weatherData">The weather data.</param>
        /// <param name="startDate">First date for decile table.</param>
        /// <results>Montly decile data.</results>
        private static DataTable CreateDecileWeather(DataTable weatherData, DateTime startDate)
        {
            DateTime firstDate = DataTableUtilities.GetDateFromRow(weatherData.Rows[0]);
            DataView weatherView = new DataView(weatherData);
            weatherView.RowFilter = string.Format("Date >= #{0:yyyy-MM-dd}#", new DateTime(firstDate.Year, startDate.Month, startDate.Day));

            // Create an array of lists, 1 for each month.
            List<double>[] sumsForEachMonth = new List<double>[12];
            for (int i = 0; i < 12; i++)
                sumsForEachMonth[i] = new List<double>();

            double sum = 0.0;
            foreach (DataRowView row in weatherView)
            {
                // Get the date and rain for the row.
                DateTime rowDate = DataTableUtilities.GetDateFromRow(row.Row);
                double value = Convert.ToDouble(row["rain"]);

                // Accumulate the value every day.
                sum += value;

                // At the end of each month, store the accumulated value into the right array element.
                // if (rowDate.AddDays(1).Day == 1)  // end of month?  - GOOD
                if (rowDate.Day == 1)  // end of month?   - REPRODUCE BUG IN YP
                    sumsForEachMonth[rowDate.Month-1].Add(sum);

                if (rowDate.Day == 1 && rowDate.Month == startDate.Month)
                    sum = value;
            }

            DataTable decile = new DataTable();
            decile.Columns.Add("Date", typeof(DateTime));
            decile.Columns.Add("RainDecile1", typeof(double));
            decile.Columns.Add("RainDecile5", typeof(double));
            decile.Columns.Add("RainDecile9", typeof(double));

            DateTime decileDate = new DateTime(startDate.Year, startDate.Month, 1); ;
            for (int i = 0; i < 12; i++)
            {
                DataRow row = decile.NewRow();
                row["Date"] = decileDate;
                if (i == 0)
                {
                    row["RainDecile1"] = 0;
                    row["RainDecile5"] = 0;
                    row["RainDecile9"] = 0;
                }
                else
                {
                    row["RainDecile1"] = GetValueForProbability(10, sumsForEachMonth[decileDate.Month - 1].ToArray());
                    row["RainDecile5"] = GetValueForProbability(50, sumsForEachMonth[decileDate.Month - 1].ToArray());
                    row["RainDecile9"] = GetValueForProbability(90, sumsForEachMonth[decileDate.Month - 1].ToArray());
                }

                decile.Rows.Add(row);
                decileDate = decileDate.AddMonths(1);
            }

            return decile;
        }

        /// <summary>Gets the value for probability.</summary>
        /// <param name="probability">The probability.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        private static double GetValueForProbability(double probability, double[] values)
        {
            double[] probValues = MathUtilities.ProbabilityDistribution(values.Length, false);
            Array.Sort(values);
            for (int i = 0; i < probValues.Length; i++)
            {
                if (probValues[i] >= probability)
                    return values[i];
            }

            return probValues[probValues.Length - 1];  // last element.
        }

        /// <summary>Returns true if the specified file has already been writen.</summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>True if written. False otherwise.</returns>
        private bool AlreadyWritten(string fileName)
        {
            bool found = false;

            string baseFileName = Path.GetFileNameWithoutExtension(fileName);
            foreach (string fileNameCreated in weatherfilesWritten)
                if (baseFileName.StartsWith(fileNameCreated))
                    found = true;
            
            if (!found)
                weatherfilesWritten.Add(baseFileName);
            return found;
        }

        /// <summary>Creates a patch file from the SILO data and the observed data for
        /// the dates between 'startDate' and 'endDate'</summary>
        /// <param name="SILOData">The SILO data.</param>
        /// <param name="observedData">The observed data.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date</param>
        /// <returns>The patch data</returns>
        private static DataTable CreatePatchFile(DataTable SILOData, DataTable observedData, 
                                                DateTime startDate, DateTime endDate)
        {
            DataTable table;
            DataView yearlyDataView = new DataView(SILOData);
            yearlyDataView.RowFilter = string.Format("Date >= #{0:yyyy-MM-dd}# and Date <= #{1:yyyy-MM-dd}#",
                                                     startDate, endDate);

            table = yearlyDataView.ToTable();
            AddCodesColumn(table, 'S');
            if (observedData != null)
                OverlayData(observedData, table);
            return table;
        }

        /// <summary>Adds the codes column to the specified weatherData.</summary>
        /// <param name="weatherData">The weather data.</param>
        private static void AddCodesColumn(DataTable weatherData, char code)
        {
            if (!weatherData.Columns.Contains("codes"))
                weatherData.Columns.Add("codes", typeof(string));

            // Count the number of code characters we need.
            int count = 0;
            foreach (string fieldToInclude in fieldsToOverlay)
                if (weatherData.Columns.Contains(fieldToInclude))
                    count++;

            string codeString = new string(code, count);
            foreach (DataRow row in weatherData.Rows)
                row["codes"] = codeString;
        }

        /// <summary>
        /// Write the specified data table to a text file.
        /// </summary>
        /// <param name="weatherData">The data to write.</param>
        /// <param name="fileName">The name of the file to create.</param>
        public static void WriteWeatherFile(DataTable weatherData, string fileName,
                                            double latitude, double longitude,
                                            double tav, double amp)
        {
            StreamWriter writer = new StreamWriter(fileName);
            writer.WriteLine("Latitude = " + latitude.ToString());
            writer.WriteLine("Longitude = " + longitude.ToString());
            writer.WriteLine("TAV = " + tav.ToString());
            writer.WriteLine("AMP = " + amp.ToString());
            writer.WriteLine("! Codes: -    SILO (unpatched)");
            writer.WriteLine("         S    SILO (patched)");
            writer.WriteLine("         O    Observed");
            writer.WriteLine("         P    POAMA");

            // Work out column formats and widths.
            string formatString = string.Empty;
            string headings = string.Empty;
            string units = string.Empty;
            int i = 0;
            foreach (DataColumn column in weatherData.Columns)
            {
                int columnWidth = 0;
                string columnFormat = string.Empty;
                string columnUnits = string.Empty;
                if (column.DataType == typeof(DateTime))
                {
                    columnFormat += "yyyy-MM-dd";
                    columnWidth = 12;
                    columnUnits = "(yyyy-mm-dd)";
                }
                else if (column.ColumnName.Contains("radn"))
                {
                    columnFormat += "F1";
                    columnWidth = 9;
                    columnUnits = "(MJ/m^2)";
                }
                else if (column.ColumnName.Contains("maxt"))
                {
                    columnFormat += "F1";
                    columnWidth = 9;
                    columnUnits = "(oC)";
                }
                else if (column.ColumnName.Contains("mint"))
                {
                    columnFormat += "F1";
                    columnWidth = 9;
                    columnUnits = "(oC)";
                }
                else if (column.ColumnName.Contains("rain"))
                {
                    columnFormat += "F1";
                    columnWidth = 7;
                    columnUnits = "(mm)";
                }
                else
                {
                    columnFormat += string.Empty;
                    columnWidth = 9;
                    columnUnits = "()";
                }

                if (columnWidth > 0)
                {
                    headings += string.Format("{0," + columnWidth.ToString() + "}", column.ColumnName);
                    units += string.Format("{0," + columnWidth.ToString() + "}", columnUnits);
                    formatString += "{" + i + "," + columnWidth;
                    if (columnFormat != string.Empty)
                        formatString += ":" + columnFormat;
                    formatString += "}";
                    i++;
                }
            }

            // Write headings and units
            writer.WriteLine(headings);
            writer.WriteLine(units);

            // Write data.
            object[] values = new object[weatherData.Columns.Count];
            foreach (DataRow row in weatherData.Rows)
            {
                // Create an object array to pass to writeline.
                for (int c = 0; c < weatherData.Columns.Count; c++)
                    values[c] = row[c];

                writer.WriteLine(formatString, values);
            }
            
            writer.Close();
        }

        /// <summary>
        /// Overlay data from table1 on top of table2 using the date in each row. Date
        /// dates in both tables have to exactly match before the data is overlaid.
        /// </summary>
        /// <param name="table1">First data table</param>
        /// <param name="table2">The data table that will change</param>
        private static void OverlayData(DataTable table1, DataTable table2)
        {
            if (table2.Rows.Count > 0)
            {
                // This algorithm assumes that table2 does not have missing days.
                DateTime firstDate = DataTableUtilities.GetDateFromRow(table2.Rows[0]);
                DateTime lastDate = DataTableUtilities.GetDateFromRow(table2.Rows[table2.Rows.Count-1]);

                // Filter the first table so that it is in the same range as table2.
                DataView table1View = new DataView(table1);
                table1View.RowFilter = string.Format("Date >= #{0:yyyy-MM-dd}# and Date <= #{1:yyyy-MM-dd}#",
                                                     firstDate, lastDate);

                foreach (DataRowView table1Row in table1View)
                {
                    DateTime table1Date = DataTableUtilities.GetDateFromRow(table1Row.Row);

                    int table2RowIndex = (table1Date - firstDate).Days;
                    if (table2RowIndex >= 0 && table2RowIndex < table2.Rows.Count)
                    {
                        DataRow table2Row = table2.Rows[table2RowIndex];
                        if (DataTableUtilities.GetDateFromRow(table2Row) == table1Date)
                        {
                            // Found the matching row
                            OverlayRowData(table1Row.Row, table2Row);
                        }
                        else
                            throw new Exception("Non consecutive dates found in SILO data");
                    }
                    else
                    {
                        // Table 1 data is outside the range of table 2.
                    }
                }
            }
        }

        /// <summary>
        /// Overlay data of fromRow into toRow where the columns match
        /// </summary>
        /// <param name="fromRow">From row</param>
        /// <param name="toRow">To row</param>
        private static void OverlayRowData(DataRow fromRow, DataRow toRow)
        {
            char[] fromRowCodes = fromRow["codes"].ToString().ToCharArray();
            char[] toRowCodes = toRow["codes"].ToString().ToCharArray();
            
            foreach (DataColumn fromColumn in fromRow.Table.Columns)
            {
                if (StringUtilities.Contains(fieldsToOverlay, fromColumn.ColumnName))
                {
                    // See if this column is in table2.
                    foreach (DataColumn toColumn in toRow.Table.Columns)
                    {
                        if (toColumn.ColumnName.Equals(fromColumn.ColumnName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Matching column - move data from table1Row to table2Row
                            if (!Convert.IsDBNull(fromRow[fromColumn]))
                            {
                                toRow[toColumn] = fromRow[fromColumn];

                                // Update codes
                                int fromCodeIndex = fromColumn.Ordinal - 1; // First column in fromRow will be Date but Date doesn't have a code.
                                int toCodeIndex = StringUtilities.IndexOfCaseInsensitive(fieldsToOverlay, toColumn.ColumnName);
                                toRowCodes[toCodeIndex] = fromRowCodes[fromCodeIndex];
                            }
                        }
                    }
                }
            }
            toRow["codes"] = new string(toRowCodes);
        }

        /// <summary>Extracts weather data from silo.</summary>
        /// <param name="stationNumber">The station number.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <exception cref="System.Exception">Cannot find SILO!</exception>
        /// <returns>The APSIM text file from SILO</returns>
        private static ApsimTextFile ExtractMetFromSILO(int stationNumber, DateTime startDate, DateTime endDate)
        {
            MemoryStream siloStream = ExtractMetStreamFromSILO(stationNumber, startDate, endDate);
            if (siloStream != null)
            {
                // Convert the memory stream to a data table.
                siloStream.Seek(0, SeekOrigin.Begin);
                ApsimTextFile inputFile = new ApsimTextFile();
                inputFile.Open(siloStream);
                return inputFile;
            }
            else
                return null;
        }

        /// <summary>
        /// Extracts the SILO data and returns it in a memory stream
        /// </summary>
        /// <param name="stationNumber">The station number.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <exception cref="System.Exception">Cannot find SILO!</exception>
        /// <returns>The SILO data stream</returns>
        public static MemoryStream ExtractMetStreamFromSILO(int stationNumber, DateTime startDate, DateTime endDate)
        {
            if (startDate < DateTime.Now)
            {
                string serverAddress = "http://apsrunet.apsim.info/cgi-bin";
                HttpWebRequest SILO = null;
                HttpWebResponse SILOResponse = null;
                MemoryStream siloStream = new MemoryStream();
                try
                {
                    string requestString = serverAddress + "/getData.tcl?format=apsim&station=" +
                                           stationNumber.ToString() +
                                           "&ddStart=" + startDate.Day.ToString() +
                                           "&mmStart=" + startDate.Month.ToString() +
                                           "&yyyyStart=" + startDate.Year.ToString() +
                                           "&ddFinish=" + endDate.Day.ToString() +
                                           "&mmFinish=" + endDate.Month.ToString() +
                                           "&yyyyFinish=" + endDate.Year.ToString();

                    SILO = (HttpWebRequest)WebRequest.Create(requestString);
                    SILOResponse = (HttpWebResponse)SILO.GetResponse();
                    Stream streamResponse = SILOResponse.GetResponseStream();


                    // Reads 1024 characters at a time.    
                    byte[] read = new byte[1024];
                    int count = streamResponse.Read(read, 0, 1024);
                    while (count > 0)
                    {
                        // Dumps the 1024 characters into our memory stream.
                        siloStream.Write(read, 0, count);
                        count = streamResponse.Read(read, 0, 1024);
                    }
                    return siloStream;
                }
                catch (Exception)
                {
                    throw new Exception("Cannot find SILO!");
                }
                finally
                {
                    // Releases the resources of the response.
                    if (SILOResponse != null)
                        SILOResponse.Close();
                }
            }
            return null;
        }

        /// <summary>
        /// Add year, month, day and date columns to the specified Table.
        /// </summary>
        public static void AddDateToTable(DataTable table)
        {
            if (!table.Columns.Contains("Date"))
            {
                List<DateTime> dates = new List<DateTime>();
                foreach (DataRow Row in table.Rows)
                    dates.Add(DataTableUtilities.GetDateFromRow(Row));
                DataTableUtilities.AddColumnOfObjects(table, "Date", dates.ToArray());
                table.Columns["Date"].SetOrdinal(0);

                // remove year, day, pan, vp, code columns.
                int yearColumn = table.Columns.IndexOf("Year");
                if (yearColumn != -1)
                    table.Columns.RemoveAt(yearColumn);
                int dayColumn = table.Columns.IndexOf("Day");
                if (dayColumn != -1)
                    table.Columns.RemoveAt(dayColumn);
                int panColumn = table.Columns.IndexOf("pan");
                if (panColumn != -1)
                    table.Columns.RemoveAt(panColumn);
                int vpColumn = table.Columns.IndexOf("vp");
                if (vpColumn != -1)
                    table.Columns.RemoveAt(vpColumn);
                int codeColumn = table.Columns.IndexOf("code");
                if (codeColumn != -1)
                    table.Columns.RemoveAt(codeColumn);
            }
        }

        /// <summary>
        /// A simple class for holding data from a weather file.
        /// </summary>
        public class Data
        {
            /// <summary>The latitude</summary>
            public double Latitude { get; private set; }

            /// <summary>The longitude</summary>
            public double Longitude { get; private set; }

            /// <summary>The tav</summary>
            public double TAV { get; private set; }

            /// <summary>The amp</summary>
            public double AMP { get; private set; }

            /// <summary>The daily data.</summary>
            public DataTable DailyData { get; private set; }

            /// <summary>Returns the last date in the weather data.</summary>
            public DateTime LastDate
            {
                get
                {
                    if (DailyData.Rows.Count == 0)
                        return DateTime.MinValue;
                    else
                        return DataTableUtilities.GetDateFromRow(DailyData.Rows[DailyData.Rows.Count - 1]);
                }
            }

            /// <summary>
            /// Constructor for a weather data instance.
            /// </summary>
            /// <param name="data">Daily data.</param>
            /// <param name="latitude">The latitude.</param>
            /// <param name="longitude">The longitude.</param>
            /// <param name="tav">The average temp.</param>
            /// <param name="amp">The temp. amplitude.</param>
            public Data(DataTable data, double latitude, double longitude, 
                        double tav, double amp)
            {
                this.DailyData = data;
                this.Latitude = latitude;
                this.Longitude = longitude;
                this.TAV = tav;
                this.AMP = amp;
            }
        }
    }
    
}
