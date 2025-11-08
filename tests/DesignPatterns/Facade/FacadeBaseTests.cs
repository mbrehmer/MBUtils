using MBUtils.DesignPatterns.Facade;

namespace MBUtils.Tests.DesignPatterns.Facade
{
    public class FacadeBaseTests
    {
        [Fact]
        public void FacadeBase_Execute_CallsExecuteCore()
        {
            TestFacade facade = new TestFacade();
            IFacade iFacade = facade;

            iFacade.Execute();

            Assert.True(facade.ExecuteCalled);
        }

        [Fact]
        public void FacadeBaseWithResult_Execute_ReturnsExpectedResult()
        {
            TestFacadeWithResult facade = new TestFacadeWithResult(42);
            IFacade<int> iFacade = facade;

            int result = iFacade.Execute();

            Assert.Equal(42, result);
            Assert.True(facade.ExecuteCalled);
        }

        [Fact]
        public void ComplexFacade_Execute_CoordinatesSubsystems()
        {
            // Simulate a facade coordinating multiple subsystems
            ComplexFacade facade = new ComplexFacade();
            IFacade<string> iFacade = facade;

            string result = iFacade.Execute();

            Assert.Equal("Service1:OK|Service2:OK|Service3:OK", result);
        }

        // Test implementations

        private sealed class TestFacade : FacadeBase
        {
            public bool ExecuteCalled { get; private set; }

            protected override void ExecuteCore()
            {
                ExecuteCalled = true;
            }
        }

        private sealed class TestFacadeWithResult : FacadeBase<int>
        {
            private readonly int _result;
            public bool ExecuteCalled { get; private set; }

            public TestFacadeWithResult(int result)
            {
                _result = result;
            }

            protected override int ExecuteCore()
            {
                ExecuteCalled = true;
                return _result;
            }
        }

        private sealed class ComplexFacade : FacadeBase<string>
        {
            private readonly SubsystemService1 _service1;
            private readonly SubsystemService2 _service2;
            private readonly SubsystemService3 _service3;

            public ComplexFacade()
            {
                _service1 = new SubsystemService1();
                _service2 = new SubsystemService2();
                _service3 = new SubsystemService3();
            }

            protected override string ExecuteCore()
            {
                string result1 = _service1.Process();
                string result2 = _service2.Process();
                string result3 = _service3.Process();
                return $"{result1}|{result2}|{result3}";
            }
        }

        // Simulated subsystem components

        private sealed class SubsystemService1
        {
            public string Process() => "Service1:OK";
        }

        private sealed class SubsystemService2
        {
            public string Process() => "Service2:OK";
        }

        private sealed class SubsystemService3
        {
            public string Process() => "Service3:OK";
        }
    }
}