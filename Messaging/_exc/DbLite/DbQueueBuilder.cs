using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging.DbSLite
{
    public class DbLieQueueBuilder
    {


        const string QCoverInsert = @"
                    insert into QCover (  
                        Host,
                        MessageState,
                        Command,
                        Priority,
                        UniqueId,
                        Retry,
                        ArrivedTime,
                        SentTime,
                        Modified,
                        Expiration,
                        MessageId,
                        BodyStream)
                        values(
                        @Host,
                        @MessageState,
                        @Command,
                        @Priority,
                        @UniqueId,
                        @Retry,
                        @ArrivedTime,
                        @SentTime,
                        @Modified,
                        @Expiration,
                        @MessageId,
                        @BodyStream)";

        const string QCoverUpdate = @"
                    update QCover (  
                        Host,
                        MessageState,
                        Command,
                        Priority,
                        UniqueId,
                        Retry,
                        ArrivedTime,
                        SentTime,
                        Modified,
                        Expiration,
                        MessageId,
                        BodyStream)
                        values(
                        @Host,
                        @MessageState,
                        @Command,
                        @Priority,
                        @UniqueId,
                        @Retry,
                        @ArrivedTime,
                        @SentTime,
                        @Modified,
                        @Expiration,
                        @MessageId,
                        @BodyStream)";

        #region statements

        const string FifoQueue = @"
                    create table FifoQueue (  
                        Host varchar(50) not null,
                        MessageState tinyint not null,
                        Command tinyint not null,
                        Priority tinyint not null,
                        UniqueId bigint not null,
                        Retry tinyint not null,
                        ArrivedTime datetime null,
                        SentTime datetime null,
                        Modified datetime null,
                        Expiration int null,
                        MessageId int null,
                        BodyStream varbinary(max)); 
                    go";
        const string cdxFifoQueue = @"
                    create clustered index cdxFifoQueue on FifoQueue (UniqueId);
                    go";

        const string QCover = @"
                    create table QCover (  
                        Host varchar(50) not null,
                        MessageState tinyint not null,
                        Command tinyint not null,
                        Priority tinyint not null,
                        UniqueId bigint PRIMARY KEY ASC not null,
                        Retry tinyint not null,
                        ArrivedTime datetime null,
                        SentTime datetime null,
                        Modified datetime null,
                        Expiration int null,
                        MessageId int null,
                        BodyStream varbinary(max)); 
                    go";
        const string cdxQCover = @"
                    create clustered index cdxQCover on QCover (Id);
                    go";

        const string SuspendQueue = @"
                    create table SuspendQueue (  
                        Id bigint not null identity(1,1),
                        Host varchar(50) not null,
                        MessageState tinyint not null,
                        Command tinyint not null,
                        Priority tinyint not null,
                        UniqueId bigint not null,
                        Retry tinyint not null,
                        ArrivedTime datetime null,
                        SentTime datetime null,
                        Modified datetime null,
                        Expiration int null,
                        MessageId int null,
                        BodyStream varbinary(max)); 
                    go";
        const string cdxSuspendQueue = @"
                    create clustered index cdxSuspendQueue on SuspendQueue (Id);
                    go";

        const string TransQueue = @"
                    create table TransQueue (  
                        UniqueId bigint PRIMARY KEY ASC not null,
                        Host varchar(50) not null,
                        MessageState tinyint not null,
                        Modified datetime null,
                        Expiration int null
                       ); 
                    go";
        const string cdxTransQueue = @"
                    create clustered index cdxTransQueue on TransQueue (UniqueId);
                    go";


        const string sp_enqueue_fifo = @"
                    create procedure sp_enqueue_fifo 
                        @Host varchar(50),
                        @MessageState tinyint,
                        @Command tinyint,
                        @Priority tinyint,
                        @UniqueId bigint,
                        @Retry tinyint,
                        @ArrivedTime datetime,
                        @SentTime datetime,
                        @Modified datetime,
                        @Expiration int,
                        @MessageId int,
                        @BodyStream varbinary(max)
                        as  
                        set nocount on;  
                        insert into FifoQueue (Host,MessageState,Command,Priority,UniqueId,Retry,ArrivedTime,SentTime,Modified,Expiration,MessageId,BodyStream) 
                                        values (@Host,@MessageState,@Command,@Priority,@UniqueId,@Retry,@ArrivedTime,@SentTime,@Modified,@Expiration,@MessageId,@BodyStream); 
                    go";

        const string sp_dequeue_fifo_trans = @"
                    create procedure sp_dequeue_fifo_trans
                        @Host varchar(50),
                        @Expiration int
                        as  
                        set nocount on;  
                        declare 
                        @UniqueId bigint

                        UPDATE  Q
                        SET     MessageState = 1,
                                @UniqueId = UniqueId,
                                Retry=Retry+1
                        FROM    (
                                select top(1) UniqueId,MessageState,BodyStream      
                                from FifoQueue with (rowlock, updlock, readpast) 
                                where Host=@Host and MessageState=0  
                                order by Priority,UniqueId
                                ) Q;
                        if @UniqueId is null
                            return;
                        insert into TransQueue (UniqueId,Host,Modified,Expiration) 
                                        values (@UniqueId,@Host,getdate(),@Expiration); 
                    go";

        const string sp_dequeue_fifo_trans_item = @"
                    create procedure sp_dequeue_fifo_trans_item
                        @Host varchar(50),
                        @UniqueId bigint,
                        @Expiration int
                        as  
                        set nocount on;  
                        declare 
                        @ExistsId bigint

                        UPDATE  Q
                        SET     MessageState = 1,
                                Retry=Retry+1,
                                @ExistsId=UniqueId
                        FROM    (
                                select top(1) UniqueId,MessageState,BodyStream      
                                from FifoQueue with (rowlock, updlock, readpast) 
                                where Host=@Host and UniqueId=@UniqueId and MessageState=0  
                                ) Q;
                        if @ExistsId is null
                            return;
                        insert into TransQueue (UniqueId,Host,Modified,Expiration) 
                                        values (@UniqueId,@Host,getdate(),@Expiration); 
                    go";

        const string sp_fifo_commit = @"
                    create procedure sp_fifo_commit
                        @UniqueId bigint
                        as  
                        set nocount on;  
                        
                        delete from FifoQueue with (rowlock, readpast)
                            where UniqueId=@UniqueId;

                        delete from TransQueue with (rowlock, readpast)
                            where UniqueId=@UniqueId;
                    go";

        const string sp_fifo_abort = @"
                    create procedure sp_fifo_abort
                        @UniqueId bigint,
                        @MaxRetry tinyint
                        as  
                        set nocount on;  
                        declare 
                        @Retry tinyint

                        UPDATE  Q
                        SET     
                        @Retry=Retry+1
                        MessageState=case when Retry+1 >= @MaxRetry then 2 else 0 end,
                        Retry=Retry+1
                        from FifoQueue Q with (rowlock, updlock, readpast) 
                        where UniqueId=@UniqueId   

                        delete from TransQueue with (rowlock, readpast)
                            where UniqueId=@UniqueId;

                        if @Retry >= @MaxRetry
                        begin
                        insert into SuspendQueue (Host,MessageState,Command,Priority,UniqueId,Retry,ArrivedTime,SentTime,Modified,Expiration,MessageId,BodyStream) 
                        select Host,MessageState,Command,Priority,UniqueId,Retry,ArrivedTime,SentTime,Modified,Expiration,MessageId,BodyStream
                        from FifoQueue
                        where UniqueId=@UniqueId

                        delete from FifoQueue with (rowlock, readpast)
                            where UniqueId=@UniqueId;

                        end

                    go";

        const string sp_fifo_trans_job = @"
                    create procedure sp_fifo_trans_job
                        as  
                        set nocount on;  
                        
                        declare 
                        @curTime datetime,
                        @MaxRetry tinyint
                        
                        set @curTime=getdate();
                        set @MaxRetry=3;

                        CREATE TABLE #TempTable(UniqueId bigint)

                        INSERT INTO #TempTable (UniqueId) 
                        select top 100 UniqueId 
                        from TransQueue
                        where DATEADD(minute,Expiration,Modified) < @curTime

                        insert into SuspendQueue (Host,MessageState,Command,Priority,UniqueId,Retry,ArrivedTime,SentTime,Modified,Expiration,MessageId,BodyStream) 
                        select q.Host,q.MessageState,q.Command,q.Priority,q.UniqueId,q.Retry,q.ArrivedTime,q.SentTime,q.Modified,q.Expiration,q.MessageId,q.BodyStream
                        from FifoQueue q inner join #TempTable t on q.UniqueId=t.UniqueId
                        where q.Retry>=@MaxRetry

                        delete q
                        from FifoQueue q inner join #TempTable t on q.UniqueId=t.UniqueId
                        where q.Retry>=@MaxRetry

                        delete q
                        from TransQueue q inner join #TempTable t on q.UniqueId=t.UniqueId

                        update q
                        set
                        q.Retry=q.Retry+1,
                        Command=0
                        from FifoQueue q inner join #TempTable t on q.UniqueId=t.UniqueId
                        where q.Retry<@MaxRetry
                        

                    go";

        const string sp_dequeue_fifo = @"
                    create procedure sp_dequeue_fifo
                        @Host varchar(50)
                        as  
                        set nocount on;  
                        with cte as (    
                            select top(1) BodyStream      
                            from FifoQueue with (rowlock, readpast) 
                            where Host=@Host and MessageState=0  
                            order by Priority,UniqueId)  
                        delete from cte
                            output deleted.BodyStream;
                    go";

        const string sp_dequeue_fifo_fast = @"
                    create procedure sp_dequeue_fifo_alt
                        @Host varchar(50)
                        as  
                        set nocount on;  
                        delete top(1) 
                        from FifoQueue 
                        output deleted.BodyStream
                        where 
                        UniqueId = (select top(1) UniqueId  
                                from FifoQueue with (rowlock, updlock, readpast)
                                order by Priority,UniqueId);
                    go";

        const string sp_dequeue_fifo_item = @"
                    create procedure sp_dequeue_fifo_item
                        @Host varchar(50),
                        @UniqueId bigint
                        as  
                        set nocount on;  
                        with cte as (    
                            select top(1) BodyStream      
                            from FifoQueue with (rowlock, readpast) 
                            where Host=@Host and UniqueId=@UniqueId and MessageState=0)  
                        delete from cte
                            output deleted.BodyStream;
                    go";

        const string sp_peek_fifo = @"
                    create procedure sp_peek_fifo
                        @Host varchar(50)
                        as  
                        set nocount on;  
                        select top(1) BodyStream      
                        from FifoQueue with (rowlock, readpast) 
                        where Host=@Host and MessageState=0  
                        order by Priority,UniqueId
                    go";

        const string sp_peek_fifo_item = @"
                    create procedure sp_peek_fifo_item
                        @Host varchar(50),
                        @UniqueId bigint
                        as  
                        set nocount on;  
                        select top(1) BodyStream      
                        from FifoQueue with (rowlock, readpast) 
                        where Host=@Host and UniqueId=@UniqueId and MessageState=0  
                    go";

        const string sp_fifo_count = @"
                    create procedure sp_fifo_count
                        @Host varchar(50)
                        as  
                        set nocount on;  
                        select count(*) as Count      
                        from FifoQueue with (nolock) 
                        where Host=@Host and MessageState=0 
                    go";

        const string sp_fifo_clear = @"
                    create procedure sp_fifo_clear
                        @Host varchar(50)
                        as  
                        set nocount on;
  
                        delete      
                        from FifoQueue with
                        where Host=@Host

                        delete
                        from TransQueue 
                        where Host=@Host
                    go";

        

        
        #endregion

    }
}
