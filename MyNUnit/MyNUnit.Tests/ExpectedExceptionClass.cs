namespace MyNUnit.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyTestAttribute = Attributes.TestAttribute;

public class ExceptionTest
{
    [MyTest(Expected = typeof(ArgumentException))]
    public void ThrowsExpectedException()
    {
        throw new ArgumentException();
    }

    [MyTest(Expected = typeof(ArithmeticException))]
    public void ThrowsDifferentException()
    {
        throw new ArgumentNullException();
    }

    [MyTest(Expected = typeof(ArithmeticException))]
    public void DoesntThrowException()
    {
    }
}