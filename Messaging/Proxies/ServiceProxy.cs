using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Nistec.Messaging;
using System.IO;

namespace Nistec.Messaging.Proxies
{
    public interface IServiceProxy
    {
        MessageState SendItem(Stream item);
    }

    public class DynamicProxy<TChannel> : ServiceProxy<TChannel, QueueItem> where TChannel : IServiceProxy
    {
        public DynamicProxy(string endpointUrl)
            : base(endpointUrl)
        {

        }
        protected override MessageState Send(QueueItem item)
        {
            return Proxy.SendItem(item.BodyStream);
        }
    }

    public abstract class ServiceProxy<TChannel,TMsg> : IDisposable
    {

        public string EndpointUrl { get; set; }

        public ServiceProxy(string endpointUrl)
        {

            EndpointUrl = endpointUrl;
        }

        //public ServiceProxy()
        //{

        //    EndpointUrl = "MailerService";
        //}

        public void Dispose()
        {
            Close();
        }

        ChannelFactory<TChannel> factory = null;
        TChannel m_proxy;

        protected TChannel Proxy
        {
            get
            {
                if (m_proxy == null)
                {
                    factory = new ChannelFactory<TChannel>(EndpointUrl);
                    m_proxy = factory.CreateChannel();//endpointAddress);                }
                }
                return m_proxy;
            }
        }

        public void Close()
        {
            if (m_proxy != null)
            {
                ((IClientChannel)m_proxy).Close();
                m_proxy = default(TChannel);
            }
            if (factory != null)
            {
                factory.Close();
                //factory.Abort();
                factory = null;
            }
        }

        protected abstract MessageState Send(TMsg item);


        public MessageState Invoke(TMsg item, bool closeOnFinished = false)
        {

            TChannel service = default(TChannel);
            MessageState State = MessageState.None;

            try
            {
                service = Proxy;
                if (service == null)
                {
                    throw new CommunicationException(
                       String.Format("Unable to connect to service at {0}", EndpointUrl));
                }

                State = Send(item);
                
                return State;
            }
            catch (ObjectDisposedException dex)
            {
                closeOnFinished = true;
                throw new MessageException(MessageState.RemoteConnectionError, "ServiceProxy ObjectDisposedException:" + dex.Message);
                //return MessageState.RemoteConnectionError;
            }

            catch (FaultException fex)
            {
                closeOnFinished = true;
                throw new MessageException(MessageState.RemoteConnectionError, "ServiceProxy FaultException:" + fex.Message);
                //return MessageState.RemoteConnectionError;
            }

            catch (CommunicationException cex)
            {
                closeOnFinished = true;
                throw new MessageException(MessageState.RemoteConnectionError, "ServiceProxy CommunicationException:" + cex.Message);

                //return MessageState.RemoteConnectionError;
            }
            catch (Exception ex)
            {
                throw new  MessageException(MessageState.UnExpectedError, "ServiceProxy Exception:" + ex.Message);
                //return MessageState.UnExpectedError;
            }
            finally
            {
                if (closeOnFinished)
                {
                    Close();
                }
            }
        }
    }

}
