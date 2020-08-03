//licHeader
//===============================================================================================================
// System  : Nistec.Cache - Nistec.Cache Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of cache core.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Diagnostics;
using System.Collections;
using Nistec.Runtime;
using Nistec.Data.Entities;
using Nistec.Generic;
using Nistec.IO;
using Nistec.Channels;
using System.Net.Sockets;
using Nistec.Serialization;
using Nistec.Channels.Http;
using System.Net;


namespace Nistec.Messaging
{
    /// <summary>
    /// Represent a cache message for pipe communications.
    /// </summary>
    [Serializable]
    public class QItemStream : MessageStream, IQueueItem//, ICloneable
    {

        #region ctor
        /// <summary>
        /// Initialize a new instance of cache message.
        /// </summary>
        public QItemStream() : base() { Formatter = MessageStream.DefaultFormatter; }
        /// <summary>
        /// Initialize a new instance of cache message.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        public QItemStream(string command, string key, object value, int expiration)
            : this()
        {
            Command = command;
            Key = key;
            Expiration = expiration;
            SetBody(value);
        }
        /// <summary>
        /// Initialize a new instance of cache message.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <param name="sessionId"></param>
        public QItemStream(string command, string key, object value, int expiration, string sessionId)
            : this()
        {
            Command = command;
            Key = key;
            Expiration = expiration;
            Id = sessionId;
            SetBody(value);
        }

        #endregion   

        #region Read/Write pipe

        internal object ReadResponse(NamedPipeClientStream stream, Type type, int InBufferSize = 8192)
        {
          

            using (AckStream ack = AckStream.Read(stream,type, InBufferSize))
            {
                if ((int)ack.State > (int)MessageState.Ok)
                {
                    throw new Exception(ack.Message);
                }
                return ack.Value;
            }
        }

        internal TResponse ReadResponse<TResponse>(NamedPipeClientStream stream, int InBufferSize = 8192)
        {

            using (AckStream ack = AckStream.Read(stream, typeof(TResponse), InBufferSize))
            {
                if ((int)ack.State > (int)MessageState.Ok)
                {
                    throw new Exception(ack.Message);
                }
                return ack.GetValue<TResponse>();
            }
        }

        internal static QItemStream ReadRequest(NamedPipeServerStream pipeServer, int InBufferSize = 8192)
        {
            QItemStream message = new QItemStream();
            message.EntityRead(pipeServer, null);
            return message;
        }

        internal static void WriteResponse(NamedPipeServerStream pipeServer, NetStream bResponse)
        {
            if (bResponse == null)
            {
                return;
            }

            int cbResponse = bResponse.iLength;

            pipeServer.Write(bResponse.ToArray(), 0, cbResponse);

            pipeServer.Flush();

        }

        #endregion

        #region Read/Write tcp

        internal static NetStream FaultStream(string faultDescription)
        {
            var message = new QItemStream("Fault", "Fault", faultDescription, 0);
            return message.Serialize();
        }

        internal static QItemStream ReadRequest(NetworkStream streamServer, int InBufferSize = 8192)
        {
            var message = new QItemStream();
            message.EntityRead(streamServer, null);
            return message;
        }

        internal static void WriteResponse(NetworkStream streamServer, NetStream bResponse)
        {
            if (bResponse == null)
            {
                return;
            }

            int cbResponse = bResponse.iLength;

            streamServer.Write(bResponse.ToArray(), 0, cbResponse);

            streamServer.Flush();

        }


        #endregion

        #region Read/Write http

        internal static QItemStream ReadRequest(HttpRequestInfo request)
        {
            var message = new QItemStream();
            message.EntityRead(request.Body, null);
            return message;
        }

        internal static void WriteResponse(HttpListenerContext context, NetStream bResponse)
        {
            var response = context.Response;
            if (bResponse == null)
            {
                response.StatusCode = (int)HttpStatusCode.NoContent;
                response.StatusDescription = "No response";
                return;
            }

            int cbResponse = bResponse.iLength;
            byte[] buffer = bResponse.ToArray();

            

            response.StatusCode = (int)HttpStatusCode.OK;
            response.StatusDescription = HttpStatusCode.OK.ToString();
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();

        }


        #endregion

        #region extension

