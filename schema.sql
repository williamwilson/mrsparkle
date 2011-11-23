if not exists(select 1 from information_schema.TABLES where TABLE_NAME = 'message')
begin
	print 'creating message'
	
	create table [message] (
		id varchar(25) not null,
		sequence int not null,
		room varchar(100) not null,
		[time] datetime not null,
		[from] varchar(100) not null,
		body varchar(1000) null
	)
end
go

grant insert, select, update, delete on [message] to public
go

if not exists(select 1 from sys.indexes where name = 'message_id')
begin
	print 'creating message_id index'

	create unique nonclustered index message_id on [message] (id,sequence)
end
go

if not exists(select 1 from information_schema.tables where table_name = 'hashtag')
begin
	print 'creating hashtag'
	
	create table hashtag (
		tag varchar(100) not null,
		message_id varchar(25) not null
	)
end
go

grant insert, select, update, delete on [hashtag] to public
go

if not exists(select 1 from sys.indexes where name = 'hashtag_tag')
begin
	print 'creating hashtag_tag index'
	
	create clustered index hashtag_tag on hashtag (tag)
end
go

/*
drop table [message]
drop table hashtag
*/

select * from [message]
select * from hashtag

select *
from [message]
where room= 'unityforms'
	and [time] >= '2011-11-13' 
	and [time] <= dateadd(d, 1, CONVERT(date, '2011-11-13'))
order by [time] desc, id, sequence