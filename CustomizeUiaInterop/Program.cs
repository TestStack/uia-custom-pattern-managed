using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CustomizeUiaInterop
{
    internal class Program
    {
        private static void ShowHelp()
        {
            Console.WriteLine("CustomizeUiaInterop <input> <output>");
        }

        private static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                ShowHelp();
                return 1;
            }

            var inputFilePath = args[0];
            var outputFilePath = args[1];

            try
            {
                // Read all the text
                var contents = File.ReadAllText(inputFilePath);

                var updated = Regex.Replace(contents,
                                            @"\[out\] int32\& pPropertyIds,",
                                            @"[in] native int pPropertyIds,");

                updated = Regex.Replace(updated,
                                        @"\[out\] int32\& pEventIds",
                                        @"[in] native int pEventIds");

                updated = Regex.Replace(updated,
                                        @"\[in\] valuetype Interop.UIAutomationCore.UIAutomationParameter& pParams",
                                        @"[in] native int pParams");

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