        internal static string[] SplitArg(IKeyValue dic, string key, string valueIfNull)
        {
            string val = dic.Get<string>(key, valueIfNull);
            if (val == null)
                return valueIfNull == null ? null : new string[] { valueIfNull };
            return val.SplitTrim('|');
        }

       
        internal static TimeSpan TimeArg(IKeyValue dic, string key, string valueIfNull)
        {
            string val = dic.Get<string>(key, valueIfNull);
            TimeSpan time = string.IsNullOrEmpty(val) ? TimeSpan.Zero : TimeSpan.Parse(val);
            return time;
        }

        internal static string[] SplitArg(IDictionary dic, string key, string valueIfNull)
        {
            string val = dic.Get<string>(key, valueIfNull);
            if (val == null)
                return valueIfNull == null ? null : new string[] { valueIfNull };
            return val.SplitTrim('|');
        }

        internal static TimeSpan TimeArg(IDictionary dic, string key, string valueIfNull)
        {
            string val = dic.Get<string>(key, valueIfNull);
            TimeSpan time = string.IsNullOrEmpty(val) ? TimeSpan.Zero : TimeSpan.Parse(val);
            return time;
        }

        /// <summary>
        /// Convert <see cref="IDictionary"/> to <see cref="MessageStream"/>.
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static MessageStream ConvertFrom(IDictionary dict)
        {
            QItemStream message = new QItemStream()
            {
                Command = dict.Get<string>("Command"),
                Key = dict.Get<string>("Key"),
                Args = dict.Get<GenericNameValue>("Args"),
                BodyStream = dict.Get<NetStream>("Body", null),//, ConvertDescriptor.Implicit),
                Expiration = dict.Get<int>("Expiration", 0),
                IsDuplex = dict.Get<bool>("IsDuplex", true),
                Modified = dict.Get<DateTime>("Modified", DateTime.Now),
                TypeName = dict.Get<string>("TypeName"),
                Id = dict.Get<string>("Id")
            };

            return message;
        }
        /// <summary>
        /// Convert stream to json.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToJson(NetStream stream, JsonFormat format)
        {
            using (BinaryStreamer streamer = new BinaryStreamer(stream))
            {
                var obj = streamer.Decode();
                if (obj == null)
                    return null;
                else
                    return JsonSerializer.Serialize(obj, null, format);
            }
        }
        #endregion

        #region ReadAck tcp

        /// <summary>
        /// Read response from server.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="readTimeout"></param>
        /// <param name="InBufferSize"></param>
        public object ReadAck(NetworkStream stream, int readTimeout, int InBufferSize)
        {
            using (AckStream ack = AckStream.Read(stream,typeof(object), readTimeout, InBufferSize))
            {
                if ((int)ack.State > (int)MessageState.Ok)
                {
                    throw new MessageException(ack);
                }
                return ack.Value;
            }
        }

        /// <summary>
        /// Read response from server.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <param name="readTimeout"></param>
        /// <param name="InBufferSize"></param>
        /// <returns></returns>
        public object ReadAck(NetworkStream stream, Type type, int readTimeout, int InBufferSize)
        {

            using (AckStream ack = AckStream.Read(stream,type, readTimeout, InBufferSize))
            {
                if ((int)ack.State > (int)MessageState.Ok)
                {
                    throw new MessageException(ack);
                }
                return ack.Value;
            }
        }

        /// <summary>
        /// Read response from server.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="stream"></param>
        /// <param name="readTimeout"></param>
        /// <param name="InBufferSize"></param>
        /// <returns></returns>
        public TResponse ReadAck<TResponse>(NetworkStream stream, int readTimeout, int InBufferSize)
        {

            using (AckStream ack = AckStream.Read(stream, typeof(TResponse), readTimeout, InBufferSize))
            {
                if ((int)ack.State > (int)MessageState.Ok)
                {
                    throw new MessageException(ack);
                }
                return ack.GetValue<TResponse>();
            }
        }

        #endregion

        #region ReadAck pipe

        public object ReadAck(NamedPipeClientStream stream, Type type, int InBufferSize = 8192)
        {

            using (AckStream ack = AckStream.Read(stream, type, InBufferSize))
            {
                if ((int)ack.State > (int)MessageState.Ok)
                {
                    throw new Exception(ack.Message);
                }
                return ack.Value;
            }
        }

        public TResponse ReadAck<TResponse>(NamedPipeClientStream stream, int InBufferSize = 8192)
        {

            using (AckStream ack = AckStream.Read(stream, typeof(TResponse), InBufferSize))
            {
                if ((int)ack.State > (int)MessageState.Ok)
                {
                    throw new Exception(ack.Message);
                }
                return ack.GetValue<TResponse>();
            }
        }

 
        #endregion

    }

}
