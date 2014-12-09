with nonleaves as 
(
	select distinct f1.*
	from filter f1 inner join Filter f2 
		on f1.fltFilterID=f2.fltParentID and f1.fltTypeID in (1,2)
)
SELECT 
		e.ententityid as ID
	, e.entName  as Name
	, n.fltFilterID  as FilterID
	, n.fltName  as [Filter Name]
from nonleaves n 
	INNER JOIN Filter_Entity FE 
		ON FE.fieFilterID=n.fltFilterID 
			and n.fltFilterID not 
				in (3,51,55,137,180,208,233,253,260,258,259,296,306,307,327,315,314,316,359,369, 405 ,410 ,411 ,412 ,413)
	INNER JOIN Entity E 
		ON E.entEntityID=fe.fieEntityID	and e.entEntityTypeID>0 
order by fltFilterID, ententityid

