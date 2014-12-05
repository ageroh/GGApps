select entEntityID,title_s,entName,BODY_T  ,ent_hidden_BodyRaw , 'CHAR (' as 'SearchString'
from Entity 
where BODY_T like '%char(%'
UNION
select entEntityID,title_s,entName,BODY_T  ,ent_hidden_BodyRaw , '&#' as 'SearchString'
from Entity 
where BODY_T like '%&#%'
UNION
select entEntityID,title_s,entName,BODY_T, ent_hidden_BodyRaw , 'FONT-STYLE FONT' as 'SearchString'
from Entity 
where ent_hidden_BodyRaw  like '%<font%'  or ent_hidden_BodyRaw  like '%style="font%'