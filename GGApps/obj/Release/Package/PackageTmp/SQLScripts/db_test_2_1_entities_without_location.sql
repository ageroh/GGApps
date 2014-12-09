select 
	  ententityid as ID
	, entName as Name
from entity
where not exists
(select 1
from [Category_Entity] inner join Category on catCategoryID=[caeCategoryID]
where entEntityID=[caeEntityID]
and (catcode like '0#2#3#%' or catcode like '0#2#137#%' or catcode like '0#2#180#%' or catcode like '0#2#208#%' 
	 or catcode like '0#2#233#%' or catcode like '0#2#253#%' or catcode like '0#2#260#%' or catcode like '0#2#258#%' 
	 or catcode like '0#2#259#%' or catcode like '0#2#296#%' or catcode like '0#2#306#%' or catcode like '0#2#307#%'
	 or catcode like '0#2#327#%' or catcode like '0#2#315#%' or catcode like '0#2#314#%' or catcode like '0#2#316#%' 
	 or catcode like '0#2#359#%' or catcode like '0#2#369#%' or catcode like '0#2#405#%' or catcode like '0#2#410#%' 
	 or catcode like '0#2#411#%' or catcode like '0#2#412#%' or catcode like '0#2#413#%' ))
and entEntityTypeID>0
and entStatusID=3

