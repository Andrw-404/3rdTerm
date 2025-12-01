namespace MyNUnit.Tests;

using Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyAfterAttribute = Attributes.AfterAttribute;
using MyAfterClassAttribute = Attributes.AfterClassAttribute;
using MyBeforeAttribute = Attributes.BeforeAttribute;
using MyBeforeClassAttribute = Attributes.BeforeClassAttribute;
using MyTestAttribute = Attributes.TestAttribute;

public class LifecycleTest
{
    public static List<string> Loger = new List<string>();

    [MyBeforeClass]
    public static void BeforeClass() => Loger.Add("BeforeClass");

    [MyAfterClass]
    public static void AfterClass() => Loger.Add("AfterClass");

    [MyBefore]
    public static void Before() => Loger.Add("Before");

    [MyAfter]
    public static void After() => Loger.Add("After");

    [MyTest]
    public static void FirstTest() => Loger.Add("Test1");

    [MyTest]
    public static void SecondTest() => Loger.Add("Test2");
}