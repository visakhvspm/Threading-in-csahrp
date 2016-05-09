using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AllAbtThread
{
    class Program
    {
        static void Main(string[] args)
        {
            #region basic Thread Creations
            Thread objThread1 = new Thread(ThreadMetho1);
            objThread1.Start();

            Thread objThread2 = new Thread(new ThreadStart(ThreadMetho1));
            objThread2.Start();

            Thread objThread3 = new Thread(new ParameterizedThreadStart(ThreadMethodParameter));
            objThread3.Start(2);

            Thread objThread4 = new Thread(delegate(object a) { Console.WriteLine("Hi this is a test delegate" + a); });
            objThread4.Start(" 5");         

            Thread objThread5 = new Thread((object abc) => { Console.WriteLine(abc); });
            objThread5.Start(6);

            Thread objThread6 = new Thread(() => MethodWithMultipleParam(1, 2, 3)); // Thread call with multiple parameters
            objThread6.Start();

            #endregion

            #region JoinThreads

            Thread objThreadJoin1 = new Thread(PrintMe1);

            Thread objThreadJoin2 = new Thread(PrintMe2);

            objThreadJoin1.Start();
            objThreadJoin1.Join(); // Wait complete and start next thread but continue main thread operation

            objThreadJoin2.Start();


            #endregion

            #region Thread Pools
            ThreadPool.QueueUserWorkItem(DoWork);
            ThreadPool.QueueUserWorkItem(DoWork, "Test - data");

            //Using Asynchronous delegates

            Func<string, int> ThreadAsynch = GetStringLength; //Second Param as return type
            IAsyncResult AsychResult1 = ThreadAsynch.BeginInvoke("Test String", null, null); //Invoke in Thread Pool (Asynch)
            int returnVa = ThreadAsynch.Invoke("Test String"); //Invoke in main Thread
            int stringLenght = ThreadAsynch.EndInvoke(AsychResult1);

            Func<int, int, int, string> ThreadAsynchMulti = MethodWithMultipleParam3; //last Param as return type
            IAsyncResult AsychResult2 = ThreadAsynchMulti.BeginInvoke(1,2,3, null, null); //Invoke in Thread Pool (Asynch)  with multiple parameters         
            string result = ThreadAsynchMulti.EndInvoke(AsychResult2);

            #endregion

            #region Thread Exception Handling 
            ThreadPool.QueueUserWorkItem(Exceptionhandler, 0);

            try
            {
                Action MyAction = ExceptionGenertor;
                IAsyncResult resultValue = MyAction.BeginInvoke(null, null);
                MyAction.EndInvoke(resultValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Get exception out side
            }
            #endregion

            #region Thread Priority

            //Setting the thread priority to highest doesn’t equate to real-time execution as a thread’s 
            //priority will always depends on its parent container or process priority.

            Process prc1 = Process.GetCurrentProcess();
            prc1.PriorityClass = ProcessPriorityClass.High;
           

            #endregion

            #region Thread Synch

            TestSleep();
            Console.ReadKey();
            TestJoin();
            //TestTaskWait();

            #endregion

            #region Thread Lock
            TestThreadSafe testThreadSafe = new TestThreadSafe();
            Thread t1 = new Thread(testThreadSafe.DoDivide);
            Thread t2 = new Thread(testThreadSafe.DoDivide);
            //t1.Start();
            //t2.Start();  
            #endregion

            #region Thread Monitor
            TestMonitor testMonitor = new TestMonitor();
            Thread tm1 = new Thread(testMonitor.DoDivide);
            Thread tm2 = new Thread(testMonitor.DoDivide);
            //tm1.Start();
            //tm2.Start();  
            //Monitor vs Lock has some doubt
            #endregion

            #region dead lock
            DeadLock.StatThreads();

            #endregion

            //have to see data parallelism over task parallelism.

            Console.ReadKey();
        }

        #region Thread Synch
        static Thread thread1 = null;
        static Thread thread2 = null;

        static void DoWork1()
        {
            Console.WriteLine("Inside DoWork1, Thread 1 state: {0}", thread1.ThreadState);
            Console.WriteLine("Inside DoWork1, Thread 2 state: {0}", thread2.ThreadState);

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("DoWork1: " + i);
            }
        }

        static void DoWork2()
        {
            Console.WriteLine("Inside DoWork2, Thread 1 state: {0}", thread1.ThreadState);
            Console.WriteLine("Inside DoWork2, Thread 2 state: {0}", thread2.ThreadState);

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("DoWork2: " + i);
            }
        }

        static int DoWork3()
        {
            ////In real word, some expensive operation will be there.  
            ////Putting sleep to simulate load.  
            Thread.Sleep(1000);
            return 5;
        }

        static void TestJoin()
        {
            Console.WriteLine("\nTesting Join");
            thread1 = new Thread(DoWork1);
            thread2 = new Thread(DoWork2);
            thread1.Start();
            thread1.Join();
            thread2.Start();
        }

        static void TestSleep()
        {
            Console.WriteLine("\nTesting Sleep");
            Console.WriteLine("Thread sleep started time: {0}", DateTime.Now);
            Thread.Sleep(2000); //Sleep Main thread 
            Console.WriteLine("Thread sleep ended time: {0}", DateTime.Now);
        }

        static void TestTaskWait()
        {
            //Console.WriteLine("\nTesting Task.Wait");
            //Console.WriteLine("Task started time: {0}", DateTime.Now);
            //var task = Task.Factory.StartNew(DoWork3);
            //task.Wait();
            //Console.WriteLine("Task ended time: {0}, Result: {1}", DateTime.Now, task.Result);
        }

        #endregion

        #region staticMethos
        static void ThreadMetho1()
        {
            Console.WriteLine("My first Thread Method");
        }

        static void ThreadMethodParameter(object number)
        {
            Console.WriteLine(number as int?);
        }

        static void PrintMe1()
        {
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine("Print me 1");                
            }
        }
        static void PrintMe2()
        {
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine("Print me 2");
             
                Thread thr = new Thread(() =>
                {
                    for (int i1 = 0; i1 < 10; i1++)
                    {
                       // Console.WriteLine(i + "Inside print me 2 in " + i + "th time");
                    }
                });

                thr.Start();
                //Thread.Sleep(2000); //Sleep current thread 
            }
        }

        static void DoWork(object data)
        {
            Console.WriteLine("I am a pooled thread using QueueUserWorkItem. My value: {0}", data);
        }

        static void MethodWithMultipleParam(int a, int b, int c)
        {
            Console.WriteLine(a + "" + b + "" + c);
        }

        static int GetStringLength(string inputString)
        {
            Console.WriteLine("I am a pooled thread using AsynchronousDelegates. My value: {0}", inputString);
            return inputString.Length;
        }

        static string MethodWithMultipleParam3(int a, int b, int c)
        {
            string abc = a + "" + b + "" + c;
            return abc;
        }

        static void Exceptionhandler(object number)
        {
            try
            {
                int c = Convert.ToInt32(number) / Convert.ToInt32(number);
            }
            catch (Exception ex)
            {
                Console.WriteLine("All exceptions should handle inside Thread", ex.Message);
               // throw ex; // This will break tool
            }
        }

        static void ExceptionGenertor()
        {
            throw new Exception("Test Exception");
        }
        #endregion
    }

    class TestThreadSafe
    {
        int num1 = 0;
        int num2 = 0;
        Random rnd = new Random();

        private static Object obj = new object();

        public void DoDivide()
        {           
            //lock (this) //Lock whole object and can cause dead lock
            lock (obj) // Best practice, lock only this variable access
            {
                for (int i = 0; i < 1000000; i++)
                {
                    num1 = rnd.Next(1, 4);
                    num2 = rnd.Next(1, 4);
                    Console.WriteLine(num1 / num2);
                    num1 = 0;
                    num2 = 0;
                }
            }
        }
    }

    class TestMonitor
    {
        int num1 = 0;
        int num2 = 0;
        Random rnd = new Random();
        private static object myLock = new object();
        public void DoDivide()
        {
            Monitor.Enter(myLock); //// This ensures that only one thread can enter.   
            {
                try
                {
                    for (int i = 0; i < 5; i++)
                    {
                        num1 = rnd.Next(1, 5);
                        num2 = rnd.Next(1, 5);
                        Console.WriteLine("Division of {0} and {1} is: {2}", num1, num2, num1 / num2);
                        num1 = 0;
                        num2 = 0;
                    }
                }
                finally
                {
                    Monitor.Exit(myLock); ////This releases the lock.  
                }
            }

            //The advantage of lock over monitor is if due to some exception like memory overflow, or something 
            //If we are not able to execute finally block it wont release monitor, but in lock it will work as fine
        }
    }

    class DeadLock
    {
        static object lockObj1 = new object();
        static object lockObj2 = new object();
        static Thread t1 = new Thread(DoWork1);
        static Thread t2 = new Thread(DoWork2);

        static void DoWork1()
        {
            lock (lockObj1)
            {
                Console.WriteLine("Inside DoWork1: lockObj1 grabbed");
                Thread.Sleep(1000);
                Console.WriteLine("Inside DoWork1, t1 thread state: {0}", t1.ThreadState);
                Console.WriteLine("Inside DoWork1, t2 thread state: {0}", t2.ThreadState);
                lock (lockObj2) //Deadlock, below code is not executed  
                {
                    Console.WriteLine("Inside DoWork1: lockObj2 grabbed");
                }
            }
        }

        static void DoWork2()
        {
            lock (lockObj2)
            {
                Console.WriteLine("Inside DoWork2: lockObj2 grabbed");
                Thread.Sleep(500);
                Console.WriteLine("Inside DoWork2, t1 thread state: {0}", t1.ThreadState);
                Console.WriteLine("Inside DoWork2, t2 thread state: {0}", t2.ThreadState);
                lock (lockObj1) ////Deadlock, below code is not executed  
                {
                    Console.WriteLine("Inside DoWork2: lockObj1 grabbed");
                }
            }
        }

        public static void StatThreads()
        {
            t1.Start();
            t2.Start();  
        }
    }

}
