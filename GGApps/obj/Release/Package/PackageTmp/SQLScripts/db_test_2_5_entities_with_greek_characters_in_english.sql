select 
entEntityID, 
entEntityTypeID, 
case isnull(PATINDEX('%[�-��-٢-�]%',title_s),0)
when 0 then ''
when 1 then '{'+SUBSTRING(TITLE_S,1,1)+'}'+SUBSTRING(TITLE_S,2,1000)
else SUBSTRING(TITLE_S,1,PATINDEX('%[�-��-٢-�]%',title_s)-1)+'{'+SUBSTRING(TITLE_S,PATINDEX('%[�-��-٢-�]%',title_s),1)+'}'+SUBSTRING(TITLE_S,PATINDEX('%[�-��-٢-�]%',title_s)+1,1000)
end as TITLE_S,isnull(PATINDEX('%[�-��-٢-�]%',title_s),0) as pos1,
case isnull(PATINDEX('%[�-��-٢-�]%',SHORT_DESCRIPTION_T),0)
when 0 then ''
when 1 then '{'+SUBSTRING(SHORT_DESCRIPTION_T,1,1)+'}'+SUBSTRING(SHORT_DESCRIPTION_T,2,1000)
else SUBSTRING(SHORT_DESCRIPTION_T,1,PATINDEX('%[�-��-٢-�]%',SHORT_DESCRIPTION_T)-1)+'{'+SUBSTRING(SHORT_DESCRIPTION_T,PATINDEX('%[�-��-٢-�]%',SHORT_DESCRIPTION_T),1)+'}'+SUBSTRING(SHORT_DESCRIPTION_T,PATINDEX('%[�-��-٢-�]%',SHORT_DESCRIPTION_T)+1,1000)
end as SHORT_DESCRIPTION_T,isnull(PATINDEX('%[�-��-٢-�]%',SHORT_DESCRIPTION_T),0) as pos2,
case isnull(PATINDEX('%[�-��-٢-�]%',BODY_T),0)
when 0 then ''
when 1 then '{'+SUBSTRING(BODY_T,1,1)+'}'+SUBSTRING(BODY_T,2,1000)
else SUBSTRING(BODY_T,1,PATINDEX('%[�-��-٢-�]%',BODY_T)-1)+'{'+SUBSTRING(BODY_T,PATINDEX('%[�-��-٢-�]%',BODY_T),1)+'}'+SUBSTRING(BODY_T,PATINDEX('%[�-��-٢-�]%',BODY_T)+1,1000)
end as BODY_T,isnull(PATINDEX('%[�-��-٢-�]%',BODY_T),0) as pos3,
case isnull(PATINDEX('%[�-��-٢-�]%',CAPTION_S),0)
when 0 then ''
when 1 then '{'+SUBSTRING(CAPTION_S,1,1)+'}'+SUBSTRING(CAPTION_S,2,1000)
else SUBSTRING(CAPTION_S,1,PATINDEX('%[�-��-٢-�]%',CAPTION_S)-1)+'{'+SUBSTRING(CAPTION_S,PATINDEX('%[�-��-٢-�]%',CAPTION_S),1)+'}'+SUBSTRING(CAPTION_S,PATINDEX('%[�-��-٢-�]%',CAPTION_S)+1,1000)
end as CAPTION_S,isnull(PATINDEX('%[�-��-٢-�]%',CAPTION_S),0) as pos4,
case isnull(PATINDEX('%[�-��-٢-�]%',OPENING_HOURS_S),0)
when 0 then ''
when 1 then '{'+SUBSTRING(OPENING_HOURS_S,1,1)+'}'+SUBSTRING(OPENING_HOURS_S,2,1000)
else SUBSTRING(OPENING_HOURS_S,1,PATINDEX('%[�-��-٢-�]%',OPENING_HOURS_S)-1)+'{'+SUBSTRING(OPENING_HOURS_S,PATINDEX('%[�-��-٢-�]%',OPENING_HOURS_S),1)+'}'+SUBSTRING(OPENING_HOURS_S,PATINDEX('%[�-��-٢-�]%',OPENING_HOURS_S)+1,1000)
end as OPENING_HOURS_S,isnull(PATINDEX('%[�-��-٢-�]%',OPENING_HOURS_S),0) as pos5,
case isnull(PATINDEX('%[�-��-٢-�]%',PRICE_S),0)
when 0 then ''
when 1 then '{'+SUBSTRING(PRICE_S,1,1)+'}'+SUBSTRING(PRICE_S,2,1000)
else SUBSTRING(PRICE_S,1,PATINDEX('%[�-��-٢-�]%',PRICE_S)-1)+'{'+SUBSTRING(PRICE_S,PATINDEX('%[�-��-٢-�]%',PRICE_S),1)+'}'+SUBSTRING(PRICE_S,PATINDEX('%[�-��-٢-�]%',PRICE_S)+1,1000)
end as PRICE_S,isnull(PATINDEX('%[�-��-٢-�]%',PRICE_S),0) as pos6,
case isnull(PATINDEX('%[�-��-٢-�]%',USEFUL_TIPS_T),0)
when 0 then ''
when 1 then '{'+SUBSTRING(USEFUL_TIPS_T,1,1)+'}'+SUBSTRING(USEFUL_TIPS_T,2,1000)
else SUBSTRING(USEFUL_TIPS_T,1,PATINDEX('%[�-��-٢-�]%',USEFUL_TIPS_T)-1)+'{'+SUBSTRING(USEFUL_TIPS_T,PATINDEX('%[�-��-٢-�]%',USEFUL_TIPS_T),1)+'}'+SUBSTRING(USEFUL_TIPS_T,PATINDEX('%[�-��-٢-�]%',USEFUL_TIPS_T)+1,1000)
end as USEFUL_TIPS_T,isnull(PATINDEX('%[�-��-٢-�]%',USEFUL_TIPS_T),0) as pos7,
case isnull(PATINDEX('%[�-��-٢-�]%',ADDRESS_S),0)
when 0 then ''
when 1 then '{'+SUBSTRING(ADDRESS_S,1,1)+'}'+SUBSTRING(ADDRESS_S,2,1000)
else SUBSTRING(ADDRESS_S,1,PATINDEX('%[�-��-٢-�]%',ADDRESS_S)-1)+'{'+SUBSTRING(ADDRESS_S,PATINDEX('%[�-��-٢-�]%',ADDRESS_S),1)+'}'+SUBSTRING(ADDRESS_S,PATINDEX('%[�-��-٢-�]%',ADDRESS_S)+1,1000)
end as ADDRESS_S,isnull(PATINDEX('%[�-��-٢-�]%',ADDRESS_S),0) as pos8,
case isnull(PATINDEX('%[�-��-٢-�]%',PATH_C),0)
when 0 then ''
when 1 then '{'+SUBSTRING(PATH_C,1,1)+'}'+SUBSTRING(PATH_C,2,1000)
else SUBSTRING(PATH_C,1,PATINDEX('%[�-��-٢-�]%',PATH_C)-1)+'{'+SUBSTRING(PATH_C,PATINDEX('%[�-��-٢-�]%',PATH_C),1)+'}'+SUBSTRING(PATH_C,PATINDEX('%[�-��-٢-�]%',PATH_C)+1,1000)
end as PATH_C,isnull(PATINDEX('%[�-��-٢-�]%',PATH_C),0) as pos9,
case isnull(PATINDEX('%[�-��-٢-�]%',EDITORIAL_T),0)
when 0 then ''
when 1 then '{'+SUBSTRING(EDITORIAL_T,1,1)+'}'+SUBSTRING(EDITORIAL_T,2,1000)
else SUBSTRING(EDITORIAL_T,1,PATINDEX('%[�-��-٢-�]%',EDITORIAL_T)-1)+'{'+SUBSTRING(EDITORIAL_T,PATINDEX('%[�-��-٢-�]%',EDITORIAL_T),1)+'}'+SUBSTRING(EDITORIAL_T,PATINDEX('%[�-��-٢-�]%',EDITORIAL_T)+1,1000)
end as EDITORIAL_T,isnull(PATINDEX('%[�-��-٢-�]%',EDITORIAL_T),0) as pos10
from entity
where isnull(PATINDEX('%[�-��-٢-�]%',title_s),0)+
isnull(PATINDEX('%[�-��-٢-�]%',SHORT_DESCRIPTION_T),0)+
isnull(PATINDEX('%[�-��-٢-�]%',BODY_T),0)+
isnull(PATINDEX('%[�-��-٢-�]%',CAPTION_S),0)+
isnull(PATINDEX('%[�-��-٢-�]%',OPENING_HOURS_S),0)+
isnull(PATINDEX('%[�-��-٢-�]%',PRICE_S),0)+
isnull(PATINDEX('%[�-��-٢-�]%',USEFUL_TIPS_T ),0)+
isnull(PATINDEX('%[�-��-٢-�]%',ADDRESS_S),0)+
isnull(PATINDEX('%[�-��-٢-�]%',PATH_C),0)+
isnull(PATINDEX('%[�-��-٢-�]%',EDITORIAL_T),0)>0
and ententitytypeid>0
order by 2, 1