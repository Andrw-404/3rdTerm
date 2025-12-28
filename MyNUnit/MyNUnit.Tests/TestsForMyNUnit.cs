// <copyright file="TestsForMyNUnit.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace MyNUnit.Tests;

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static MyNUnit.Tests.LifecycleTest;

public class TestsForMyNUnit
{
    private TestRunner? runner;
    private string? path;

    [OneTimeSetUp]
    public void Setup()
    {
        this.runner = new TestRunner();
        this.path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }

    [SetUp]
    public void ResetStat()
    {
        Loger.Clear();
    }

    [Test]
    public void BasicTests_ShouldIdentifySpecificity()
    {
        var result = this.runner!.RunTest(this.path!);
        var classResult = result.Where(x => x.ClassName == nameof(PassFailIgnoreScenarios));
        Assert.That(classResult.Count(), Is.EqualTo(3));

        var passing = classResult.First(x => x.MethodName == nameof(PassFailIgnoreScenarios.PassingTest));
        Assert.That(passing.IsSuccess, Is.True);

        var failed = classResult.First(x => x.MethodName == nameof(PassFailIgnoreScenarios.FailingTest));
        Assert.That(failed.IsSuccess, Is.False);

        var ignored = classResult.First(x => x.MethodName == nameof(PassFailIgnoreScenarios.IgnoredTest));
        Assert.That(ignored.IsIgnored, Is.True);
    }

    [Test]
    public void ExceptionTests_ShouldWasCorrectVersus()
    {
        var result = this.runner!.RunTest(this.path!);
        var classResult = result.Where(x => x.ClassName == nameof(ExceptionTest));
        Assert.That(classResult.Count(), Is.EqualTo(3));

        var success = classResult.First(x => x.MethodName == nameof(ExceptionTest.ThrowsExpectedException));
        Assert.That(success.IsSuccess, Is.True);

        var wrong = classResult.First(x => x.MethodName == nameof(ExceptionTest.ThrowsDifferentException));
        Assert.That(wrong.IsSuccess, Is.False);

        var noException = classResult.First(x => x.MethodName == nameof(ExceptionTest.DoesntThrowException));
        Assert.That(noException.IsSuccess, Is.False);
    }

    [Test]
    public void LifecycleTests_ShouldRunAllMethodsInCorrectCount()
    {
        this.runner!.RunTest(this.path!);

        var expectedLog = new List<string> { "BeforeClass", "Before", "Test1", "After", "AfterClass", "BeforeClass", "Before", "Test2", "After", "AfterClass" };
    }
}