select entEntityID as ID,title_s as Title,entName as Name ,BODY_T as Body ,ent_hidden_BodyRaw as [Hidden Body], 'CHAR (' as 'SearchString'
from Entity 
where BODY_T like '%char(%'
UNION
select entEntityID as ID,title_s as Title, entName  as Name, BODY_T as Body , ent_hidden_BodyRaw  as [Hidden Body], 'FONT-STYLE FONT' as 'SearchString'
from Entity 
where ent_hidden_BodyRaw  like '%<font%'  or ent_hidden_BodyRaw  like '%style="font%'