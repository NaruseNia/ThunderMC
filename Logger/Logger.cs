namespace Logger
{
    public class LogBuilder
    {
        private List<LoggerObject> _messages = new List<LoggerObject>();

        private struct LoggerObject
        {
            public string message;
            public ConsoleColor color;
        }

        public LogBuilder Init()
        {
            _messages = new List<LoggerObject>();
            return this;
        }

        public LogBuilder Add(object message, ConsoleColor color)
        {
            _messages.Add(new LoggerObject { message = (string)message ?? "", color = color });
            return this;
        }

        public LogBuilder Add(object message)
        {
            _messages.Add(new LoggerObject { message = (string)message, color = ConsoleColor.Gray });
            return this;
        }

        public LogBuilder Newline()
        {
            _messages.Add(new LoggerObject { message = "\n" });
            return this;
        }

        public LogBuilder Newline(uint count)
        {
            string _out = "";
            for (int i = 0; i < count; i++)
            {
                _out += "\n";
            }
            _messages.Add(new LoggerObject { message = _out });
            return this;
        }

        public void Put()
        {
            _messages.ForEach((obj) =>
            {
                Console.ForegroundColor = obj.color;
                Console.Write(obj.message);
                Console.ResetColor();
            });
        }
    }
}