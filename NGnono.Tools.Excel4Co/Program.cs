using CommandLine;
using NGnono.Framework.Extension;
using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace NGnono.Tools.Excel4Co
{

    internal class ArgsOption
    {
        [Option('f', "filepath", Required = true, HelpText = "xls 文件路径")]
        public string FilePath { get; set; }
    }

    class Program
    {
        /// <summary>
        /// 结尾不带"/" 
        /// </summary>
        private static string _appPath = Environment.CurrentDirectory;

        static void Main(string[] args)
        {
            var options = new ArgsOption();
            if (!Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine(("xls解析错误"));
                foreach (var s in args)
                {
                    Console.WriteLine(s);
                }
                Console.ReadLine();

                return;
            }

            var datas = ReadSourceData(options.FilePath);

            if (datas.Count == 0)
            {
                Console.WriteLine("没有读取到数据");
                Console.ReadLine();
                return;
            }

            WriteFile(datas);

            Console.WriteLine("执行完毕");
            Console.ReadLine();
        }

        private static Dictionary<string, List<string>> ReadSourceData(string filePath)
        {
            var fs = new FileStream(filePath, FileMode.Open);
            var workbook = new HSSFWorkbook(fs);

            var datas = new Dictionary<string, List<string>>();

            var sheets = workbook.NumberOfSheets;
            for (var i = 0; i < sheets; i++)
            {
                var sheet = workbook.GetSheetAt(i);
                var headerRow = sheet.GetRow(0);
                var cellCount = headerRow.LastCellNum;

                for (var j = (sheet.FirstRowNum + 1); j < sheet.LastRowNum; j++)
                {
                    var row = sheet.GetRow(j);
                    var first = true;
                    string key = null;
                    for (int k = row.FirstCellNum; k < cellCount; k++)
                    {
                        if (k == 1)
                        {
                            continue;
                        }
                        var cellVal = row.GetCell(k);
                        if (cellVal != null)
                        {
                            var v = cellVal.StringCellValue;
                            if (String.IsNullOrWhiteSpace(v))
                            {
                                Console.WriteLine("读取excel 时 cell为空了");
                                first = false;

                                continue;
                            }

                            v = v.ToLower();
                            if (first)
                            {
                                v = v.Replace(" ", String.Empty);
                                datas.TryInsertKey(v, new List<string>());
                                key = v;
                            }
                            else
                            {
                                if (String.IsNullOrWhiteSpace(key))
                                {
                                    Console.WriteLine("Key is NULL or Empty");
                                    break;
                                }

                                datas[key].TryAdd(v);
                            }
                        }

                        first = false;
                    }
                }

            }

            return datas;
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="datas"></param>
        private static void WriteFile(Dictionary<string, List<string>> datas)
        {
            var outPath = _appPath + "/out/";
            var fileName = DateTime.Now.ToFileTime();
            var fileFullName = outPath + fileName + ".csv";

            var fs = new FileInfo(fileFullName);
            if (!fs.Directory.Exists)
            {
                fs.Directory.Create();
            }

            using (var sw = fs.CreateText())
            {
                foreach (var data in datas)
                {
                    sw.WriteLine(String.Format("{0}>>{1}", data.Key, String.Join(",", data.Value)));
                }
            }
        }
    }
}
