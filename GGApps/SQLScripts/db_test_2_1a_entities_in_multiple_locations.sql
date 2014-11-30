select 
	  ententityid 
	, entName
	, COUNT(*)
FROM entity e 
inner join filter_entity fe 
	on e.ententityid=fe.fieentityid
inner join filter f 
	on f.fltfilterid=fe.fiefilterid and flttypeid=2 
where ententitytypeid>0 
group by ententityid,entname
having COUNT(*)>1


