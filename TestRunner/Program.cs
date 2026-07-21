using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TestRunner
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("========== Test Runner ==========\n");

            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("TestRunner.exe <ClassName> <MethodName>");
                return 2;
            }

            string className = args[0];
            string methodName = args[1];

            object instance = null;
            Type testType = null;

            try
            {
                //--------------------------------------------------
                // Load TestCases.dll
                //--------------------------------------------------

                string assemblyPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "TestCases.dll");

                if (!File.Exists(assemblyPath))
                    throw new FileNotFoundException($"Cannot find '{assemblyPath}'.");

                var asm = Assembly.LoadFrom(assemblyPath);

                Console.WriteLine($"Assembly Loaded : {assemblyPath}\n");

                //--------------------------------------------------
                // Find Test Class
                //--------------------------------------------------

                testType = asm.GetTypes()
                    .FirstOrDefault(t =>
                        t.Name.Equals(className, StringComparison.OrdinalIgnoreCase) ||
                        t.FullName.Equals(className, StringComparison.OrdinalIgnoreCase));

                if (testType == null)
                {
                    Console.WriteLine($"Class '{className}' not found.");
                    return 3;
                }

                Console.WriteLine($"Class : {testType.FullName}");

                //--------------------------------------------------
                // Create Instance
                //--------------------------------------------------

                instance = Activator.CreateInstance(testType);

                //--------------------------------------------------
                // Setup()
                //--------------------------------------------------

                Console.WriteLine("\nRunning Setup...");

                InvokeInheritedMethod(testType, instance, "Setup");

                //--------------------------------------------------
                // Start()
                //--------------------------------------------------

                Console.WriteLine("Running Start...");

                var startMethod = testType.GetMethod(
                    "Start",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);

                startMethod?.Invoke(instance, null);

                //--------------------------------------------------
                // Test Method
                //--------------------------------------------------

                Console.WriteLine($"\nRunning Test : {methodName}");

                var testMethod = testType.GetMethod(
                    methodName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                if (testMethod == null)
                {
                    Console.WriteLine($"Test method '{methodName}' not found.");
                    return 4;
                }

                var stopwatch = Stopwatch.StartNew();

                testMethod.Invoke(instance, null);

                stopwatch.Stop();

                Console.WriteLine($"\nPASS ({stopwatch.ElapsedMilliseconds} ms)");

                return 0;
            }
            catch (TargetInvocationException ex)
            {
                Console.WriteLine("\n========== TEST FAILED ==========\n");

                Console.WriteLine(ex.InnerException?.ToString());

                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n========== TEST FAILED ==========\n");

                Console.WriteLine(ex);

                return 1;
            }
            finally
            {
                //--------------------------------------------------
                // Cleanup()
                //--------------------------------------------------

                try
                {
                    if (instance != null && testType != null)
                    {
                        Console.WriteLine("\nRunning Cleanup...");

                        InvokeInheritedMethod(testType, instance, "Cleanup");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cleanup failed: {ex.Message}");
                }

                Console.WriteLine("\n========== Finished ==========");
            }
        }

        private static void InvokeInheritedMethod(Type type, object instance, string methodName)
        {
            Type current = type;

            while (current != null)
            {
                var method = current.GetMethod(
                    methodName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                if (method != null)
                {
                    Console.WriteLine($" -> {method.DeclaringType.Name}.{method.Name}");
                    method.Invoke(instance, null);
                    return;
                }

                current = current.BaseType;
            }
        }
    }
}