using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using System.Threading;
using System.DirectoryServices.Protocols;

using MControl.Messaging.Net;
using MControl.Messaging.Net.IO;
using MControl.Messaging.Net.Dns;
using MControl.Messaging.Net.Auth;
using MControl.Messaging.Net.Tcp;
using MControl.Messaging.Net.MQ;

namespace MControl.QueueServer
{
    /// <summary>
    /// Implements mail server virtual server.
    /// </summary>
    public class VirtualServer
    {
        private Server              m_OwnerServer       = null;
        private string              m_ID                 = "";
        private string              m_Name               = "";
        private string              m_ApiInitString      = "";
        private IMailServerApi      m_Api               = null;
        private bool                m_Running            = false;
        private Dns_Client          m_DnsClient         = null;
        private Smtp_Server         m_Smtp_Server        = null;
        private Pop3_Server         m_Pop3Server        = null;
        private Imap_Server         m_Imap_Server        = null;
        private RelayServer         m_RelayServer       = null;
        //private SIP_Proxy           m_SipServer         = null;
        private FetchPop3           m_FetchServer       = null;
        private RecycleBinManager   m_RecycleBinManager = null;
        private BadLoginManager     m_BadLoginManager   = null;
        private System.Timers.Timer m_Timer             = null;
        // Settings
        private DateTime                     m_SettingsDate;
        private string                       m_MailStorePath      = "";
        private MailServerAuthType_enum      m_AuthType           = MailServerAuthType_enum.Integrated;
        private string                       m_Auth_Win_Domain    = "";
        private string                       m_Auth_LDAP_Server   = "";
        private string                       m_Auth_LDAP_DN       = "";
        private bool                         m_SmtpRequireAuth   = false;
        private string                       m_SmtpDefaultDomain = "";        
        private string                       m_Server_LogPath     = "";
		private string                       m_SmtpLogPath       = "";
		private string                       m_Pop3_LogPath       = "";
		private string                       m_ImapLogPath       = "";
        private string                       m_Relay_LogPath      = "";
		private string                       m_Fetch_LogPath      = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="server">Server what owns this virtual server.</param>
        /// <param name="id">Virtual server ID.</param>
        /// <param name="name">Virtual server name.</param>
        /// <param name="apiInitString">Virtual server api initi string.</param>
        /// <param name="api">Virtual server API.</param>
        public VirtualServer(Server server,string id,string name,string apiInitString,IMailServerApi api)
        {
            m_OwnerServer  = server;
            m_ID            = id;
            m_Name          = name;
            m_ApiInitString = apiInitString;
            m_Api          = api;
        }


        #region Events Handling

        #region Smtp Events

        #region method m_Smtp_Server_SessionCreated

        /// <summary>
        /// Is called when new Smtp server session has created.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_Smtp_Server_SessionCreated(object sender, TcpServerTaskEventArgs<MqTask> e)
        {
            e.Task.Started += new EventHandler<MqStartedEventArgs>(m_Smtp_Server_Session_Started);
            e.Task.GetMessageStream += new EventHandler<MqMessageEventArgs>(m_Smtp_Server_Session_GetMessageStream);
            e.Task.MessageStoringCanceled += new EventHandler(m_Smtp_Server_Session_MessageStoringCanceled);
            e.Task.MessageStoringCompleted += new EventHandler<MqMessageStoredEventArgs>(m_Smtp_Server_Session_MessageStoringCompleted);

            // Add session supported authentications.
            if (m_AuthType == MailServerAuthType_enum.Windows || m_AuthType == MailServerAuthType_enum.Ldap)
            {
                // For windows or LDAP auth, we can allow only plain text authentications, because otherwise 
                // we can't do auth against windows (it requires user name and password).

                #region PLAIN

                Auth_SASL_ServerMechanism_Plain auth_plain = new Auth_SASL_ServerMechanism_Plain(false);
                auth_plain.Authenticate += new EventHandler<Auth_e_Authenticate>(delegate(object s, Auth_e_Authenticate e1)
                {
                    try
                    {
                        e1.IsAuthenticated = Authenticate(e.Session.RemoteEndPoint.Address, e1.UserName, e1.Password);
                    }
                    catch (Exception x)
                    {
                        OnError(x);
                        e1.IsAuthenticated = false;
                    }
                });
                e.Session.Authentications.Add(auth_plain.Name, auth_plain);

                #endregion

                #region LOGIN

                Auth_SASL_ServerMechanism_Login auth_login = new Auth_SASL_ServerMechanism_Login(false);
                auth_login.Authenticate += new EventHandler<Auth_e_Authenticate>(delegate(object s, Auth_e_Authenticate e1)
                {
                    try
                    {
                        e1.IsAuthenticated = Authenticate(e.Session.RemoteEndPoint.Address, e1.UserName, e1.Password);
                    }
                    catch (Exception x)
                    {
                        OnError(x);
                        e1.IsAuthenticated = false;
                    }
                });
                e.Session.Authentications.Add(auth_login.Name, auth_login);

                #endregion
            }
            else
            {
                #region DIGEST-MD5

                Auth_SASL_ServerMechanism_DigestMd5 auth_digestmMd5 = new Auth_SASL_ServerMechanism_DigestMd5(false);
                auth_digestmMd5.Realm = e.Session.LocalHostName;
                auth_digestmMd5.GetUserInfo += new EventHandler<Auth_e_UserInfo>(delegate(object s, Auth_e_UserInfo e1)
                {

                    FillUserInfo(e1);
                });
                e.Session.Authentications.Add(auth_digestmMd5.Name, auth_digestmMd5);

                #endregion

                #region CRAM-MD5

                Auth_SASL_ServerMechanism_CramMd5 auth_cramMd5 = new Auth_SASL_ServerMechanism_CramMd5(false);
                auth_cramMd5.GetUserInfo += new EventHandler<Auth_e_UserInfo>(delegate(object s, Auth_e_UserInfo e1)
                {
                    FillUserInfo(e1);
                });
                e.Session.Authentications.Add(auth_cramMd5.Name, auth_cramMd5);

                #endregion

                #region PLAIN

                Auth_SASL_ServerMechanism_Plain auth_plain = new Auth_SASL_ServerMechanism_Plain(false);
                auth_plain.Authenticate += new EventHandler<Auth_e_Authenticate>(delegate(object s, Auth_e_Authenticate e1)
                {
                    try
                    {
                        e1.IsAuthenticated = Authenticate(e.Session.RemoteEndPoint.Address, e1.UserName, e1.Password);
                    }
                    catch (Exception x)
                    {
                        OnError(x);
                        e1.IsAuthenticated = false;
                    }
                });
                e.Session.Authentications.Add(auth_plain.Name, auth_plain);

                #endregion

                #region LOGIN

                Auth_SASL_ServerMechanism_Login auth_login = new Auth_SASL_ServerMechanism_Login(false);
                auth_login.Authenticate += new EventHandler<Auth_e_Authenticate>(delegate(object s, Auth_e_Authenticate e1)
                {
                    try
                    {
                        e1.IsAuthenticated = Authenticate(e.Session.RemoteEndPoint.Address, e1.UserName, e1.Password);
                    }
                    catch (Exception x)
                    {
                        OnError(x);
                        e1.IsAuthenticated = false;
                    }
                });
                e.Session.Authentications.Add(auth_login.Name, auth_login);

                #endregion
            }
        }
                                                
        #endregion

        #region method m_Smtp_Server_Session_Started

        /// <summary>
        /// Is called when Smtp server sessions starts session processing.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_Smtp_Server_Session_Started(object sender,MqStarted e)
        {
            if(!IsAccessAllowed(Service_enum.Smtp,e.Session.RemoteEndPoint.Address)){
                e.Reply = new Smtp_Reply(554,"Your IP address is blocked.");
            }
        }

        #endregion

        #region method m_Smtp_Server_Session_Ehlo

        /// <summary>
        /// Is called when Smtp server session gets EHLO/HELO command.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_Smtp_Server_Session_Ehlo(object sender,MqEhlo e)
        {
            
        }

        #endregion

        #region method m_Smtp_Server_Session_MailFrom

        /// <summary>
        /// Is called when Smtp server session gets MAIL command.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_Smtp_Server_Session_MailFrom(object sender, MqMailFrom e)
        {
            if (m_SmtpRequireAuth && !e.Session.IsAuthenticated)
            {
                e.Reply = new Smtp_Reply(530, "5.7.0  Authentication required.");
                return;
            }

            // Block blank domain(user@) email addresses.
            if (e.MailFrom.Mailbox.IndexOf('@') != -1 && e.MailFrom.Mailbox.Substring(e.MailFrom.Mailbox.IndexOf('@') + 1).Length < 1)
            {
                e.Reply = new Smtp_Reply(501, "MAIL FROM: address(" + e.MailFrom + ") domain name must be specified.");
                return;
            }

            try
            {
                //--- Filter sender -----------------------------------------------------//					
                DataView dvFilters = m_Api.GetFilters();
                dvFilters.RowFilter = "Enabled=true AND Type='ISmtpSenderFilter'";
                dvFilters.Sort = "Cost";
                foreach (DataRowView drViewFilter in dvFilters)
                {
                    string assemblyFile = drViewFilter.Row["Assembly"].ToString();
                    // File is without path probably, try to load it from filters folder
                    if (!File.Exists(assemblyFile))
                    {
                        assemblyFile = m_OwnerServer.StartupPath + "\\Filters\\" + assemblyFile;
                    }

                    Assembly ass = Assembly.LoadFrom(assemblyFile);
                    Type tp = ass.GetType(drViewFilter.Row["ClassName"].ToString());
                    object filterInstance = Activator.CreateInstance(tp);
                    ISmtpSenderFilter filter = (ISmtpSenderFilter)filterInstance;

                    string error = null;
                    if (!filter.Filter(e.MailFrom.Mailbox, m_Api, e.Session, out error))
                    {
                        if (error != null)
                        {
                            e.Reply = new Smtp_Reply(550, error);
                        }
                        else
                        {
                            e.Reply = new Smtp_Reply(550, "Sender rejected.");
                        }

                        return;
                    }
                }
                //----------------------------------------------------------------------//
            }
            catch (Exception x)
            {
                e.Reply = new Smtp_Reply(500, "Internal server error.");
                Error.DumpError(this.Name, x,false, new System.Diagnostics.StackTrace());
            }
        }

        #endregion

        #region method m_Smtp_Server_Session_RcptTo

