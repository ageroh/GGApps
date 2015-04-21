select 
		ententityid as ID
	  , entName as Name 
FROM entity e 
where not exists
(
  select 'x'
  from filter_entity fe 
  inner join filter f on f.fltfilterid=fe.fiefilterid and flttypeid=1
  where e.ententityid=fe.fieentityid
)
and ententitytypeid>0 
order by ententityid,entname
