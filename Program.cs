using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DocGen
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Поиск XML файла документации...");
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;

            // Ищем первый попавшийся xml файл
            string[] xmlFiles = Directory.GetFiles(currentDir, "*.xml");

            if (xmlFiles.Length == 0)
            {
                Console.WriteLine("Ошибка: XML файлы не найдены в директории приложения.");
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey();
                return;
            }

            string xmlPath = xmlFiles[0];
            Console.WriteLine($"Найден файл: {Path.GetFileName(xmlPath)}");

            try
            {
                GenerateReport(xmlPath, currentDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при генерации отчета: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        static void GenerateReport(string xmlPath, string outputDir)
        {
            XDocument doc = XDocument.Load(xmlPath);

            // Словарь для группировки методов по классам
            var methodsByClass = new Dictionary<string, List<MethodData>>();
            var classSummaries = new Dictionary<string, string>();

            foreach (var member in doc.Descendants("member"))
            {
                string nameAttr = member.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(nameAttr)) continue;

                // Если это описание класса
                if (nameAttr.StartsWith("T:"))
                {
                    string className = nameAttr.Substring(2);
                    string summary = GetTextClean(member.Element("summary"));
                    classSummaries[className] = summary;
                }
                // Если это описание метода
                else if (nameAttr.StartsWith("M:"))
                {
                    string fullMethodName = nameAttr.Substring(2);

                    int parenIndex = fullMethodName.IndexOf('(');
                    string nameWithoutParams = parenIndex != -1 ? fullMethodName.Substring(0, parenIndex) : fullMethodName;

                    int lastDot = nameWithoutParams.LastIndexOf('.');
                    if (lastDot == -1) continue; // Пропускаем, если не можем определить класс

                    string className = nameWithoutParams.Substring(0, lastDot);
                    string methodName = nameWithoutParams.Substring(lastDot + 1);

                    // Извлекаем типы параметров из сигнатуры метода для типа данных
                    string[] paramTypes = new string[0];
                    if (parenIndex != -1)
                    {
                        string argsString = fullMethodName.Substring(parenIndex + 1).TrimEnd(')');
                        // Заменяем XML-специфичные скобки для Generic типов на стандартные
                        argsString = argsString.Replace("{", "<").Replace("}", ">");
                        paramTypes = argsString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    }

                    string summary = GetTextClean(member.Element("summary"));
                    string returns = GetTextClean(member.Element("returns"));

                    var paramElements = member.Elements("param").ToList();
                    List<string> parametersList = new List<string>();

                    for (int i = 0; i < paramElements.Count; i++)
                    {
                        var p = paramElements[i];
                        string pName = p.Attribute("name")?.Value ?? "arg";
                        string pDesc = GetTextClean(p);
                        string pType = (i < paramTypes.Length) ? $" ({paramTypes[i]})" : "";

                        parametersList.Add($"<b>{pName}</b>{pType}: {pDesc}");
                    }

                    if (!methodsByClass.ContainsKey(className))
                    {
                        methodsByClass[className] = new List<MethodData>();
                    }

                    methodsByClass[className].Add(new MethodData
                    {
                        Name = methodName,
                        Summary = string.IsNullOrEmpty(summary) ? "-" : summary,
                        Returns = string.IsNullOrEmpty(returns) ? "-" : returns,
                        Parameters = parametersList.Count > 0 ? string.Join("<br><br>", parametersList) : "-"
                    });
                }
            }

            // Создаем HTML, который Word воспримет как свой родной документ
            string outputPath = Path.Combine(outputDir, "Проектная_документация.doc");
            StringBuilder html = new StringBuilder();

            // Добавляем правильные мета-теги, чтобы Word корректно прочитал кириллицу
            html.AppendLine("<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word' xmlns='http://www.w3.org/TR/REC-html40'>");
            html.AppendLine("<head>");
            html.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Times New Roman', Times, serif; font-size: 12pt; }");
            html.AppendLine("h1 { text-align: center; font-size: 16pt; margin-bottom: 24pt; }");
            html.AppendLine("h2 { font-size: 14pt; margin-top: 18pt; margin-bottom: 10pt; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; margin-bottom: 20pt; }");
            html.AppendLine("th, td { border: 1px solid black; padding: 6pt; text-align: left; vertical-align: top; }");
            html.AppendLine("th { background-color: #F2F2F2; font-weight: bold; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");

            html.AppendLine("<h1>ПРОЕКТНАЯ ДОКУМЕНТАЦИЯ</h1>");

            foreach (var kvp in methodsByClass)
            {
                string className = kvp.Key;
                // Пытаемся взять summary класса для красивого заголовка (например "Служба авторизации")
                string classHeader = classSummaries.ContainsKey(className) && !string.IsNullOrEmpty(classSummaries[className])
                    ? classSummaries[className]
                    : className;

                // Название секции (по шаблону: Страница авторизации / Класс)
                html.AppendLine($"<h2>{classHeader} ({className.Split('.').Last()})</h2>");

                html.AppendLine("<table>");
                html.AppendLine("<tr><th width='15%'>Метод</th><th width='25%'>Назначение</th><th width='30%'>Входные данные</th><th width='30%'>Выходные данные</th></tr>");

                foreach (var method in kvp.Value)
                {
                    html.AppendLine("<tr>");
                    html.AppendLine($"<td>{method.Name}</td>");
                    html.AppendLine($"<td>{method.Summary}</td>");
                    html.AppendLine($"<td>{method.Parameters}</td>");
                    html.AppendLine($"<td>{method.Returns}</td>");
                    html.AppendLine("</tr>");
                }
                html.AppendLine("</table>");
            }

            html.AppendLine("</body></html>");

            // Сохраняем с BOM(Byte Order Mark) для 100% корректного распознавания кодировки Вордом
            File.WriteAllText(outputPath, html.ToString(), new UTF8Encoding(true));

            Console.WriteLine($"Успех! Отчет сгенерирован и сохранен по пути:");
            Console.WriteLine(outputPath);
        }

        // Вспомогательный метод для очистки текста из XML-тегов (удаление лишних пробелов и переносов строк)
        static string GetTextClean(XElement element)
        {
            if (element == null || string.IsNullOrWhiteSpace(element.Value)) return string.Empty;

            var lines = element.Value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(l => l.Trim())
                                     .Where(l => l.Length > 0);

            return string.Join(" ", lines);
        }
    }

    class MethodData
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public string Parameters { get; set; }
        public string Returns { get; set; }
    }
}