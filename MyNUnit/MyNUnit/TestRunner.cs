namespace MyNUnit;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Attributes;

public class TestRunner
{
    public void RunTest(string path)
    {
        var dlls = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
        var results = new ConcurrentBag<TestResult>();

        Parallel.ForEach(dlls, (dllPath) =>
        {
            try
            {
                var assembly = Assembly.LoadFrom(dllPath);
                var testClasses = assembly.GetTypes().Where(x => x.GetMethods().Any(y => y.GetCustomAttribute<TestAttribute>() != null));

                Parallel.ForEach(testClasses, (testClass) =>
                {
                    RunTestsInClass(testClass, results);
                });
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{exception.Message}");
            }
        });

        this.Print(results);

    }

    private bool RunStaticMethods(Type type, Type attributeType, out string error)
    {
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(x => x.GetCustomAttribute(attributeType) != null);

        foreach (var method in methods)
        {
            try
            {
                method.Invoke(null, null);
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        error = null;
        return true;
    }

    private void RunSingleTest(Type testClass, MethodInfo methodInfo, ConcurrentBag<TestResult> results)
    {
        var attribute = methodInfo.GetCustomAttribute<TestAttribute>();
        var result = new TestResult
        {
            ClassName = testClass.Name,
            MethodName = methodInfo.Name,
        };

        if (!string.IsNullOrEmpty(attribute.Ignore))
        {
            result.IsIgnored = true;
            result.IgnoreReason = attribute.Ignore;
            result.TestTime = TimeSpan.Zero;
            results.Add(result);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        object instance = null;

        try
        {
            instance = Activator.CreateInstance(testClass);
            RunBeforeAndAfterMethods(testClass, instance, typeof(BeforeAttribute));

            try
            {
                methodInfo.Invoke(instance, null);
                if (attribute.Expected != null)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = $"Ожидаемое({attribute.Expected.Name}) исключение не выбросилось";
                }
                else
                {
                    result.IsSuccess = true;
                }
            }
            catch (TargetInvocationException targetInvocationException)
            {
                var ex = targetInvocationException.InnerException;
                if (attribute.Expected != null && attribute.Expected.IsInstanceOfType(ex))
                {
                    result.IsSuccess = true;
                }
                else
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = $"{ex.GetType().Name}: {ex.Message}";
                }
            }

            RunBeforeAndAfterMethods(testClass, instance, typeof(AfterAttribute));
        }
        catch
        {
            result.IsSuccess = false;
        }
        finally
        {
            stopwatch.Stop(); 
            result.TestTime = stopwatch.Elapsed;
            results.Add(result);
        }
    }

    private void RunBeforeAndAfterMethods(Type type, object instance, Type attributeType)
    {
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetCustomAttribute(attributeType) != null);

        foreach (var method in methods)
        {
            try
            {
                method.Invoke(instance, null);
            }
            catch (TargetInvocationException exception)
            {
                throw;
            }
        }
    }

    private void RunTestsInClass(Type testClass, ConcurrentBag<TestResult> results)
    {
        if (!RunStaticMethods(testClass, typeof(BeforeClassAttribute), out string beforeClassError))
        {
            FailAllTests(testClass, results, $"{beforeClassError}");
            return;
        }

        var testMethods = testClass.GetMethods().Where(x => x.GetCustomAttribute<TestAttribute>() != null);

        foreach (var testMethod in testMethods)
        {
            RunSingleTest(testClass, testMethod, results);
        }

        RunStaticMethods(testClass, typeof(AfterClassAttribute), out _);
    }

    private void FailAllTests(Type testClass, ConcurrentBag<TestResult> results, string reason)
    {
        var methods = testClass.GetMethods().Where(x => x.GetCustomAttribute<TestAttribute>() != null);
        foreach (var method in methods)
        {
            results.Add(new TestResult
            {
                ClassName = testClass.Name,
                MethodName = method.Name,
                IsSuccess = false,
                ErrorMessage = reason,
                TestTime = TimeSpan.Zero,
            });
        }
    }

    private void Print(ConcurrentBag<TestResult> results)
    {
        Console.WriteLine("\nРезультаты тестирования");
        var passed = results.Where(x => x.IsSuccess).OrderBy(x => x.ClassName).ThenBy(x => x.MethodName).ToList();
        var failed = results.Where(x => !x.IsSuccess && !x.IsIgnored).OrderBy(x => x.ClassName).ThenBy(x => x.MethodName).ToList();
        var ignored = results.Where(x => x.IsIgnored).OrderBy(x => x.ClassName).ThenBy(x => x.MethodName).ToList();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Успешно {passed.Count}");
        foreach (var result in passed)
        {
            Console.WriteLine($" {result.ClassName}.{result.MethodName} ({result.TestTime.TotalMilliseconds:F5} мс)");
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\b Провалено {failed.Count}");
        foreach (var result in failed)
        {
            Console.WriteLine($" {result.ClassName}.{result.MethodName} ({result.TestTime.TotalMilliseconds:F5} мс) \n ОШИБКА: {result.ErrorMessage}");
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Проигнорировано {ignored.Count}");
        foreach (var result in ignored)
        {
            Console.WriteLine($" Пропущен {result.ClassName}.{result.MethodName} по причине {result.IgnoreReason}");
        }

        Console.ResetColor();
    }
}