namespace MyNUnit.Tests;

using MyTestAttribute = Attributes.TestAttribute;
using MyBeforeAttribute = Attributes.BeforeAttribute;
using MyAfterAttribute = Attributes.AfterAttribute;
using MyBeforeClassAttribute = Attributes.BeforeClassAttribute;
using MyAfterClassAttribute = Attributes.AfterClassAttribute;

public class TestsForTest
{
    [MyTest]
    public void PassingTest()
    {
    }

    [MyTest]
    public void FailingTest()
    {
        throw new Exception("aaa");
    }

    [MyTest(Ignore = "qqq")]
    public void IgnoredTest()
    {
    }
}

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