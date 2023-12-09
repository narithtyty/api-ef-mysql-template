using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public static class ExcelExtensions
{
    public static void CreateExcelFile(IEnumerable<IDictionary<string, object>> data, string filePath)
    {
        //data = data.Select(r => r.ToDictionary(pair => pair.Key, pair => pair.Value));
        // Create a new Excel workbook and sheet
        IWorkbook workbook = new XSSFWorkbook();
        ISheet sheet = workbook.CreateSheet("Sheet1");

        // Create header row
        var headerRow = sheet.CreateRow(0);
        int colIndex = 0;

        foreach (var columnName in data.First().Keys)
        {
            headerRow.CreateCell(colIndex++).SetCellValue(columnName);
        }
        int rowIndex = 1;
        foreach (var row in data)
        {
            var dataRow = sheet.CreateRow(rowIndex++);
            colIndex = 0;

            foreach (var value in row.Values)
            {
                dataRow.CreateCell(colIndex++).SetCellValue(value.ToString());
            }
        }

        // Save the workbook to a file
        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            workbook.Write(fs);
        }

    }
    public static IEnumerable<IDictionary<string, object>> GetDataFromExcel(string filePath)
    {
        List<IDictionary<string, object>> result = new List<IDictionary<string, object>>();

        // Open the Excel file
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            IWorkbook workbook = new XSSFWorkbook(fs);
            ISheet sheet = workbook.GetSheetAt(0); // Assuming the data is in the first sheet

            // Get column names from the header row
            IRow headerRow = sheet.GetRow(0);
            List<string> columnNames = new List<string>();
            for (int i = 0; i < headerRow.LastCellNum; i++)
            {
                ICell cell = headerRow.GetCell(i);
                columnNames.Add(cell.StringCellValue);
            }

            // Read data rows
            for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)
            {
                IRow dataRow = sheet.GetRow(rowIdx);
                Dictionary<string, object> rowData = new Dictionary<string, object>();

                for (int colIdx = 0; colIdx < columnNames.Count; colIdx++)
                {
                    ICell cell = dataRow.GetCell(colIdx);
                    string columnName = columnNames[colIdx];

                    // Add data to the dictionary
                    rowData[columnName] = GetCellValue(cell);
                }

                result.Add(rowData);
            }
        }

        return result;
    }

    private static object GetCellValue(ICell cell)
    {
        if (cell == null)
        {
            return null;
        }

        switch (cell.CellType)
        {
            case CellType.Numeric:
                return cell.NumericCellValue;

            case CellType.String:
                return cell.StringCellValue;

            case CellType.Boolean:
                return cell.BooleanCellValue;

            case CellType.Formula:
                // Handle formulas based on your requirements
                return EvaluateFormulaCell(cell);

            default:
                return null;
        }
    }

    private static object EvaluateFormulaCell(ICell cell)
    {
        return cell.CellFormula;
    }
}