        /// <summary>
        /// Is called when Smtp server session gets RCPT command.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_Smtp_Server_Session_RcptTo(object sender, MqRcptTo e)
        {
            try
            {
                string mailTo = e.RcptTo.Mailbox;

                // If domain isn't specified, add default domain
                if (mailTo.IndexOf("@") == -1)
                {
                    mailTo += "@" + m_SmtpDefaultDomain;
                }

                //1) is local domain or relay needed
                //2) can map email address to mailbox
                //3) is alias
                //4) if matches any routing pattern

                // check if e-domain is local
                if (m_Api.DomainExists(mailTo))
                {
                    string user = m_Api.MapUser(mailTo);
                    if (user == null)
                    {
                        e.Reply = new Smtp_Reply(550, "No such user here.");

                        // Check if mailing list.
                        if (m_Api.MailingListExists(mailTo))
                        {
                            // Not authenticated, see is anyone access allowed
                            if (!e.Session.IsAuthenticated)
                            {
                                if (m_Api.CanAccessMailingList(mailTo, "anyone"))
                                {
                                    e.Reply = new Smtp_Reply(250, "OK.");
                                }
                            }
                            // Authenticated, see if user has access granted
                            else
                            {
                                if (m_Api.CanAccessMailingList(mailTo, e.Session.AuthenticatedUserIdentity.Name))
                                {
                                    e.Reply = new Smtp_Reply(250, "OK.");
                                }
                            }
                        }
                        // At last check if matches any routing pattern.
                        else
                        {
                            DataView dv = m_Api.GetRoutes();
                            foreach (DataRowView drV in dv)
                            {
                                // We have matching route
                                if (Convert.ToBoolean(drV["Enabled"]) && SCore.IsAstericMatch(drV["Pattern"].ToString(), mailTo))
                                {
                                    e.Reply = new Smtp_Reply(250, "OK.");
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Validate mailbox size
                        if (m_Api.ValidateMailboxSize(user))
                        {
                            e.Reply = new Smtp_Reply(552, "Requested mail action aborted: Mailbox <" + e.RcptTo.Mailbox + "> is full.");
                        }
                        else
                        {
                            e.Reply = new Smtp_Reply(250, "OK.");
                        }
                    }
                }
                // Foreign recipient.
                else
                {
                    e.Reply = new Smtp_Reply(550, "Relay not allowed.");

                    // This isn't domain what we want.
                    // 1)If user Authenticated, check if relay is allowed for this user.
                    // 2)Check if relay is allowed for this ip.
                    if (e.Session.IsAuthenticated)
                    {
                        if (IsRelayAllowed(e.Session.AuthenticatedUserIdentity.Name, e.Session.RemoteEndPoint.Address))
                        {
                            e.Reply = new Smtp_Reply(250, "User not local will relay.");
                        }
                    }
                    else if (IsRelayAllowed("", e.Session.RemoteEndPoint.Address))
                    {
                        e.Reply = new Smtp_Reply(250, "User not local will relay.");
                    }
                }
            }
            catch (Exception x)
            {
                e.Reply = new Smtp_Reply(500, "Internal server error.");
                Error.DumpError(this.Name, x, false, new System.Diagnostics.StackTrace());
            }
        }

        #endregion

        #region method m_Smtp_Server_Session_GetMessageStream

        /// <summary>
        /// Is raised when Smtp server session needs to get stream where to store incoming message.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_Smtp_Server_Session_GetMessageStream(object sender,MqMessage e)
        {
            if(!Directory.Exists(m_MailStorePath + "IncomingSMTP")){
                Directory.CreateDirectory(m_MailStorePath + "IncomingSMTP");
            }
            
            e.Stream = new FileStream(API_Utlis.PathFix(m_MailStorePath + "IncomingSMTP\\" + Guid.NewGuid().ToString().Replace("-","") + ".eml"),FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite,32000,FileOptions.DeleteOnClose); 
            e.Session.Tags["MessageStream"] = e.Stream;
        }

        #endregion

        #region method m_Smtp_Server_Session_MessageStoringCanceled

        /// <summary>
        /// Is called when message storing has canceled.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_Smtp_Server_Session_MessageStoringCanceled(object sender,EventArgs e)
        {
            try{
                // Close file. .NET will delete that file we use FileOptions.DeleteOnClose.
                ((IDisposable)((MqTask)sender).Tags["MessageStream"]).Dispose();
            }
            catch{
                // We don't care about errors here.
            }            
        }

        #endregion

        #region method m_Smtp_Server_Session_MessageStoringCompleted

        /// <summary>
        /// Is called when Smtp server has completed message storing.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_Smtp_Server_Session_MessageStoringCompleted(object sender,MqMessageStored e)
        {
            try{
                e.Stream.Position = 0;

                ProcessAndStoreMessage(e.Session.From.ENVID,e.Session.From.Mailbox,e.Session.From.RET,e.Session.To,e.Stream,e);
            }
            catch(Exception x){
                Error.DumpError(this.Name,x);

                e.Reply = new Smtp_Reply(552,"Requested mail action aborted: Internal server error.");
            }
            finally{
                // Close file. .NET will delete that file we use FileOptions.DeleteOnClose.
                ((FileStream)e.Stream).Dispose();
            }
        }

        #endregion
                        

		#region method Smtp_Server_SessionLog

        private void Smtp_Server_SessionLog(object sender,MControl.Messaging.Net.Log.WriteLogEventArgs e)
        {
            Logger.WriteLog(m_SmtpLogPath + "smtp-" + DateTime.Today.ToString("yyyyMMdd") + ".log",e.LogEntry);
        }		

		#endregion

		#endregion		

      
        #region Common Events

		#region method OnServer_SysError

		private void OnServer_SysError(object sender,MControl.Messaging.Net.Error_EventArgs e)
		{
			OnError(e.Exception);
		}

		#endregion

		#endregion



        #region method m_Timer_Elapsed

        private void m_Timer_Elapsed(object sender,System.Timers.ElapsedEventArgs e)
        {
            try{
				LoadSettings();				
			}
			catch(Exception x){
                OnError(x);
			}
        }

        #endregion

        #endregion


        #region method Start

        /// <summary>
        /// Starts this virtual server.
        /// </summary>
        public void Start()
        {
            if(m_Running){
                return;
            }
            m_Running = true;

            m_DnsClient = new Dns_Client();

            m_Smtp_Server = new Smtp_Server();
            m_Smtp_Server.Error += new MControl.Messaging.Net.ErrorEventHandler(OnServer_SysError);
            m_Smtp_Server.SessionCreated += new EventHandler<TcpServerTaskEventArgs<MqTask>>(m_Smtp_Server_SessionCreated);

            m_Pop3Server = new Pop3_Server();            
            m_Pop3Server.Error += new MControl.Messaging.Net.ErrorEventHandler(OnServer_SysError);            
            m_Pop3Server.SessionCreated += new EventHandler<TcpServerTaskEventArgs<Pop3_Session>>(m_Pop3Server_SessionCreated);
                        
            m_Imap_Server = new Imap_Server();
            m_Imap_Server.Error += new MControl.Messaging.Net.ErrorEventHandler(OnServer_SysError);
            m_Imap_Server.SessionCreated += new EventHandler<TcpServerTaskEventArgs<Imap_Session>>(m_Imap_Server_SessionCreated);
            
            m_RelayServer = new RelayServer(this);
            m_RelayServer.DnsClient = m_DnsClient;

            m_FetchServer = new FetchPop3(this,m_Api);

            //m_SipServer = new SIP_Proxy(new SIP_Stack());
            //m_SipServer.Authenticate += new SIP_AuthenticateEventHandler(m_SipServer_Authenticate);
            //m_SipServer.IsLocalUri += new SIP_IsLocalUriEventHandler(m_SipServer_IsLocalUri);
            //m_SipServer.AddressExists += new SIP_AddressExistsEventHandler(m_SipServer_AddressExists);
            //m_SipServer.Registrar.CanRegister += new SIP_CanRegisterEventHandler(m_SipServer_CanRegister);
            //m_SipServer.Stack.Error += new EventHandler<ExceptionEventArgs>(m_SipServer_Error);
            
            m_RecycleBinManager = new RecycleBinManager(m_Api);
 
            m_BadLoginManager = new BadLoginManager();

            m_Timer = new System.Timers.Timer();            
            m_Timer.Interval = 15000;
            m_Timer.Elapsed += new System.Timers.ElapsedEventHandler(m_Timer_Elapsed);
            m_Timer.Enabled = true;

            LoadSettings();
        }
                                                                                                                                                        
        #endregion

        #region method Stop

        /// <summary>
        /// Stops this virtual server.
        /// </summary>
        public void Stop()
        {
            m_Running = false;

            if(m_DnsClient != null){
                m_DnsClient.Dispose();
                m_DnsClient = null;
            }
            if(m_Smtp_Server != null){
                try{
                    m_Smtp_Server.Dispose();
                }
                catch{
                }
                m_Smtp_Server = null;
            }
            if(m_Pop3Server != null){
                try{
                    m_Pop3Server.Dispose();
                }
                catch{
                }
                m_Pop3Server = null;
            }
            if(m_Imap_Server != null){
                try{
                    m_Imap_Server.Dispose();
                }
                catch{
                }
                m_Imap_Server = null;
            }
            if(m_RelayServer != null){
                try{
                    m_RelayServer.Dispose();
                }
                catch{
                }
                m_RelayServer = null;
            }
            if(m_FetchServer != null){
                try{
                    m_FetchServer.Dispose();                    
                }
                catch{
                }
                m_FetchServer = null;
            } 
            //if(m_SipServer != null){
            //    try{
            //        m_SipServer.Stack.Stop();                    
            //    }
            //    catch{
            //    }
            //    m_SipServer = null;
            //} 
            if(m_Timer != null){
                try{
                    m_Timer.Dispose();
                }
                catch{
                }
                m_Timer = null;
            }
            if(m_RecycleBinManager != null){
                try{
                    m_RecycleBinManager.Dispose();
                }
                catch{
                }
                m_RecycleBinManager = null;
            }
            if(m_BadLoginManager != null){
                try{
                    m_BadLoginManager.Dispose();
                }
                catch{
                }
                m_BadLoginManager = null;
            }

            //mcontrol
            RelayPool.TrimRelayPool(m_Name);
            //SocketPool.CloseActivePool();
            NetLog.DebugFormat("VirtualServer stoped :{0}", m_Name);
        }

        #endregion


        #region method Authenticate

        /// <summary>
        /// Authenticates specified user.
        /// </summary>
        /// <param name="ip">IP address of remote computer.</param>
        /// <param name="userName">User name.</param>
        /// <param name="password">Password.</param>
        /// <returns>Returns true if user authenticated sucessfully, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> or <b>userName</b> is null reference.</exception>
        public bool Authenticate(IPAddress ip,string userName,string password)
        {
            if(ip == null){
                throw new ArgumentNullException("ip");
            }
            if(userName == null){
                throw new ArgumentNullException("userName");
            }
          
            try{ 
                // See if too many bad logins for specified IP and user if so block auth.
                if(m_BadLoginManager.IsExceeded(ip.ToString(),userName)){
                    return false;
                }

                bool validated = false;            
                // Integrated auth
                if(m_AuthType == MailServerAuthType_enum.Integrated){
                    foreach(DataRowView dr in m_Api.GetUsers("ALL")){
                        if(userName.ToLowerInvariant() == dr["UserName"].ToString().ToLowerInvariant()){
                            if(password == dr["Password"].ToString()){
                                validated = true;
                            }
                            break;
                        }
                    }
                }
                // Windows auth
                else if(m_AuthType == MailServerAuthType_enum.Windows){    
                    if(m_Api.UserExists(userName)){
                        validated = WinLogon.Logon(m_Auth_Win_Domain,userName,password);
                    }
                }
                // LDAP auth
                else if(m_AuthType == MailServerAuthType_enum.Ldap){
                    try{
                        string dn = m_Auth_LDAP_DN.Replace("%user",userName);

                        using(LdapConnection ldap = new LdapConnection(new LdapDirectoryIdentifier(m_Auth_LDAP_Server),new System.Net.NetworkCredential(dn,password),System.DirectoryServices.Protocols.AuthType.Basic)){
                            ldap.SessionOptions.ProtocolVersion = 3;
                            ldap.Bind();
                        }

                        validated = true;
                    }
                    catch{
                    }
                }
                                               						
			    // Increase bad login info
			    if(!validated){
                    m_BadLoginManager.Put(ip.ToString(),userName);
			    }

                // Update user last login
                if(validated){
                    m_Api.UpdateUserLastLoginTime(userName);
                }

			    return validated;
            }
            catch(Exception x){
                OnError(x);
                return false;
            }
        }

        #endregion


        #region method LoadSettings

        private void LoadSettings()
		{
			try{
				lock(this){
					DataRow dr = m_Api.GetSettings();

                    // See if settings changed. Skip loading if steeings has not changed.
                    if(Convert.ToDateTime(dr["SettingsDate"]).Equals(m_SettingsDate)){
                        return;
                    }
                    m_SettingsDate = Convert.ToDateTime(dr["SettingsDate"]);
                                  
                    //--- Try to get mailstore path from API init string ----------------------------------//
                    m_MailStorePath = "Settings\\MailStore";
                    // mailstorepath=
			        string[] parameters = m_ApiInitString.Replace("\r\n","\n").Split('\n');
			        foreach(string param in parameters){
                        if(param.ToLower().IndexOf("mailstorepath=") > -1){
					        m_MailStorePath = param.Substring(14);
                        }
			        }
                    // Fix mail store path, if isn't ending with \
			        if(m_MailStorePath.Length > 0 && !m_MailStorePath.EndsWith("\\")){
				        m_MailStorePath += "\\"; 
			        }
                    if(!Path.IsPathRooted(m_MailStorePath)){
				        m_MailStorePath = m_OwnerServer.StartupPath + m_MailStorePath;
			        }
                    // Make path directory separator to suit for current platform
                    m_MailStorePath = API_Utlis.PathFix(m_MailStorePath);                    
                    //------------------------------------------------------------------------------------//

                    //--- System settings -----------------------------------------------------------------//
                    m_AuthType         = (MailServerAuthType_enum)ConvertEx.ToInt32(dr["ServerAuthenticationType"]);
                    m_Auth_Win_Domain  = ConvertEx.ToString(dr["ServerAuthWinDomain"]);
                    m_Auth_LDAP_Server = ConvertEx.ToString(dr["LdapServer"]);
                    m_Auth_LDAP_DN     = ConvertEx.ToString(dr["LdapDN"]);
                    //-------------------------------------------------------------------------------------//

                    #region General

                    List<string> dnsServers = new List<string>();
                    foreach(DataRow drX in dr.Table.DataSet.Tables["DnsServers"].Rows){
                        dnsServers.Add(drX["IP"].ToString());
                    }
                    MControl.Messaging.Net.Dns.Client.Dns_Client.DnsServers = dnsServers.ToArray();

                    #endregion

                    #region Smtp

                    //------- Smtp Settings ---------------------------------------------//
                    try{
                        List<IPBindInfo> smtpIpBinds = new List<IPBindInfo>();
                        foreach(DataRow dr_Bind in dr.Table.DataSet.Tables["SmtpBindings"].Rows){
                            smtpIpBinds.Add(new IPBindInfo(
                                ConvertEx.ToString(dr_Bind["HostName"]),
                                IPAddress.Parse(ConvertEx.ToString(dr_Bind["IP"])),
                                ConvertEx.ToInt32(dr_Bind["Port"]),
                                ParseSslMode(dr_Bind["SSL"].ToString()),
                                PaseCertificate(dr_Bind["SSL_Certificate"])
                            ));
                        }                        
                        m_Smtp_Server.Bindings = smtpIpBinds.ToArray();                        
					    m_Smtp_Server.MaxConnections      = ConvertEx.ToInt32(dr["SmtpThreads"]);
                        m_Smtp_Server.MaxConnectionsPerIP = ConvertEx.ToInt32(dr["SmtpMaxConnectionsPerIP"]);
					    m_Smtp_Server.SessionIdleTimeout  = ConvertEx.ToInt32(dr["MqTaskIdleTimeOut"]);
					    m_Smtp_Server.MaxMessageSize      = ConvertEx.ToInt32(dr["MaxMessageSize"]) * 1000000;       // Mb to byte.
					    m_Smtp_Server.MaxRecipients       = ConvertEx.ToInt32(dr["MaxRecipients"]);
					    m_Smtp_Server.MaxBadCommands      = ConvertEx.ToInt32(dr["SmtpMaxBadCommands"]);
                        m_Smtp_Server.MaxTransactions     = ConvertEx.ToInt32(dr["SmtpMaxTransactions"]);
                        m_Smtp_Server.GreetingText        = ConvertEx.ToString(dr["SmtpGreetingText"]);                        
                        m_Smtp_Server.ServiceExtentions   = new string[]{
                            Smtp_ServiceExtensions.PIPELINING,
                            Smtp_ServiceExtensions.SIZE,
                            Smtp_ServiceExtensions.STARTTLS,
                            Smtp_ServiceExtensions._8BITMIME,
                            Smtp_ServiceExtensions.BINARYMIME,
                            Smtp_ServiceExtensions.CHUNKING,
                            Smtp_ServiceExtensions.DSN
                        };
                        m_SmtpRequireAuth                = ConvertEx.ToBoolean(dr["SmtpRequireAuth"]);					
					    m_SmtpDefaultDomain              = ConvertEx.ToString(dr["SmtpDefaultDomain"]);                        
                        if(ConvertEx.ToBoolean(dr["SmtpEnabled"])){
                            m_Smtp_Server.Start();
                        }
                        else{
                            m_Smtp_Server.Stop();
                        }
                    }
                    catch(Exception x){
                        OnError(x);
                    }
                    //-------------------------------------------------------------------//

                    #endregion

                    #region Pop3

                    //------- Pop3 Settings -------------------------------------//
                    try{
					    List<IPBindInfo> pop3IpBinds = new List<IPBindInfo>();
                        foreach(DataRow dr_Bind in dr.Table.DataSet.Tables["Pop3_Bindings"].Rows){
                            pop3IpBinds.Add(new IPBindInfo(
                                ConvertEx.ToString(dr_Bind["HostName"]),
                                IPAddress.Parse(ConvertEx.ToString(dr_Bind["IP"])),
                                ConvertEx.ToInt32(dr_Bind["Port"]),
                                ParseSslMode(dr_Bind["SSL"].ToString()),
                                PaseCertificate(dr_Bind["SSL_Certificate"])
                            ));
                        }
                        m_Pop3Server.Bindings = pop3IpBinds.ToArray();
					    m_Pop3Server.MaxConnections      = ConvertEx.ToInt32(dr["Pop3_Threads"]);
                        m_Pop3Server.MaxConnectionsPerIP = ConvertEx.ToInt32(dr["Pop3_MaxConnectionsPerIP"]);
					    m_Pop3Server.SessionIdleTimeout  = ConvertEx.ToInt32(dr["Pop3_SessionIdleTimeOut"]);
					    m_Pop3Server.MaxBadCommands      = ConvertEx.ToInt32(dr["Pop3_MaxBadCommands"]);
                        m_Pop3Server.GreetingText        = ConvertEx.ToString(dr["Pop3_GreetingText"]);
                        if(ConvertEx.ToBoolean(dr["Pop3_Enabled"])){
                            m_Pop3Server.Start();
                        }
                        else{
                            m_Pop3Server.Stop();
                        }
                    }
                    catch(Exception x){
                        OnError(x);
                    }
                    //-----------------------------------------------------------//

                    #endregion

                    #region Imap

                    //------- Imap Settings -------------------------------------//
                    try{
					    List<IPBindInfo> imapIpBinds = new List<IPBindInfo>();
                        foreach(DataRow dr_Bind in dr.Table.DataSet.Tables["ImapBindings"].Rows){
                            imapIpBinds.Add(new IPBindInfo(
                                ConvertEx.ToString(dr_Bind["HostName"]),
                                IPAddress.Parse(ConvertEx.ToString(dr_Bind["IP"])),
                                ConvertEx.ToInt32(dr_Bind["Port"]),
                                ParseSslMode(dr_Bind["SSL"].ToString()),
                                PaseCertificate(dr_Bind["SSL_Certificate"])
                            ));
                        }
                        m_Imap_Server.Bindings = imapIpBinds.ToArray();
					    m_Imap_Server.MaxConnections      = ConvertEx.ToInt32(dr["ImapThreads"]);
                        m_Imap_Server.MaxConnectionsPerIP = ConvertEx.ToInt32(dr["ImapThreads"]);
					    m_Imap_Server.SessionIdleTimeout  = ConvertEx.ToInt32(dr["Imap_SessionIdleTimeOut"]);
					    m_Imap_Server.MaxBadCommands      = ConvertEx.ToInt32(dr["ImapMaxBadCommands"]);                        
                        m_Imap_Server.GreetingText = ConvertEx.ToString(dr["ImapGreetingText"]);
					    if(ConvertEx.ToBoolean(dr["ImapEnabled"])){
                            m_Imap_Server.Start();
                        }
                        else{
                            m_Imap_Server.Stop();
                        }
                    }
                    catch(Exception x){
                        OnError(x);
                    }
                    //-----------------------------------------------------------//

                    #endregion

                    #region Relay

                    //------- Relay ----------------------------------------------
                    try{
                        List<IPBindInfo> relayIpBinds = new List<IPBindInfo>();
                        foreach(DataRow dr_Bind in dr.Table.DataSet.Tables["Relay_Bindings"].Rows){
                            relayIpBinds.Add(new IPBindInfo(
                                ConvertEx.ToString(dr_Bind["HostName"]),
                                IPAddress.Parse(ConvertEx.ToString(dr_Bind["IP"])),
                                0,
                                SslMode.None,
                                null
                            ));
                        }

                        List<Relay_SmartHost> relaySmartHosts = new List<Relay_SmartHost>();
                        foreach(DataRow drX in dr.Table.DataSet.Tables["Relay_SmartHosts"].Rows){
                            relaySmartHosts.Add(new Relay_SmartHost(
                                ConvertEx.ToString(drX["Host"]),
                                ConvertEx.ToInt32(drX["Port"]),
                                (SslMode)Enum.Parse(typeof(SslMode),drX["SslMode"].ToString()),
                                ConvertEx.ToString(drX["UserName"]),
                                ConvertEx.ToString(drX["Password"])
                            ));
                        }
                        
                        m_RelayServer.RelayMode = (Relay_Mode)Enum.Parse(typeof(Relay_Mode),dr["Relay_Mode"].ToString());
                        m_RelayServer.SmartHostsBalanceMode = (BalanceMode)Enum.Parse(typeof(BalanceMode),dr["Relay_SmartHostsBalanceMode"].ToString());
                        m_RelayServer.SmartHosts = relaySmartHosts.ToArray();
                        m_RelayServer.SessionIdleTimeout = ConvertEx.ToInt32(dr["Relay_SessionIdleTimeOut"]);
                        m_RelayServer.MaxConnections = ConvertEx.ToInt32(dr["MaxRelayThreads"]);
                        m_RelayServer.MaxConnectionsPerIP = ConvertEx.ToInt32(dr["Relay_MaxConnectionsPerIP"]);    
                        m_RelayServer.RelayInterval = ConvertEx.ToInt32(dr["RelayInterval"]);
                        m_RelayServer.RelayRetryInterval = ConvertEx.ToInt32(dr["RelayRetryInterval"]);
                        m_RelayServer.DelayedDeliveryNotifyAfter = ConvertEx.ToInt32(dr["RelayUndeliveredWarning"]);
                        m_RelayServer.UndeliveredAfter = ConvertEx.ToInt32(dr["RelayUndelivered"]) * 60;
                        m_RelayServer.DelayedDeliveryMessage = null;
                        m_RelayServer.UndeliveredMessage = null;
                        foreach(DataRow drReturnMessage in dr.Table.DataSet.Tables["ServerReturnMessages"].Rows){
                            if(drReturnMessage["MessageType"].ToString() == "delayed_delivery_warning"){
                                m_RelayServer.DelayedDeliveryMessage = new ServerReturnMessage(drReturnMessage["Subject"].ToString(),drReturnMessage["BodyTextRtf"].ToString());
                            }
                            else if(drReturnMessage["MessageType"].ToString() == "undelivered"){
                                m_RelayServer.UndeliveredMessage = new ServerReturnMessage(drReturnMessage["Subject"].ToString(),drReturnMessage["BodyTextRtf"].ToString());
                            }                            
                        }
                        m_RelayServer.Bindings = relayIpBinds.ToArray();
                        if(ConvertEx.ToBoolean(dr["LogRelayCmds"])){
                            m_RelayServer.Logger = new MControl.Messaging.Net.Log.Logger();
                            m_RelayServer.Logger.WriteLog += new EventHandler<MControl.Messaging.Net.Log.WriteLogEventArgs>(m_RelayServer_WriteLog);
                        }
                        else{
                            if(m_RelayServer.Logger != null){
                                m_RelayServer.Logger.Dispose();
                                m_RelayServer.Logger = null;
                            }                            
                        }               
                        if(dr["Relay_LogPath"].ToString().Length == 0){
						    m_Relay_LogPath = m_OwnerServer.StartupPath + "Logs\\Relay\\";
					    }
					    else{
						    m_Relay_LogPath = dr["Relay_LogPath"].ToString() + "\\";
					    }
                        m_RelayServer.StoreUndeliveredMessages = ConvertEx.ToBoolean(dr["StoreUndeliveredMessages"]);
                        
                        //mcontrol
                        LoadSettings_Relay(dr);

                        if(!m_RelayServer.IsRunning){
                            m_RelayServer.Start();
                        }
                        //mcontrol
                        LoadSettings_NotifyServer(dr);
                    }
                    catch(Exception x){
                        OnError(x);
                    }
                    //------------------------------------------------------------

                    #endregion

                    #region FETCH

                    //----- Fetch Pop3 settings ----------------------------------//
                    try{
                        m_FetchServer.Enabled       = ConvertEx.ToBoolean(dr["FetchPop3_Enabled"]);
                        m_FetchServer.FetchInterval = ConvertEx.ToInt32(dr["FetchPop3_Interval"]);
                    }                    
                    catch(Exception x){
                        OnError(x);
                    }
                    //------------------------------------------------------------//

                    #endregion

                    #region SIP

                    //List<IPBindInfo> sipIpBinds = new List<IPBindInfo>();
                    //foreach(DataRow dr_Bind in dr.Table.DataSet.Tables["SIP_Bindings"].Rows){
                    //    if(dr_Bind["Protocol"].ToString().ToUpper() == "Tcp"){
                    //        sipIpBinds.Add(new IPBindInfo(
                    //            ConvertEx.ToString(dr_Bind["HostName"]),
                    //            IPAddress.Parse(ConvertEx.ToString(dr_Bind["IP"])),
                    //            ConvertEx.ToInt32(dr_Bind["Port"]),
                    //            ParseSslMode(dr_Bind["SSL"].ToString()),
                    //            PaseCertificate(dr_Bind["SSL_Certificate"])
                    //        ));
                    //    }
                    //    else{
                    //        sipIpBinds.Add(new IPBindInfo(
                    //            ConvertEx.ToString(dr_Bind["HostName"]),
                    //            BindInfoProtocol.Udp,
                    //            IPAddress.Parse(ConvertEx.ToString(dr_Bind["IP"])),
                    //            ConvertEx.ToInt32(dr_Bind["Port"])
                    //        ));
                    //    }
                    //}
                    //m_SipServer.Stack.BindInfo = sipIpBinds.ToArray();

                    //if(ConvertEx.ToBoolean(dr["SIP_Enabled"])){
                    //    m_SipServer.Stack.Start();
                    //}
                    //else{
                    //    m_SipServer.Stack.Stop();
                    //}
                    //m_SipServer.Stack.MinimumExpireTime = ConvertEx.ToInt32(dr["SIP_MinExpires"]);
                    //m_SipServer.ProxyMode               = (SIP_ProxyMode)Enum.Parse(typeof(SIP_ProxyMode),dr["SIP_ProxyMode"].ToString());

                    #endregion

                    #region LOGGING

                    //----- Logging settings -------------------------------------//
                    try{
                        if(ConvertEx.ToBoolean(dr["LogSMTPCmds"],false)){                            
                            m_Smtp_Server.Logger = new MControl.Messaging.Net.Log.Logger();
                            m_Smtp_Server.Logger.WriteLog += new EventHandler<MControl.Messaging.Net.Log.WriteLogEventArgs>(Smtp_Server_SessionLog);
                        }
                        else{
                            m_Smtp_Server.Logger = null;
                        }
                        if(ConvertEx.ToBoolean(dr["LogPOP3Cmds"],false)){                            
                            m_Pop3Server.Logger = new MControl.Messaging.Net.Log.Logger();
                            m_Pop3Server.Logger.WriteLog += new EventHandler<MControl.Messaging.Net.Log.WriteLogEventArgs>(Pop3_Server_SessionLog);
                        }
                        else{
                            m_Pop3Server.Logger = null;
                        }
                        if(ConvertEx.ToBoolean(dr["LogIMAPCmds"],false)){                            
                            m_Imap_Server.Logger = new MControl.Messaging.Net.Log.Logger();
                            m_Imap_Server.Logger.WriteLog += new EventHandler<MControl.Messaging.Net.Log.WriteLogEventArgs>(Imap_Server_SessionLog);
                        }
                        else{
                            m_Imap_Server.Logger = null;
                        }
					    m_FetchServer.LogCommands = ConvertEx.ToBoolean(dr["LogFetchPOP3Cmds"],false);
    					
					    m_SmtpLogPath   = API_Utlis.PathFix(ConvertEx.ToString(dr["SmtpLogPath"]) + "\\");
					    m_Pop3_LogPath   = API_Utlis.PathFix(ConvertEx.ToString(dr["Pop3_LogPath"]) + "\\");
					    m_ImapLogPath   = API_Utlis.PathFix(ConvertEx.ToString(dr["ImapLogPath"]) + "\\");
					    m_Server_LogPath = API_Utlis.PathFix(ConvertEx.ToString(dr["Server_LogPath"]) + "\\");
					    m_Fetch_LogPath  = API_Utlis.PathFix(ConvertEx.ToString(dr["FetchPop3_LogPath"]) + "\\");
    					
					    //----- If no log path, use default ----------------
					    if(dr["SmtpLogPath"].ToString().Trim().Length == 0){
						    m_SmtpLogPath = API_Utlis.PathFix(m_OwnerServer.StartupPath + "Logs\\Smtp\\");
					    }
					    if(dr["Pop3_LogPath"].ToString().Trim().Length == 0){
						    m_Pop3_LogPath = API_Utlis.PathFix(m_OwnerServer.StartupPath + "Logs\\Pop3\\");
					    }
					    if(dr["ImapLogPath"].ToString().Trim().Length == 0){
						    m_ImapLogPath = API_Utlis.PathFix(m_OwnerServer.StartupPath + "Logs\\Imap\\");
					    }
					    if(dr["Server_LogPath"].ToString().Trim().Length == 0){
						    m_Server_LogPath = API_Utlis.PathFix(m_OwnerServer.StartupPath + "Logs\\Server\\");
					    }					
					    if(dr["FetchPop3_LogPath"].ToString().Trim().Length == 0){
						    m_Fetch_LogPath = API_Utlis.PathFix(m_OwnerServer.StartupPath + "Logs\\FetchPOP3\\");
					    }
					    m_FetchServer.LogPath = m_Fetch_LogPath;
				    }
                    catch(Exception x){
                        OnError(x);
                    }
					//------------------------------------------------------------//

                    #endregion

			//		SCore.WriteLog(m_Server_LogPath + "server.log","//---- Server settings loaded " + DateTime.Now);
				}
			}
			catch(Exception x){
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}                
                
		#endregion

        #region method IsRelayAllowed

		/// <summary>
		/// Checks if relay is allowed to specified User/IP.
        /// First user 'Allow Relay' checked, if not allowed, then checked if relay denied for that IP,
        /// at last checks if relay is allowed for that IP.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="ip"></param>
		/// <returns>Returns true if relay is allowed.</returns>
        private bool IsRelayAllowed(string userName, IPAddress ip)
        {
            if (userName != null && userName.Length > 0)
            {
                if ((m_Api.GetUserPermissions(userName) & UserPermissions_enum.Relay) != 0)
                {
                    return true;
                }
            }

            using (DataView dv = m_Api.GetSecurityList())
            {
                // Check if ip is denied
                foreach (DataRowView drV in dv)
                {
                    if (Convert.ToBoolean(drV["Enabled"]) && Convert.ToInt32(drV["Service"]) == (int)Service_enum.Relay && Convert.ToInt32(drV["Action"]) == (int)IPSecurityAction_enum.Deny)
                    {
                        // See if IP matches range
                        if (Net_Utils.CompareIP(IPAddress.Parse(drV["StartIP"].ToString()), ip) >= 0 && Net_Utils.CompareIP(IPAddress.Parse(drV["EndIP"].ToString()), ip) <= 0)
                        {
                            return false;
                        }
                    }
                }

                // Check if ip is allowed
                foreach (DataRowView drV in dv)
                {
                    if (Convert.ToBoolean(drV["Enabled"]) && Convert.ToInt32(drV["Service"]) == (int)Service_enum.Relay && Convert.ToInt32(drV["Action"]) == (int)IPSecurityAction_enum.Allow)
                    {
                        // See if IP matches range
                        if (Net_Utils.CompareIP(IPAddress.Parse(drV["StartIP"].ToString()), ip) >= 0 && Net_Utils.CompareIP(IPAddress.Parse(drV["EndIP"].ToString()), ip) <= 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
		
		#endregion

        #region method IsAccessAllowed

		/// <summary>
		/// Checks if specified service access is allowed for specified IP.
		/// </summary>
		/// <param name="service">Smtp or Pop3 or Imap.</param>
		/// <param name="ip"></param>
		/// <returns>Returns true if allowed.</returns>
		public bool IsAccessAllowed(Service_enum service,IPAddress ip)
		{
			using(DataView dv = m_Api.GetSecurityList()){
                // Check if ip is denied
                foreach(DataRowView drV in dv){
                    if(Convert.ToBoolean(drV["Enabled"]) && Convert.ToInt32(drV["Service"]) == (int)service && Convert.ToInt32(drV["Action"]) == (int)IPSecurityAction_enum.Deny){
                        // See if IP matches range
                        if(Net_Utils.CompareIP(IPAddress.Parse(drV["StartIP"].ToString()),ip) >= 0 && Net_Utils.CompareIP(IPAddress.Parse(drV["EndIP"].ToString()),ip) <= 0){
                            return false;
                        }
                    }
                }

                // Check if ip is allowed
                foreach(DataRowView drV in dv){
                    if(Convert.ToBoolean(drV["Enabled"]) && Convert.ToInt32(drV["Service"]) == (int)service && Convert.ToInt32(drV["Action"]) == (int)IPSecurityAction_enum.Allow){
                        // See if IP matches range
                        if(Net_Utils.CompareIP(IPAddress.Parse(drV["StartIP"].ToString()),ip) >= 0 && Net_Utils.CompareIP(IPAddress.Parse(drV["EndIP"].ToString()),ip) <= 0){
                            return true;
                        }
                    }
                }
            }
            
            return false;
		}
		
		#endregion
        
        #region method PaseCertificate

        /// <summary>
        /// Parses x509 certificate from specified data. Returns null if no certificate to load.
        /// </summary>
        /// <param name="cert">Certificate data.</param>
        /// <returns>Returns parse certificate or null if no certificate.</returns>
        private X509Certificate2 PaseCertificate(object cert)
        {
            if(cert == null){
                return null;
            }
            if(cert == DBNull.Value){
                return null;
            }
            else{
                /* NOTE: MS X509Certificate2((byte[])) has serious bug, it will create temp file
                 * and leaves it open. The result is temp folder will get full.
                */
                String tmpFile = Path.GetTempFileName();
                try{                    
                    using(FileStream fs = File.Open(tmpFile,FileMode.Open)){
                        fs.Write((byte[])cert,0,((byte[])cert).Length);
                    }

                    X509Certificate2 c = new X509Certificate2(tmpFile);
               
                    return c;
                }
                finally{
                    File.Delete(tmpFile);
                }                
            }
        }

        #endregion

        #region method ParseSslMode

        /// <summary>
        /// Parses SSL mode from string.
        /// </summary>
        /// <param name="value">Ssl mode string.</param>
        /// <returns>Returns parsed SSL mode.</returns>
        private SslMode ParseSslMode(string value)
        {
            if(value.ToLower() == "false"){ // REMOVE ME: remove in next versions
                return SslMode.None;
            }
            else if(value.ToLower() == "true"){ // REMOVE ME: remove in next versions
                return SslMode.SSL;
            }
            else{
                return (SslMode)Enum.Parse(typeof(SslMode),value);
            }
        }

        #endregion

        #region method FillUserInfo

        /// <summary>
        /// Gets and fills specified user info.
        /// </summary>
        /// <param name="userInfo">User info.</param>
        /// <exception cref="ArgumentNullException">Is riased when <b>userInfo</b> is null reference.</exception>
        private void FillUserInfo(Auth_e_UserInfo userInfo)
        {
            if(userInfo == null){
                throw new ArgumentNullException("userInfo");
            }

            try{
                foreach(DataRowView dr in m_Api.GetUsers("")){
                    if(userInfo.UserName.ToLowerInvariant() == dr["UserName"].ToString().ToLowerInvariant()){
                        userInfo.UserExists = true;
                        userInfo.Password = dr["Password"].ToString();
                        break;
                    }
                }
            }
            catch(Exception x){
                OnError(x);
            }
        }

        #endregion

        #region method ProcessAndStoreMessage

        /// <summary>
        /// Processes and stores message.
        /// </summary>
        /// <param name="sender">Mail from.</param>
        /// <param name="recipient">Recipient to.</param>
        /// <param name="msgStream">Message stream. Stream position must be there where message begins.</param>
        /// <param name="e">Event data.</param>
		public void ProcessAndStoreMessage(string sender,string[] recipient,Stream msgStream,MqMessageStored e)
		{
            List<Smtp_RcptTo> recipients = new List<Smtp_RcptTo>();
            foreach(string r in recipient){
                recipients.Add(new Smtp_RcptTo(r,Smtp_Dsn_Notify.NotSpecified,null));
            }

            ProcessAndStoreMessage(null,sender,Smtp_Dsn_Ret.NotSpecified,recipients.ToArray(),msgStream,e);
        }

        /// <summary>
        /// Processes and stores message.
        /// </summary>
        /// <param name="envelopeID">Envelope ID_(MAIL FROM: ENVID).</param>
        /// <param name="sender">Mail from.</param>
        /// <param name="ret">Specifies what parts of message are returned in DSN report.</param>
        /// <param name="recipients">Message recipients.</param>
        /// <param name="msgStream">Message stream. Stream position must be there where message begins.</param>
        /// <param name="e">Event data.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>recipients</b> or <b>msgStream</b> is nulll reference.</exception>
		public void ProcessAndStoreMessage(string envelopeID,string sender,Smtp_Dsn_Ret ret,Smtp_RcptTo[] recipients,Stream msgStream,MqMessageStored e)
		{
            if(recipients == null){
                throw new ArgumentNullException("recipients");
            }
            if(msgStream == null){
                throw new ArgumentNullException("msgStream");
            }

            /* Message processing.
                *) Message filters.
                *) Global message rules.
                *) Process recipients.
            */

            List<Smtp_RcptTo> dsn_Delivered = new List<Smtp_RcptTo>();

            string[] to = new string[recipients.Length];
            for(int i=0;i<to.Length;i++){
                to[i] = recipients[i].Mailbox;
            }
         
			#region Global Filtering stuff
            
			//--- Filter message -----------------------------------------------//
			Stream filteredMsgStream = msgStream;
			DataView dvFilters = m_Api.GetFilters();
			dvFilters.RowFilter = "Enabled=true AND Type='ISmtpMessageFilter'";
			dvFilters.Sort = "Cost";			
			foreach(DataRowView drViewFilter in dvFilters){
                try{
				    filteredMsgStream.Position = 0;

				    string assemblyFile = API_Utlis.PathFix(drViewFilter.Row["Assembly"].ToString());
				    // File is without path probably, try to load it from filters folder
				    if(!File.Exists(assemblyFile)){
					    assemblyFile = API_Utlis.PathFix(m_OwnerServer.StartupPath + "\\Filters\\" + assemblyFile);
				    }

				    Assembly ass = Assembly.LoadFrom(assemblyFile);
				    Type tp = ass.GetType(drViewFilter.Row["ClassName"].ToString());
				    object filterInstance = Activator.CreateInstance(tp);
				    ISmtpMessageFilter filter = (ISmtpMessageFilter)filterInstance;
    						
				    string errorText = "";
                    MqTask session = null;
                    if(e != null){
                        session = e.Session;
                    }
				    FilterResult result = filter.Filter(filteredMsgStream,out filteredMsgStream,sender,to,m_Api,session,out errorText);
                    if(result == FilterResult.DontStore){
                        // Just skip messge, act as message is stored
                        e.Reply = new Smtp_Reply(552,"Requested mail action aborted: Message discarded by server filter.");
						return; 
                    }
                    else if(result == FilterResult.Error){
                        if(e != null){
                            e.Reply = new Smtp_Reply(552,"Requested mail action aborted: " + errorText);
                        }
                        else{
                            // NOTE: 26.01.2006 - e maybe null if that method is called server internally and no smtp session.                            
                        }
						return;
                    }
                    
                    // Filter didn't return message stream
                    if(filteredMsgStream == null){
				        e.Reply = new Smtp_Reply(552,"Requested mail action aborted: Message discarded by server filter.");
                        return;
                    }
                }
                catch(Exception x){
                    // Filtering failed, log error and allow message through.
                    OnError(x);
                }
			}
			//---------------------------------------------------------------//
			#endregion

            #region Global Message Rules
  
            filteredMsgStream.Position = 0;
                         
            Mail_Message mime = null;
            try{
                mime = Mail_Message.ParseFromStream(filteredMsgStream);
            }
            // Invalid message syntax, block such message.
            catch{
                e.Reply = new Smtp_Reply(552,"Requested mail action aborted: Message has invalid structure/syntax.");
                               
                try{
                    if(!Directory.Exists(this.MailStorePath + "Unparseable")){
                        Directory.CreateDirectory(this.MailStorePath + "Unparseable");
                    }
                                
                    using(FileStream fs = File.Create(this.MailStorePath + "Unparseable\\" + Guid.NewGuid().ToString().Replace("-","") + ".eml")){
                        filteredMsgStream.Position = 0;
                        Net_Utils.StreamCopy(filteredMsgStream,fs,32000);
                    }
                }
                catch{
                }

                return;
            }

            //--- Check Global Message Rules --------------------------------------------------------------//
            bool   deleteMessage = false;
            string storeFolder   = "Inbox";
            string smtpErrorText = null;   

            // Loop rules
            foreach(DataRowView drV_Rule in m_Api.GetGlobalMessageRules()){
                // Reset stream position
                filteredMsgStream.Position = 0;

                if(Convert.ToBoolean(drV_Rule["Enabled"])){
                    string ruleID = drV_Rule["RuleID"].ToString();
                    GlobalMessageRule_CheckNextRule_enum checkNextIf = (GlobalMessageRule_CheckNextRule_enum)(int)drV_Rule["CheckNextRuleIf"];
                    string matchExpression = drV_Rule["MatchExpression"].ToString();

                    // e may be null if server internal method call and no actual session !
                    MqTask session = null;
                    if(e != null){
                        session = e.Session;
                    }
                    GlobalMessageRuleProcessor ruleEngine = new GlobalMessageRuleProcessor();
                    bool matches = ruleEngine.Match(matchExpression,sender,to,session,mime,(int)filteredMsgStream.Length);
                    if(matches){                        
                        // Do actions
                        GlobalMessageRuleActionResult result = ruleEngine.DoActions(
                            m_Api.GetGlobalMessageRuleActions(ruleID),
                            this,
                            filteredMsgStream,
                            sender,
                            to
                        );

                        if(result.DeleteMessage){
                            deleteMessage = true;
                        }
                        if(result.StoreFolder != null){                            
                            storeFolder = result.StoreFolder;                           
                        }
                        if(result.ErrorText != null){
                            smtpErrorText = result.ErrorText;
                        }
                    }

                    //--- See if we must check next rule -------------------------------------------------//
                    if(checkNextIf == GlobalMessageRule_CheckNextRule_enum.Always){
                        // Do nothing
                    }
                    else if(checkNextIf == GlobalMessageRule_CheckNextRule_enum.IfMatches && !matches){
                        break;
                    }
                    else if(checkNextIf == GlobalMessageRule_CheckNextRule_enum.IfNotMatches && matches){
                        break;
                    }
                    //------------------------------------------------------------------------------------//
                }
            }

            // Return error to connected client
            if(smtpErrorText != null){
                e.Reply = new Smtp_Reply(552,"Requested mail action aborted: " + smtpErrorText);
                return;
            }

            // Just don't store message
            if(deleteMessage){
                return;
            }
            
            // Reset stream position
            filteredMsgStream.Position = 0;
            //--- End of Global Rules -------------------------------------------------------------------//

            #endregion
            
            #region Process recipients
                  
            HashSet<string> processedItems = new HashSet<string>();

            Queue<Smtp_RcptTo> recipientsQueue = new Queue<Smtp_RcptTo>();
            // Queue current recipients for processing.
            foreach(Smtp_RcptTo recipient in recipients){
                recipientsQueue.Enqueue(recipient);
            }

            while(recipientsQueue.Count > 0){
                /* Process order
                    *) Local user
                    *) Local address
                    *) Local mailing list address
                    *) Route
                    *) Relay
                */

                Smtp_RcptTo recipient = recipientsQueue.Dequeue();

                // Check if we already have processed this item. Skip dublicate items.
                // This method also avoids loops when 2 recipients reference each other.
                if(processedItems.Contains(recipient.Mailbox)){
                    continue;
                }
                processedItems.Add(recipient.Mailbox);
                               

                #region Local user

                if(recipient.Mailbox.IndexOf('@') == -1 && m_Api.UserExists(recipient.Mailbox)){
                    // Add user to processed list.
                    processedItems.Add(recipient.Mailbox);

                    // Delivery status notification(DSN) requested to this user.
                    if((recipient.Notify & Smtp_Dsn_Notify.Success) != 0){
                        dsn_Delivered.Add(recipient);
                    }

                    ProcessUserMsg(sender,recipient.Mailbox,recipient.Mailbox,storeFolder,filteredMsgStream,e);

                    continue;
                }

                #endregion

                #region Local address

                string localUser = m_Api.MapUser(recipient.Mailbox);
                if(localUser != null){
                    // Add user to processed list.
                    processedItems.Add(localUser);

                    // Delivery status notification(DSN) requested to this user.
                    if((recipient.Notify & Smtp_Dsn_Notify.Success) != 0){
                        dsn_Delivered.Add(recipient);
                    }

                    ProcessUserMsg(sender,recipient.Mailbox,localUser,storeFolder,filteredMsgStream,e);
                }

                #endregion

                #region Mailing list address

                else if(m_Api.MailingListExists(recipient.Mailbox)){
                    // Delivery status notification(DSN) requested to this user.
                    if((recipient.Notify & Smtp_Dsn_Notify.Success) != 0){
                        dsn_Delivered.Add(recipient);
                    }

                    Queue<string> processQueue = new Queue<string>();
                    processQueue.Enqueue(recipient.Mailbox);

                    // Loop while there are mailing lists or nested mailing list available
                    while(processQueue.Count > 0){
                        string mailingList = processQueue.Dequeue(); 
                          
                        // Process mailing list members
					    foreach(DataRowView drV in m_Api.GetMailingListAddresses(mailingList)){
                            string member = drV["Address"].ToString();

                            // Member is asteric pattern matching server emails
                            if(member.IndexOf('*') > -1){
                                DataView dvServerAddresses = m_Api.GetUserAddresses("");
                                foreach(DataRowView drvServerAddress in dvServerAddresses){
                                    string serverAddress = drvServerAddress["Address"].ToString();
                                    if(SCore.IsAstericMatch(member,serverAddress)){
                                        recipientsQueue.Enqueue(new Smtp_RcptTo(serverAddress,Smtp_Dsn_Notify.NotSpecified,null));                                        
                                    }
                                }
                            }
                            // Member is user or group, not email address
                            else if(member.IndexOf('@') == -1){                            
                                // Member is group, replace with actual users
                                if(m_Api.GroupExists(member)){
                                    foreach(string user in m_Api.GetGroupUsers(member)){
                                        recipientsQueue.Enqueue(new Smtp_RcptTo(user,Smtp_Dsn_Notify.NotSpecified,null));                                        
                                    }
                                }
                                // Member is user
                                else if(m_Api.UserExists(member)){
                                    recipientsQueue.Enqueue(new Smtp_RcptTo(member,Smtp_Dsn_Notify.NotSpecified,null));                                    
                                }
                                // Unknown member, skip it.
                                else{
                                }
                            }
                            // Member is nested mailing list
                            else if(m_Api.MailingListExists(member)){
                                processQueue.Enqueue(member);                                
                            }
                            // Member is normal email address
                            else{
                                recipientsQueue.Enqueue(new Smtp_RcptTo(member,Smtp_Dsn_Notify.NotSpecified,null));                                
                            }					
					    }
                    }
                }

                #endregion

                else{
                    bool isRouted = false;

                    #region Check Routing

                    foreach(DataRowView drRoute in m_Api.GetRoutes()){
                        // We have matching route
                        if(Convert.ToBoolean(drRoute["Enabled"]) && SCore.IsAstericMatch(drRoute["Pattern"].ToString(),recipient.Mailbox)){
                            string           description = drRoute["Action"].ToString();    
                            RouteAction_enum action      = (RouteAction_enum)Convert.ToInt32(drRoute["Action"]);
                            byte[]           actionData  = (byte[])drRoute["ActionData"];

                            #region RouteToEmail

                            if(action == RouteAction_enum.RouteToEmail){
                                XmlTable table = new XmlTable("ActionData");
                                table.Parse(actionData);

                                // Add email to process queue.
                                recipientsQueue.Enqueue(new Smtp_RcptTo(table.GetValue("EmailAddress"),Smtp_Dsn_Notify.NotSpecified,null));

                                // Log
                                if(e != null){
                                    e.Session.LogAddText("Route '[" + description + "]: " + drRoute["Pattern"].ToString() + "' routed to email '" + table.GetValue("EmailAddress") + "'.");
                                }
                            }

                            #endregion

                            #region RouteToHost

                            else if(action == RouteAction_enum.RouteToHost){
                                XmlTable table = new XmlTable("ActionData");
                                table.Parse(actionData);  

                                msgStream.Position = 0;

                                // Route didn't match, so we have relay message.
                                this.RelayServer.StoreRelayMessage(                    
                                    Guid.NewGuid().ToString(),
                                    envelopeID,
                                    msgStream,
                                    HostEndPoint.Parse(table.GetValue("Host") + ":" + table.GetValue("Port")),
                                    sender,
                                    recipient.Mailbox,
                                    recipient.ORCPT,
                                    recipient.Notify,
                                    ret,0
                                );
                             
                                // Log
                                if(e != null){
                                    e.Session.LogAddText("Route '[" + description + "]: " + drRoute["Pattern"].ToString() + "' routed to host '" + table.GetValue("Host") + ":" + table.GetValue("Port") + "'.");
                                }
                            }

                            #endregion

                            #region RouteToMailbox

                            else if(action == RouteAction_enum.RouteToMailbox){
                                XmlTable table = new XmlTable("ActionData");
                                table.Parse(actionData);

                                ProcessUserMsg(sender,recipient.Mailbox,table.GetValue("Mailbox"),storeFolder,filteredMsgStream,e);

                                // Log
                                if(e != null){
                                    e.Session.LogAddText("Route '[" + description + "]: " + drRoute["Pattern"].ToString() + "' routed to user '" + table.GetValue("Mailbox") + "'.");
                                }
                            }

                            #endregion
                             
                            isRouted = true;
                            break;
                        }
                    }

                   #endregion

                    // Route didn't match, so we have relay message.
                    if(!isRouted){
                        filteredMsgStream.Position = 0;

                        this.RelayServer.StoreRelayMessage(                    
                            Guid.NewGuid().ToString(),
                            envelopeID,
                            filteredMsgStream,
                            null,
                            sender,
                            recipient.Mailbox,
                            recipient.ORCPT,
                            recipient.Notify,
                            ret,0
                        );
                    }
                }
            }

            #endregion

            
            #region DSN "delivered"

            // Send DSN for requested recipients.
            if(dsn_Delivered.Count > 0 && !string.IsNullOrEmpty(sender)){
                try{
                    string dsn_to = "";
                    for(int i=0;i<dsn_Delivered.Count;i++){
                        if(i == (dsn_Delivered.Count - 1)){
                            dsn_to += dsn_Delivered[i].Mailbox;
                        }
                        else{
                            dsn_to += dsn_Delivered[i].Mailbox + "; ";
                        }
                    }

                    string reportingMTA = "";
                    if(e != null && !string.IsNullOrEmpty(e.Session.LocalHostName)){
                        reportingMTA = e.Session.LocalHostName;
                    }
                    else{
                        reportingMTA = System.Net.Dns.GetHostName();
                    }

                    ServerReturnMessage messageTemplate = null;
                    if(messageTemplate == null){
                        string bodyRtf = "" +
                        "{\\rtf1\\ansi\\ansicpg1257\\deff0\\deflang1061{\\fonttbl{\\f0\\froman\\fcharset0 Times New Roman;}{\\f1\froman\\fcharset186{\\*\\fname Times New Roman;}Times New Roman Baltic;}{\\f2\fswiss\\fcharset186{\\*\\fname Arial;}Arial Baltic;}}\r\n" +
                        "{\\colortbl ;\\red0\\green128\\blue0;\\red128\\green128\\blue128;}\r\n" +
                        "{\\*\\generator Msftedit 5.41.21.2508;}\\viewkind4\\uc1\\pard\\sb100\\sa100\\lang1033\\f0\\fs24\\par\r\n" +
                        "Your message WAS SUCCESSFULLY DELIVERED to:\\line\\lang1061\\f1\\tab\\cf1\\lang1033\\b\\f0 " + dsn_to + "\\line\\cf0\\b0 and you explicitly requested a delivery status notification on success.\\par\\par\r\n" +
                        "\\cf2 Your original message\\lang1061\\f1 /header\\lang1033\\f0  is attached to this e-mail\\lang1061\\f1 .\\lang1033\\f0\\par\\r\\n" +
                        "\\cf0\\line\\par\r\n" +
                        "\\pard\\lang1061\\f2\\fs20\\par\r\n" +
                        "}\r\n";

                        messageTemplate = new ServerReturnMessage("DSN SUCCESSFULLY DELIVERED: " + mime.Subject,bodyRtf);
                    }

                    string rtf = messageTemplate.BodyTextRtf;

                    Mail_Message dsnMsg = new Mail_Message();
                    dsnMsg.MimeVersion = "1.0";
                    dsnMsg.Date = DateTime.Now;
                    dsnMsg.From = new Mail_MailboxList();
                    dsnMsg.From.Add(new Mail_Mailbox("Mail Delivery Subsystem","postmaster@local"));
                    dsnMsg.To = new Mail_AddressList();
                    dsnMsg.To.Add(new Mail_Mailbox(null,sender));
                    dsnMsg.Subject = messageTemplate.Subject;

                    //--- multipart/report -------------------------------------------------------------------------------------------------
                    Mime_Header_ContentType contentType_multipartReport = new Mime_Header_ContentType(Mime_MediaTypes.Multipart.report);            
                    contentType_multipartReport.Parameters["report-type"] = "delivery-status";
                    contentType_multipartReport.Param_Boundary = Guid.NewGuid().ToString().Replace('-','.');
                    Mime_Body_MultipartReport multipartReport = new Mime_Body_MultipartReport(contentType_multipartReport);
                    dsnMsg.Body = multipartReport;

                        //--- multipart/alternative -----------------------------------------------------------------------------------------
                        Mime_Entity entity_multipart_alternative = new Mime_Entity();
                        Mime_Header_ContentType contentType_multipartAlternative = new Mime_Header_ContentType(Mime_MediaTypes.Multipart.alternative);
                        contentType_multipartAlternative.Param_Boundary = Guid.NewGuid().ToString().Replace('-','.');
                        Mime_Body_MultipartAlternative multipartAlternative = new Mime_Body_MultipartAlternative(contentType_multipartAlternative);
                        entity_multipart_alternative.Body = multipartAlternative;
                        multipartReport.BodyParts.Add(entity_multipart_alternative);

                            //--- text/plain ---------------------------------------------------------------------------------------------------
                            Mime_Entity entity_text_plain = new Mime_Entity();
                            Mime_Body_Text text_plain = new Mime_Body_Text(Mime_MediaTypes.Text.plain);
                            entity_text_plain.Body = text_plain;
                            text_plain.SetText(Mime_TransferEncodings.QuotedPrintable,Encoding.UTF8,SCore.RtfToText(rtf));
                            multipartAlternative.BodyParts.Add(entity_text_plain);

                            //--- text/html -----------------------------------------------------------------------------------------------------
                            Mime_Entity entity_text_html = new Mime_Entity();
                            Mime_Body_Text text_html = new Mime_Body_Text(Mime_MediaTypes.Text.html);
                            entity_text_html.Body = text_html;
                            text_html.SetText(Mime_TransferEncodings.QuotedPrintable,Encoding.UTF8,SCore.RtfToHtml(rtf));
                            multipartAlternative.BodyParts.Add(entity_text_html);

                        //--- message/delivery-status
                        Mime_Entity entity_message_deliveryStatus = new Mime_Entity();                        
                        Mime_Body_MessageDeliveryStatus body_message_deliveryStatus = new Mime_Body_MessageDeliveryStatus();
                        entity_message_deliveryStatus.Body = body_message_deliveryStatus;
                        multipartReport.BodyParts.Add(entity_message_deliveryStatus);
            
                        //--- per-message-fields ----------------------------------------------------------------------------
                        Mime_Header_Collection messageFields = body_message_deliveryStatus.MessageFields;
                        if(!string.IsNullOrEmpty(envelopeID)){
                            messageFields.Add(new Mime_Header_Unstructured("Original-Envelope-Id",envelopeID));
                        }
                        messageFields.Add(new Mime_Header_Unstructured("Arrival-Date",Mime_Utils.DateTimeToRfc2822(DateTime.Now)));
                        if(e != null && !string.IsNullOrEmpty(e.Session.EhloHost)){
                            messageFields.Add(new Mime_Header_Unstructured("Received-From-MTA","dns;" + e.Session.EhloHost));
                        }
                        messageFields.Add(new Mime_Header_Unstructured("Reporting-MTA","dns;" + reportingMTA));
                        //---------------------------------------------------------------------------------------------------

                        foreach(Smtp_RcptTo r in dsn_Delivered){
                            //--- per-recipient-fields --------------------------------------------------------------------------
                            Mime_Header_Collection recipientFields = new Mime_Header_Collection(new Mime_Header_Provider());
                            if(r.ORCPT != null){
                                recipientFields.Add(new Mime_Header_Unstructured("Original-Recipient",r.ORCPT));
                            }
                            recipientFields.Add(new Mime_Header_Unstructured("Final-Recipient","rfc822;" + r.Mailbox));
                            recipientFields.Add(new Mime_Header_Unstructured("Action","delivered"));
                            recipientFields.Add(new Mime_Header_Unstructured("Status","2.0.0"));
                            body_message_deliveryStatus.RecipientBlocks.Add(recipientFields);
                            //---------------------------------------------------------------------------------------------------
                        }
                                            
                        //--- message/rfc822
                        if(mime != null){
                            Mime_Entity entity_message_rfc822 = new Mime_Entity();
                            Mime_Body_MessageRfc822 body_message_rfc822 = new Mime_Body_MessageRfc822();
                            entity_message_rfc822.Body = body_message_rfc822;
                            if(ret == Smtp_Dsn_Ret.FullMessage){
                                body_message_rfc822.Message = mime;
                            }
                            else{
                                MemoryStream ms = new MemoryStream();
                                mime.Header.ToStream(ms,null,null);
                                ms.Position = 0;
                                body_message_rfc822.Message = Mail_Message.ParseFromStream(ms);
                            }
                            multipartReport.BodyParts.Add(entity_message_rfc822);
                        }

                    using(MemoryStream strm = new MemoryStream()){
					    dsnMsg.ToStream(strm,new Mime_Encoding_EncodedWord(Mime_EncodedWordEncoding.Q,Encoding.UTF8),Encoding.UTF8);
					    ProcessAndStoreMessage("",new string[]{sender},strm,null);
				    }
                }
                catch(Exception x){
                    Error.DumpError(this.Name,x);
                }
            }

            #endregion
        }
                
        #endregion

        #region method ProcessUserMsg

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="recipient"></param>
        /// <param name="userName"></param>
        /// <param name="storeFolder">Message folder where message will be stored. For example 'Inbox'.</param>
        /// <param name="msgStream"></param>
        /// <param name="e">Event data.</param>
		internal void ProcessUserMsg(string sender,string recipient,string userName,string storeFolder,Stream msgStream,MqMessageStored e)
        {
            string userID = m_Api.GetUserID(userName);
            // This value can be null only if user deleted during this session, so just skip next actions.
            if(userID == null){
                return;
            }

            #region User Message rules
	
            Stream filteredMsgStream = msgStream;
			filteredMsgStream.Position = 0;

            Mail_Message mime = null;
            try{
                mime = Mail_Message.ParseFromStream(filteredMsgStream);
            }
            // Invalid message syntax, block such message.
            catch{
                e.Reply = new Smtp_Reply(552,"Requested mail action aborted: Message has invalid structure/syntax.");
                return;
            }

            string[] to = new string[]{recipient};

            //--- Check User Message Rules --------------------------------------------------------------//
            bool   deleteMessage = false;
            string smtpErrorText = null;   

            // Loop rules
            foreach(DataRowView drV_Rule in m_Api.GetUserMessageRules(userName)){
                // Reset stream position
                filteredMsgStream.Position = 0;

                if(Convert.ToBoolean(drV_Rule["Enabled"])){
                    string ruleID = drV_Rule["RuleID"].ToString();
                    GlobalMessageRule_CheckNextRule_enum checkNextIf = (GlobalMessageRule_CheckNextRule_enum)(int)drV_Rule["CheckNextRuleIf"];
                    string matchExpression = drV_Rule["MatchExpression"].ToString();

                    // e may be null if server internal method call and no actual session !
                    MqTask session = null;
                    if(e != null){
                        session = e.Session;
                    }
                    GlobalMessageRuleProcessor ruleEngine = new GlobalMessageRuleProcessor();
                    bool matches = ruleEngine.Match(matchExpression,sender,to,session,mime,(int)filteredMsgStream.Length);
                    if(matches){                        
                        // Do actions
                        GlobalMessageRuleActionResult result = ruleEngine.DoActions(
                            m_Api.GetUserMessageRuleActions(userID,ruleID),
                            this,
                            filteredMsgStream,
                            sender,
                            to
                        );

                        if(result.DeleteMessage){
                            deleteMessage = true;
                        }
                        if(result.StoreFolder != null){                            
                            storeFolder = result.StoreFolder;                           
                        }
                        if(result.ErrorText != null){
                            smtpErrorText = result.ErrorText;
                        }
                    }

                    //--- See if we must check next rule -------------------------------------------------//
                    if(checkNextIf == GlobalMessageRule_CheckNextRule_enum.Always){
                        // Do nothing
                    }
                    else if(checkNextIf == GlobalMessageRule_CheckNextRule_enum.IfMatches && !matches){
                        break;
                    }
                    else if(checkNextIf == GlobalMessageRule_CheckNextRule_enum.IfNotMatches && matches){
                        break;
                    }
                    //------------------------------------------------------------------------------------//
                }
            }

            // Return error to connected client
            if(smtpErrorText != null){
                e.Reply = new Smtp_Reply(552,"Requested mail action aborted: " + smtpErrorText);
                return;
            }

            // Just don't store message
            if(deleteMessage){
                return;
            }
            
            // Reset stream position
            filteredMsgStream.Position = 0;
            //--- End of Global Rules -------------------------------------------------------------------//
            
            #endregion

            // ToDo: User message filtering
			#region User message filtering

				//--- Filter message -----------------------------------------------//
		/*		MemoryStream filteredMsgStream = msgStream;
				DataView dvFilters = m_Api.GetFilters();
				dvFilters.RowFilter = "Enabled=true AND Type=ISmtpUserMessageFilter";
				dvFilters.Sort = "Cost";			
				foreach(DataRowView drViewFilter in dvFilters){
					string assemblyFile = drViewFilter.Row["Assembly"].ToString();
					// File is without path probably, try to load it from filters folder
					if(!File.Exists(assemblyFile)){
						assemblyFile = m_SartUpPath + "\\Filters\\" + assemblyFile;
					}

					Assembly ass = Assembly.LoadFrom(assemblyFile);
					Type tp = ass.GetType(drViewFilter.Row["ClassName"].ToString());
					object filterInstance = Activator.CreateInstance(tp);
					ISmtpMessageFilter filter = (ISmtpMessageFilter)filterInstance;
												
					FilterResult result = filter.Filter(filteredMsgStream,out filteredMsgStream,sender,recipient,m_Api);
					switch(result)
					{
						case FilterResult.DontStore:
							return; // Just skip messge, act as message is stored

						case FilterResult.Error:
							// ToDO: Add implementaion here or get rid of it (use exception instead ???).
							return;
					}
				}*/
				//---------------------------------------------------------------//

				#endregion

            filteredMsgStream.Position = 0;

            /* RFC 2821 4.4.
                When the delivery Smtp server makes the "final delivery" of a message, it inserts 
                a return-path line at the beginning of the mail data. This use of return-path is 
                required; mail systems MUST support it. The return-path line preserves the information 
                in the <reverse-path> from the MAIL command.
            */
            MultiStream finalStoreStream = new MultiStream();
            // e can be null if method caller isn't Smtp session.
            if(e != null){
                finalStoreStream.AppendStream(new MemoryStream(Encoding.Default.GetBytes("Return-Path: <" + e.Session.From.Mailbox + ">\r\n")));
            }
            finalStoreStream.AppendStream(filteredMsgStream);
            
			try{
				m_Api.StoreMessage("system",userName,storeFolder,finalStoreStream,DateTime.Now,new string[]{"Recent"});
			}
			catch{
                // Storing probably failed because there isn't such folder, just store to user inbox.
				m_Api.StoreMessage("system",userName,"Inbox",finalStoreStream,DateTime.Now,new string[]{"Recent"});
			}
        }

        #endregion

        #region method AstericMatch

		/// <summary>
		/// Checks if specified text matches to specified asteric pattern.
		/// </summary>
		/// <param name="pattern">Asteric pattern. Foe example: *xxx,*xxx*,xx*aa*xx, ... .</param>
		/// <param name="text">Text to match.</param>
		/// <returns></returns>
		public bool AstericMatch(string pattern,string text)
		{
			pattern = pattern.ToLower();
			text = text.ToLower();

			if(pattern == ""){
				pattern = "*";
			}

			while(pattern.Length > 0){
				// *xxx[*xxx...]
				if(pattern.StartsWith("*")){
					// *xxx*xxx
					if(pattern.IndexOf("*",1) > -1){
						string indexOfPart = pattern.Substring(1,pattern.IndexOf("*",1) - 1);
						if(text.IndexOf(indexOfPart) == -1){
							return false;
						}

						text = text.Substring(text.IndexOf(indexOfPart) + indexOfPart.Length + 1);
						pattern = pattern.Substring(pattern.IndexOf("*",1) + 1);
					}
					// *xxx   This is last pattern	
					else{				
						return text.EndsWith(pattern.Substring(1));
					}
				}
				// xxx*[xxx...]
				else if(pattern.IndexOfAny(new char[]{'*'}) > -1){
					string startPart = pattern.Substring(0,pattern.IndexOfAny(new char[]{'*'}));
		
					// Text must startwith
					if(!text.StartsWith(startPart)){
						return false;
					}

					text = text.Substring(text.IndexOf(startPart) + startPart.Length);
					pattern = pattern.Substring(pattern.IndexOfAny(new char[]{'*'}));
				}
				// xxx
				else{
					return text == pattern;
				}
			}

			return true;
		}

		#endregion

        #region method FolderMatches

		/// <summary>
		/// Gets if folder matches to specified folder pattern.
		/// </summary>
		/// <param name="folderPattern">Folder pattern. * and % between path separators have same meaning (asteric pattern). 
		/// If % is at the end, then matches only last folder child folders and not child folder child folders.</param>
		/// <param name="folder">Folder name with full path.</param>
		/// <returns></returns>
		private bool FolderMatches(string folderPattern,string folder)
		{
			folderPattern = folderPattern.ToLower();
			folder = folder.ToLower();

			string[] folderParts = folder.Split('/');
			string[] patternParts = folderPattern.Split('/');

			// pattern is more nested than folder
			if(folderParts.Length < patternParts.Length){
				return false;				
			}
			// This can happen only if * at end
			else if(folderParts.Length > patternParts.Length && !folderPattern.EndsWith("*")){
				return false;					
			}
			else{
				// Loop patterns
				for(int i=0;i<patternParts.Length;i++){
					string patternPart = patternParts[i].Replace("%","*");
					
					// This is asteric pattern
					if(patternPart.IndexOf('*') > -1){
						if(!AstericMatch(patternPart,folderParts[i])){
							return false;
						}
						// else process next pattern
					}
					// No *, this must be exact match
					else{
						if(folderParts[i] != patternPart){
							return false;
						}
					}
				}
			}

			return true;
		}

		#endregion

        #region method GenerateMessageMissing

        /// <summary>
        /// Generates message missing message.
        /// </summary>
        /// <returns>Returns message missing message.</returns>
        public static Mail_Message GenerateMessageMissing()
        {
            Mail_Message msg = new Mail_Message();
            msg.MimeVersion = "1.0";
            msg.MessageID = Mime_Utils.CreateMessageID();
            msg.Date = DateTime.Now;
            msg.From = new Mail_MailboxList();
            msg.From.Add(new Mail_Mailbox("system","system"));
            msg.To = new Mail_AddressList();
            msg.To.Add(new Mail_Mailbox("system","system"));
            msg.Subject = "[MESSAGE MISSING] Message no longer exists on server !";

            //--- multipart/mixed -------------------------------------------------------------------------------------------------
            Mime_Header_ContentType contentType_multipartMixed = new Mime_Header_ContentType(Mime_MediaTypes.Multipart.mixed);
            contentType_multipartMixed.Param_Boundary = Guid.NewGuid().ToString().Replace('-','.');
            Mime_Body_MultipartMixed multipartMixed = new Mime_Body_MultipartMixed(contentType_multipartMixed);
            msg.Body = multipartMixed;

                //--- text/plain ---------------------------------------------------------------------------------------------------
                Mime_Entity entity_text_plain = new Mime_Entity();
                Mime_Body_Text text_plain = new Mime_Body_Text(Mime_MediaTypes.Text.plain);
                entity_text_plain.Body = text_plain;
                text_plain.SetText(Mime_TransferEncodings.QuotedPrintable,Encoding.UTF8,"NOTE: Message no longer exists on server.\r\n\r\nMessage is deleted by Administrator or anti-virus software.\r\n");
                multipartMixed.BodyParts.Add(entity_text_plain);

            return msg;
        }

        #endregion


        #region method OnError

        /// <summary>
        /// Is called when error happens.
        /// </summary>
        /// <param name="x"></param>
        private void OnError(Exception x)
        {
            Error.DumpError(this.Name,x);
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets virtual server ID.
        /// </summary>
        public string ID
        {
            get{ return m_ID; }
        }

        /// <summary>
        /// Starts or stops server.
        /// </summary>
        public bool Enabled
        {
            get{ return m_Running; }

            set{
                if(value){
                    Start();
                }
                else{
                    Stop();
                }
            }
        }

        /// <summary>
        /// Gets virtual server name
        /// </summary>
        public string Name
        {
            get{ return m_Name; }
        }

        /// <summary>
        /// Gets this virtual server API.
        /// </summary>
        public IMailServerApi API
        {
            get{ return m_Api; }
        }

        /// <summary>
        /// Gets mailstore path.
        /// </summary>
        public string MailStorePath
        {
            get{ return m_MailStorePath; }
        }

        /// <summary>
        /// Gets virtual server DNS client.
        /// </summary>
        public Dns_Client DnsClient
        {
            get{ return m_DnsClient; }
        }

        /// <summary>
        /// Gets this virtual server Smtp server. Returns null if server is stopped.
        /// </summary>
        public Smtp_Server Smtp_Server
        {
            get{ return m_Smtp_Server; }
        }

        /// <summary>
        /// Gets this virtual server Pop3 server. Returns null if server is stopped.
        /// </summary>
        public Pop3_Server Pop3Server
        {
            get{ return m_Pop3Server; }
        }

        /// <summary>
        /// Gets this virtual server Imap server. Returns null if server is stopped.
        /// </summary>
        public Imap_Server Imap_Server
        {
            get{ return m_Imap_Server; }
        }

        /// <summary>
        /// Gets this virtual server Relay server. Returns null if server is stopped.
        /// </summary>
        public RelayServer RelayServer
        {
            get{ return m_RelayServer; }
        }

        ///// <summary>
        ///// Gets this virtual server SIP server. Returns null if server is stopped.
        ///// </summary>
        //public SIP_Proxy SipServer
        //{
        //    get{ return m_SipServer; }
        //}


        //---- ?? Used by management server log viewer.

        internal string SmtpLogsPath
        {
            get{ return m_SmtpLogPath; }
        }

        internal string Pop3_LogsPath
        {
            get{ return m_Pop3_LogPath; }
        }

        internal string ImapLogsPath
        {
            get{ return m_ImapLogPath; }
        }

        internal string RELAY_LogsPath
        {
            get{ return m_Relay_LogPath; }
        }

        internal string FETCH_LogsPath
        {
            get{ return m_Fetch_LogPath; }
        }
                
        #endregion


        #region MControl

        //NOTIF-DSN
        private NotifyServer m_NotifyServer = null;
        private string m_NotifyUrl = null;
        private NotifyDsnType m_NotifyType = NotifyDsnType.None;


        //NOTIFY-DSN
        /// <summary>
        /// Gets this virtual server Notify server. Returns null if server is stopped.
        /// </summary>
        public NotifyServer NotifyServer
        {
            get { return m_NotifyServer; }
        }

        private void LoadSettings_Relay(DataRow dr)
        {
            m_RelayServer.EnableConnectionPool = ConvertEx.ToBoolean(dr["EnableConnectionPool"]);
            m_RelayServer.EnableChunkItems = ConvertEx.ToBoolean(dr["EnableChunkItems"]);

        }

        private void LoadSettings_NotifyServer(DataRow dr)
        {
            //NOTIFY-DSN
            m_NotifyUrl = ConvertEx.ToString(dr["NotifyUrl"]);
            m_NotifyType = (NotifyDsnType)Enum.Parse(typeof(NotifyDsnType), dr["NotifyType"].ToString());
            //RelayServer_WriteLog("NotifyUrl:"+m_NotifyUrl);
            //RelayServer_WriteLog("NotifyType:" + m_NotifyType.ToString());

            if (m_NotifyType == NotifyDsnType.All || m_NotifyType == NotifyDsnType.Failure)
            {
                //NOTIF-DNS
                m_NotifyServer = new NotifyServer(this);

                m_NotifyServer.NotifyUrl = m_NotifyUrl;
                m_NotifyServer.NotifyType = m_NotifyType;
                m_NotifyServer.RelayInterval = m_RelayServer.RelayInterval;
                if (!m_NotifyServer.IsRunning)
                {
                    m_NotifyServer.Start();
                }
            }
            //end notify
        }


        #region Notify message

        /// <summary>
        /// Gets or sets if send notify Notify for relay.
        /// </summary>
        public NotifyDsnType NotifyType
        {
            get
            {
                return m_NotifyType;
            }

            set
            {
                m_NotifyType = value;
            }
        }
        /// <summary>
        /// Gets or sets Notify Url for relay.
        /// </summary>
        public string NotifyUrl
        {
            get
            {
                return m_NotifyUrl;
            }

            set
            {
                m_NotifyUrl = value;
            }
        }

        /// <summary>
        /// Process Dsn notification Message
        /// </summary>
        /// <param name="item"></param>
        public void ProcessDsnMessage(DsnItem item)
        {
            try
            {

                if (NotifyType == NotifyDsnType.Proc)
                {
                    item.NotifyUrl = NotifyUrl;
                    string response = item.ExecuteNotifyProc();
                    //RelayServer_WriteLog("ExecuteNotifyProc :" + response);
                    NetLog.Debug(response);
                }
                else
                {
                    string messageId = Guid.NewGuid().ToString();//relayMessage.MessageID;
                    ProcessDsnMessage(item.ToByte(), messageId);
                }

                item.Dispose();
            }
            catch (Exception ex)
            {
                RelayServer_WriteLog("ProcessDsnMessage error:" + ex.Message);
            }
        }


        //nissim dsn
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsn"></param>
        /// <param name="messageId"></param>
        public void ProcessDsnMessage(string dsn, string messageId)
        {
            try
            {
                string DsnPath = this.MailStorePath + "Dsn";

                if (!Directory.Exists(DsnPath))
                {
                    Directory.CreateDirectory(DsnPath);
                }

                string path = DsnPath + "\\" + messageId + ".dsn";
                File.AppendAllText(path, dsn);

            }
            catch
            {

            }

        }
        //nissim dsn
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsn"></param>
        /// <param name="messageId"></param>
        public void ProcessDsnMessage(byte[] dsn, string messageId)
        {
            try
            {
                string DsnPath = this.MailStorePath + "Dsn";

                if (!Directory.Exists(DsnPath))
                {
                    Directory.CreateDirectory(DsnPath);
                }

                string path = DsnPath + "\\" + messageId + ".dsn";
                File.WriteAllBytes(path, dsn);

            }
            catch
            {

            }

        }

        #endregion

        #region method m_RelayServer_WriteLog

        //private void m_RelayServer_WriteLog(object sender, MControl.Messaging.Net.Log.WriteLogEventArgs e)
        //{
        //    Logger.WriteLog(m_Relay_LogPath + "relay-" + DateTime.Today.ToString("yyyyMMdd") + ".log", e.LogEntry);
        //}
        /// <summary>
        /// RelayServer_WriteLog
        /// </summary>
        /// <param name="text"></param>
        public void RelayServer_WriteLog(string text)
        {
            Logger.WriteLog(m_Relay_LogPath + "relay-" + DateTime.Today.ToString("yyyyMMdd") + ".log", text);
        }

        #endregion
        #endregion

    }
}
