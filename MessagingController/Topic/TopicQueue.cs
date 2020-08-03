using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using System.Messaging;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using Nistec.Runtime;
using Nistec.Messaging.Remote;
using Nistec.Generic;
using Nistec.Threading;
using System.IO;
using Nistec.Logging;
using System.Threading.Tasks;
using Nistec.Messaging.Server;
using Nistec.Messaging.Io;
using System.Transactions;
using System.Collections.Concurrent;

namespace Nistec.Messaging.Topic
{

    

        /*
          public class TopicQueue
          {

              PriorityMemQueue _queue = new PriorityMemQueue("local");
              CancellationTokenSource canceller = new CancellationTokenSource();
              Action<Ptr> _action;
              int intervalMilliseconds;

              #region message events

              /// <summary>
              /// ErrorOcurred
              /// </summary>
              public event GenericEventHandler<string> ErrorOcurred;
              /// <summary>
              /// Message Received
              /// </summary>
              public event GenericEventHandler<Ptr> MessageReceived;
              /// <summary>
              /// Message Arraived
              /// </summary>
              public event GenericEventHandler<Ptr> MessageArraived;




              protected virtual void OnMessageArraived(GenericEventArgs<Ptr> e)
              {
                  if (MessageArraived != null)
                      MessageArraived(this, e);
              }

              protected virtual void OnMessageReceived(GenericEventArgs<Ptr> e)
              {

                  if (MessageReceived != null)
                      MessageReceived(this, e);
              }
              protected virtual void OnErrorOcurred(GenericEventArgs<string> e)
              {
                  if (ErrorOcurred != null)
                      ErrorOcurred(this, e);
              }

              /// <summary>
              /// OnErrorOcurred
              /// </summary>
              /// <param name="msg"></param>
              private void OnErrorOcurred(string msg)
              {
                  Console.WriteLine("ErrorOcurred: " + msg);
                  OnErrorOcurred(new GenericEventArgs<string>(msg));
              }

              #endregion

              #region ctor
              public TopicQueue(int intervalMilliseconds = 100)
              {
                  this.intervalMilliseconds = intervalMilliseconds;
              }
              public TopicQueue(Action<Ptr> action, int intervalMilliseconds = 100)
              {
                  this.intervalMilliseconds = intervalMilliseconds;
                  _action = action;

              }
              #endregion

              #region properties

              long _counter;
              /// <summary>
              /// Gets the number of elements contained in the Queue{T}.
              /// </summary>
              public long Count
              {
                  get { return Interlocked.Read(ref _counter); }
              }

              /// <summary>
              /// Get indicating whether the queue is empty.
              /// </summary>
              public bool IsEmpty
              {
                  get { return Interlocked.Read(ref _counter) == 0; }
              }

              bool _isalive = false;
              /// <summary>
              /// Get indicating whether the queue listener ia alive.
              /// </summary>
              public bool IsAlive
              {
                  get { return _isalive; }
              }
              #endregion

              #region enqueue/dequeue

              /// <summary>
              /// Adds an object to the end of the Queue{T}.
              /// </summary>
              /// <param name="item"></param>
              public void Enqueue(Ptr item)
              {
                  _queue.Enqueue(item);
                  Interlocked.Increment(ref _counter);
                  Thread.Sleep(1);

                  OnMessageArraived(new GenericEventArgs<T>(item));

                  Console.WriteLine("<{0}> QListener item added, Count: {1}", Thread.CurrentThread.ManagedThreadId, Interlocked.Read(ref _counter));
              }
              /// <summary>
              /// Attempts to remove and return the object at the beginning of the Queue{T}.
              /// </summary>
              /// <returns></returns>
              public Ptr Dequeue()
              {
                  Ptr item;
                  if (_queue.TryDequeue(out item))
                  {
                      Interlocked.Decrement(ref _counter);
                  }
                  //_queue.TryTake(out item);
                  return item;
              }
              #endregion

              #region start/stop

              /// <summary>
              /// Start the queue listener.
              /// </summary>
              public void Start()
              {
                  _isalive = true;
                  // Start queue listener...
                  Task listener = Task.Factory.StartNew(() =>
                  {
                      while (_isalive)
                      {
                          Ptr item;
                          if (_queue.TryDequeue(out item))
                          {
                              Interlocked.Decrement(ref _counter);
                              if (_action != null)
                              {
                                  _action(item);
                              }
                              else
                                  OnMessageReceived(new GenericEventArgs<Ptr>(item));
                          }
                          Thread.Sleep(intervalMilliseconds);
                      }

                      Console.WriteLine("QListener stoped...");
                  },
                  canceller.Token,
                  TaskCreationOptions.LongRunning,
                  TaskScheduler.Default);

                  Console.WriteLine("QListener started...");
              }


              /// <summary>
              /// Stop the queue listener.
              /// </summary>
              public void Stop()
              {
                  _isalive = false;
                  // Shut down the listener...
                  canceller.Cancel();
                  //listener.Wait();
              }
              #endregion

          }
          */
    }


