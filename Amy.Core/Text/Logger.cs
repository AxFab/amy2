using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Text
{
    public class Logger
    {
        class LogSev
        {
            public bool IsError;
            public bool IsIgnored;
            public string Message;
        }

        public void Error<T>(T code, Token token, params object[] args) where T : Enum => Error(code.ToString(), token, args);
        public void Message<T>(T code, Token token, params object[] args) where T : Enum => Message(code.ToString(), token, args);
        public void Error(string code, Token token, params object[] args)
        {
            var sev = FindInfo(code);
            Log(true, code, sev.Message, token, args);
        }
        public void Message(string code, Token token, params object[] args)
        {
            var sev = FindInfo(code);
            if (sev.IsIgnored)
                return;
            Log(sev.IsError, code, sev.Message, token, args);
        }

        private LogSev FindInfo(string code)
        {
            return new LogSev
            {
                IsIgnored = false,
                IsError = false,
                Message = "Issue",
            };
        }
        private void Log(bool error, string code, string message, Token token, params object[] args)
        {
            var formatedMsg = string.Format(message, args);
            Console.WriteLine($"[{(error ? "ERR." : "WARN")}] {code} - {formatedMsg} at {token.Filename} l.{token.Row}");
        }
    }
}
