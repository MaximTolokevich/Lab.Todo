using System;
using System.Collections;
using System.Linq;

namespace Lab.Todo.Web.Models
{
    public class ExceptionViewModel
    {
        public string Message { get; set; }
        public string Type { get; set; }
        public string StackTrace { get; set; }
        public string Data { get; set; }

        public ExceptionViewModel(Exception ex)
        {
            Message = ex.Message;
            Type = ex.GetType().ToString();
            StackTrace = ex.StackTrace?.Replace("\n", "<br>");
            Data = string.Join(" ", ex.Data.Cast<DictionaryEntry>()
                                           .ToDictionary(d => d.Key.ToString(), d => d.Value)
                                           .Select(s => $"{s.Key}: {s.Value}")
                                           .ToArray());
        }
    }
}