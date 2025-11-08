using MBUtils.DesignPatterns.Observer;

namespace MBUtils.Tests.DesignPatterns.Observer
{
    public class SubjectBaseTests
    {
        [Fact]
        public void Attach_NullObserver_ThrowsArgumentNullException()
        {
            TestSubject subject = new TestSubject();
            ISubject<string> isubject = subject;

            Assert.Throws<ArgumentNullException>(() => isubject.Attach(null!));
        }

        [Fact]
        public void AttachAsync_NullObserver_ThrowsArgumentNullException()
        {
            TestSubject subject = new TestSubject();
            ISubject<string> isubject = subject;

            Assert.Throws<ArgumentNullException>(() => isubject.AttachAsync(null!));
        }

        [Fact]
        public void Detach_NullObserver_ThrowsArgumentNullException()
        {
            TestSubject subject = new TestSubject();
            ISubject<string> isubject = subject;

            Assert.Throws<ArgumentNullException>(() => isubject.Detach(null!));
        }

        [Fact]
        public void DetachAsync_NullObserver_ThrowsArgumentNullException()
        {
            TestSubject subject = new TestSubject();
            ISubject<string> isubject = subject;

            Assert.Throws<ArgumentNullException>(() => isubject.DetachAsync(null!));
        }

        [Fact]
        public void Notify_SingleObserver_ReceivesNotification()
        {
            TestSubject subject = new TestSubject();
            TestObserver observer = new TestObserver();
            ISubject<string> isubject = subject;

            isubject.Attach(observer);
            isubject.Notify("test data");

            Assert.Single(observer.ReceivedData);
            Assert.Equal("test data", observer.ReceivedData[0]);
        }

        [Fact]
        public void Notify_MultipleObservers_AllReceiveNotification()
        {
            TestSubject subject = new TestSubject();
            TestObserver observer1 = new TestObserver();
            TestObserver observer2 = new TestObserver();
            TestObserver observer3 = new TestObserver();
            ISubject<string> isubject = subject;

            isubject.Attach(observer1);
            isubject.Attach(observer2);
            isubject.Attach(observer3);

            isubject.Notify("broadcast");

            Assert.Single(observer1.ReceivedData);
            Assert.Single(observer2.ReceivedData);
            Assert.Single(observer3.ReceivedData);
            Assert.Equal("broadcast", observer1.ReceivedData[0]);
            Assert.Equal("broadcast", observer2.ReceivedData[0]);
            Assert.Equal("broadcast", observer3.ReceivedData[0]);
        }

        [Fact]
        public void Notify_MultipleNotifications_ObserverReceivesAll()
        {
            TestSubject subject = new TestSubject();
            TestObserver observer = new TestObserver();
            ISubject<string> isubject = subject;

            isubject.Attach(observer);
            isubject.Notify("first");
            isubject.Notify("second");
            isubject.Notify("third");

            Assert.Equal(3, observer.ReceivedData.Count);
            Assert.Equal("first", observer.ReceivedData[0]);
            Assert.Equal("second", observer.ReceivedData[1]);
            Assert.Equal("third", observer.ReceivedData[2]);
        }

        [Fact]
        public void Notify_AfterDetach_ObserverDoesNotReceive()
        {
            TestSubject subject = new TestSubject();
            TestObserver observer = new TestObserver();
            ISubject<string> isubject = subject;

            isubject.Attach(observer);
            isubject.Notify("first");
            isubject.Detach(observer);
            isubject.Notify("second");

            Assert.Single(observer.ReceivedData);
            Assert.Equal("first", observer.ReceivedData[0]);
        }

        [Fact]
        public void Detach_NotAttachedObserver_DoesNotThrow()
        {
            TestSubject subject = new TestSubject();
            TestObserver observer = new TestObserver();
            ISubject<string> isubject = subject;

            Exception? exception = Record.Exception(() => isubject.Detach(observer));
            Assert.Null(exception);
        }

        [Fact]
        public void Notify_ObserverThrowsException_ExceptionPropagates()
        {
            TestSubject subject = new TestSubject();
            ThrowingObserver throwingObserver = new ThrowingObserver();
            ISubject<string> isubject = subject;

            isubject.Attach(throwingObserver);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => isubject.Notify("data"));
            Assert.Equal("Observer error", exception.Message);
        }

