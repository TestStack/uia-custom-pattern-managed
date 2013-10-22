using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CustomizeUiaInterop
{
    class Program
    {
        static void ShowHelp()
        {
            Console.WriteLine("CustomizeUiaInterop <input> <output>");
        }

        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                ShowHelp();
                return 1;
            }

            string inputFilePath = args[0];
            string outputFilePath = args[1];

            try
            {
                // Read all the text
                string contents = File.ReadAllText(inputFilePath);

                string updated = Regex.Replace(contents,
                    "\\[out\\] int32\\& pPropertyIds,",
                    "[in] native int pPropertyIds,");

                updated = Regex.Replace(updated,
                    "\\[out\\] int32\\& pEventIds",
                    "[in] native int pEventIds");

                updated = Regex.Replace(updated,
                    "\\[in\\] valuetype Interop.UIAutomationCore.UIAutomationParameter& pParams",
                    "[in] native int pParams");

                // Write all the text
                File.WriteAllText(outputFilePath, updated);
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception:");
                Console.WriteLine(e.ToString());
                return 1;
            }

            return 0;
        }
    }
}
