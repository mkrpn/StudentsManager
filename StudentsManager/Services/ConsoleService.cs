namespace StudentsManager.Services
{
    public class ConsoleService
    {
        public string EnterData(string message, Func<string, bool> validator)
        {
            string data;

            do
            {
                Console.WriteLine(message);

                data = Console.ReadLine();
            }
            while (!validator(data));

            return data;
        }

        public void WriteExceptionToConsole(Exception ex)
        {
            WriteErrorline(ex.ToString());
        }

        internal void WriteSuccess(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(text);
            Console.ResetColor();
        }

        internal void WriteError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(text);
            Console.ResetColor();
        }

        internal void Write(string text)
        {
            Console.Write(text);
        }

        internal void WriteLine(string consoleLine = null)
        {
            Console.WriteLine(consoleLine);
        }

        internal void WriteErrorline(string consoleLine)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(consoleLine);
            Console.ResetColor();
        }

        internal void WriteSuccessline(string consoleLine)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(consoleLine);
            Console.ResetColor();
        }
    }
}
