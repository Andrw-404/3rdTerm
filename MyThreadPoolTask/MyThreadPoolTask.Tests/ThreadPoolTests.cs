using MyThreadPoolTask;

namespace MyThreadPoolTask.Tests
{
    public class ThreadPoolTests
    {
        private MyThreadPool threadPool;

        [SetUp]
        public void Setup()
        {
            this.threadPool = new MyThreadPool(5);
        }

        [TearDown]
        public void TearDown()
        {
            this.threadPool.Shutdown();
        }

        [Test]
        public void Submit_SinglTask_ReturnCorrectResult()
        {
            var task = this.threadPool.Submit(() => 42 * 2);
            Assert.That(task.Result, Is.EqualTo(84));
            Assert.That(task.IsCompleted, Is.True);
        }

        [Test]
        public void Submit_MultiplyTask_ReturnCorrectResult()
        {
            var tasks = new List<IMyTask<int>>();
            for (int i = 0; i < 10; ++i)
            {
                int local = i;
                tasks.Add(this.threadPool.Submit(() => local * 2));
            }

            for (int i = 0; i < 10; ++i)
            {
                Assert.That(tasks[i].Result, Is.EqualTo(i * 2));
                Assert.That(tasks[i].IsCompleted, Is.True);
            }
        }
    }
}
