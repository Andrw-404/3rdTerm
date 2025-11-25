using MyNUnit;

if (args.Length == 0)
{
    Console.WriteLine("Укажите путь");
    return;
}

string path = args[0]; 
if (!Directory.Exists(path))
{
    Console.WriteLine($"Папка {path} не найдена");
    return;
}

var runner = new TestRunner();
runner.RunTest(path);