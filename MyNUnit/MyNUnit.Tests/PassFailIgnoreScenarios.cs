// <copyright file="MockClasses.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace MyNUnit.Tests;

using MyTestAttribute = Attributes.TestAttribute;

public class PassFailIgnoreScenarios
{
    [MyTest]
    public void PassingTest()
    {
    }

    [MyTest]
    public void FailingTest() => throw new Exception("aaa");

    [MyTest(Ignore = "qqq")]
    public void IgnoredTest()
    {
    }
}