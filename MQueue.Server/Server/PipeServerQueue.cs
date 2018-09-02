using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using Nistec.Channels;
using Nistec.Messaging.Remote;
using Nistec.Runtime;
using Nistec.Generic;
using Nistec.IO;

namespace Nistec.Messaging.Server
{

    public class PipeServerEnqueue : PipeServerBase
    {

        #region ctor

        /// <summary>
        /// Constractor default.
        /// </summary>
         public PipeServerEnqueue()
            : base(QueueDefaults.EnqueuePipeName, true)
        {

        }

        /// <summary>
        /// Constractor with settings
        /// </summary>
        /// <param name="settings"></param>
        protected PipeServerEnqueue(PipeQueueSettings settings)
            : base(settings)
        {

        }
        #endregion

        #region override

        protected override void OnStart()
        {
            base.OnStart();
            //AgentManager.Start();//McLock.Lock.ValidateSystemKey());
        }

        protected override void OnStop()
        {
            base.OnStop();

            //AgentManager.Stop();
        }

        protected override void OnLoad()
        {
            base.OnLoad();
        }
        #endregion

        #region exec methods

       
        //protected override QueueRequest ReadRequest(NamedPipeServerStream stream)
        //{
           
        //    QueueRequest message = new QueueRequest();
        //    message.EntityRead(stream, null);
        //    return message;

        //    //return QueueItem.ReadServerRquest(stream);
        //}

        //protected override NetStream ExecRequset(QueueRequest message)
        //{
        //    return AgentManager.Queue.Enqueue(message);
        //}

        #endregion

    }

    public class PipeServerDequeue : PipeServerBase
    {

        #region ctor

        /// <summary>
        /// Constractor default.
        /// </summary>
         public PipeServerDequeue()
            : base(QueueDefaults.DequeuePipeName, true)
        {

        }

        /// <summary>
        /// Constractor with settings
        /// </summary>
        /// <param name="settings"></param>
        protected PipeServerDequeue(PipeQueueSettings settings)
            : base(settings)
        {

        }
        #endregion

        #region override

        protected override void OnStart()
        {
            base.OnStart();
            //AgentManager.Start();//McLock.Lock.ValidateSystemKey());
        }

        protected override void OnStop()
        {
            base.OnStop();

            //AgentManager.Stop();
        }

        protected override void OnLoad()
        {
            base.OnLoad();
        }

        #endregion

        #region exec methods

        //protected override QueueRequest ReadRequest(NamedPipeServerStream stream)
        //{
        //    QueueRequest message = new QueueRequest();
        //    message.EntityRead(stream, null);
        //    return message;
        //}

        //protected override NetStream ExecRequset(QueueRequest message)
        //{
        //    IQueueItem item= AgentManager.Queue.ExecGet(message);
        //    if (item == null)
        //    {
        //        return null;
        //    }
        //    return item.BodyStream;
        //}

        

        #endregion

    }

    public class PipeServerManager : PipeServerBase
    {

        #region ctor

        /// <summary>
        /// Constractor default.
        /// </summary>
        public PipeServerManager()
            : base(QueueDefaults.QueueManagerPipeName, true)
        {
 

        }

        /// <summary>
        /// Constractor with settings
        /// </summary>
        /// <param name="settings"></param>
        protected PipeServerManager(PipeQueueSettings settings)
            : base(settings)
        {

        }
        #endregion

        #region override

        protected override void OnStart()
        {
            base.OnStart();
            //AgentManager.Start();//McLock.Lock.ValidateSystemKey());
        }

        protected override void OnStop()
        {
            base.OnStop();

            //AgentManager.Stop();
        }

        protected override void OnLoad()
        {
            base.OnLoad();
        }

        #endregion

        #region exec methods

        
        //protected override QueueRequest ReadRequest(NamedPipeServerStream stream)
        //{
        //    QueueRequest message = new QueueRequest();
        //    message.EntityRead(stream, null);
        //    return message;
        //}

        //protected override NetStream ExecRequset(QueueRequest message)
        //{
        //    return AgentManager.Queue.ExecRequset(message);
        //}

        #endregion

    }
}