        [Fact]
        public void Notify_ObserverThrowsException_SubsequentObserversNotNotified()
        {
            TestSubject subject = new TestSubject();
            ThrowingObserver throwingObserver = new ThrowingObserver();
            TestObserver observer = new TestObserver();
            ISubject<string> isubject = subject;

            isubject.Attach(throwingObserver);
            isubject.Attach(observer);

            Assert.Throws<InvalidOperationException>(() => isubject.Notify("data"));
            Assert.Empty(observer.ReceivedData);
        }

        [Fact]
        public async Task NotifyAsync_SingleAsyncObserver_ReceivesNotification()
        {
            TestSubject subject = new TestSubject();
            TestAsyncObserver observer = new TestAsyncObserver();
            ISubject<string> isubject = subject;

            isubject.AttachAsync(observer);
            await isubject.NotifyAsync("async data");

            Assert.Single(observer.ReceivedData);
            Assert.Equal("async data", observer.ReceivedData[0]);
        }

        [Fact]
        public async Task NotifyAsync_MultipleAsyncObservers_AllReceiveNotification()
        {
            TestSubject subject = new TestSubject();
            TestAsyncObserver observer1 = new TestAsyncObserver();
            TestAsyncObserver observer2 = new TestAsyncObserver();
            ISubject<string> isubject = subject;

            isubject.AttachAsync(observer1);
            isubject.AttachAsync(observer2);

            await isubject.NotifyAsync("broadcast");

            Assert.Single(observer1.ReceivedData);
            Assert.Single(observer2.ReceivedData);
            Assert.Equal("broadcast", observer1.ReceivedData[0]);
            Assert.Equal("broadcast", observer2.ReceivedData[0]);
        }

        [Fact]
        public async Task NotifyAsync_AfterDetachAsync_ObserverDoesNotReceive()
        {
            TestSubject subject = new TestSubject();
            TestAsyncObserver observer = new TestAsyncObserver();
            ISubject<string> isubject = subject;

            isubject.AttachAsync(observer);
            await isubject.NotifyAsync("first");
            isubject.DetachAsync(observer);
            await isubject.NotifyAsync("second");

            Assert.Single(observer.ReceivedData);
            Assert.Equal("first", observer.ReceivedData[0]);
        }

        [Fact]
        public async Task NotifyAsync_MixedObservers_AllReceiveNotification()
        {
            TestSubject subject = new TestSubject();
            TestObserver syncObserver = new TestObserver();
            TestAsyncObserver asyncObserver = new TestAsyncObserver();
            ISubject<string> isubject = subject;

            isubject.Attach(syncObserver);
            isubject.AttachAsync(asyncObserver);

            await isubject.NotifyAsync("mixed");

            Assert.Single(syncObserver.ReceivedData);
            Assert.Single(asyncObserver.ReceivedData);
            Assert.Equal("mixed", syncObserver.ReceivedData[0]);
            Assert.Equal("mixed", asyncObserver.ReceivedData[0]);
        }

