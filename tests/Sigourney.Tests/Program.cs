// Copyright (c) 2020 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;

namespace Sigourney.Tests
{
    static class Program
    {
        static int assertCount = 0;
        static int errorCount = 0;

        private static void PrintInColor(ConsoleColor color, string message, params object[] fmtArgs)
        {
            var colorPrevious = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine("Error: {0}", string.Format(message, fmtArgs));
            Console.ForegroundColor = colorPrevious;
        }

        private static void Assert(bool condition, string message)
        {
            assertCount++;
            if (condition) return;
            PrintInColor(ConsoleColor.Red, message);
            errorCount++;
        }

        static int Main()
        {
            var thisAssembly = typeof(Program).Assembly;
            Assert(thisAssembly.GetType("TestWeaver1Rulez") != null, "The type weaver #1 added was not found.");
            Assert(thisAssembly.GetType("ProcessedByTestWeaver1") != null, "The type Sigourney added to mark weaver #1 was not found.");

            if (errorCount == 0)
            {
                PrintInColor(ConsoleColor.Green, "Success. {0} asserts passed.", assertCount);
                return 0;
            }
            else
            {
                PrintInColor(ConsoleColor.Red, "Failure. {0} out of {1} asserts failed.", assertCount, errorCount);
                return 1;
            }
        }
    }
}
