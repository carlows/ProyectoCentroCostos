﻿using Excel;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace CentroCostos.Infrastructure
{
    public class ExcelData
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        string _path;

        public ExcelData(string path)
        {
            _path = path;
        }

        // Obtiene el reader con el path del archivo excel
        public IExcelDataReader getExcelReader()
        {
            FileStream stream = File.Open(_path, FileMode.Open, FileAccess.Read);

            IExcelDataReader reader = null;
            try
            {
                if (_path.EndsWith(".xls"))
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                if (_path.EndsWith(".xlsx"))
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }

                return reader;
            }
            catch (Exception ex)
            {
                logger.Error("Error al cargar documento excel. {0}", ex);
            }
        }

        // Obtiene los nombres de las hojas de calculo
        public IEnumerable<string> getWorksheetNames()
        {
            var reader = this.getExcelReader();
            var workbook = reader.AsDataSet();
            var sheets = from DataTable sheet in workbook.Tables select sheet.TableName;
            return sheets;
        }

        // Obtiene la data de las hojas de calculo
        // sheet: Especifica el nombre de la hoja de calculo
        // firstRowIsColumnNames: Especifica si la primera fila tiene los nombres de las columnas        
        public IEnumerable<DataRow> getData(string sheet, bool firstRowIsColumnNames = true)
        {
            var reader = this.getExcelReader();
            reader.IsFirstRowAsColumnNames = firstRowIsColumnNames;
            var workSheet = reader.AsDataSet().Tables[sheet];
            var rows = from DataRow a in workSheet.Rows select a;
            return rows;
        }
    }
}