        [Fact]
        public async Task NotifyAsync_AsyncObserverThrowsException_ExceptionPropagates()
        {
            TestSubject subject = new TestSubject();
            ThrowingAsyncObserver throwingObserver = new ThrowingAsyncObserver();
            ISubject<string> isubject = subject;

            isubject.AttachAsync(throwingObserver);

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => isubject.NotifyAsync("data"));
            Assert.Equal("Async observer error", exception.Message);
        }

        [Fact]
        public async Task NotifyAsync_CancellationToken_PassedToAsyncObservers()
        {
            TestSubject subject = new TestSubject();
            CancellationTokenCapturingObserver observer = new CancellationTokenCapturingObserver();
            ISubject<string> isubject = subject;
            CancellationTokenSource cts = new CancellationTokenSource();

            isubject.AttachAsync(observer);
            await isubject.NotifyAsync("data", cts.Token);

            Assert.True(observer.ReceivedToken.HasValue);
            Assert.Equal(cts.Token, observer.ReceivedToken.Value);
        }

        [Fact]
        public void ProtectedNotify_CalledFromDerivedClass_NotifiesObservers()
        {
            TestSubject subject = new TestSubject();
            TestObserver observer = new TestObserver();
            ISubject<string> isubject = subject;

            isubject.Attach(observer);
            subject.TriggerNotification("protected test");

            Assert.Single(observer.ReceivedData);
            Assert.Equal("protected test", observer.ReceivedData[0]);
        }

        [Fact]
        public async Task ProtectedNotifyAsync_CalledFromDerivedClass_NotifiesObservers()
        {
            TestSubject subject = new TestSubject();
            TestAsyncObserver observer = new TestAsyncObserver();
            ISubject<string> isubject = subject;

            isubject.AttachAsync(observer);
            await subject.TriggerNotificationAsync("protected async test");

            Assert.Single(observer.ReceivedData);
            Assert.Equal("protected async test", observer.ReceivedData[0]);
        }

        [Fact]
        public async Task Notify_ConcurrentAttachDetach_ThreadSafe()
        {
            TestSubject subject = new TestSubject();
            ISubject<string> isubject = subject;
            List<TestObserver> observers = new List<TestObserver>();

            for (int i = 0; i < 10; i++)
            {
                observers.Add(new TestObserver());
            }

            Task attachTask = Task.Run(() =>
            {
                foreach (TestObserver observer in observers)
                {
                    isubject.Attach(observer);
                    Thread.Sleep(1);
                }
            });

            Task notifyTask = Task.Run(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    isubject.Notify($"message {i}");
                    Thread.Sleep(1);
                }
            });

            await Task.WhenAll(attachTask, notifyTask);

            // Test passes if no exceptions were thrown
            Assert.True(true);
        }

        [Fact]
        public void Notify_ObserverNotificationOrder_PreservedAsAttached()
        {
            TestSubject subject = new TestSubject();
            OrderTrackingObserver observer1 = new OrderTrackingObserver(1);
            OrderTrackingObserver observer2 = new OrderTrackingObserver(2);
            OrderTrackingObserver observer3 = new OrderTrackingObserver(3);
            ISubject<string> isubject = subject;
            List<int> notificationOrder = new List<int>();

            observer1.OnNotified = (id) => notificationOrder.Add(id);
            observer2.OnNotified = (id) => notificationOrder.Add(id);
            observer3.OnNotified = (id) => notificationOrder.Add(id);

            isubject.Attach(observer1);
            isubject.Attach(observer2);
            isubject.Attach(observer3);

            isubject.Notify("test");

            Assert.Equal(3, notificationOrder.Count);
            Assert.Equal(1, notificationOrder[0]);
            Assert.Equal(2, notificationOrder[1]);
            Assert.Equal(3, notificationOrder[2]);
        }

        // Test helper classes

        private sealed class TestSubject : SubjectBase<string>
        {
            public void TriggerNotification(string data)
            {
                Notify(data);
            }

            public Task TriggerNotificationAsync(string data, CancellationToken cancellationToken = default)
            {
                return NotifyAsync(data, cancellationToken);
            }
        }

        private sealed class TestObserver : MBUtils.DesignPatterns.Observer.IObserver<string>
        {
            public List<string> ReceivedData { get; } = new List<string>();

            void MBUtils.DesignPatterns.Observer.IObserver<string>.Update(string data)
            {
                ReceivedData.Add(data);
            }
        }

        private sealed class TestAsyncObserver : IAsyncObserver<string>
        {
            public List<string> ReceivedData { get; } = new List<string>();

            async Task IAsyncObserver<string>.UpdateAsync(string data, CancellationToken cancellationToken)
            {
                await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                ReceivedData.Add(data);
            }
        }

        private sealed class ThrowingObserver : MBUtils.DesignPatterns.Observer.IObserver<string>
        {
            void MBUtils.DesignPatterns.Observer.IObserver<string>.Update(string data)
            {
                throw new InvalidOperationException("Observer error");
            }
        }

        private sealed class ThrowingAsyncObserver : IAsyncObserver<string>
        {
            Task IAsyncObserver<string>.UpdateAsync(string data, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("Async observer error");
            }
        }

        private sealed class CancellationTokenCapturingObserver : IAsyncObserver<string>
        {
            public CancellationToken? ReceivedToken { get; private set; }

            Task IAsyncObserver<string>.UpdateAsync(string data, CancellationToken cancellationToken)
            {
                ReceivedToken = cancellationToken;
                return Task.CompletedTask;
            }
        }

        private sealed class OrderTrackingObserver : MBUtils.DesignPatterns.Observer.IObserver<string>
        {
            private readonly int _id;
            public Action<int>? OnNotified { get; set; }

            public OrderTrackingObserver(int id)
            {
                _id = id;
            }

            void MBUtils.DesignPatterns.Observer.IObserver<string>.Update(string data)
            {
                OnNotified?.Invoke(_id);
            }
        }
    }
}